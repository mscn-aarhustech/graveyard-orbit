using System.Text.Json;
using GraveyardOrbit.Game.Assets;

namespace GraveyardOrbit.Tests;

internal sealed class TestAssets : IDisposable
{
    public string Root { get; } = Path.Combine(Path.GetTempPath(), "graveyard-tests-" + Guid.NewGuid());
    public AssetCatalog Catalog { get; }

    public TestAssets(Dictionary<string, string>? manifest = null)
    {
        Directory.CreateDirectory(Root);
        File.WriteAllText(Path.Combine(Root, "sprite.png"), "Fake loader fixture, not a real PNG.");
        File.WriteAllText(Path.Combine(Root, "asset-manifest.json"),
            JsonSerializer.Serialize(manifest ?? new Dictionary<string, string> { ["ship"] = "sprite.png" }));
        Catalog = new AssetCatalog(Root);
    }

    public void Dispose() => Directory.Delete(Root, true);
    public static AssetCatalog RuntimeCatalog() => new(Path.Combine(AppContext.BaseDirectory, "assets"));
}
