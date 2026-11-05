using GraveyardOrbit.Game.Assets;
using Raylib_cs;

namespace GraveyardOrbit.Game.Audio;

public sealed class SoundManager : IDisposable
{
    private readonly Dictionary<string, Sound> sounds = [];
    public bool Available { get; }
    public bool Muted { get; set; }

    public SoundManager(AssetCatalog catalog)
    {
        Raylib.InitAudioDevice();
        Available = Raylib.IsAudioDeviceReady();
        if (!Available) return; // Audio-less classroom machines should still run the game.
        try
        {
            foreach (string name in new[] { "laserSound", "explosionSound" })
            {
                Sound sound = Raylib.LoadSound(catalog.Resolve(name));
                if (!Raylib.IsSoundValid(sound)) throw new IOException($"Cannot load sound: {name}");
                sounds.Add(name, sound);
            }
            Raylib.SetMasterVolume(0.3f);
        }
        catch
        {
            Dispose();
            throw;
        }
    }

    public void Play(string name)
    {
        if (Available && !Muted) Raylib.PlaySound(sounds[name]);
    }

    public void Dispose()
    {
        foreach (Sound sound in sounds.Values) Raylib.UnloadSound(sound);
        sounds.Clear();
        if (Raylib.IsAudioDeviceReady()) Raylib.CloseAudioDevice();
    }
}
