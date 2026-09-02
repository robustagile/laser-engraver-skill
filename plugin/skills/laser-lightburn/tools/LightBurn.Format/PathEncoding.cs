using System.Text;

namespace LightBurn.Format;

/// <summary>
/// Encodes the <c>VertList</c> / <c>PrimList</c> strings of a <see cref="PathShape"/>.
/// </summary>
/// <remarks>
/// <para>
/// <c>VertList</c> is a run of concatenated tokens with no separators between them:
/// <c>V</c>&#160;x&#160;y for each point, each optionally followed by <c>c0x</c>&#160;cx&#160;cy
/// (the handle leaving the point) and <c>c1x</c>&#160;cx&#160;cy (the handle arriving at it).
/// </para>
/// <para>
/// <c>PrimList</c> is likewise concatenated: <c>L</c>&#160;a&#160;b for a straight segment
/// between vertex indices a and b, <c>B</c>&#160;a&#160;b for a cubic bezier that uses
/// a's outgoing handle and b's incoming handle.
/// </para>
/// </remarks>
public static class PathEncoding
{
    public static string EncodeVertexList(IReadOnlyList<Vertex> vertices)
    {
        var builder = new StringBuilder();
        foreach (var vertex in vertices)
        {
            builder.Append('V')
                   .Append(Numbers.Format(vertex.X))
                   .Append(' ')
                   .Append(Numbers.Format(vertex.Y));

            AppendHandle(builder, "c0x", vertex.ControlOut);
            AppendHandle(builder, "c1x", vertex.ControlIn);
        }

        return builder.ToString();
    }

    public static string EncodePrimitiveList(IReadOnlyList<PathPrimitive> primitives)
    {
        var builder = new StringBuilder();
        foreach (var primitive in primitives)
        {
            builder.Append(primitive.Kind == PrimitiveKind.Line ? 'L' : 'B')
                   .Append(Numbers.Format(primitive.From))
                   .Append(' ')
                   .Append(Numbers.Format(primitive.To));
        }

        return builder.ToString();
    }

    /// <summary>
    /// Checks that every primitive references a vertex that exists. A dangling index
    /// produces a file LightBurn may open with silently missing geometry, which is worse
    /// than a load error, so callers should run this before writing.
    /// </summary>
    public static void Validate(PathShape path)
    {
        foreach (var primitive in path.Primitives)
        {
            if (primitive.From < 0 || primitive.From >= path.Vertices.Count ||
                primitive.To < 0 || primitive.To >= path.Vertices.Count)
            {
                throw new InvalidOperationException(
                    $"Primitive {primitive.Kind} {primitive.From}->{primitive.To} references a vertex " +
                    $"outside the list of {path.Vertices.Count}.");
            }

            if (primitive.Kind != PrimitiveKind.Bezier)
            {
                continue;
            }

            if (path.Vertices[primitive.From].ControlOut is null ||
                path.Vertices[primitive.To].ControlIn is null)
            {
                throw new InvalidOperationException(
                    $"Bezier {primitive.From}->{primitive.To} needs an outgoing handle on vertex " +
                    $"{primitive.From} and an incoming handle on vertex {primitive.To}.");
            }
        }
    }

    private static void AppendHandle(StringBuilder builder, string token, (double X, double Y)? handle)
    {
        if (handle is null)
        {
            return;
        }

        var (hx, hy) = handle.Value;

        builder.Append(token)
               .Append(Numbers.Format(hx))
               .Append(' ')
               .Append(Numbers.Format(hy));
    }
}
