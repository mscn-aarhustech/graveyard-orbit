using System.Numerics;
using GraveyardOrbit.Game.Input;
using GraveyardOrbit.SpacePhysics;

namespace GraveyardOrbit.Game.Simulation;

public enum GameEvent { Shot, AsteroidDestroyed, ShipDestroyed }

/// <summary>Headless gameplay model. Raylib is deliberately absent here.</summary>
public sealed class GameSession : IDisposable
{
    public const float FixedDelta = 1f / 60;
    public const float PlanetRadius = 2.4f;
    public const float EscapeRadius = 9;
    public const float GravityParameter = 120;
    private readonly PhysicsWorld physics = new();
    private readonly List<Entity> entities = [];
    private readonly List<GameEvent> events = [];
    private float fireCooldown;
    public IReadOnlyList<Entity> Entities => entities;
    public Entity? Ship { get; private set; }
    public bool IsGameOver => Ship is null;
    public int Score { get; private set; }
    public float Elapsed { get; private set; }
    public int AsteroidsRemaining => entities.Count(e => e.Kind == EntityKind.Asteroid);

    public GameSession()
    {
        entities.Add(physics.CreateCircle(EntityKind.Planet, Vector2.Zero, PlanetRadius, Vector2.Zero));
        Vector2 shipPosition = new(7.5f, 0);
        Ship = physics.CreateCircle(EntityKind.Ship, shipPosition, 0.38f,
            Orbit.CircularVelocity(shipPosition, Vector2.Zero, GravityParameter));
        Ship.Heading = MathF.PI / 2;
        entities.Add(Ship);
        for (int i = 0; i < 6; i++)
        {
            float angle = i * MathF.Tau / 6;
            float radius = 4.8f + (i % 3) * 0.9f;
            Vector2 position = new Vector2(MathF.Cos(angle), MathF.Sin(angle)) * radius;
            entities.Add(physics.CreateCircle(EntityKind.Asteroid, position, 0.3f + (i % 2) * 0.1f,
                Orbit.CircularVelocity(position, Vector2.Zero, GravityParameter)));
        }
    }

    public void Step(InputState input)
    {
        if (IsGameOver) return;
        Elapsed += FixedDelta;
        fireCooldown -= FixedDelta;
        if (Ship is { } ship)
        {
            ship.Heading += input.Turn * 2.5f * FixedDelta;
            float modifier = input.Precision ? 0.3f : input.Boost ? 2f : 1f;
            PhysicsWorld.ApplyForce(ship, Rocket.Thrust(ship.Heading, input.Throttle,
                PhysicsWorld.Mass(ship) * 7 * modifier));
            if (input.Fire && fireCooldown <= 0) Fire(ship);
        }

        foreach (Entity entity in entities)
        {
            if (entity.Kind != EntityKind.Planet)
                PhysicsWorld.ApplyForce(entity, Gravity.Force(entity.Position, Vector2.Zero,
                    GravityParameter, PhysicsWorld.Mass(entity), PlanetRadius));
        }

        HashSet<Entity> remove = [];
        foreach ((Entity a, Entity b) in physics.Step(FixedDelta))
        {
            if (remove.Contains(a) || remove.Contains(b)) continue;
            if (a.Kind == EntityKind.Ship || b.Kind == EntityKind.Ship)
            {
                Entity destroyedShip = a.Kind == EntityKind.Ship ? a : b;
                remove.Add(destroyedShip);
                Ship = null;
                events.Add(GameEvent.ShipDestroyed);
            }
            else if (a.Kind == EntityKind.Bullet || b.Kind == EntityKind.Bullet)
            {
                Entity bullet = a.Kind == EntityKind.Bullet ? a : b;
                Entity target = a == bullet ? b : a;
                remove.Add(bullet);
                if (target.Kind == EntityKind.Asteroid)
                {
                    remove.Add(target);
                    Score += 100;
                    events.Add(GameEvent.AsteroidDestroyed);
                }
            }
        }
        foreach (Entity entity in entities)
        {
            entity.LifeRemaining -= FixedDelta;
            if (entity.Kind == EntityKind.Bullet &&
                (entity.LifeRemaining <= 0 || entity.Position.Length() > 28)) remove.Add(entity);
        }
        foreach (Entity entity in remove)
        {
            physics.Destroy(entity);
            entities.Remove(entity);
        }
        // No screen wrapping: escape is a deliberate gameplay rule, not hidden drag.
        if (Ship is { } remainingShip && remainingShip.Position.Length() > EscapeRadius)
        {
            physics.Destroy(remainingShip);
            entities.Remove(remainingShip);
            Ship = null;
            events.Add(GameEvent.ShipDestroyed);
        }
    }

    private void Fire(Entity ship)
    {
        Vector2 forward = new(MathF.Cos(ship.Heading), MathF.Sin(ship.Heading));
        Entity bullet = physics.CreateCircle(EntityKind.Bullet,
            ship.Position + forward * 0.65f, 0.1f, PhysicsWorld.Velocity(ship) + forward * 14);
        bullet.Heading = ship.Heading;
        bullet.LifeRemaining = 2;
        entities.Add(bullet);
        fireCooldown = 0.22f;
        events.Add(GameEvent.Shot);
    }

    public GameEvent[] DrainEvents()
    {
        GameEvent[] result = events.ToArray();
        events.Clear();
        return result;
    }

    public void Dispose() => physics.Dispose();
}
