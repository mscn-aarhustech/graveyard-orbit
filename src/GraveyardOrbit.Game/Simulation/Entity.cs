using System.Numerics;
using Box2D.NET;
using static Box2D.NET.B2Bodies;

namespace GraveyardOrbit.Game.Simulation;

public enum EntityKind { Planet, Ship, Asteroid, Bullet }

public sealed class Entity(EntityKind kind, B2BodyId body, B2ShapeId shape, float radius)
{
    public EntityKind Kind { get; } = kind;
    public B2BodyId Body { get; } = body;
    public B2ShapeId Shape { get; } = shape;
    public float Radius { get; } = radius;
    public float Heading { get; set; }
    public float LifeRemaining { get; set; } = float.PositiveInfinity;
    public Vector2 Position
    {
        get
        {
            B2Vec2 p = b2Body_GetPosition(Body);
            return new Vector2(p.X, p.Y);
        }
    }
}
