using System.Numerics;
using Box2D.NET;
using static Box2D.NET.B2Bodies;
using static Box2D.NET.B2MathFunction;

namespace GraveyardOrbit.Game;

public enum EntityKind { Planet, Ship, Asteroid, Bullet }

// Box2D owns position, velocity, mass, and rotation. Do not maintain a second copy.
public sealed class Entity(EntityKind kind, B2BodyId body, B2ShapeId shape, float radius)
{
    public EntityKind Kind { get; } = kind;
    public B2BodyId Body { get; } = body;
    public B2ShapeId Shape { get; } = shape;
    public float Radius { get; } = radius;
    public float Lifetime { get; set; } = float.PositiveInfinity;
    public float Mass => b2Body_GetMass(Body);
    public float Angle => b2Rot_GetAngle(b2Body_GetRotation(Body));
    public Vector2 Position => ToVector(b2Body_GetPosition(Body));
    public Vector2 Velocity => ToVector(b2Body_GetLinearVelocity(Body));

    public void SetVelocity(Vector2 velocity) => b2Body_SetLinearVelocity(Body, ToBox(velocity));
    public void SetPosition(Vector2 position) => b2Body_SetTransform(Body, ToBox(position), b2Body_GetRotation(Body));
    public void SetAngle(float angle) => b2Body_SetTransform(Body, b2Body_GetPosition(Body), b2MakeRot(angle));
    public void ApplyForce(Vector2 force) => b2Body_ApplyForceToCenter(Body, ToBox(force), true);
    public void ApplyImpulse(Vector2 impulse) => b2Body_ApplyLinearImpulseToCenter(Body, ToBox(impulse), true);
    public void Turn(float input) => b2Body_ApplyTorque(Body, input * b2Body_GetRotationalInertia(Body) * 5, true);

    private static Vector2 ToVector(B2Vec2 value) => new(value.X, value.Y);
    private static B2Vec2 ToBox(Vector2 value) => new(value.X, value.Y);
}
