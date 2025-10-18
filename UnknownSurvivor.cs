using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.DI;
using SPTarkov.Server.Core.Helpers;
using SPTarkov.Server.Core.Models.Eft.Common.Tables;
using SPTarkov.Server.Core.Models.Spt.Config;
using SPTarkov.Server.Core.Models.Spt.Mod;
using SPTarkov.Server.Core.Routers;
using SPTarkov.Server.Core.Utils;
using System.Reflection;
using SPTarkov.Server.Core.Servers;
using Path = System.IO.Path;
using Range = SemanticVersioning.Range;

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

[Injectable(TypePriority = OnLoadOrder.PostDBModLoader + 1)]
public class AddTraderWithAssortJson(
    ModHelper modHelper,
    ImageRouter imageRouter,
    ConfigServer configServer,
    TimeUtil timeUtil,
    UnknownSurvivorAssortJsonHelper addCustomTraderHelper,
    WTTServerCommonLib.WTTServerCommonLib commonLib
)
    : IOnLoad
{
    private readonly TraderConfig _traderConfig = configServer.GetConfig<TraderConfig>();
    private readonly RagfairConfig _ragfairConfig = configServer.GetConfig<RagfairConfig>();

    public Task OnLoad()
    {
        var pathToMod = modHelper.GetAbsolutePathToModFolder(Assembly.GetExecutingAssembly());
        bool debugLogging = true;

        
        commonLib.CustomItemServiceExtended.CreateCustomItems(Assembly.GetExecutingAssembly());
        commonLib.CustomQuestService.CreateCustomQuests(Assembly.GetExecutingAssembly());
        commonLib.CustomLootspawnService.CreateCustomLootSpawns(Assembly.GetExecutingAssembly());
        commonLib.CustomLocaleService.CreateCustomLocales(Assembly.GetExecutingAssembly());
        commonLib.CustomQuestZoneService.CreateCustomQuestZones(Assembly.GetExecutingAssembly());

        
        var traderImagePath = Path.Combine(pathToMod, "res/unknownsurvivor.jpg");
        var traderBase = modHelper.GetJsonDataFromFile<TraderBase>(pathToMod, "db/base.json");

        imageRouter.AddRoute(traderBase.Avatar.Replace(".jpg", ""), traderImagePath);
        addCustomTraderHelper.SetTraderUpdateTime(_traderConfig, traderBase, timeUtil.GetHoursAsSeconds(1), timeUtil.GetHoursAsSeconds(2));

        _ragfairConfig.Traders.TryAdd(traderBase.Id, true);
        addCustomTraderHelper.AddTraderWithEmptyAssortToDb(traderBase);
        addCustomTraderHelper.AddTraderToLocales(traderBase, "Survivor", "Ex-Handler...");

        
        var assort = modHelper.GetJsonDataFromFile<TraderAssort>(pathToMod, "db/assort.json");
        addCustomTraderHelper.OverwriteTraderAssort(traderBase.Id, assort);

        if (debugLogging)
        {
            Console.WriteLine("[DEBUG] Unknown Survivor trader successfully initialized.");
        }

        return Task.CompletedTask;
    }
}
