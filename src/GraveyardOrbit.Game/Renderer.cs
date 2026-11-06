using System.Numerics;
using Raylib_cs;

namespace GraveyardOrbit.Game;

public sealed class Renderer(Camera camera, Assets assets)
{
    private static readonly Color Cyan = new(109, 226, 235, 255);
    private static readonly Color Muted = new(155, 166, 194, 255);
    public Rectangle MuteButton => new(camera.Width - 92, 8, 84, 22);
    public Rectangle RestartButton => new(camera.Width / 2f - 59, camera.Height / 2f + 47, 118, 26);

    public void Draw(Game game, Controls input)
    {
        Raylib.ClearBackground(new Color(9, 12, 25, 255));
        DrawGrid();
        Vector2 topLeft = camera.WorldToScreen(new Vector2(-PhysicsWorld.HalfWidth, -PhysicsWorld.HalfHeight));
        Raylib.DrawRectangleLinesEx(new Rectangle(topLeft.X, topLeft.Y,
            PhysicsWorld.HalfWidth * 2 * Camera.PixelsPerMetre,
            PhysicsWorld.HalfHeight * 2 * Camera.PixelsPerMetre), 3, new Color(132, 89, 86, 255));
        foreach (Entity entity in game.Entities)
        {
            if (entity.Kind == EntityKind.Planet) DrawPlanet(entity);
            else DrawSprite(entity);
            if (entity == game.Ship && input.Throttle != 0)
            {
                Vector2 direction = new(MathF.Cos(entity.Angle), MathF.Sin(entity.Angle));
                Raylib.DrawCircleV(camera.WorldToScreen(entity.Position) - direction * (input.Throttle > 0 ? 10 : -10), 2, Color.Orange);
            }
        }
        Raylib.DrawText("GRAVEYARD ORBIT", 10, 10, 18, Cyan);
        Raylib.DrawText($"SALVAGE {game.Score}   ROCKS {game.AsteroidsRemaining}", 10, 33, 10, Muted);
        Raylib.DrawRectangleRec(MuteButton, new Color(25, 36, 54, 255));
        Raylib.DrawText(!assets.AudioAvailable ? "NO AUDIO" : assets.Muted ? "UNMUTE" : "MUTE", camera.Width - 82, 15, 10, Cyan);
        if (camera.Width >= 560)
            Raylib.DrawText("W/S THRUST  A/D TURN  SPACE FIRE  SHIFT BOOST  CTRL PRECISE", 10, camera.Height - 25, 10, Muted);
        else
        {
            Raylib.DrawText("W/S THRUST  A/D TURN  SPACE FIRE", 10, camera.Height - 38, 10, Muted);
            Raylib.DrawText("SHIFT BOOST  CTRL PRECISE", 10, camera.Height - 25, 10, Muted);
        }
        Raylib.DrawText("F11 FULLSCREEN  ENTER RESTART  M MUTE  ESC EXIT", 10, camera.Height - 12, 10, Muted);
        if (game.IsGameOver)
        {
            int x = camera.Width / 2;
            int y = camera.Height / 2;
            Raylib.DrawRectangle(x - 150, y - 51, 300, 137, new Color(8, 12, 25, 235));
            Raylib.DrawText("SHIP LOST", x - 69, y - 29, 24, Cyan);
            Raylib.DrawText("Gentle nudges work. Hard impacts do not.", x - 129, y + 9, 10, Muted);
            Raylib.DrawRectangleRec(RestartButton, new Color(25, 36, 54, 255));
            Raylib.DrawText("ENTER / CLICK", x - 44, y + 55, 10, Cyan);
        }
        else if (game.AsteroidsRemaining == 0)
            Raylib.DrawText("ORBIT CLEARED! ENTER FOR ANOTHER RUN", 10, 52, 10, Cyan);
    }

    private void DrawPlanet(Entity planet)
    {
        Vector2 centre = camera.WorldToScreen(planet.Position);
        float radius = planet.Radius * Camera.PixelsPerMetre;
        Raylib.DrawCircleV(centre, radius + 2, new Color(32, 57, 77, 255));
        Raylib.DrawCircleV(centre, radius, new Color(64, 90, 110, 255));
        // Craters rotate with the physical planet rather than a fixed decorative position.
        Vector2 offset = new(MathF.Cos(planet.Angle), MathF.Sin(planet.Angle));
        Raylib.DrawCircleV(centre + offset * radius * 0.4f, radius * 0.23f, new Color(47, 65, 84, 255));
        Raylib.DrawCircleV(centre - offset * radius * 0.45f, radius * 0.16f, new Color(47, 65, 84, 255));
    }

    private void DrawSprite(Entity entity)
    {
        string name = entity.Kind switch { EntityKind.Ship => "playerShip", EntityKind.Asteroid => "asteroidLarge", _ => "laser" };
        Texture2D texture = assets.Sprite(name);
        Vector2 position = camera.WorldToScreen(entity.Position);
        float height = entity.Kind == EntityKind.Ship ? 18 : entity.Kind == EntityKind.Bullet ? 8 : entity.Radius * Camera.PixelsPerMetre * 2.3f;
        float width = height * texture.Width / texture.Height;
        float rotation = entity.Angle * 180 / MathF.PI + (entity.Kind == EntityKind.Asteroid ? 0 : 90);
        Raylib.DrawTexturePro(texture, new Rectangle(0, 0, texture.Width, texture.Height),
            new Rectangle(position.X, position.Y, width, height), new Vector2(width / 2, height / 2), rotation, Color.White);
    }

    private void DrawGrid()
    {
        Vector2 minimum = camera.ScreenToWorld(Vector2.Zero);
        Vector2 maximum = camera.ScreenToWorld(new Vector2(camera.Width, camera.Height));
        Color colour = new(19, 25, 39, 255);
        for (float x = MathF.Floor(minimum.X / 4) * 4; x <= maximum.X; x += 4)
        {
            Vector2 point = camera.WorldToScreen(new Vector2(x, 0));
            Raylib.DrawLine((int)point.X, 0, (int)point.X, camera.Height, colour);
        }
        for (float y = MathF.Floor(minimum.Y / 4) * 4; y <= maximum.Y; y += 4)
        {
            Vector2 point = camera.WorldToScreen(new Vector2(0, y));
            Raylib.DrawLine(0, (int)point.Y, camera.Width, (int)point.Y, colour);
        }
    }
}
