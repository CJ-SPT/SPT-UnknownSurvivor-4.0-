using BepInEx;
using HarmonyLib;

namespace UnknownSurvivor;


[BepInPlugin("com.dsnyder.SurvivorQuestExtension", "Survivor Quest Extension", "1.0.0")]
public class Plugin : BaseUnityPlugin
{
    private void Awake()
    {
        try
        {
            var harmony = new Harmony("com.dsnyder.SurvivorQuestExtension");
            harmony.PatchAll();
            Logger.LogInfo("Survivor Quest Extension loaded successfully!");
        }
        catch (Exception ex)
        {
            Logger.LogError($"Failed to load: {ex}");
        }
    }
}