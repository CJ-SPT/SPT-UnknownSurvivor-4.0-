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
using Path = System.IO.Path;



namespace UnknownSurvivor;


public record ModMetadata : AbstractModMetadata
{
    public override string ModGuid { get; init; } = "com.dsnyder.unknownsurvivor";
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


[Injectable(TypePriority = OnLoadOrder.PostDBModLoader + 1)]
public class AddTraderWithAssortJson(
    ModHelper modHelper,
    ImageRouter imageRouter,
    ConfigServer configServer,
    TimeUtil timeUtil,
    UnknownSurvivorAssortJsonHelper addCustomTraderHelper,
    DatabaseServer databaseServer,
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
    commonLib.CustomLootspawnService.AddCustomLootSpawns(Assembly.GetExecutingAssembly());

    
    var tables = databaseServer.GetTables();
    var questFiles = Directory.GetFiles(Path.Combine(pathToMod, "db/quests"), "*.json");
    
    foreach (var questFile in questFiles)
    {
        var quest = modHelper.GetJsonDataFromFile<Quest>(pathToMod, $"db/quests/{Path.GetFileName(questFile)}");
        tables.Templates.Quests[quest.Id] = quest;

        if (debugLogging)
        {
            Console.WriteLine($"[DEBUG] Loaded quest: {quest.Id}");
        }
    }

    
    var questImagesFolder = Path.Combine(pathToMod, "db/images");
    int questImageCount = 0;

    if (Directory.Exists(questImagesFolder))
    {
        foreach (var filePath in Directory.GetFiles(questImagesFolder))
        {
            var ext = Path.GetExtension(filePath).ToLower();
            if (ext == ".png" || ext == ".jpg")
            {
                var fileName = Path.GetFileNameWithoutExtension(filePath);
                var route = $"/files/quest/icon/{fileName}";

                imageRouter.AddRoute(route, filePath);

                if (debugLogging)
                {
                    Console.WriteLine($"[DEBUG] Registered quest image: {fileName}{ext}");
                }

                questImageCount++;
            }
        }
    }

    if (debugLogging)
    {
        Console.WriteLine($"[DEBUG] Total quest images loaded: {questImageCount}");
    }

    
    var questLocales = modHelper.GetJsonDataFromFile<Dictionary<string, string>>(
        pathToMod,
        "db/locales/questlocales.json"
    );

    if (tables.Locales.Global.TryGetValue("en", out var lazyLoadedValue))
    {
        lazyLoadedValue.AddTransformer(localesDb =>
        {
            foreach (var kvp in questLocales)
            {
                localesDb[kvp.Key] = kvp.Value;
            }

            return localesDb;
        });
    }
    
    var traderImagePath = Path.Combine(pathToMod, "res/unknownsurvivor.jpg");
    var traderBase = modHelper.GetJsonDataFromFile<TraderBase>(pathToMod, "db/base.json");

    imageRouter.AddRoute(traderBase.Avatar.Replace(".jpg", ""), traderImagePath);
    addCustomTraderHelper.SetTraderUpdateTime(_traderConfig, traderBase, timeUtil.GetHoursAsSeconds(1),
        timeUtil.GetHoursAsSeconds(2));

    _ragfairConfig.Traders.TryAdd(traderBase.Id, true);
    addCustomTraderHelper.AddTraderWithEmptyAssortToDb(traderBase);
    addCustomTraderHelper.AddTraderToLocales(traderBase, "Survivor", "Ex-Handler...");

    var assort = modHelper.GetJsonDataFromFile<TraderAssort>(pathToMod, "db/assort.json");
    addCustomTraderHelper.OverwriteTraderAssort(traderBase.Id, assort);

    return Task.CompletedTask;
}

}