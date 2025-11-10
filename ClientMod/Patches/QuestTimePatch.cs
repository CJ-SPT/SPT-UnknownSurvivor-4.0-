using Comfort.Common;
using EFT;
using EFT.Interactive;
using HarmonyLib;
using UnityEngine;

namespace UnknownSurvivor.Patches;

[HarmonyPatch(typeof(GetActionsClass), "smethod_3")]
public static class QuestTimePatch
{
        
    private static readonly Dictionary<string, (float from, float to)> QuestTimeWindows = new()
    {
        { "Survivor_MarkMeds_Supply_Quest", (20f, 5f) },
        { "NightOps_ExtractIntel", (22f, 5f) }
    };

    [HarmonyPrefix]
    public static bool Prefix(GamePlayerOwner owner, PlaceItemTrigger itemTrigger, ref ActionsReturnClass __result)
    {
            
        __result = new ActionsReturnClass();

            
        if (QuestTimeWindows.TryGetValue(itemTrigger.Id, out var window))
        {
            float currentHour = Singleton<AbstractGame>.Instance?.PastTime % 24f ?? 0f;

            bool inWindow = window.from < window.to
                ? currentHour >= window.from && currentHour <= window.to
                : currentHour >= window.from || currentHour <= window.to;

            if (!inWindow)
            {
                    
                __result.Actions.Clear();

                    
                NotificationManagerClass.DisplayWarningNotification(
                    $"You can only complete this quest between {window.from:00}:00 and {window.to:00}:00!"
                );

                Debug.Log($"[QuestTimePatch] Blocked action for '{itemTrigger.Id}' at {currentHour:0.0}h (Allowed {window.from}-{window.to})");

                    
                return false;
            }
        }
            
        return true;
    }
}
