using GraveyardOrbit.Game.Input;
using GraveyardOrbit.Game.Simulation;

namespace GraveyardOrbit.Tests;

[Collection("Box2D")]
public sealed class GameSessionTests
{
    [Fact]
    public void Level_starts_with_ship_planet_and_six_asteroids()
    {
        using GameSession game = new();
        Assert.NotNull(game.Ship);
        Assert.Equal(6, game.AsteroidsRemaining);
        Assert.Single(game.Entities, e => e.Kind == EntityKind.Planet);
    }

    [Fact]
    [Trait("Category", "Integration")]
    public void A_shot_through_Box2D_contact_removes_asteroid_and_awards_salvage()
    {
        using GameSession game = new();
        Entity ship = Assert.IsType<Entity>(game.Ship);
        // Place the ship just outside the first asteroid, then fire towards it.
        Box2D.NET.B2Bodies.b2Body_SetTransform(ship.Body, new Box2D.NET.B2Vec2(5.8f, 0),
            new Box2D.NET.B2Rot(1, 0));
        ship.Heading = MathF.PI;
        game.Step(new InputState(0, 0, true, false, false));
        Assert.Equal(100, game.Score);
        Assert.Equal(5, game.AsteroidsRemaining);
        Assert.Contains(GameEvent.AsteroidDestroyed, game.DrainEvents());
        Assert.False(game.IsGameOver);
    }

    [Fact]
    public void Fire_creates_a_bullet_and_one_sound_event_then_cooldown_blocks_repeat()
    {
        using GameSession game = new();
        InputState input = new(0, 0, true, false, false);
        game.Step(input);
        Assert.Single(game.Entities, e => e.Kind == EntityKind.Bullet);
        Assert.Equal(new[] { GameEvent.Shot }, game.DrainEvents());
        game.Step(input);
        Assert.Empty(game.DrainEvents());
        Assert.Single(game.Entities, e => e.Kind == EntityKind.Bullet);
    }
}
