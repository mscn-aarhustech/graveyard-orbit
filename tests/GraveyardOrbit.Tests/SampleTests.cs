using System.Numerics;
using GraveyardOrbit.Game;
using GraveyardOrbit.SpacePhysics;
using Raylib_cs;
using GameSession = GraveyardOrbit.Game.Game;

// Box2D uses some process-global state; keep these small examples sequential.
[assembly: CollectionBehavior(DisableTestParallelization = true)]

namespace GraveyardOrbit.Tests;

public sealed class SampleTests
{
    [Fact]
    public void Gravity_is_equal_and_opposite_and_follows_inverse_square_law()
    {
        Vector2 first = SpaceMath.GravityForce(Vector2.Zero, 100, new Vector2(10, 0), 2, 1);
        Vector2 second = SpaceMath.GravityForce(new Vector2(10, 0), 2, Vector2.Zero, 100, 1);
        Vector2 distant = SpaceMath.GravityForce(Vector2.Zero, 100, new Vector2(20, 0), 2, 1);
        Assert.Equal(-first, second);
        Assert.Equal(2f, first.X, 5);
        Assert.Equal(first.Length() / 4, distant.Length(), 5);
    }

    [Fact]
    public void Circular_orbit_moves_both_bodies_and_conserves_pair_momentum()
    {
        var velocities = SpaceMath.CreateCircularOrbit(Vector2.Zero, 1000, new Vector2(10, 0), 2, 0.035f);
        Assert.True(velocities.First.Y < 0);
        Assert.True(velocities.Second.Y > 0);
        Assert.InRange((1000 * velocities.First + 2 * velocities.Second).Length(), 0, 0.00001f);
        float expected = MathF.Sqrt(0.035f * 1002 / 10);
        Assert.Equal(expected, (velocities.Second - velocities.First).Length(), 5);
    }

    [Fact]
    public void Creating_another_orbit_preserves_the_existing_shared_velocity()
    {
        Vector2 shared = new(3, -2);
        var velocities = SpaceMath.CreateCircularOrbit(Vector2.Zero, 1000, new Vector2(10, 0), 2, 0.035f, shared);
        Vector2 centreVelocity = (1000 * velocities.First + 2 * velocities.Second) / 1002;
        Assert.InRange(Vector2.Distance(shared, centreVelocity), 0, 0.00001f);
    }

    [Fact]
    public void Initial_system_has_zero_mass_centre_velocity_and_does_not_drift_while_coasting()
    {
        using GameSession game = new();
        Assert.InRange(game.CentreOfMassVelocity().Length(), 0, 0.00001f);
        for (int i = 0; i < 120; i++) game.Step(default);
        Assert.InRange(game.CentreOfMassVelocity().Length(), 0, 0.0001f);
    }

    [Fact]
    public void Even_a_bullet_attracts_the_planet_and_the_planet_responds()
    {
        using PhysicsWorld world = new();
        Entity planet = world.Create(EntityKind.Planet, Vector2.Zero, 0.5f, 1000);
        Entity bullet = world.Create(EntityKind.Bullet, new Vector2(10, 0), 0.08f, 0.01f);
        Vector2 force = SpaceMath.GravityForce(planet.Position, planet.Mass, bullet.Position, bullet.Mass, 0.035f);
        planet.ApplyForce(force);
        bullet.ApplyForce(-force);
        world.Step(GameSession.FixedDelta);
        Assert.True(planet.Velocity.X > 0);
        Assert.True(bullet.Velocity.X < 0);
        Assert.InRange((planet.Mass * planet.Velocity + bullet.Mass * bullet.Velocity).Length(), 0, 0.000001f);
    }

    [Fact]
    public void Planet_is_a_dynamic_body_not_a_fixed_background()
    {
        using GameSession game = new();
        game.Planet.ApplyImpulse(new Vector2(100, 0));
        game.Step(default);
        Assert.True(game.Planet.Position.X > 0);
    }

    [Fact]
    public void Gentle_asteroid_contact_does_not_destroy_objects()
    {
        using GameSession game = new();
        var pair = ArrangeCollision(game, 0.35f);
        for (int i = 0; i < 180; i++) game.Step(default);
        Assert.Equal(6, game.AsteroidsRemaining);
        Assert.Equal(0, game.Score);
        Assert.InRange(Vector2.Distance(pair.First.Position, pair.Second.Position),
            pair.First.Radius + pair.Second.Radius - 0.05f,
            pair.First.Radius + pair.Second.Radius + 0.2f);
    }

    [Fact]
    public void A_fast_asteroid_collision_clears_rocks_without_any_shot()
    {
        using GameSession game = new();
        ArrangeCollision(game, 3);
        for (int i = 0; i < 30; i++) game.Step(default);
        Assert.Equal(4, game.AsteroidsRemaining);
        Assert.Equal(200, game.Score);
        Assert.DoesNotContain(GameEvent.Shot, game.DrainEvents());
    }

