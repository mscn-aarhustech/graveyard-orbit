using System.Numerics;
using GraveyardOrbit.Game.Assets;
using GraveyardOrbit.Game.Audio;
using GraveyardOrbit.Game.Input;
using GraveyardOrbit.Game.Rendering;
using GraveyardOrbit.Game.Simulation;
using Raylib_cs;

namespace GraveyardOrbit.Game;

public static class Program
{
    public static int Main(string[] args)
    {
        bool smoke = args.Contains("--smoke-test");
        Raylib.SetConfigFlags(ConfigFlags.ResizableWindow);
        Raylib.InitWindow(1280, 720, "Graveyard Orbit");
        Raylib.SetWindowMinSize(640, 360);
        Raylib.SetTargetFPS(60);
        RenderTexture2D canvas = default;
        GameSession? game = null;
        try
        {
            AssetCatalog catalog = new(Path.Combine(AppContext.BaseDirectory, "assets"));
            using SpriteManager sprites = new(catalog, new RaylibTextureLoader());
            using SoundManager sounds = new(catalog);
            GameRenderer renderer = new(sprites);
            canvas = Raylib.LoadRenderTexture(GameRenderer.Width, GameRenderer.Height);
            Raylib.SetTextureFilter(canvas.Texture, TextureFilter.Point);
            game = new GameSession();
            double accumulator = 0;
            int frames = 0;
            bool smokeShot = false;
            while (!Raylib.WindowShouldClose() && (!smoke || frames < 120))
            {
                Rectangle destination = Letterbox();
                Vector2 mouse = Raylib.GetMousePosition();
                Vector2 logicalMouse = new((mouse.X - destination.X) * GameRenderer.Width / destination.Width,
                    (mouse.Y - destination.Y) * GameRenderer.Height / destination.Height);
                bool click = Raylib.IsMouseButtonPressed(MouseButton.Left);
                if (Raylib.IsKeyPressed(KeyboardKey.M) || click && Raylib.CheckCollisionPointRec(logicalMouse, GameRenderer.MuteButton))
                    sounds.Muted = !sounds.Muted;
                if (Raylib.IsKeyPressed(KeyboardKey.Enter) || game.IsGameOver && click && Raylib.CheckCollisionPointRec(logicalMouse, GameRenderer.RestartButton))
                {
                    game.Dispose();
                    game = new GameSession();
                    accumulator = 0;
                }
                InputState input = smoke ? new InputState(0, 0, frames < 15, false, false) : KeyboardInput.Read();
                // Cap catch-up after a stalled frame. Never pass a variable dt to Box2D.
                accumulator += smoke ? GameSession.FixedDelta : Math.Min(Raylib.GetFrameTime(), 0.25f);
                while (accumulator >= GameSession.FixedDelta)
                {
                    game.Step(input);
                    accumulator -= GameSession.FixedDelta;
                }
                foreach (GameEvent signal in game.DrainEvents())
                {
                    smokeShot |= signal == GameEvent.Shot;
                    sounds.Play(signal == GameEvent.Shot ? "laserSound" : "explosionSound");
                }
                Raylib.BeginTextureMode(canvas);
                renderer.Draw(game, input, sounds.Muted, sounds.Available);
                Raylib.EndTextureMode();
                Raylib.BeginDrawing();
                Raylib.ClearBackground(Color.Black);
                Raylib.DrawTexturePro(canvas.Texture,
                    new Rectangle(0, 0, canvas.Texture.Width, -canvas.Texture.Height),
                    destination, Vector2.Zero, 0, Color.White);
                Raylib.EndDrawing();
                frames++;
            }
            if (smoke)
            {
                Raylib.TakeScreenshot("smoke.png");
                Console.WriteLine($"SMOKE frames={frames} shot={smokeShot} audio={sounds.Available} entities={game.Entities.Count}");
                return frames == 120 && smokeShot ? 0 : 2;
            }
            return 0;
        }
        catch (Exception exception)
        {
            Console.Error.WriteLine(exception);
            return 1;
        }
        finally
        {
            game?.Dispose();
            if (canvas.Id != 0) Raylib.UnloadRenderTexture(canvas);
            Raylib.CloseWindow();
        }
    }

    private static Rectangle Letterbox()
    {
        float scale = MathF.Min((float)Raylib.GetScreenWidth() / GameRenderer.Width,
            (float)Raylib.GetScreenHeight() / GameRenderer.Height);
        // Integer upscale keeps pixels crisp; fractional scaling handles small windows.
        if (scale >= 1) scale = MathF.Floor(scale);
        float width = GameRenderer.Width * scale;
        float height = GameRenderer.Height * scale;
        return new Rectangle((Raylib.GetScreenWidth() - width) / 2, (Raylib.GetScreenHeight() - height) / 2, width, height);
    }
}
