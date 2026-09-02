using LightBurn.Format;

namespace LightBurn.Probes;

/// <summary>
/// One format question, isolated. A probe is only worth writing if the competing answers make
/// the file look obviously different when opened.
/// </summary>
public sealed record Probe(string Name, string Question, string HowToTell, Func<LightBurnProject> Build);

public static class ProbeCatalogue
{
    public static IReadOnlyList<Probe> All { get; } =
    [
        new Probe(
            "10-fiber-layer-settings",
            "Do doOutput, hide, frequency and QPulseWidth survive in the older format version "
                + "this writer emits? Every one of those names and units is verified - but only "
                + "from files LightBurn saved in its own format, which says nothing about ours.",
            "With a fiber/galvo device profile selected: C00 must read 5 kHz and 150 ns; C01 must "
                + "have its Output checkbox OFF; C02 must have its Show checkbox OFF; C03 sets "
                + "none of them and shows LightBurn's own defaults, so it says what 'unchanged' "
                + "looks like. If C00 reads defaults, one candidate cause is element order - this "
                + "writer puts frequency after doOutput, LightBurn puts it before.",
            FiberLayerSettings),
    ];

    private static LightBurnProject FiberLayerSettings()
    {
        var project = new LightBurnProject();

        // Deliberately the same numbers as the saved files, so the only difference between the
        // two readings is the format version.
        project.AddLayer(new CutSetting
        {
            Index = 0,
            Name = "C00 5kHz 150ns",
            Type = CutSettingType.Cut,
            MaxPower = 20,
            Speed = 100,
            FrequencyHz = 5000,
            PulseWidthNs = 150,
        });

        project.AddLayer(new CutSetting
        {
            Index = 1,
            Name = "C01 output OFF",
            Type = CutSettingType.Cut,
            MaxPower = 20,
            Speed = 100,
            Output = false,
        });

        project.AddLayer(new CutSetting
        {
            Index = 2,
            Name = "C02 hidden",
            Type = CutSettingType.Cut,
            MaxPower = 20,
            Speed = 100,
            Hidden = true,
        });

        // The control: nothing set. Without it there is no way to tell "the value was read" from
        // "the value happens to match LightBurn's default".
        project.AddLayer(new CutSetting
        {
            Index = 3,
            Name = "C03 nothing set",
            Type = CutSettingType.Cut,
            MaxPower = 20,
            Speed = 100,
        });

        project.Add(RectShape.FromCorner(5, 5, 12, 12, cutIndex: 0));
        project.Add(Label("5kHz 150ns", 5, 22, cutIndex: 0));

        project.Add(RectShape.FromCorner(22, 5, 12, 12, cutIndex: 1));
        project.Add(Label("no output", 22, 22, cutIndex: 1));

        project.Add(RectShape.FromCorner(39, 5, 12, 12, cutIndex: 2));
        project.Add(Label("hidden", 39, 22, cutIndex: 2));

        project.Add(RectShape.FromCorner(56, 5, 12, 12, cutIndex: 3));
        project.Add(Label("control", 56, 22, cutIndex: 3));

        return project;
    }

    /// <summary>
    /// A text label placed by its top edge: LightBurn anchors text at the TOP of the line by
    /// default, so glyphs run downwards from the transform's translation.
    /// </summary>
    private static TextShape Label(string text, double x, double y, int cutIndex) => new()
    {
        Text = text,
        Height = 4,
        CutIndex = cutIndex,
        Transform = Matrix2D.Translation(x, y),
    };
}
