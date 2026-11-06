using System.Numerics;
using Box2D.NET;
using static Box2D.NET.B2Bodies;
using static Box2D.NET.B2Shapes;
using static Box2D.NET.B2Types;
using static Box2D.NET.B2Worlds;
using static Box2D.NET.B2Geometries;

namespace GraveyardOrbit.Game;

// A null entity in a collision means one of the external arena walls.
public readonly record struct Collision(Entity? First, Entity? Second, float ImpactSpeed, bool Began);

public sealed class PhysicsWorld : IDisposable
{
    public const float HalfWidth = 50;
    public const float HalfHeight = 32;
    private readonly B2WorldId world;
    private readonly Dictionary<B2ShapeId, Entity> entities = [];
    private bool disposed;

    public PhysicsWorld()
    {
        B2WorldDef definition = b2DefaultWorldDef();
        definition.gravity = new B2Vec2(0, 0);
        definition.enableSleep = false;
        definition.hitEventThreshold = 0.1f;
        world = b2CreateWorld(definition);
        // Four static fixtures are an external constraint, not gravitating game entities.
        CreateWall(new Vector2(-HalfWidth - 0.5f, 0), 0.5f, HalfHeight + 1);
        CreateWall(new Vector2(HalfWidth + 0.5f, 0), 0.5f, HalfHeight + 1);
        CreateWall(new Vector2(0, -HalfHeight - 0.5f), HalfWidth + 1, 0.5f);
        CreateWall(new Vector2(0, HalfHeight + 0.5f), HalfWidth + 1, 0.5f);
    }

    public Entity Create(EntityKind kind, Vector2 position, float radius, float mass)
    {
        ObjectDisposedException.ThrowIf(disposed, this);
        if (radius <= 0 || mass <= 0) throw new ArgumentOutOfRangeException(nameof(radius));
        B2BodyDef bodyDef = b2DefaultBodyDef();
        bodyDef.type = B2BodyType.b2_dynamicBody; // Even the planet and bullets are dynamic.
        bodyDef.position = new B2Vec2(position.X, position.Y);
        bodyDef.enableSleep = false;
        bodyDef.isBullet = kind == EntityKind.Bullet;
        // Small attitude stabilization on the ship only; no translational drag.
        bodyDef.angularDamping = kind == EntityKind.Ship ? 0.8f : 0;
        B2BodyId body = b2CreateBody(world, bodyDef);
        B2ShapeDef shapeDef = ShapeDefinition();
        shapeDef.density = mass / (MathF.PI * radius * radius);
        ulong category = kind switch { EntityKind.Planet => 1, EntityKind.Ship => 2, EntityKind.Asteroid => 4, _ => 8 };
        // Collision filters do NOT filter gravitational attraction.
        shapeDef.filter = new B2Filter(category, kind == EntityKind.Bullet ? 5UL : kind == EntityKind.Ship ? 5UL : 15UL, 0);
        B2ShapeId shape = b2CreateCircleShape(body, shapeDef, new B2Circle(new B2Vec2(0, 0), radius));
        Entity entity = new(kind, body, shape, radius);
        entities.Add(shape, entity);
        return entity;
    }

    public List<Collision> Step(float delta)
    {
        b2World_Step(world, delta, 4);
        B2ContactEvents contacts = b2World_GetContactEvents(world);
        List<Collision> result = [];
        // Read actual pre-impact normal speeds supplied by Box2D, not post-bounce velocities.
        for (int i = 0; i < contacts.hitCount; i++)
        {
            var hit = contacts.hitEvents[i];
            result.Add(new Collision(Find(hit.shapeIdA), Find(hit.shapeIdB), hit.approachSpeed, false));
        }
        for (int i = 0; i < contacts.beginCount; i++)
        {
            var begin = contacts.beginEvents[i];
            result.Add(new Collision(Find(begin.shapeIdA), Find(begin.shapeIdB), 0, true));
        }
        // All buffers are copied before gameplay removes bodies.
        return result;
    }

    public void Destroy(Entity entity)
    {
        entities.Remove(entity.Shape);
        b2DestroyBody(entity.Body);
    }

    public void Dispose()
    {
        if (disposed) return;
        b2DestroyWorld(world);
        entities.Clear();
        disposed = true;
    }

    private Entity? Find(B2ShapeId shape) => entities.GetValueOrDefault(shape);

    private static B2ShapeDef ShapeDefinition()
    {
        B2ShapeDef definition = b2DefaultShapeDef();
        definition.material.friction = 0.7f;
        definition.material.restitution = 0.05f;
        definition.enableContactEvents = true;
        definition.enableHitEvents = true;
        return definition;
    }

    private void CreateWall(Vector2 position, float halfWidth, float halfHeight)
    {
        B2BodyDef bodyDef = b2DefaultBodyDef();
        bodyDef.position = new B2Vec2(position.X, position.Y);
        B2BodyId body = b2CreateBody(world, bodyDef);
        b2CreatePolygonShape(body, ShapeDefinition(), b2MakeBox(halfWidth, halfHeight));
    }
}
