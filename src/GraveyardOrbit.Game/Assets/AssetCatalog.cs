using System.Text.Json;

namespace GraveyardOrbit.Game.Assets;

public sealed class AssetCatalog
{
    private readonly string root;
    private readonly Dictionary<string, string> entries;
    public IReadOnlyCollection<string> Names => entries.Keys;

    public AssetCatalog(string assetRoot)
    {
        root = Path.GetFullPath(assetRoot);
        entries = JsonSerializer.Deserialize<Dictionary<string, string>>(
            File.ReadAllText(Path.Combine(root, "asset-manifest.json")))
            ?? throw new InvalidDataException("Asset manifest is empty.");
    }

    public string Resolve(string name)
    {
        if (!entries.TryGetValue(name, out string? relative))
            throw new KeyNotFoundException($"Unknown asset '{name}'. Check asset-manifest.json.");
        // Both slash forms are normalized so a manifest behaves the same on every OS.
        relative = relative.Replace('\\', '/');
        if (Path.IsPathRooted(relative) || relative.Contains(':'))
            throw new InvalidDataException($"Asset '{name}' must be a relative path.");
        string full = Path.GetFullPath(Path.Combine(root, relative));
        string back = Path.GetRelativePath(root, full);
        if (back == ".." || back.StartsWith(".." + Path.DirectorySeparatorChar))
            throw new InvalidDataException($"Asset '{name}' escapes the assets folder.");
        if (!File.Exists(full)) throw new FileNotFoundException($"Missing asset '{name}'.", full);
        return full;
    }
}
