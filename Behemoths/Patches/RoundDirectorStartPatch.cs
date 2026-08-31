using Behemoths.Services;
using HarmonyLib;

namespace Behemoths.Patches
{
    /// <summary>
    /// StartRoundLogic runs once on the host (and in singleplayer) right after the
    /// extraction goal is set from the level's original total value. Boosting the
    /// placed valuables here means the bigger payout is pure profit: it doesn't feed
    /// the spawn-value budget that decides how many valuables appear, and it doesn't
    /// raise the goal you have to hit.
    /// </summary>
    [HarmonyPatch(typeof(RoundDirector), "StartRoundLogic")]
    internal static class RoundDirectorStartPatch
    {
        [HarmonyPostfix]
        public static void Postfix()
        {
            if (!SemiFunc.IsMasterClientOrSingleplayer()) return;
            BossRoundService.BoostLevelValuables();
        }
    }
}
