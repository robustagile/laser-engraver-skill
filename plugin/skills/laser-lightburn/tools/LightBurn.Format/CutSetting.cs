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
    /// <remarks>
    /// Verified: unchecking Output in LightBurn 2.1.04 wrote <c>&lt;doOutput Value="0"/&gt;</c>.
    /// See <c>probe/12-fiber-do-output-hide-saved.lbrn</c>. LightBurn omits the element when the
    /// layer is output, which is why a saved file with everything enabled shows nothing either
    /// way; this writer emits it explicitly so a reference layer is never ambiguous.
    /// </remarks>
    public bool Output { get; init; } = true;

    /// <summary>
    /// Whether the layer is hidden in the editor. Maps to <c>hide</c>. Editor visibility only —
    /// it is <see cref="Output"/> that decides whether the laser fires.
    /// </summary>
    /// <remarks>
    /// Verified against the same saved file: unchecking Show wrote <c>&lt;hide Value="1"/&gt;</c>.
    /// Emitted only when true, since LightBurn omits it at its default and there is nothing to
    /// disambiguate.
    /// </remarks>
    public bool Hidden { get; init; }

    /// <summary>Raster line interval in millimetres. Only meaningful for scan/image layers.</summary>
    public double? Interval { get; init; }

    /// <summary>
    /// Q-switch pulse frequency in Hz — a fiber-source setting, ignored by CO2/diode profiles.
    /// </summary>
    /// <remarks>
    /// Verified against a file LightBurn 2.1.04 saved: the element is <c>frequency</c> and the
    /// value is in **hertz** — a UI showing 5 kHz wrote <c>5000</c>. See
    /// <c>probe/11-fiber-frequency-qpulsewidth-saved.lbrn</c>.
    /// </remarks>
    public double? FrequencyHz { get; init; }

    /// <summary>
    /// Pulse duration in nanoseconds — a MOPA setting, and along with
    /// <see cref="FrequencyHz"/> the pair that colour marking lives on.
    /// </summary>
    /// <remarks>
    /// Verified against the same saved file: the element is <c>QPulseWidth</c> and the value is
    /// in **nanoseconds** — a UI showing 150 ns wrote <c>150</c>. A plain fiber source has no
    /// adjustable pulse duration, so leave this unset for one.
    /// </remarks>
    public double? PulseWidthNs { get; init; }

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
