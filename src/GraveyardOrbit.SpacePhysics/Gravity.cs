using System.Numerics;

namespace GraveyardOrbit.SpacePhysics;

/// <summary>Newtonian point-source gravity. Units: metres, kilograms, seconds.</summary>
public static class Gravity
{
    // mu = G * planetMass. Gameplay uses a scaled mu, not a real astronomical mass.
    // Outside minimumRadius this is exactly mu / r^2; the clamp avoids a singularity.
    public static Vector2 Acceleration(Vector2 position, Vector2 centre, float mu, float minimumRadius)
    {
        if (!float.IsFinite(mu) || mu < 0) throw new ArgumentOutOfRangeException(nameof(mu));
        if (!float.IsFinite(minimumRadius) || minimumRadius <= 0)
            throw new ArgumentOutOfRangeException(nameof(minimumRadius));
        Vector2 offset = centre - position;
        float distance = offset.Length();
        if (distance < 0.0001f) return Vector2.Zero;
        float radius = MathF.Max(distance, minimumRadius);
        return offset / distance * (mu / (radius * radius));
    }

    public static Vector2 Force(Vector2 position, Vector2 centre, float mu, float mass, float minimumRadius)
    {
        if (!float.IsFinite(mass) || mass <= 0) throw new ArgumentOutOfRangeException(nameof(mass));
        return mass * Acceleration(position, centre, mu, minimumRadius);
    }
}
