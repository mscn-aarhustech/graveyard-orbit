using System.Numerics;
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
        Game? game = null;
        try
        {
            using Assets assets = new();
            assets.StartAudio();
            Camera camera = new(Raylib.GetScreenWidth(), Raylib.GetScreenHeight());
            Renderer renderer = new(camera, assets);
            game = new Game();
            double accumulator = 0;
            bool fullscreen = false;
            Vector2 windowPosition = default;
            Vector2 windowSize = new(1280, 720);
            int frames = 0;
            int initialWidth = camera.Width;
            int largestWidth = initialWidth;
            int fullscreenChanges = 0;
            bool shot = false;
            while (!Raylib.WindowShouldClose() && (!smoke || frames < 180))
            {
                if (Raylib.IsKeyPressed(KeyboardKey.F11) || smoke && (frames == 30 || frames == 90))
                {
                    ToggleFullscreen(ref fullscreen, ref windowPosition, ref windowSize);
                    fullscreenChanges++;
                }
                camera.Resize(Raylib.GetScreenWidth(), Raylib.GetScreenHeight());
                largestWidth = Math.Max(largestWidth, camera.Width);
                if (canvas.Id == 0 || canvas.Texture.Width != camera.Width || canvas.Texture.Height != camera.Height)
                {
                    if (canvas.Id != 0) Raylib.UnloadRenderTexture(canvas);
                    canvas = Raylib.LoadRenderTexture(camera.Width, camera.Height);
                    Raylib.SetTextureFilter(canvas.Texture, TextureFilter.Point);
                }
                Rectangle destination = new((Raylib.GetScreenWidth() - camera.Width * Camera.PixelScale) / 2f,
                    (Raylib.GetScreenHeight() - camera.Height * Camera.PixelScale) / 2f,
                    camera.Width * Camera.PixelScale, camera.Height * Camera.PixelScale);
                Vector2 mouse = (Raylib.GetMousePosition() - new Vector2(destination.X, destination.Y)) / Camera.PixelScale;
                bool click = Raylib.IsMouseButtonPressed(MouseButton.Left);
                if (Raylib.IsKeyPressed(KeyboardKey.M) || click && Raylib.CheckCollisionPointRec(mouse, renderer.MuteButton))
                    assets.Muted = !assets.Muted;
                if (Raylib.IsKeyPressed(KeyboardKey.Enter) || game.IsGameOver && click && Raylib.CheckCollisionPointRec(mouse, renderer.RestartButton))
                {
                    game.Dispose();
                    game = new Game();
                    accumulator = 0;
                }
                Controls input = smoke ? new Controls(0, 0, frames < 12, false, false) : ReadControls();
                accumulator += smoke ? Game.FixedDelta : Math.Min(Raylib.GetFrameTime(), 0.25f);
                while (accumulator >= Game.FixedDelta)
                {
                    game.Step(input);
                    accumulator -= Game.FixedDelta;
                }
                foreach (GameEvent signal in game.DrainEvents())
                {
                    shot |= signal == GameEvent.Shot;
                    assets.Play(signal);
                }
                if (game.Ship is { } ship) camera.Follow(ship.Position);
                Raylib.BeginTextureMode(canvas);
                renderer.Draw(game, input);
                Raylib.EndTextureMode();
                Raylib.BeginDrawing();
                Raylib.ClearBackground(Color.Black);
                Raylib.DrawTexturePro(canvas.Texture, new Rectangle(0, 0, camera.Width, -camera.Height),
                    destination, Vector2.Zero, 0, Color.White);
                Raylib.EndDrawing();
                frames++;
            }
            if (smoke)
            {
                Raylib.TakeScreenshot("smoke.png");
                Console.WriteLine($"SMOKE frames={frames} shot={shot} audio={assets.AudioAvailable} fullscreenChanges={fullscreenChanges} logicalWidth={initialWidth}->{largestWidth}->{camera.Width}");
                return frames == 180 && shot && fullscreenChanges == 2 && camera.Width == initialWidth ? 0 : 2;
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

    private static Controls ReadControls() => new(
        Axis(KeyboardKey.W, KeyboardKey.S), Axis(KeyboardKey.D, KeyboardKey.A),
        Raylib.IsKeyDown(KeyboardKey.Space),
        Raylib.IsKeyDown(KeyboardKey.LeftShift) || Raylib.IsKeyDown(KeyboardKey.RightShift),
        Raylib.IsKeyDown(KeyboardKey.LeftControl) || Raylib.IsKeyDown(KeyboardKey.RightControl));

    private static float Axis(KeyboardKey positive, KeyboardKey negative) =>
        (Raylib.IsKeyDown(positive) ? 1 : 0) - (Raylib.IsKeyDown(negative) ? 1 : 0);

    private static void ToggleFullscreen(ref bool fullscreen, ref Vector2 position, ref Vector2 size)
    {
        if (!fullscreen)
        {
            position = Raylib.GetWindowPosition();
            size = new Vector2(Raylib.GetScreenWidth(), Raylib.GetScreenHeight());
        }
        Raylib.ToggleBorderlessWindowed();
        fullscreen = !fullscreen;
        if (!fullscreen)
        {
            Raylib.SetWindowSize((int)size.X, (int)size.Y);
            Raylib.SetWindowPosition((int)position.X, (int)position.Y);
        }
    }
}