    [Fact]
    public void Rigid_wall_prevents_a_fast_body_leaving_the_arena()
    {
        using PhysicsWorld world = new();
        Entity body = world.Create(EntityKind.Ship, new Vector2(49, 0), 0.4f, 1);
        body.SetVelocity(new Vector2(20, 0));
        for (int i = 0; i < 120; i++)
        {
            world.Step(GameSession.FixedDelta);
            Assert.True(body.Position.X <= PhysicsWorld.HalfWidth - body.Radius + 0.03f);
        }
    }

    [Fact]
    public void Slow_asteroids_can_rest_in_a_stack_against_the_dynamic_planet()
    {
        using GameSession game = new();
        foreach (Entity entity in game.Entities) entity.SetVelocity(Vector2.Zero);
        game.Ship!.SetPosition(new Vector2(-35, -25));
        Entity[] rocks = game.Entities.Where(e => e.Kind == EntityKind.Asteroid).ToArray();
        float surface = -game.Planet.Radius;
        for (int i = 0; i < rocks.Length; i++)
        {
            if (i < 3)
            {
                rocks[i].SetPosition(new Vector2(0, surface - rocks[i].Radius - 0.01f));
                surface -= 2 * rocks[i].Radius + 0.01f;
            }
            else rocks[i].SetPosition(new Vector2(30, i * 3));
        }
        for (int i = 0; i < 180; i++) game.Step(default);
        Assert.Equal(6, game.AsteroidsRemaining);
        Assert.InRange(Vector2.Distance(game.Planet.Position, rocks[0].Position),
            game.Planet.Radius + rocks[0].Radius - 0.05f,
            game.Planet.Radius + rocks[0].Radius + 0.05f);
        for (int i = 0; i < 2; i++)
            Assert.InRange(Vector2.Distance(rocks[i].Position, rocks[i + 1].Position),
                rocks[i].Radius + rocks[i + 1].Radius - 0.05f,
                rocks[i].Radius + rocks[i + 1].Radius + 0.05f);
        Assert.InRange((rocks[2].Velocity - game.Planet.Velocity).Length(), 0, 0.1f);
    }

    [Fact]
    public void Fullscreen_reveals_more_world_without_changing_zoom_or_camera_target()
    {
        Camera camera = new(1280, 720);
        Vector2 ship = new(12, 5);
        camera.Follow(ship);
        float before = camera.WorldToScreen(ship + Vector2.UnitX).X - camera.WorldToScreen(ship).X;
        camera.Resize(1920, 1080);
        Assert.Equal(960, camera.Width);
        Assert.Equal(540, camera.Height);
        Assert.Equal(new Vector2(480, 270), camera.WorldToScreen(ship));
        Assert.Equal(before, camera.WorldToScreen(ship + Vector2.UnitX).X - camera.WorldToScreen(ship).X);
        Assert.Equal(ship, camera.ScreenToWorld(new Vector2(480, 270)));
    }

    [Fact]
    public void Selected_assets_exist_and_unknown_names_fail_clearly()
    {
        using Assets assets = new();
        foreach (string name in new[] { "playerShip", "asteroidLarge", "laser", "laserSound", "explosionSound" })
            Assert.True(new FileInfo(assets.Find(name)).Length > 0);
        Assert.Throws<KeyNotFoundException>(() => assets.Find("missingShip"));
    }

    [Fact]
    [Trait("Category", "Native")]
    public void Native_raylib_can_decode_a_real_Kenney_sprite()
    {
        using Assets assets = new();
        Image image = Raylib.LoadImage(assets.Find("playerShip"));
        try { Assert.True(Raylib.IsImageValid(image)); }
        finally { Raylib.UnloadImage(image); }
    }

    private static (Entity First, Entity Second) ArrangeCollision(GameSession game, float speed)
    {
        Entity[] asteroids = game.Entities.Where(e => e.Kind == EntityKind.Asteroid).ToArray();
        for (int i = 0; i < asteroids.Length; i++)
        {
            asteroids[i].SetPosition(new Vector2(20, i * 3));
            asteroids[i].SetVelocity(Vector2.Zero);
        }
        game.Ship!.SetPosition(new Vector2(-25, -20));
        game.Ship.SetVelocity(Vector2.Zero);
        asteroids[0].SetPosition(new Vector2(15, -10));
        asteroids[1].SetPosition(new Vector2(17, -10));
        asteroids[0].SetVelocity(new Vector2(speed, 0));
        asteroids[1].SetVelocity(new Vector2(-speed, 0));
        return (asteroids[0], asteroids[1]);
    }
}
