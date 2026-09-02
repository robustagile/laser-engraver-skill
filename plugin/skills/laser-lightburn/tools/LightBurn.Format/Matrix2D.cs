using System.Globalization;

namespace LightBurn.Format;

/// <summary>
/// A 2D affine transform, serialised to LightBurn's <c>XForm</c> element as "a b c d e f".
/// Point mapping matches SVG's <c>matrix(a,b,c,d,e,f)</c>:
/// x' = a*x + c*y + e, y' = b*x + d*y + f.
/// </summary>
public readonly record struct Matrix2D(double A, double B, double C, double D, double E, double F)
{
    public static Matrix2D Identity => new(1, 0, 0, 1, 0, 0);

    public static Matrix2D Translation(double dx, double dy) => new(1, 0, 0, 1, dx, dy);

    public static Matrix2D Scaling(double sx, double sy) => new(sx, 0, 0, sy, 0, 0);

    /// <summary>Counter-clockwise rotation about the origin.</summary>
    public static Matrix2D Rotation(double radians)
    {
        var cos = Math.Cos(radians);
        var sin = Math.Sin(radians);
        return new Matrix2D(cos, sin, -sin, cos, 0, 0);
    }

    public static Matrix2D RotationDegrees(double degrees) =>
        Rotation(degrees * Math.PI / 180.0);

    /// <summary>
    /// Returns the transform equivalent to applying this one first, then <paramref name="next"/>.
    /// </summary>
    public Matrix2D Then(in Matrix2D next) => new(
        A * next.A + B * next.C,
        A * next.B + B * next.D,
        C * next.A + D * next.C,
        C * next.B + D * next.D,
        E * next.A + F * next.C + next.E,
        E * next.B + F * next.D + next.F);

    public (double X, double Y) Apply(double x, double y) =>
        (A * x + C * y + E, B * x + D * y + F);

    /// <summary>Applies only the linear part, ignoring translation — for offsets and directions.</summary>
    public (double X, double Y) ApplyVector(double x, double y) =>
        (A * x + C * y, B * x + D * y);

    public string ToXFormString() => string.Join(
        ' ',
        Numbers.Format(A),
        Numbers.Format(B),
        Numbers.Format(C),
        Numbers.Format(D),
        Numbers.Format(E),
        Numbers.Format(F));

    public override string ToString() => ToXFormString();
}

/// <summary>
/// Number formatting for the file. LightBurn is culture-invariant and writes plain
/// decimals, so every numeric value in the document must go through here — a comma
/// decimal separator from a localised machine produces a file LightBurn silently
/// mis-parses.
/// </summary>
public static class Numbers
{
    public static string Format(double value) =>
        value.ToString("0.######", CultureInfo.InvariantCulture);

    public static string Format(int value) =>
        value.ToString(CultureInfo.InvariantCulture);

    public static string Format(bool value) => value ? "1" : "0";
}
