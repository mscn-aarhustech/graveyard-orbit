using System.Numerics;

namespace GraveyardOrbit.SpacePhysics;

// Pure calculations. No rendering, Box2D, or knowledge of ships versus bullets.
public static class SpaceMath
{
    public static Vector2 GravityForce(Vector2 firstPosition, float firstMass,
        Vector2 secondPosition, float secondMass, float gravityConstant, float minimumDistance = 0.1f)
    {
        CheckMass(firstMass);
        CheckMass(secondMass);
        if (!float.IsFinite(gravityConstant) || gravityConstant < 0)
            throw new ArgumentOutOfRangeException(nameof(gravityConstant));
        if (!float.IsFinite(minimumDistance) || minimumDistance <= 0)
            throw new ArgumentOutOfRangeException(nameof(minimumDistance));
        Vector2 offset = secondPosition - firstPosition;
        float distance = offset.Length();
        if (distance < 0.00001f) return Vector2.Zero;
        // Newton's inverse-square law outside this small collision-overlap safeguard.
        float radius = MathF.Max(distance, minimumDistance);
        return offset / distance * (gravityConstant * firstMass * secondMass / (radius * radius));
    }

    public static Vector2 Thrust(float angle, float throttle, float maximumForce)
    {
        if (!float.IsFinite(throttle) || throttle < -1 || throttle > 1)
            throw new ArgumentOutOfRangeException(nameof(throttle));
        return new Vector2(MathF.Cos(angle), MathF.Sin(angle)) * (throttle * maximumForce);
    }

    // Two-body circular initial conditions, not a constraint keeping either body on rails.
    // The shared velocity preserves any pre-existing momentum of the pair.
    public static (Vector2 First, Vector2 Second) CreateCircularOrbit(
        Vector2 firstPosition, float firstMass, Vector2 secondPosition, float secondMass,
        float gravityConstant, Vector2 sharedVelocity = default)
    {
        CheckMass(firstMass);
        CheckMass(secondMass);
        if (!float.IsFinite(gravityConstant) || gravityConstant <= 0)
            throw new ArgumentOutOfRangeException(nameof(gravityConstant));
        Vector2 radial = secondPosition - firstPosition;
        float distance = radial.Length();
        if (distance < 0.00001f) throw new ArgumentException("An orbit needs separated bodies.");
        float totalMass = firstMass + secondMass;
        Vector2 tangent = new(-radial.Y / distance, radial.X / distance);
        Vector2 relativeVelocity = tangent * MathF.Sqrt(gravityConstant * totalMass / distance);
        return (sharedVelocity - relativeVelocity * (secondMass / totalMass),
            sharedVelocity + relativeVelocity * (firstMass / totalMass));
    }

    private static void CheckMass(float mass)
    {
        if (!float.IsFinite(mass) || mass <= 0) throw new ArgumentOutOfRangeException(nameof(mass));
    }
}
