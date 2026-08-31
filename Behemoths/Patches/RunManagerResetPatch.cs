using Behemoths.Services;
using HarmonyLib;

namespace Behemoths.Patches
{
    /// <summary>
    /// ResetProgress runs when a fresh run begins (it zeroes levelsCompleted). The boss
    /// cooldown lives in static fields that survive returning to the menu, so reset it here
    /// or a new game would inherit the previous run's cooldown.
    /// </summary>
    [HarmonyPatch(typeof(RunManager), nameof(RunManager.ResetProgress))]
    internal static class RunManagerResetPatch
    {
        [HarmonyPostfix]
        public static void Postfix()
        {
            BossRoundService.ResetRun();
        }
    }
}
