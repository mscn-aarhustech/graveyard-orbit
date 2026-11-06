using System.Numerics;
using GraveyardOrbit.SpacePhysics;

namespace GraveyardOrbit.Game;

public readonly record struct Controls(float Throttle, float Turn, bool Fire, bool Boost, bool Precision);
public enum GameEvent { Shot, AsteroidDestroyed, ShipDestroyed }

public sealed class Game : IDisposable
{
    public const float FixedDelta = 1f / 60;
    public const float GravityConstant = 0.035f;
    public const float DestructiveImpactSpeed = 3;
    private readonly PhysicsWorld physics = new();
    private readonly List<Entity> entities = [];
    private readonly List<GameEvent> events = [];
    private float fireCooldown;
    public IReadOnlyList<Entity> Entities => entities;
    public Entity Planet { get; }
    public Entity? Ship { get; private set; }
    public bool IsGameOver => Ship is null;
    public int Score { get; private set; }
    public int AsteroidsRemaining => entities.Count(e => e.Kind == EntityKind.Asteroid);

    public Game()
    {
        Planet = physics.Create(EntityKind.Planet, Vector2.Zero, 2.2f, 1000);
        entities.Add(Planet);
        Ship = physics.Create(EntityKind.Ship, new Vector2(11.5f, 0), 0.4f, 1);
        Ship.SetAngle(MathF.PI / 2);
        entities.Add(Ship);
        CreateCircularOrbit(Planet, Ship);
        for (int i = 0; i < 6; i++)
        {
            float angle = i * MathF.Tau / 6;
            float radius = 5.5f + i % 3 * 1.8f;
            Vector2 position = new Vector2(MathF.Cos(angle), MathF.Sin(angle)) * radius;
            Entity asteroid = physics.Create(EntityKind.Asteroid, position, 0.4f + i % 2 * 0.15f, 3 + i % 2);
            entities.Add(asteroid);
            CreateCircularOrbit(Planet, asteroid);
        }
        // Correct floating-point roundoff ONCE. Never cancel real movement each frame.
        Vector2 drift = CentreOfMassVelocity();
        foreach (Entity entity in entities) entity.SetVelocity(entity.Velocity - drift);
    }

    private static void CreateCircularOrbit(Entity first, Entity second)
    {
        Vector2 pairVelocity = (first.Mass * first.Velocity + second.Mass * second.Velocity) /
            (first.Mass + second.Mass);
        var velocities = SpaceMath.CreateCircularOrbit(first.Position, first.Mass,
            second.Position, second.Mass, GravityConstant, pairVelocity);
        first.SetVelocity(velocities.First);
        second.SetVelocity(velocities.Second);
    }

    public Vector2 CentreOfMassVelocity()
    {
        Vector2 momentum = Vector2.Zero;
        float mass = 0;
        foreach (Entity entity in entities)
        {
            momentum += entity.Mass * entity.Velocity;
            mass += entity.Mass;
        }
        return momentum / mass;
    }

    public void Step(Controls input)
    {
        if (IsGameOver) return;
        fireCooldown -= FixedDelta;
        if (Ship is { } ship)
        {
            ship.Turn(input.Turn);
            float modifier = input.Precision ? 0.25f : input.Boost ? 2 : 1;
            ship.ApplyForce(SpaceMath.Thrust(ship.Angle, input.Throttle, ship.Mass * 3 * modifier));
            if (input.Fire && fireCooldown <= 0) Fire(ship);
        }
        ApplyGravity();
        List<Collision> collisions = physics.Step(FixedDelta);
        HashSet<Entity> remove = [];
        foreach (Collision collision in collisions)
        {
            // The removal set scores each body once, even during a collision chain.
            Entity? bullet = collision.First?.Kind == EntityKind.Bullet ? collision.First :
                collision.Second?.Kind == EntityKind.Bullet ? collision.Second : null;
            if (bullet is not null)
            {
                if (remove.Contains(bullet)) continue;
                Entity? target = collision.First == bullet ? collision.Second : collision.First;
                remove.Add(bullet);
                if (target?.Kind == EntityKind.Asteroid) remove.Add(target);
            }
            else if (collision.ImpactSpeed >= DestructiveImpactSpeed)
            {
                AddDestructible(collision.First, remove);
                AddDestructible(collision.Second, remove);
            }
            // Gentle contacts are handled entirely by Box2D's friction/contact solver.
        }
        foreach (Entity entity in entities)
        {
            entity.Lifetime -= FixedDelta;
            if (entity.Kind == EntityKind.Bullet && entity.Lifetime <= 0) remove.Add(entity);
        }
        foreach (Entity entity in remove)
        {
            if (entity.Kind == EntityKind.Asteroid)
            {
                Score += 100;
                events.Add(GameEvent.AsteroidDestroyed);
            }
            if (entity == Ship)
            {
                Ship = null;
                events.Add(GameEvent.ShipDestroyed);
            }
            physics.Destroy(entity);
            entities.Remove(entity);
        }
    }

    private void ApplyGravity()
    {
        for (int i = 0; i < entities.Count; i++)
        {
            for (int j = i + 1; j < entities.Count; j++)
            {
                Entity first = entities[i];
                Entity second = entities[j];
                Vector2 force = SpaceMath.GravityForce(first.Position, first.Mass,
                    second.Position, second.Mass, GravityConstant);
                first.ApplyForce(force);
                second.ApplyForce(-force); // Bullets and the planet follow exactly the same rule.
            }
        }
    }

    private void Fire(Entity ship)
    {
        Vector2 forward = new(MathF.Cos(ship.Angle), MathF.Sin(ship.Angle));
        Entity bullet = physics.Create(EntityKind.Bullet, ship.Position + forward * 0.6f, 0.08f, 0.01f);
        bullet.SetAngle(ship.Angle);
        bullet.SetVelocity(ship.Velocity + forward * 12);
        bullet.Lifetime = 4;
        // Equal-and-opposite launch impulse. Ammunition mass is not deducted in this stub.
        ship.ApplyImpulse(-forward * (bullet.Mass * 12));
        entities.Add(bullet);
        fireCooldown = 0.3f;
        events.Add(GameEvent.Shot);
    }

    private static void AddDestructible(Entity? entity, HashSet<Entity> remove)
    {
        if (entity?.Kind is EntityKind.Ship or EntityKind.Asteroid) remove.Add(entity);
    }

    public GameEvent[] DrainEvents()
    {
        GameEvent[] result = events.ToArray();
        events.Clear();
        return result;
    }

    public void Dispose() => physics.Dispose();
}
