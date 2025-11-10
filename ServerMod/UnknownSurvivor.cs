using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.DI;
using SPTarkov.Server.Core.Helpers;
using SPTarkov.Server.Core.Models.Eft.Common.Tables;
using SPTarkov.Server.Core.Models.Spt.Config;
using SPTarkov.Server.Core.Models.Spt.Mod;
using SPTarkov.Server.Core.Routers;
using SPTarkov.Server.Core.Utils;
using System.Reflection;
using SPTarkov.Server.Core.Models.Eft.Common;
using SPTarkov.Server.Core.Servers;
using Path = System.IO.Path;


namespace UnknownSurvivor;

[Injectable(TypePriority = OnLoadOrder.PostDBModLoader + 2)]
public class AddTraderWithAssortJson(
    ModHelper modHelper,
    ImageRouter imageRouter,
    ConfigServer configServer,
    TimeUtil timeUtil,
    UnknownSurvivorAssortJsonHelper addCustomTraderHelper,
    WTTServerCommonLib.WTTServerCommonLib wttCommon
    
)
    : IOnLoad
{
    private readonly TraderConfig _traderConfig = configServer.GetConfig<TraderConfig>();
    private readonly RagfairConfig _ragfairConfig = configServer.GetConfig<RagfairConfig>();

    public async Task OnLoad()
    {
        var pathToMod = modHelper.GetAbsolutePathToModFolder(Assembly.GetExecutingAssembly());
        bool debugLogging = true;
        
        Assembly assembly = Assembly.GetExecutingAssembly();

        await wttCommon.CustomItemServiceExtended.CreateCustomItems(assembly);
        await wttCommon.CustomLootspawnService.CreateCustomLootSpawns(assembly);
        await wttCommon.CustomQuestZoneService.CreateCustomQuestZones(assembly);
        
        var traderImagePath = Path.Combine(pathToMod, "res/unknownsurvivor.jpg");
        var traderBase = modHelper.GetJsonDataFromFile<TraderBase>(pathToMod, "db/base.json");
        
        
        imageRouter.AddRoute(traderBase.Avatar.Replace(".jpg", ""), traderImagePath);
        addCustomTraderHelper.SetTraderUpdateTime(_traderConfig, traderBase, timeUtil.GetHoursAsSeconds(1), timeUtil.GetHoursAsSeconds(2));

        _ragfairConfig.Traders.TryAdd(traderBase.Id, true);
        addCustomTraderHelper.AddTraderWithEmptyAssortToDb(traderBase);
        addCustomTraderHelper.AddTraderToLocales(traderBase, "Survivor", "Ex-Handler...");
         
        await wttCommon.CustomQuestService.CreateCustomQuests(assembly);
        
        var assort = modHelper.GetJsonDataFromFile<TraderAssort>(pathToMod, "db/assort.json");
        addCustomTraderHelper.OverwriteTraderAssort(traderBase.Id, assort);

        if (debugLogging)
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("Survivor is eager to meet you!");
            Console.ResetColor();
        }

        await Task.CompletedTask;
    }
}
