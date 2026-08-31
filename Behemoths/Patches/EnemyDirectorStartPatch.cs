using Behemoths.Services;
using HarmonyLib;

namespace Behemoths.Patches
{
    /// <summary>
    /// EnemyDirector is rebuilt once per level, so its Awake is our "new level began"
    /// signal. Awake runs before any Update, so the boss-level decision is made before
    /// the arrival screen reads it (and before monsters spawn).
    /// </summary>
    [HarmonyPatch(typeof(EnemyDirector), "Awake")]
    internal static class EnemyDirectorStartPatch
    {
        [HarmonyPostfix]
        public static void Postfix()
        {
            if (!SemiFunc.IsMasterClientOrSingleplayer()) return;
            BossRoundService.DecideForLevel();
        }
    }
}
