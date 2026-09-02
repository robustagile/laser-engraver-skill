using System.Text;
using System.Xml;

namespace LightBurn.Format;

public sealed class LightBurnWriterOptions
{
    /// <summary>Runs <see cref="LightBurnProject.Validate"/> before writing.</summary>
    public bool Validate { get; init; } = true;

    public bool Indent { get; init; } = true;
}

/// <summary>
/// Writes a <see cref="LightBurnProject"/> as <c>.lbrn</c> (the plain-XML variant).
/// </summary>
/// <remarks>
/// The newer <c>.lbrn2</c> packs geometry into binary blobs; LightBurn still opens
/// <c>.lbrn</c>, so this writer targets the readable form deliberately.
/// </remarks>
public sealed class LightBurnWriter(LightBurnWriterOptions? options = null)
{
    private readonly LightBurnWriterOptions options = options ?? new LightBurnWriterOptions();

    /// <summary>
    /// Writes the project to <paramref name="path"/>, replacing any existing file.
    /// </summary>
    /// <remarks>
    /// Goes via a temporary file and an atomic move, so an interrupted write cannot leave a
    /// half-written or half-overwritten document in place. A truncated .lbrn that still
    /// parses is the worst outcome here — it would be sent to the machine as if it were the
    /// real job.
    /// </remarks>
    public void Save(LightBurnProject project, string path)
    {
        var directory = Path.GetDirectoryName(Path.GetFullPath(path))!;
        var temporaryPath = Path.Combine(directory, $".{Path.GetFileName(path)}.{Guid.NewGuid():N}.tmp");

        try
        {
            using (var stream = new FileStream(temporaryPath, FileMode.Create, FileAccess.Write, FileShare.None))
            {
                WriteTo(project, stream);
                stream.Flush(flushToDisk: true);
            }

            File.Move(temporaryPath, path, overwrite: true);
        }
        catch
        {
            File.Delete(temporaryPath);
            throw;
        }
    }

    public string ToXml(LightBurnProject project)
    {
        using var stream = new MemoryStream();
        WriteTo(project, stream);
        return new UTF8Encoding(encoderShouldEmitUTF8Identifier: false).GetString(stream.ToArray());
    }

    /// <summary>
    /// Writes to a stream rather than a string, so the XML declaration states the encoding
    /// the bytes are actually in. An <see cref="XmlWriter"/> over a
    /// <see cref="StringBuilder"/> always declares utf-16 no matter what the settings say.
    /// </summary>
    private void WriteTo(LightBurnProject project, Stream stream)
    {
        if (options.Validate)
        {
            project.Validate();
        }

        var settings = new XmlWriterSettings
        {
            Indent = options.Indent,
            IndentChars = "    ",
            Encoding = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false),
        };

