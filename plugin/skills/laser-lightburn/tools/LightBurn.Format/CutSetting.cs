namespace LightBurn.Format;

public enum CutSettingType
{
    /// <summary>Vector cut/mark — follows the outline.</summary>
    Cut,

    /// <summary>Raster fill.</summary>
    Scan,


    /// <summary>Image (dithered bitmap) layer.</summary>
    Image,

    /// <summary>Kerf/offset tool layer.</summary>
    Offset,
}

/// <summary>
/// A pass nested inside a layer, giving LightBurn's combined modes. A <see cref="CutSettingType.Cut"/>
/// sub-layer on a <see cref="CutSettingType.Scan"/> layer is "Fill+Line": the fill runs, then
/// a vector pass follows the outline. This is what EZCAD does by default when hatching.
/// </summary>
/// <remarks>
/// Verified against a LightBurn-authored file. Serialised as
/// <c>&lt;SubLayer type="Cut" index="1"&gt;</c> carrying its own speed and power, nested
/// inside the parent <c>CutSetting</c>. There is no <c>Scan+Cut</c> layer type.
/// </remarks>
public sealed record SubLayer
{
    public CutSettingType Type { get; init; } = CutSettingType.Cut;

    /// <summary>Sub-layer ordinal within the parent layer. LightBurn writes 1 for the contour pass.</summary>
    public int Index { get; init; } = 1;

    /// <summary>Percent, 0..100.</summary>
    public required double MaxPower { get; init; }

    /// <summary>Millimetres per second.</summary>
    public required double Speed { get; init; }

    /// <summary>
    /// Passes for the sub-layer alone, independent of the parent's
    /// <see cref="CutSetting.NumPasses"/>.
    /// </summary>
    /// <remarks>
    /// Verified: LightBurn reads this, so a contour can run a different number of passes from
    /// the fill it rides on — see <c>probe/08-sublayer-passes.lbrn</c>. Written explicitly
    /// even at its default, since LightBurn's own files omit default values and so show
    /// nothing either way.
    /// </remarks>
    public int NumPasses { get; init; } = 1;
}

/// <summary>
/// One LightBurn layer: the machine parameters plus the index that
/// <see cref="Shape.CutIndex"/> refers to.
/// </summary>
/// <remarks>
/// LightBurn writes far more sub-elements than are modelled here, and the set drifts
/// between releases. Anything omitted falls back to LightBurn's own default when the
/// file is opened; use <see cref="Extra"/> to emit settings this model doesn't name.
/// </remarks>
public sealed class CutSetting
{
    /// <summary>Layer index. 0..29 correspond to the colour layers C00..C29.</summary>
    public required int Index { get; init; }

    /// <summary>Layer name. Defaults to the conventional "C00" form when not set.</summary>
    public string? Name { get; init; }

    public CutSettingType Type { get; init; } = CutSettingType.Cut;

    /// <summary>Percent, 0..100.</summary>
    public double MaxPower { get; init; } = 20;

    /// <summary>Millimetres per second.</summary>
    public double Speed { get; init; } = 100;

    public int NumPasses { get; init; } = 1;

    /// <summary>Ordering hint; lower numbers are output first.</summary>
    public int Priority { get; init; }

    /// <summary>Whether the layer is actually output. Maps to <c>doOutput</c>.</summary>
    public bool Output { get; init; } = true;

    /// <summary>Raster line interval in millimetres. Only meaningful for scan/image layers.</summary>
    public double? Interval { get; init; }

    /// <summary>
    /// Q-switch pulse frequency in Hz — a fiber-source setting, ignored by CO2/diode profiles.
    /// </summary>
    public double? FrequencyHz { get; init; }

    /// <summary>
    /// Optional nested pass. A <see cref="CutSettingType.Cut"/> sub-layer here turns a fill
    /// layer into Fill+Line.
    /// </summary>
    public SubLayer? SubLayer { get; init; }

    /// <summary>
    /// Extra <c>&lt;name Value="..."/&gt;</c> entries appended verbatim, for settings this
    /// model doesn't cover. Values must already be invariant-culture formatted.
    /// </summary>
    public Dictionary<string, string> Extra { get; } = [];

    public string EffectiveName => Name ?? $"C{Index:00}";
}
