using SPTarkov.Server.Core.Models.Spt.Mod;
using Range = SemanticVersioning.Range;
using Version = SemanticVersioning.Version;

namespace UnknownSurvivor;

public record ModMetadata : AbstractModMetadata
{
    public override string ModGuid { get; init; } = "com.dsnyder.unknownsurvivor";
    public override string Name { get; init; } = "Unknown Survivor";
    public override string Author { get; init; } = "Dsnyder";
    public override List<string>? Contributors { get; init; } = ["Dsnyder"];
    public override SemanticVersioning.Version Version { get; init; } = new("2.0.0");
    public override SemanticVersioning.Range SptVersion { get; init; } = new("~4.0.0");
    public override List<string>? Incompatibilities { get; init; } = [""];
    public override Dictionary<string, SemanticVersioning.Range>? ModDependencies { get; init; }
    public override string? Url { get; init; } = "https://github.com/Doup22/SPT-UnknownSurvivor-4.0-";
    public override bool? IsBundleMod { get; init; } = true;
    public override string? License { get; init; } = "MIT";
}