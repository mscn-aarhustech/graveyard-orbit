using GraveyardOrbit.Game.Assets;
using Raylib_cs;

namespace GraveyardOrbit.Tests;

public sealed class AssetTests
{
    [Fact]
    [Trait("Category", "Assets")]
    public void Every_manifest_entry_is_present_in_runtime_output()
    {
        AssetCatalog catalog = TestAssets.RuntimeCatalog();
        Assert.NotEmpty(catalog.Names);
        foreach (string name in catalog.Names)
            Assert.True(new FileInfo(catalog.Resolve(name)).Length > 0, name);
    }

    [Fact]
    public void Unknown_asset_reports_its_logical_name()
    {
        using TestAssets assets = new();
        var exception = Assert.Throws<KeyNotFoundException>(() => assets.Catalog.Resolve("missingShip"));
        Assert.Contains("missingShip", exception.Message);
    }

    [Theory]
    [InlineData("../outside.png")]
    [InlineData("..\\outside.png")]
    [InlineData("/absolute.png")]
    [InlineData("C:/absolute.png")]
    public void Manifest_cannot_escape_asset_root(string path)
    {
        using TestAssets assets = new(new Dictionary<string, string> { ["bad"] = path });
        Assert.Throws<InvalidDataException>(() => assets.Catalog.Resolve("bad"));
    }

    [Fact]
    public void Missing_file_fails_before_native_loading()
    {
        using TestAssets assets = new(new Dictionary<string, string> { ["ship"] = "not-there.png" });
        CountingLoader loader = new();
        using SpriteManager sprites = new(assets.Catalog, loader);
        Assert.Throws<FileNotFoundException>(() => sprites.Get("ship"));
        Assert.Equal(0, loader.Loads);
    }

    [Fact]
    public void Sprite_cache_loads_once_and_disposes_once()
    {
        using TestAssets assets = new();
        CountingLoader loader = new();
        SpriteManager sprites = new(assets.Catalog, loader);
        Assert.Equal(sprites.Get("ship").Id, sprites.Get("ship").Id);
        Assert.Equal(1, loader.Loads);
        sprites.Dispose();
        sprites.Dispose();
        Assert.Equal(1, loader.Unloads);
        Assert.Throws<ObjectDisposedException>(() => sprites.Get("ship"));
    }

    private sealed class CountingLoader : ITextureLoader
    {
        public int Loads { get; private set; }
        public int Unloads { get; private set; }
        public Texture2D Load(string path)
        {
            Loads++;
            return new Texture2D { Id = 123, Width = 16, Height = 16 };
        }
        public void Unload(Texture2D texture) => Unloads++;
    }
}
