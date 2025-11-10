using BepInEx;
using HarmonyLib;

namespace SurvivorQuestExtension
{
    [BepInPlugin("com.yourname.SurvivorQuestExtension", "Survivor Quest Extension", "1.0.0")]
    public class Plugin : BaseUnityPlugin
    {
        void Awake()
        {
            try
            {
                var harmony = new Harmony("com.yourname.SurvivorQuestExtension");
                harmony.PatchAll();
                Logger.LogInfo("Survivor Quest Extension loaded successfully!");
            }
            catch (System.Exception ex)
            {
                Logger.LogError($"Failed to load: {ex}");
            }
        }
    }
}