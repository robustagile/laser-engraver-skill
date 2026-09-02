namespace LightBurn.Format;

/// <summary>
/// A LightBurn document: layers plus geometry.
/// </summary>
/// <remarks>
/// Coordinates are millimetres with Y increasing upwards. Where the origin sits relative
/// to the work area is a property of the device profile in LightBurn, not of the file —
/// galvo profiles are typically centred on the origin, gantry machines cornered — so the
/// same document lands differently on differently configured machines.
/// </remarks>
public sealed class LightBurnProject
{
    /// <summary>
    /// Written to the <c>AppVersion</c> attribute. LightBurn tolerates opening files
    /// stamped with a different version than its own.
    /// </summary>
    public string AppVersion { get; set; } = "1.5.06";

    public int FormatVersion { get; set; } = 1;

    public double MaterialHeight { get; set; }

    public bool MirrorX { get; set; }

    public bool MirrorY { get; set; }

    public List<CutSetting> CutSettings { get; } = [];

    public List<Shape> Shapes { get; } = [];

    /// <summary>
    /// Adds a layer and returns it, so its <see cref="CutSetting.Index"/> can be handed
    /// straight to the shapes that belong on it.
    /// </summary>
    public CutSetting AddLayer(CutSetting setting)
    {
        if (CutSettings.Any(existing => existing.Index == setting.Index))
        {
            throw new InvalidOperationException($"A layer with index {setting.Index} already exists.");
        }

        CutSettings.Add(setting);
        return setting;
    }

    public T Add<T>(T shape)
        where T : Shape
    {
        Shapes.Add(shape);
        return shape;
    }

    /// <summary>
    /// Verifies the document before writing: every shape must reference a layer that
    /// exists, and every path's primitives must reference vertices that exist.
    /// </summary>
    public void Validate()
    {
        var layerIndices = CutSettings.Select(setting => setting.Index).ToHashSet();
        foreach (var shape in EnumerateShapes(Shapes))
        {
            // A group's own CutIndex carries no output settings — its children each
            // reference their own layer — so it isn't worth failing over.
            if (shape is not GroupShape && !layerIndices.Contains(shape.CutIndex))
            {
                throw new InvalidOperationException(
                    $"{shape.GetType().Name} is on layer {shape.CutIndex}, which has no CutSetting.");
            }

            if (shape is PathShape path)
            {
                PathEncoding.Validate(path);
            }
        }
    }

    private static IEnumerable<Shape> EnumerateShapes(IEnumerable<Shape> shapes)
    {
        foreach (var shape in shapes)
        {
            yield return shape;

            if (shape is GroupShape group)
            {
                foreach (var child in EnumerateShapes(group.Children))
                {
                    yield return child;
                }
            }
        }
    }
}
