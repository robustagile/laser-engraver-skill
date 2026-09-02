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
            "09-do-output",
            "Is 'doOutput' the element that marks a layer as not output?",
            "In the Cuts / Layers list, C01 must have its Output checkbox OFF while C00 has it ON. "
                + "If both are on, the element name is wrong and a guide layer would fire the laser.",
            DoOutput),

        new Probe(
            "10-frequency-units",
            "Is 'frequency' the Q-switch rate element, and is its value in Hz or kHz?",
            "Open each layer's settings with a fiber/galvo device profile selected. Exactly one of "
                + "C00 and C01 should read 42 kHz. C02 sets nothing and shows LightBurn's own default, "
                + "so it says what 'unchanged' looks like. If neither C00 nor C01 differs from C02, "
                + "the element name is wrong.",
            FrequencyUnits),
    ];

    private static LightBurnProject DoOutput()
    {
        var project = new LightBurnProject();

        project.AddLayer(new CutSetting
        {
            Index = 0,
            Name = "C00 output ON",
            Type = CutSettingType.Cut,
            MaxPower = 20,
            Speed = 100,
            Output = true,
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

        project.Add(RectShape.FromCorner(5, 5, 15, 15, cutIndex: 0));
        project.Add(Label("ON", 5, 28, cutIndex: 0));

        project.Add(RectShape.FromCorner(30, 5, 15, 15, cutIndex: 1));
        project.Add(Label("OFF", 30, 28, cutIndex: 1));

        return project;
    }

    private static LightBurnProject FrequencyUnits()
    {
        var project = new LightBurnProject();

        // The two readings of the same intent — 42 kHz — written the two plausible ways.
        project.AddLayer(new CutSetting
        {
            Index = 0,
            Name = "C00 frequency=42000",
            Type = CutSettingType.Cut,
            MaxPower = 20,
            Speed = 100,
            FrequencyHz = 42000,
        });

        project.AddLayer(new CutSetting
        {
            Index = 1,
            Name = "C01 frequency=42",
            Type = CutSettingType.Cut,
            MaxPower = 20,
            Speed = 100,
            FrequencyHz = 42,
        });

        // The control: nothing set, so it shows what LightBurn defaults to. Without it there is
        // no way to tell "the value was read" from "the value happens to match the default".
        project.AddLayer(new CutSetting
        {
            Index = 2,
            Name = "C02 frequency unset",
            Type = CutSettingType.Cut,
            MaxPower = 20,
            Speed = 100,
        });

        project.Add(RectShape.FromCorner(5, 5, 12, 12, cutIndex: 0));
        project.Add(Label("42000", 5, 22, cutIndex: 0));

        project.Add(RectShape.FromCorner(22, 5, 12, 12, cutIndex: 1));
        project.Add(Label("42", 22, 22, cutIndex: 1));

        project.Add(RectShape.FromCorner(39, 5, 12, 12, cutIndex: 2));
        project.Add(Label("unset", 39, 22, cutIndex: 2));

        return project;
    }

    /// <summary>
    /// A text label whose <paramref name="y"/> is the baseline of the top of the glyphs:
    /// LightBurn anchors text at the TOP of the line, so glyphs run downwards from the
    /// transform's translation.
    /// </summary>
    private static TextShape Label(string text, double x, double y, int cutIndex) => new()
    {
        Text = text,
        Height = 4,
        CutIndex = cutIndex,
        Transform = Matrix2D.Translation(x, y),
    };
}
