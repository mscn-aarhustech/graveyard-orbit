using System.Numerics;
using Box2D.NET;
using static Box2D.NET.B2Bodies;
using static Box2D.NET.B2Shapes;
using static Box2D.NET.B2Types;
using static Box2D.NET.B2Worlds;

namespace GraveyardOrbit.Game.Simulation;

public sealed class PhysicsWorld : IDisposable
{
    private readonly B2WorldId world;
    private readonly Dictionary<B2ShapeId, Entity> byShape = [];
    private bool disposed;

    public PhysicsWorld()
    {
        B2WorldDef definition = b2DefaultWorldDef();
        definition.gravity = new B2Vec2(0, 0); // Planetary gravity comes from our library.
        definition.enableSleep = false;
        world = b2CreateWorld(definition);
    }

    public Entity CreateCircle(EntityKind kind, Vector2 position, float radius, Vector2 velocity)
    {
        B2BodyDef bodyDef = b2DefaultBodyDef();
        bodyDef.type = kind == EntityKind.Planet ? B2BodyType.b2_staticBody : B2BodyType.b2_dynamicBody;
        bodyDef.position = ToBox(position);
        bodyDef.linearVelocity = ToBox(velocity);
        bodyDef.enableSleep = false;
        B2BodyId body = b2CreateBody(world, bodyDef);
        B2ShapeDef shapeDef = b2DefaultShapeDef();
        shapeDef.density = 1;
        shapeDef.material.restitution = 0.25f;
        shapeDef.enableContactEvents = true;
        ulong category = kind switch { EntityKind.Planet => 1, EntityKind.Ship => 2, EntityKind.Asteroid => 4, _ => 8 };
        // Bullets only hit the planet and asteroids, never their owner or one another.
        shapeDef.filter = new B2Filter(category, kind == EntityKind.Bullet ? 5UL : kind == EntityKind.Ship ? 5UL : 15UL, 0);
        B2ShapeId shape = b2CreateCircleShape(body, shapeDef, new B2Circle(new B2Vec2(0, 0), radius));
        Entity entity = new(kind, body, shape, radius);
        byShape.Add(shape, entity);
        return entity;
    }

    public static void ApplyForce(Entity entity, Vector2 force) =>
        b2Body_ApplyForceToCenter(entity.Body, ToBox(force), true);

    public static float Mass(Entity entity) => b2Body_GetMass(entity.Body);
    public static Vector2 Velocity(Entity entity)
    {
        B2Vec2 v = b2Body_GetLinearVelocity(entity.Body);
        return new Vector2(v.X, v.Y);
    }

    public List<(Entity A, Entity B)> Step(float delta)
    {
        b2World_Step(world, delta, 4);
        B2ContactEvents contacts = b2World_GetContactEvents(world);
        List<(Entity, Entity)> pairs = [];
        // Copy entity references before gameplay destroys any bodies; event buffers are transient.
        for (int i = 0; i < contacts.beginCount; i++)
        {
            var contact = contacts.beginEvents[i];
            if (byShape.TryGetValue(contact.shapeIdA, out Entity? a) &&
                byShape.TryGetValue(contact.shapeIdB, out Entity? b)) pairs.Add((a, b));
        }
        return pairs;
    }

    public void Destroy(Entity entity)
    {
        byShape.Remove(entity.Shape);
        b2DestroyBody(entity.Body);
    }

    public void Dispose()
    {
        if (disposed) return;
        b2DestroyWorld(world);
        byShape.Clear();
        disposed = true;
    }

    private static B2Vec2 ToBox(Vector2 vector) => new(vector.X, vector.Y);
}
