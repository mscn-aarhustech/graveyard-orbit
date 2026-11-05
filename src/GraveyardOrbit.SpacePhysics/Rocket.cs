using System.Numerics;

namespace GraveyardOrbit.SpacePhysics;

public static class Rocket
{
    // Angle is radians in screen coordinates: zero points right, +pi/2 points down.
    // Constant mass is intentional. Fuel and the rocket equation are student extensions.
    public static Vector2 Thrust(float angle, float throttle, float maximumForce)
    {
        if (!float.IsFinite(angle)) throw new ArgumentOutOfRangeException(nameof(angle));
        if (!float.IsFinite(throttle) || throttle < -1 || throttle > 1)
            throw new ArgumentOutOfRangeException(nameof(throttle));
        if (!float.IsFinite(maximumForce) || maximumForce < 0)
            throw new ArgumentOutOfRangeException(nameof(maximumForce));
        return new Vector2(MathF.Cos(angle), MathF.Sin(angle)) * (throttle * maximumForce);
    }
}
