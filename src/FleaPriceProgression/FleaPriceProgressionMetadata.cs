using SPTarkov.Server.Core.Models.Spt.Mod;

namespace FleaPriceProgression;

public record FleaPriceProgressionMetadata : IModMetadata
{
    public string ModGuid { get; init; } = "com.styrr.fleapriceprogression";
    public string Name { get; init; } = "Flea Price Progression";
    public string Author { get; init; } = "Styrr";
    public List<string>? Contributors { get; init; }
    public SemanticVersioning.Version Version { get; init; } = new("1.0.0");
    public SemanticVersioning.Range SptVersion { get; init; } = new("~4.1.0");
    public List<string>? Incompatibilities { get; init; } = ["xyz.drakia.livefleaprices"];
    public Dictionary<string, SemanticVersioning.Range>? ModDependencies { get; init; }
    public string? Url { get; init; }
    public bool? IsBundleMod { get; init; } = false;
    public string License { get; init; } = "MIT";
    public bool HasPrepatcher { get; init; }
}
