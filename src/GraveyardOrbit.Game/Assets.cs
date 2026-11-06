using System.Text.Json;
using Raylib_cs;

namespace GraveyardOrbit.Game;

// One owner for the small texture/sound cache. The original asset packs stay untouched.
public sealed class Assets : IDisposable
{
    private readonly string root;
    private readonly Dictionary<string, string> manifest;
    private readonly Dictionary<string, Texture2D> textures = [];
    private readonly Dictionary<string, Sound> sounds = [];
    private bool disposed;
    public bool AudioAvailable { get; private set; }
    public bool Muted { get; set; }

    public Assets(string? assetRoot = null)
    {
        root = Path.GetFullPath(assetRoot ?? Path.Combine(AppContext.BaseDirectory, "assets"));
        manifest = JsonSerializer.Deserialize<Dictionary<string, string>>(
            File.ReadAllText(Path.Combine(root, "asset-manifest.json")))
            ?? throw new InvalidDataException("Empty asset manifest.");
    }

    public string Find(string name)
    {
        ObjectDisposedException.ThrowIf(disposed, this);
        if (!manifest.TryGetValue(name, out string? relative))
            throw new KeyNotFoundException($"Unknown asset: {name}");
        relative = relative.Replace('\\', '/');
        if (Path.IsPathRooted(relative) || relative.Contains(':'))
            throw new InvalidDataException("Assets must use relative paths.");
        string full = Path.GetFullPath(Path.Combine(root, relative));
        string back = Path.GetRelativePath(root, full);
        if (back == ".." || back.StartsWith(".." + Path.DirectorySeparatorChar))
            throw new InvalidDataException("Asset path leaves the assets folder.");
        if (!File.Exists(full)) throw new FileNotFoundException($"Missing asset: {name}", full);
        return full;
    }

    public Texture2D Sprite(string name)
    {
        ObjectDisposedException.ThrowIf(disposed, this);
        if (!textures.TryGetValue(name, out Texture2D texture))
        {
            texture = Raylib.LoadTexture(Find(name));
            if (!Raylib.IsTextureValid(texture)) throw new IOException($"Cannot load sprite: {name}");
            Raylib.SetTextureFilter(texture, TextureFilter.Point);
            textures.Add(name, texture);
        }
        return texture;
    }

    public void StartAudio()
    {
        Raylib.InitAudioDevice();
        AudioAvailable = Raylib.IsAudioDeviceReady();
        if (!AudioAvailable) return;
        foreach (string name in new[] { "laserSound", "explosionSound" })
        {
            Sound sound = Raylib.LoadSound(Find(name));
            if (!Raylib.IsSoundValid(sound)) throw new IOException($"Cannot load sound: {name}");
            sounds.Add(name, sound);
        }
        Raylib.SetMasterVolume(0.3f);
    }

    public void Play(GameEvent signal)
    {
        if (AudioAvailable && !Muted)
            Raylib.PlaySound(sounds[signal == GameEvent.Shot ? "laserSound" : "explosionSound"]);
    }

    public void Dispose()
    {
        if (disposed) return;
        foreach (Texture2D texture in textures.Values) Raylib.UnloadTexture(texture);
        foreach (Sound sound in sounds.Values) Raylib.UnloadSound(sound);
        if (AudioAvailable) Raylib.CloseAudioDevice();
        disposed = true;
    }
}
