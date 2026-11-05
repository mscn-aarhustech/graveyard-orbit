using System.Numerics;
using GraveyardOrbit.Game.Simulation;
using Raylib_cs;

namespace GraveyardOrbit.Tests;

// Box2D has process-global state. Serialize world-based tests, including gameplay tests.
[CollectionDefinition("Box2D", DisableParallelization = true)]
public sealed class Box2DCollection;

[Collection("Box2D")]
public sealed class DependencyIntegrityTests
{
    [Fact]
    [Trait("Category", "Integration")]
    public void Managed_Box2D_integrates_velocity_in_zero_global_gravity()
    {
        using PhysicsWorld world = new();
        Entity body = world.CreateCircle(EntityKind.Asteroid, Vector2.Zero, 0.5f, new Vector2(2, 0));
        for (int i = 0; i < 60; i++) world.Step(GameSession.FixedDelta);
        Assert.InRange(body.Position.X, 1.99f, 2.01f);
        Assert.InRange(body.Position.Y, -0.001f, 0.001f);
    }

    [Fact]
    [Trait("Category", "Integration")]
    public void Managed_Box2D_reports_contact_between_bullet_and_asteroid()
    {
        using PhysicsWorld world = new();
        Entity asteroid = world.CreateCircle(EntityKind.Asteroid, Vector2.Zero, 0.4f, Vector2.Zero);
        Entity bullet = world.CreateCircle(EntityKind.Bullet, new Vector2(-1, 0), 0.1f, new Vector2(5, 0));
        bool hit = false;
        for (int i = 0; i < 20; i++)
            hit |= world.Step(GameSession.FixedDelta).Any(pair =>
                pair.A == asteroid && pair.B == bullet || pair.A == bullet && pair.B == asteroid);
        Assert.True(hit);
    }

    [Fact]
    [Trait("Category", "Native")]
    public void Native_raylib_decodes_a_real_Kenney_PNG_without_a_window()
    {
        Image image = Raylib.LoadImage(TestAssets.RuntimeCatalog().Resolve("playerShip"));
        try
        {
            Assert.True(Raylib.IsImageValid(image));
            Assert.True(image.Width > 0 && image.Height > 0);
        }
        finally { Raylib.UnloadImage(image); }
    }
}
