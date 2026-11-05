using Raylib_cs;

namespace GraveyardOrbit.Game.Assets;

// This one seam lets tests exercise cache ownership without opening a native window.
public interface ITextureLoader
{
    Texture2D Load(string path);
    void Unload(Texture2D texture);
}

public sealed class RaylibTextureLoader : ITextureLoader
{
    public Texture2D Load(string path)
    {
        Texture2D texture = Raylib.LoadTexture(path);
        if (!Raylib.IsTextureValid(texture)) throw new IOException($"Cannot load texture: {path}");
        Raylib.SetTextureFilter(texture, TextureFilter.Point);
        return texture;
    }
    public void Unload(Texture2D texture) => Raylib.UnloadTexture(texture);
}

public sealed class SpriteManager(AssetCatalog catalog, ITextureLoader loader) : IDisposable
{
    private readonly Dictionary<string, Texture2D> textures = [];
    private bool disposed;

    public Texture2D Get(string name)
    {
        ObjectDisposedException.ThrowIf(disposed, this);
        if (!textures.TryGetValue(name, out Texture2D texture))
        {
            texture = loader.Load(catalog.Resolve(name));
            textures.Add(name, texture);
        }
        return texture;
    }

    public void Dispose()
    {
        if (disposed) return;
        foreach (Texture2D texture in textures.Values) loader.Unload(texture);
        textures.Clear();
        disposed = true;
    }
}
