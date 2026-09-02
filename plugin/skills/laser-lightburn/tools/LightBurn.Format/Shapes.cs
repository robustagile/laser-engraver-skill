namespace LightBurn.Format;

/// <summary>Base for everything that can appear as a <c>&lt;Shape&gt;</c> element.</summary>
public abstract class Shape
{
    /// <summary>Index of the <see cref="CutSetting"/> this shape is assigned to.</summary>
    public int CutIndex { get; set; }

    /// <summary>
    /// The shape's own transform. Inside a <see cref="GroupShape"/> this composes with the
    /// group's transform; the group's is applied after the child's.
    /// </summary>
    public Matrix2D Transform { get; set; } = Matrix2D.Identity;
}

/// <summary>
/// An axis-aligned rectangle, defined by size and centred on the origin of its own
/// coordinate system — so <see cref="Shape.Transform"/>'s translation places its centre,
/// not a corner.
/// </summary>
public sealed class RectShape : Shape
{
    public double Width { get; set; }

    public double Height { get; set; }

    /// <summary>Corner radius in millimetres; 0 for square corners.</summary>
    public double CornerRadius { get; set; }

    /// <summary>Creates a rectangle from the position of its lower-left corner.</summary>
    public static RectShape FromCorner(double x, double y, double width, double height, int cutIndex = 0) =>
        new()
        {
            Width = width,
            Height = height,
            CutIndex = cutIndex,
            Transform = Matrix2D.Translation(x + width / 2, y + height / 2),
        };
}

/// <summary>An ellipse centred on the origin of its own coordinate system.</summary>
public sealed class EllipseShape : Shape
{
    public double RadiusX { get; set; }

    public double RadiusY { get; set; }

    public static EllipseShape Circle(double centreX, double centreY, double radius, int cutIndex = 0) =>
        new()
        {
            RadiusX = radius,
            RadiusY = radius,
            CutIndex = cutIndex,
            Transform = Matrix2D.Translation(centreX, centreY),
        };
}

/// <summary>
/// One point on a path, optionally carrying bezier control handles.
/// </summary>
/// <remarks>
/// Handles are absolute coordinates in the shape's local space, the same
/// convention as an SVG cubic segment — verified against a LightBurn-authored file, which
/// writes full coordinates for c0/c1 rather than offsets.
/// </remarks>
public sealed class Vertex
{
    public required double X { get; init; }

    public required double Y { get; init; }

    /// <summary>Handle leaving this vertex (the <c>c0x</c> token).</summary>
    public (double X, double Y)? ControlOut { get; init; }

    /// <summary>Handle arriving at this vertex (the <c>c1x</c> token).</summary>
    public (double X, double Y)? ControlIn { get; init; }
}

public enum PrimitiveKind
{
    /// <summary>Straight segment — the <c>L</c> token.</summary>
    Line,

    /// <summary>Cubic bezier using the endpoints' handles — the <c>B</c> token.</summary>
    Bezier,
}

/// <summary>A segment joining two vertices, referenced by their index in the vertex list.</summary>
public readonly record struct PathPrimitive(PrimitiveKind Kind, int From, int To);

/// <summary>
/// Arbitrary geometry as a vertex list plus a primitive list. Closure is implicit: a
/// contour is closed when its primitives form a cycle back to the first vertex.
/// </summary>
public sealed class PathShape : Shape
{
    public List<Vertex> Vertices { get; } = [];

    public List<PathPrimitive> Primitives { get; } = [];

    /// <summary>
    /// A rectangle built from four explicit corners rather than as a <see cref="RectShape"/>.
    /// </summary>
    /// <remarks>
    /// <see cref="RectShape"/> is verified to render at the right size, so prefer it for a
    /// plain rectangle — LightBurn keeps it editable as a rectangle. This exists for cases
    /// that need explicit corner geometry, such as feeding the same builder as other paths
    /// or guaranteeing the outline survives a shape-type change.
    /// </remarks>
    public static PathShape Rectangle(
        double centreX,
        double centreY,
        double width,
        double height,
        int cutIndex = 0)
    {
        var halfWidth = width / 2;
        var halfHeight = height / 2;

        return Polyline(
            [
                (centreX - halfWidth, centreY - halfHeight),
                (centreX + halfWidth, centreY - halfHeight),
                (centreX + halfWidth, centreY + halfHeight),
                (centreX - halfWidth, centreY + halfHeight),
            ],
            closed: true,
            cutIndex);
    }

    /// <summary>Builds a straight-segment path through the given points.</summary>
    public static PathShape Polyline(IEnumerable<(double X, double Y)> points, bool closed, int cutIndex = 0)
    {
        var path = new PathShape { CutIndex = cutIndex };
        foreach (var (x, y) in points)
        {
            path.Vertices.Add(new Vertex { X = x, Y = y });
        }

        if (path.Vertices.Count < 2)
        {
            throw new ArgumentException("A polyline needs at least two points.", nameof(points));
        }

        for (var i = 0; i < path.Vertices.Count - 1; i++)
        {
            path.Primitives.Add(new PathPrimitive(PrimitiveKind.Line, i, i + 1));
        }

        if (closed)
        {
            path.Primitives.Add(new PathPrimitive(PrimitiveKind.Line, path.Vertices.Count - 1, 0));
        }

        return path;
    }
}

public enum TextAlignment
{
    Start = 0,
    Centre = 1,
    End = 2,
}

/// <summary>
/// A live text object. LightBurn re-renders this from the named font at open time, so the
/// font must exist on the machine running LightBurn; convert to a <see cref="PathShape"/>
/// first if the output has to be reproducible across machines.
/// </summary>
/// <remarks>
/// With <see cref="TextAlignment.Start"/> alignment the transform's translation places the
/// <b>top</b> left of the line: the glyphs occupy Y from <c>translation - Height</c> up to
/// <c>translation</c>. Verified by observing where a rule drawn between two rows landed.
/// </remarks>
public sealed class TextShape : Shape
{
    public required string Text { get; set; }

    public string Font { get; set; } = "Arial";

    /// <summary>Cap height in millimetres.</summary>
    public double Height { get; set; } = 5;

    public double LetterSpacing { get; set; }

    public double LineSpacing { get; set; }

    public bool Bold { get; set; }

    public bool Italic { get; set; }

    public TextAlignment HorizontalAlignment { get; set; } = TextAlignment.Start;

    public TextAlignment VerticalAlignment { get; set; } = TextAlignment.Start;
}

/// <summary>A group whose transform applies on top of each child's own transform.</summary>
public sealed class GroupShape : Shape
{
    public List<Shape> Children { get; } = [];
}
