using System.Numerics;
using GraveyardOrbit.Game.Assets;
using GraveyardOrbit.Game.Input;
using GraveyardOrbit.Game.Simulation;
using Raylib_cs;

namespace GraveyardOrbit.Game.Rendering;

public sealed class GameRenderer(SpriteManager sprites)
{
    public const int Width = 640;
    public const int Height = 360;
    public static readonly Rectangle MuteButton = new(548, 8, 84, 22);
    public static readonly Rectangle RestartButton = new(261, 227, 118, 26);
    private const float PixelsPerMetre = 18;
    private static readonly Color Cyan = new(109, 226, 235, 255);
    private static readonly Color Muted = new(155, 166, 194, 255);

    public void Draw(GameSession game, InputState input, bool muted, bool audioAvailable)
    {
        Raylib.ClearBackground(new Color(9, 12, 25, 255));
        for (int i = 0; i < 95; i++)
            Raylib.DrawPixel((i * 197 + 31) % Width, (i * 97 + 13) % Height,
                i % 4 == 0 ? Muted : new Color(42, 49, 72, 255));

        Vector2 centre = new(Width / 2f, Height / 2f);
        Raylib.DrawCircleLines((int)centre.X, (int)centre.Y, GameSession.EscapeRadius * PixelsPerMetre,
            new Color(50, 40, 58, 255));
        Raylib.DrawCircleLines((int)centre.X, (int)centre.Y, 86, new Color(28, 38, 55, 255));
        Raylib.DrawCircleLines((int)centre.X, (int)centre.Y, 119, new Color(28, 38, 55, 255));
        // Procedural planet placeholder. The guides are decorative, NOT orbit predictions.
        Raylib.DrawCircleV(centre, GameSession.PlanetRadius * PixelsPerMetre + 3, new Color(32, 57, 77, 255));
        Raylib.DrawCircleV(centre, GameSession.PlanetRadius * PixelsPerMetre, new Color(64, 90, 110, 255));
        Raylib.DrawCircleV(centre + new Vector2(-12, -9), 11, new Color(47, 65, 84, 255));
        Raylib.DrawCircleV(centre + new Vector2(15, 14), 7, new Color(47, 65, 84, 255));
        foreach (Entity entity in game.Entities)
        {
            if (entity.Kind == EntityKind.Planet) continue;
            Vector2 position = centre + entity.Position * PixelsPerMetre;
            string sprite = entity.Kind switch
            {
                EntityKind.Ship => "playerShip",
                EntityKind.Asteroid => "asteroidLarge",
                _ => "laser"
            };
            float size = entity.Kind == EntityKind.Ship ? 22 : entity.Kind == EntityKind.Bullet ? 10 : entity.Radius * PixelsPerMetre * 2.5f;
            Texture2D texture = sprites.Get(sprite);
            // Kenney ship/laser sprites point up; simulation heading zero points right.
            float rotation = entity.Kind == EntityKind.Asteroid ? game.Elapsed * 12 : entity.Heading * 180 / MathF.PI + 90;
            Raylib.DrawTexturePro(texture, new Rectangle(0, 0, texture.Width, texture.Height),
                new Rectangle(position.X, position.Y, size, size), new Vector2(size / 2), rotation, Color.White);
            if (entity.Kind == EntityKind.Ship && input.Throttle != 0 && !game.IsGameOver)
            {
                Vector2 direction = new(MathF.Cos(entity.Heading), MathF.Sin(entity.Heading));
                Raylib.DrawCircleV(position - direction * (input.Throttle > 0 ? 12 : -12), 3, Color.Orange);
            }
        }
        Raylib.DrawText("GRAVEYARD ORBIT", 10, 10, 18, Cyan);
        Raylib.DrawText($"SALVAGE {game.Score}   ROCKS {game.AsteroidsRemaining}", 10, 33, 10, Muted);
        Raylib.DrawRectangleRec(MuteButton, new Color(25, 36, 54, 255));
        Raylib.DrawText(!audioAvailable ? "NO AUDIO" : muted ? "UNMUTE" : "MUTE", 558, 15, 10, Cyan);
        Raylib.DrawText("W/S THRUST  A/D TURN  SPACE FIRE  SHIFT BOOST  CTRL PRECISE", 10, 335, 10, Muted);
        Raylib.DrawText("ENTER RESTART   M MUTE   ESC EXIT", 10, 348, 10, Muted);
        if (game.IsGameOver)
        {
            Raylib.DrawRectangle(170, 129, 300, 137, new Color(8, 12, 25, 235));
            Raylib.DrawText("ORBIT LOST", 238, 151, 24, Cyan);
            Raylib.DrawText("Collision or escape. Try another trajectory.", 185, 189, 10, Muted);
            Raylib.DrawRectangleRec(RestartButton, new Color(25, 36, 54, 255));
            Raylib.DrawText("ENTER / CLICK", 276, 235, 10, Cyan);
        }
        else if (game.AsteroidsRemaining == 0)
            Raylib.DrawText("ORBIT CLEARED! ENTER FOR ANOTHER RUN", 139, 310, 10, Cyan);
    }
}
