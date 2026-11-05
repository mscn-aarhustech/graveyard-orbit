using System.Numerics;

namespace GraveyardOrbit.SpacePhysics;

public static class Orbit
{
    /// <summary>Initial tangent velocity for an ideal circular orbit around a fixed point mass.</summary>
    public static Vector2 CircularVelocity(Vector2 position, Vector2 centre, float mu)
    {
        if (!float.IsFinite(mu) || mu <= 0) throw new ArgumentOutOfRangeException(nameof(mu));
        Vector2 radial = position - centre;
        float radius = radial.Length();
        if (radius < 0.0001f) throw new ArgumentException("An orbit needs a nonzero radius.", nameof(position));
        Vector2 tangent = new(-radial.Y / radius, radial.X / radius);
        return tangent * MathF.Sqrt(mu / radius);
    }
}
