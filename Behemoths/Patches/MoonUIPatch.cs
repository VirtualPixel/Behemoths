using Behemoths.Configuration;
using Behemoths.Services;
using HarmonyLib;

namespace Behemoths.Patches
{
    /// <summary>
    /// On a boss level the moon-phase screen never runs: the BOSS LEVEL popup takes its
    /// slot, and the moon flag is consumed here so the game does not queue its own after.
    /// </summary>
    [HarmonyPatch(typeof(MoonUI), "Check")]
    internal static class MoonUIPatch
    {
        [HarmonyPrefix]
        public static bool Prefix()
        {
            if (!BossRoundService.IsBossLevel || !PluginConfig.AnnounceBossLevel.Value) return true;
            var rm = RunManager.instance;
            if (rm != null && rm.moonLevelChanged)
            {
                rm.moonLevelChanged = false;
                Plugin.LogInfo("[Announce] moon phase screen skipped, boss level");
            }
            return false;
        }
    }
}
