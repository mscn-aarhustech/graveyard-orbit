using System.Numerics;

namespace GraveyardOrbit.Game;

public sealed class Camera
{
    public const int PixelScale = 2;
    public const float PixelsPerMetre = 16;
    public Vector2 Centre { get; private set; }
    public int Width { get; private set; }
    public int Height { get; private set; }

    public Camera(int windowWidth, int windowHeight) => Resize(windowWidth, windowHeight);

    public void Resize(int windowWidth, int windowHeight)
    {
        Width = Math.Max(1, windowWidth / PixelScale);
        Height = Math.Max(1, windowHeight / PixelScale);
    }

    public void Follow(Vector2 position) => Centre = position;
    public Vector2 WorldToScreen(Vector2 position) =>
        (position - Centre) * PixelsPerMetre + new Vector2(Width / 2f, Height / 2f);
    public Vector2 ScreenToWorld(Vector2 position) =>
        (position - new Vector2(Width / 2f, Height / 2f)) / PixelsPerMetre + Centre;
}
