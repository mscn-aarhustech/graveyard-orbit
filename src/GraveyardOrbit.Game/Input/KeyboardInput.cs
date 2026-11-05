using Raylib_cs;

namespace GraveyardOrbit.Game.Input;

public static class KeyboardInput
{
    public static InputState Read() => new(
        Axis(KeyboardKey.W, KeyboardKey.S),
        Axis(KeyboardKey.D, KeyboardKey.A),
        Raylib.IsKeyDown(KeyboardKey.Space),
        Raylib.IsKeyDown(KeyboardKey.LeftShift) || Raylib.IsKeyDown(KeyboardKey.RightShift),
        Raylib.IsKeyDown(KeyboardKey.LeftControl) || Raylib.IsKeyDown(KeyboardKey.RightControl));

    private static float Axis(KeyboardKey positive, KeyboardKey negative) =>
        (Raylib.IsKeyDown(positive) ? 1 : 0) - (Raylib.IsKeyDown(negative) ? 1 : 0);
}