        using (var writer = XmlWriter.Create(stream, settings))
        {
            writer.WriteStartDocument();
            writer.WriteStartElement("LightBurnProject");
            writer.WriteAttributeString("AppVersion", project.AppVersion);
            writer.WriteAttributeString("FormatVersion", Numbers.Format(project.FormatVersion));
            writer.WriteAttributeString("MaterialHeight", Numbers.Format(project.MaterialHeight));
            writer.WriteAttributeString("MirrorX", project.MirrorX ? "True" : "False");
            writer.WriteAttributeString("MirrorY", project.MirrorY ? "True" : "False");

            foreach (var setting in project.CutSettings.OrderBy(setting => setting.Index))
            {
                WriteCutSetting(writer, setting);
            }

            foreach (var shape in project.Shapes)
            {
                WriteShape(writer, shape);
            }

            writer.WriteEndElement();
            writer.WriteEndDocument();
        }
    }

    private static void WriteCutSetting(XmlWriter writer, CutSetting setting)
    {
        writer.WriteStartElement("CutSetting");
        writer.WriteAttributeString("type", TypeAttribute(setting.Type));

        WriteValue(writer, "index", Numbers.Format(setting.Index));
        WriteValue(writer, "name", setting.EffectiveName);
        WriteValue(writer, "maxPower", Numbers.Format(setting.MaxPower));
        WriteValue(writer, "speed", Numbers.Format(setting.Speed));
        WriteValue(writer, "numPasses", Numbers.Format(setting.NumPasses));
        WriteValue(writer, "priority", Numbers.Format(setting.Priority));
        WriteValue(writer, "doOutput", Numbers.Format(setting.Output));

        if (setting.Hidden)
        {
            WriteValue(writer, "hide", Numbers.Format(setting.Hidden));
        }

        if (setting.Interval is { } interval)
        {
            WriteValue(writer, "interval", Numbers.Format(interval));
        }

        if (setting.FrequencyHz is { } frequency)
        {
            WriteValue(writer, "frequency", Numbers.Format(frequency));
        }

        if (setting.PulseWidthNs is { } pulseWidth)
        {
            WriteValue(writer, "QPulseWidth", Numbers.Format(pulseWidth));
        }

        foreach (var (name, value) in setting.Extra)
        {
            WriteValue(writer, name, value);
        }

        // LightBurn writes the sub-layer last, after the parent's own settings.
        if (setting.SubLayer is { } subLayer)
        {
            writer.WriteStartElement("SubLayer");
            writer.WriteAttributeString("type", TypeAttribute(subLayer.Type));
            writer.WriteAttributeString("index", Numbers.Format(subLayer.Index));
            WriteValue(writer, "maxPower", Numbers.Format(subLayer.MaxPower));
            WriteValue(writer, "speed", Numbers.Format(subLayer.Speed));
            WriteValue(writer, "numPasses", Numbers.Format(subLayer.NumPasses));
            writer.WriteEndElement();
        }

        writer.WriteEndElement();
    }

    private void WriteShape(XmlWriter writer, Shape shape)
    {
        writer.WriteStartElement("Shape");

        switch (shape)
        {
            case RectShape rect:
                writer.WriteAttributeString("Type", "Rect");
                writer.WriteAttributeString("CutIndex", Numbers.Format(rect.CutIndex));
                writer.WriteAttributeString("W", Numbers.Format(rect.Width));
                writer.WriteAttributeString("H", Numbers.Format(rect.Height));
                writer.WriteAttributeString("Cr", Numbers.Format(rect.CornerRadius));
                WriteTransform(writer, rect);
                break;

            case EllipseShape ellipse:
                writer.WriteAttributeString("Type", "Ellipse");
                writer.WriteAttributeString("CutIndex", Numbers.Format(ellipse.CutIndex));
                writer.WriteAttributeString("Rx", Numbers.Format(ellipse.RadiusX));
                writer.WriteAttributeString("Ry", Numbers.Format(ellipse.RadiusY));
                WriteTransform(writer, ellipse);
                break;

            case PathShape path:
                writer.WriteAttributeString("Type", "Path");
                writer.WriteAttributeString("CutIndex", Numbers.Format(path.CutIndex));
                WriteTransform(writer, path);
                writer.WriteElementString("VertList", PathEncoding.EncodeVertexList(path.Vertices));
                writer.WriteElementString("PrimList", PathEncoding.EncodePrimitiveList(path.Primitives));
                break;

            case TextShape text:
                writer.WriteAttributeString("Type", "Text");
                writer.WriteAttributeString("CutIndex", Numbers.Format(text.CutIndex));
                writer.WriteAttributeString("Font", text.Font);
                writer.WriteAttributeString("H", Numbers.Format(text.Height));
                writer.WriteAttributeString("LS", Numbers.Format(text.LetterSpacing));
                writer.WriteAttributeString("LnS", Numbers.Format(text.LineSpacing));
                writer.WriteAttributeString("Bold", Numbers.Format(text.Bold));
                writer.WriteAttributeString("Italic", Numbers.Format(text.Italic));
                writer.WriteAttributeString("Ah", Numbers.Format((int)text.HorizontalAlignment));
                writer.WriteAttributeString("Av", Numbers.Format((int)text.VerticalAlignment));
                writer.WriteAttributeString("Str", text.Text);
                WriteTransform(writer, text);
                break;

            case GroupShape group:
                writer.WriteAttributeString("Type", "Group");
                writer.WriteAttributeString("CutIndex", Numbers.Format(group.CutIndex));
                WriteTransform(writer, group);
                writer.WriteStartElement("Children");
                foreach (var child in group.Children)
                {
                    WriteShape(writer, child);
                }

                writer.WriteEndElement();
                break;

            default:
                throw new NotSupportedException($"No writer for shape type {shape.GetType().Name}.");
        }

        writer.WriteEndElement();
    }

    private static void WriteTransform(XmlWriter writer, Shape shape) =>
        writer.WriteElementString("XForm", shape.Transform.ToXFormString());

    /// <summary>The <c>type</c> attribute text, used by both layers and sub-layers.</summary>
    private static string TypeAttribute(CutSettingType type) => type.ToString();

    private static void WriteValue(XmlWriter writer, string name, string value)
    {
        writer.WriteStartElement(name);
        writer.WriteAttributeString("Value", value);
        writer.WriteEndElement();
    }
}
