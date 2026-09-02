using System.Globalization;
using System.Xml.Linq;
using AwesomeAssertions;
using LightBurn.Format;

namespace LightBurn.Format.Tests;

public class LightBurnWriterTests
{
    private static LightBurnProject ProjectWithOneLayer()
    {
        var project = new LightBurnProject();
        project.AddLayer(new CutSetting { Index = 0, MaxPower = 35, Speed = 800 });
        return project;
    }

    private static XDocument Write(LightBurnProject project, LightBurnWriterOptions? options = null) =>
        XDocument.Parse(new LightBurnWriter(options).ToXml(project));

    [Fact]
    public void Declaration_states_the_encoding_the_bytes_are_actually_in()
    {
        var xml = new LightBurnWriter().ToXml(ProjectWithOneLayer());

        xml.Should().StartWith("<?xml version=\"1.0\" encoding=\"utf-8\"?>");
    }

    [Fact]
    public void Saved_file_round_trips_through_a_strict_xml_parser()
    {
        var path = Path.Combine(Path.GetTempPath(), $"lbrn-{Guid.NewGuid():N}.lbrn");
        try
        {
            var project = ProjectWithOneLayer();
            project.Add(RectShape.FromCorner(0, 0, 10, 10));
            new LightBurnWriter().Save(project, path);

            var reloaded = XDocument.Load(path);

            reloaded.Root!.Elements("Shape").Should().ContainSingle();
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Fact]
    public void Failed_save_leaves_the_existing_file_untouched()
    {
        var path = Path.Combine(Path.GetTempPath(), $"lbrn-{Guid.NewGuid():N}.lbrn");
        try
        {
            var good = ProjectWithOneLayer();
            good.Add(RectShape.FromCorner(0, 0, 5, 5));
            new LightBurnWriter().Save(good, path);
            var original = File.ReadAllText(path);

            var broken = ProjectWithOneLayer();
            broken.Add(RectShape.FromCorner(0, 0, 5, 5, cutIndex: 99));
            var save = () => new LightBurnWriter().Save(broken, path);

            save.Should().Throw<InvalidOperationException>();
            File.ReadAllText(path).Should().Be(original);
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Fact]
    public void Overwriting_a_longer_file_leaves_no_tail_of_the_old_one()
    {
        var path = Path.Combine(Path.GetTempPath(), $"lbrn-{Guid.NewGuid():N}.lbrn");
        try
        {
            var big = ProjectWithOneLayer();
            for (var i = 0; i < 200; i++)
            {
                big.Add(RectShape.FromCorner(i, i, 5, 5));
            }

            var small = ProjectWithOneLayer();
            small.Add(RectShape.FromCorner(0, 0, 5, 5));

            var writer = new LightBurnWriter();
            writer.Save(big, path);
            var bigLength = new FileInfo(path).Length;
            writer.Save(small, path);

            new FileInfo(path).Length.Should().BeLessThan(bigLength);
            XDocument.Load(path).Root!.Elements("Shape").Should().ContainSingle();

            Directory.EnumerateFiles(Path.GetTempPath(), "*.tmp")
                .Where(file => Path.GetFileName(file).StartsWith($".{Path.GetFileName(path)}"))
                .Should().BeEmpty("the temporary file should not survive a successful save");
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Fact]
    public void Root_element_carries_the_document_attributes()
    {
        var document = Write(ProjectWithOneLayer());

        var root = document.Root!;
        root.Name.LocalName.Should().Be("LightBurnProject");
        root.Attribute("FormatVersion")!.Value.Should().Be("1");
        root.Attribute("MirrorX")!.Value.Should().Be("False");
    }

    [Fact]
    public void Layer_is_written_as_a_CutSetting_with_value_children()
    {
        var project = new LightBurnProject();
        project.AddLayer(new CutSetting
        {
            Index = 3,
            Type = CutSettingType.Scan,
            MaxPower = 42.5,
            Speed = 1200,
            Interval = 0.02,
        });

        var setting = Write(project).Root!.Element("CutSetting")!;

        setting.Attribute("type")!.Value.Should().Be("Scan");
        setting.Element("index")!.Attribute("Value")!.Value.Should().Be("3");
        setting.Element("name")!.Attribute("Value")!.Value.Should().Be("C03");
        setting.Element("maxPower")!.Attribute("Value")!.Value.Should().Be("42.5");
        setting.Element("interval")!.Attribute("Value")!.Value.Should().Be("0.02");
    }

    [Theory]
    [InlineData(CutSettingType.Cut, "Cut")]
    [InlineData(CutSettingType.Scan, "Scan")]
    [InlineData(CutSettingType.Image, "Image")]
    [InlineData(CutSettingType.Offset, "Offset")]
    public void Layer_type_maps_to_its_attribute_text(CutSettingType type, string expected)
    {
        var project = new LightBurnProject();
        project.AddLayer(new CutSetting { Index = 0, Type = type });

        var setting = Write(project).Root!.Element("CutSetting")!;

        setting.Attribute("type")!.Value.Should().Be(expected);
    }

    [Fact]
    public void Fill_plus_line_is_a_scan_layer_with_a_nested_cut_sub_layer()
    {
        // Matches what LightBurn itself writes: there is no "Scan+Cut" layer type.
        var project = new LightBurnProject();
        project.AddLayer(new CutSetting
        {
            Index = 0,
            Type = CutSettingType.Scan,
            Speed = 300,
            MaxPower = 100,
            Interval = 0.01,
            SubLayer = new SubLayer { MaxPower = 100, Speed = 300 },
        });

        var setting = Write(project).Root!.Element("CutSetting")!;

        setting.Attribute("type")!.Value.Should().Be("Scan");
        setting.Element("interval")!.Attribute("Value")!.Value.Should().Be("0.01");

        var subLayer = setting.Element("SubLayer")!;
        subLayer.Attribute("type")!.Value.Should().Be("Cut");
        subLayer.Attribute("index")!.Value.Should().Be("1");
        subLayer.Element("maxPower")!.Attribute("Value")!.Value.Should().Be("100");
        subLayer.Element("speed")!.Attribute("Value")!.Value.Should().Be("300");
        subLayer.Element("numPasses")!.Attribute("Value")!.Value.Should().Be("1");
    }

    [Fact]
    public void Sub_layer_passes_are_written_independently_of_the_layers()
    {
        // LightBurn honours a sub-layer's own pass count — verified with
        // probe/08-sublayer-passes.lbrn, which the user opened and reported on.
        var project = new LightBurnProject();
        project.AddLayer(new CutSetting
        {
            Index = 0,
            Type = CutSettingType.Scan,
            NumPasses = 5,
            SubLayer = new SubLayer { MaxPower = 100, Speed = 300 },
        });

        var setting = Write(project).Root!.Element("CutSetting")!;

        setting.Element("numPasses")!.Attribute("Value")!.Value.Should().Be("5");
        setting.Element("SubLayer")!.Element("numPasses")!.Attribute("Value")!.Value.Should().Be("1");
    }

    [Fact]
    public void No_sub_layer_element_when_none_is_set()
    {
        Write(ProjectWithOneLayer()).Root!.Element("CutSetting")!.Element("SubLayer").Should().BeNull();
    }

    [Fact]
    public void Frequency_is_omitted_when_not_set()
    {
        var setting = Write(ProjectWithOneLayer()).Root!.Element("CutSetting")!;

        setting.Element("frequency").Should().BeNull();
    }

    [Fact]
    public void Rect_from_corner_is_centred_by_its_transform()
    {
        var project = ProjectWithOneLayer();
        project.Add(RectShape.FromCorner(x: 10, y: 20, width: 40, height: 20));

        var shape = Write(project).Root!.Element("Shape")!;

        shape.Attribute("Type")!.Value.Should().Be("Rect");
        shape.Attribute("W")!.Value.Should().Be("40");
        shape.Attribute("H")!.Value.Should().Be("20");
        shape.Element("XForm")!.Value.Should().Be("1 0 0 1 30 30");
    }

    [Fact]
    public void Polyline_writes_vertices_and_line_primitives()
    {
        var project = ProjectWithOneLayer();
        project.Add(PathShape.Polyline([(0, 0), (10, 0), (10, 5)], closed: true));

        var shape = Write(project).Root!.Element("Shape")!;

        shape.Element("VertList")!.Value.Should().Be("V0 0V10 0V10 5");
        shape.Element("PrimList")!.Value.Should().Be("L0 1L1 2L2 0");
    }

    [Fact]
    public void Rectangle_path_has_four_corners_around_the_given_centre()
    {
        var project = ProjectWithOneLayer();
        project.Add(PathShape.Rectangle(centreX: 75, centreY: 75, width: 90, height: 50));

        var shape = Write(project).Root!.Element("Shape")!;

        shape.Attribute("Type")!.Value.Should().Be("Path");
        shape.Element("VertList")!.Value.Should().Be("V30 50V120 50V120 100V30 100");
        shape.Element("PrimList")!.Value.Should().Be("L0 1L1 2L2 3L3 0");
    }

    [Fact]
    public void Bezier_handles_are_written_as_absolute_coordinates()
    {
        var project = ProjectWithOneLayer();
        var path = new PathShape();
        path.Vertices.Add(new Vertex { X = 0, Y = 0, ControlOut = (5, 5) });
        path.Vertices.Add(new Vertex { X = 20, Y = 0, ControlIn = (15, 5) });
        path.Primitives.Add(new PathPrimitive(PrimitiveKind.Bezier, 0, 1));
        project.Add(path);

        var shape = Write(project).Root!.Element("Shape")!;

        shape.Element("VertList")!.Value.Should().Be("V0 0c0x5 5V20 0c1x15 5");
        shape.Element("PrimList")!.Value.Should().Be("B0 1");
    }

    [Fact]
    public void Group_children_are_nested_under_a_Children_element()
    {
        var project = ProjectWithOneLayer();
        var group = new GroupShape { Transform = Matrix2D.Translation(0, 25) };
        group.Children.Add(RectShape.FromCorner(0, 0, 10, 10));
        group.Children.Add(new TextShape { Text = "TEST", Height = 6 });
        project.Add(group);

        var children = Write(project).Root!.Element("Shape")!.Element("Children")!.Elements("Shape").ToList();

        children.Should().HaveCount(2);
        children[0].Attribute("Type")!.Value.Should().Be("Rect");
        children[1].Attribute("Type")!.Value.Should().Be("Text");
        children[1].Attribute("Str")!.Value.Should().Be("TEST");
    }

    [Fact]
    public void Text_with_xml_significant_characters_is_escaped()
    {
        var project = ProjectWithOneLayer();
        project.Add(new TextShape { Text = "A & B <2>" });

        var shape = Write(project).Root!.Element("Shape")!;

        shape.Attribute("Str")!.Value.Should().Be("A & B <2>");
    }

    [Fact]
    public void Shape_on_a_layer_with_no_CutSetting_is_rejected()
    {
        var project = ProjectWithOneLayer();
        project.Add(RectShape.FromCorner(0, 0, 5, 5, cutIndex: 7));

        var write = () => new LightBurnWriter().ToXml(project);

        write.Should().Throw<InvalidOperationException>().WithMessage("*layer 7*");
    }

    [Fact]
    public void Bezier_without_the_required_handles_is_rejected()
    {
        var project = ProjectWithOneLayer();
        var path = new PathShape();
        path.Vertices.Add(new Vertex { X = 0, Y = 0 });
        path.Vertices.Add(new Vertex { X = 10, Y = 0 });
        path.Primitives.Add(new PathPrimitive(PrimitiveKind.Bezier, 0, 1));
        project.Add(path);

        var write = () => new LightBurnWriter().ToXml(project);

        write.Should().Throw<InvalidOperationException>().WithMessage("*handle*");
    }

    [Fact]
    public void Primitive_referencing_a_missing_vertex_is_rejected()
    {
        var project = ProjectWithOneLayer();
        var path = new PathShape();
        path.Vertices.Add(new Vertex { X = 0, Y = 0 });
        path.Primitives.Add(new PathPrimitive(PrimitiveKind.Line, 0, 4));
        project.Add(path);

        var write = () => new LightBurnWriter().ToXml(project);

        write.Should().Throw<InvalidOperationException>().WithMessage("*outside the list*");
    }

    [Fact]
    public void Duplicate_layer_index_is_rejected()
    {
        var project = ProjectWithOneLayer();

        var add = () => project.AddLayer(new CutSetting { Index = 0 });

        add.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Numbers_stay_invariant_under_a_comma_decimal_culture()
    {
        var original = CultureInfo.CurrentCulture;
        try
        {
            CultureInfo.CurrentCulture = new CultureInfo("de-DE");

            var project = ProjectWithOneLayer();
            project.Add(RectShape.FromCorner(0, 0, 12.5, 7.25));

            var shape = Write(project).Root!.Element("Shape")!;

            shape.Attribute("W")!.Value.Should().Be("12.5");
            shape.Element("XForm")!.Value.Should().Be("1 0 0 1 6.25 3.625");
        }
        finally
        {
            CultureInfo.CurrentCulture = original;
        }
    }
}
