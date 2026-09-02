using AwesomeAssertions;
using LightBurn.Format;

namespace LightBurn.Format.Tests;

public class Matrix2DTests
{
    [Fact]
    public void Identity_leaves_a_point_alone()
    {
        Matrix2D.Identity.Apply(3, 4).Should().Be((3.0, 4.0));
    }

    [Fact]
    public void Translation_moves_a_point()
    {
        Matrix2D.Translation(10, -5).Apply(1, 1).Should().Be((11.0, -4.0));
    }

    [Fact]
    public void Then_applies_this_transform_before_the_next()
    {
        // Scale about the origin, then push right — the translation must not be scaled.
        var combined = Matrix2D.Scaling(2, 2).Then(Matrix2D.Translation(10, 0));

        combined.Apply(3, 4).Should().Be((16.0, 8.0));
    }

    [Fact]
    public void Then_is_not_commutative()
    {
        var scaleThenMove = Matrix2D.Scaling(2, 2).Then(Matrix2D.Translation(10, 0));
        var moveThenScale = Matrix2D.Translation(10, 0).Then(Matrix2D.Scaling(2, 2));

        scaleThenMove.Apply(0, 0).Should().Be((10.0, 0.0));
        moveThenScale.Apply(0, 0).Should().Be((20.0, 0.0));
    }

    [Fact]
    public void Rotation_by_ninety_degrees_maps_x_onto_y()
    {
        var (x, y) = Matrix2D.RotationDegrees(90).Apply(1, 0);

        x.Should().BeApproximately(0, 1e-12);
        y.Should().BeApproximately(1, 1e-12);
    }

    [Fact]
    public void ApplyVector_ignores_translation()
    {
        Matrix2D.Translation(100, 100).ApplyVector(2, 3).Should().Be((2.0, 3.0));
    }

    [Fact]
    public void XForm_string_is_the_six_components_in_order()
    {
        Matrix2D.Translation(30, 10).ToXFormString().Should().Be("1 0 0 1 30 10");
    }
}
