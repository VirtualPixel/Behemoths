using Behemoths.Configuration;
using Behemoths.Services;
using HarmonyLib;
using UnityEngine;

namespace Behemoths.Patches
{
    /// <summary>
    /// Boss attacks hit harder. HurtOther is the master-side fan-out monsters use to
    /// damage players (it carries the attacking monster's index and RPCs the result to
    /// the victim), so multiplying the damage here reaches every victim, even ones
    /// without the mod. Patching this single chokepoint avoids double-counting the
    /// later Hurt the RPC triggers on the victim.
    /// </summary>
    [HarmonyPatch(typeof(PlayerHealth), nameof(PlayerHealth.HurtOther))]
    internal static class PlayerHurtPatch
    {
        [HarmonyPrefix]
        public static void Prefix(ref int damage, int enemyIndex)
        {
            if (!SemiFunc.IsMasterClientOrSingleplayer()) return;
            if (!BossRoundService.IsBossLevel) return;
            if (enemyIndex < 0 || damage <= 0) return;

            float mult = PluginConfig.BossDamageMultiplier.Value;
            if (mult <= 1f) return;

            Enemy enemy = SemiFunc.EnemyGetFromIndex(enemyIndex);
            if (enemy == null || !BossRoundService.IsBoss(enemy)) return;

            damage = Mathf.Max(1, Mathf.RoundToInt(damage * mult));
        }
    }
}
