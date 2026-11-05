using System.Numerics;
using GraveyardOrbit.SpacePhysics;

namespace GraveyardOrbit.Tests;

public sealed class SpacePhysicsTests
{
    [Fact]
    public void Gravity_points_towards_planet_and_follows_inverse_square_law()
    {
        Vector2 near = Gravity.Acceleration(new Vector2(10, 0), Vector2.Zero, 100, 1);
        Vector2 far = Gravity.Acceleration(new Vector2(20, 0), Vector2.Zero, 100, 1);
        Assert.Equal(-1f, near.X, 5);
        Assert.Equal(0f, near.Y);
        Assert.Equal(near.Length() / 4, far.Length(), 5);
    }

    [Fact]
    public void Gravity_force_scales_with_body_mass()
    {
        Vector2 force = Gravity.Force(new Vector2(10, 0), Vector2.Zero, 100, 3, 1);
        Assert.Equal(-3f, force.X, 5);
    }

    [Fact]
    public void Gravity_at_planet_centre_is_finite_and_zero()
    {
        Assert.Equal(Vector2.Zero, Gravity.Acceleration(Vector2.Zero, Vector2.Zero, 120, 2.4f));
    }

    [Theory]
    [InlineData(1f, 10f)]
    [InlineData(0f, 0f)]
    [InlineData(-1f, -10f)]
    public void Thrust_supports_forward_idle_and_reverse(float throttle, float expected)
    {
        Assert.Equal(new Vector2(expected, 0), Rocket.Thrust(0, throttle, 10));
    }

    [Fact]
    public void Circular_orbit_velocity_is_tangent_with_expected_speed()
    {
        Vector2 radial = new(4, 0);
        Vector2 velocity = Orbit.CircularVelocity(radial, Vector2.Zero, 100);
        Assert.Equal(0f, Vector2.Dot(radial, velocity), 5);
        Assert.Equal(5f, velocity.Length(), 5);
    }

    [Fact]
    public void Invalid_radius_and_throttle_are_rejected()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => Gravity.Acceleration(Vector2.One, Vector2.Zero, 100, 0));
        Assert.Throws<ArgumentOutOfRangeException>(() => Rocket.Thrust(0, 2, 10));
        Assert.Throws<ArgumentException>(() => Orbit.CircularVelocity(Vector2.Zero, Vector2.Zero, 100));
    }
}
