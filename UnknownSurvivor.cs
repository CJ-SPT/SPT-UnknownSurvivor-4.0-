using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.DI;
using SPTarkov.Server.Core.Helpers;
using SPTarkov.Server.Core.Models.Eft.Common.Tables;
using SPTarkov.Server.Core.Models.Spt.Config;
using SPTarkov.Server.Core.Models.Spt.Mod;
using SPTarkov.Server.Core.Routers;
using SPTarkov.Server.Core.Servers;
using SPTarkov.Server.Core.Utils;
using System.Reflection;
using SPTarkov.Server.Core.Services;
using Path = System.IO.Path;
using WTTCommonLib;


namespace UnknownSurvivor;

// This record holds the various properties for your mod
public record ModMetadata : AbstractModMetadata
{
    public override string ModGuid { get; init; } = "4d8499d3-ac24-488e-b021-32317c60f23f";
    public override string Name { get; init; } = "Unknown Survivor";
    public override string Author { get; init; } = "Dsnyder";
    public override List<string>? Contributors { get; init; } = ["Dsnyder"];
    public override SemanticVersioning.Version Version { get; init; } = new("2.0.0");
    public override SemanticVersioning.Version SptVersion { get; init; } = new("4.0.0");
    public override List<string>? Incompatibilities { get; init; } = [""];
    public override Dictionary<string, SemanticVersioning.Version>? ModDependencies { get; init; }
    public override string? Url { get; init; } = "https://github.com/Doup22/SPT-UnknownSurvivor-4.0-";
    public override bool? IsBundleMod { get; init; } = true;
    public override string? License { get; init; } = "MIT";
}

/// <summary>
/// Feel free to use this as a base for your mod
/// </summary>
[Injectable(TypePriority = OnLoadOrder.PostDBModLoader + 1)]
public class AddTraderWithAssortJson(
    ModHelper modHelper,
    ImageRouter imageRouter,
    ConfigServer configServer,
    TimeUtil timeUtil,
    UnknownSurvivorAssortJsonHelper addCustomTraderHelper,
    DatabaseServer databaseServer,
    WTTCommonLib.WTTCommonLib commonLib

)
    : IOnLoad
{
    private readonly TraderConfig _traderConfig = configServer.GetConfig<TraderConfig>();
    private readonly RagfairConfig _ragfairConfig = configServer.GetConfig<RagfairConfig>();


    public Task OnLoad()
    {
        var pathToMod = modHelper.GetAbsolutePathToModFolder(Assembly.GetExecutingAssembly());
        
        commonLib.CustomCustomItemService.CreateCustomItems(Assembly.GetExecutingAssembly());

        var quest1 = modHelper.GetJsonDataFromFile<Quest>(pathToMod, "db/quests/quest.json");
        var quest2 = modHelper.GetJsonDataFromFile<Quest>(pathToMod, "db/quests/survivor_quest.json");

        databaseServer.GetTables().Templates.Quests[quest1.Id] = quest1;
        databaseServer.GetTables().Templates.Quests[quest2.Id] = quest2;

        var questLocales = modHelper.GetJsonDataFromFile<Dictionary<string, string>>(
            pathToMod,
            "db/locales/questlocales.json"
        );

        if (databaseServer.GetTables().Locales.Global.TryGetValue("en", out var lazyloadedValue))
        {
            lazyloadedValue.AddTransformer(localesDb =>
            {
                foreach (var kvp in questLocales)
                {
                    localesDb[kvp.Key] = kvp.Value;
                }

                return localesDb;
            });

            var traderImagePath = Path.Combine(pathToMod, "res/unknownsurvivor.jpg");
            var traderBase = modHelper.GetJsonDataFromFile<TraderBase>(pathToMod, "db/base.json");

            imageRouter.AddRoute(traderBase.Avatar.Replace(".jpg", ""), traderImagePath);
            addCustomTraderHelper.SetTraderUpdateTime(_traderConfig, traderBase, timeUtil.GetHoursAsSeconds(1),
                timeUtil.GetHoursAsSeconds(2));

            _ragfairConfig.Traders.TryAdd(traderBase.Id, true);
            addCustomTraderHelper.AddTraderWithEmptyAssortToDb(traderBase);

            addCustomTraderHelper.AddTraderToLocales(traderBase, "Survivor",
                "Ex-Handler. Scav Recruiter. Shadow Broker.\n\nNo one knows his real name. Some say he was a foreign agent, others thought that he was a scav boss who disappeared when the city burned. What’s certain is that he survived — and now he trades in more than just food and medicine.\n\nThe Survivor has dossiers, maps, and secrets on everyone. He whispers promises to desperate Scavs, binding them to his cause with rations, stims, and bandages. Rivals call him a traitor, a liar, a ghost in the system. He calls himself a builder of something greater.\n\nWork with him, and he’ll feed you, heal you, and maybe even protect you. Cross him, and you’ll discover he’s not just another trader — he’s a man with a plan, and you might just be in it.");

            var assort = modHelper.GetJsonDataFromFile<TraderAssort>(pathToMod, "db/assort.json");
            addCustomTraderHelper.OverwriteTraderAssort(traderBase.Id, assort);

            return Task.CompletedTask;
        }

        // fallback return if "en" locale is missing
        return Task.CompletedTask;
    }
}