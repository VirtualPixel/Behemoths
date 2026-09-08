using Behemoths.Services;
using HarmonyLib;

namespace Behemoths.Patches
{
    /// <summary>
    /// Boss attacks hit harder, part one of three: a handful of monsters (the Tick's bite,
    /// the Thin Man, the ceiling eye) damage players from the host through HurtOther,
    /// which RPCs the hit to the victim. Multiplying here reaches victims without the mod.
    /// Only for victims on other machines: a local victim's damage is boosted once, in
    /// Hurt, and in singleplayer HurtOther calls Hurt directly.
    /// </summary>
    [HarmonyPatch(typeof(PlayerHealth), nameof(PlayerHealth.HurtOther))]
    internal static class PlayerHurtPatch
    {
        [HarmonyPrefix]
        public static void Prefix(PlayerHealth __instance, ref int damage, int enemyIndex)
        {
            if (!GameManager.Multiplayer() || __instance.photonView.IsMine) return;
            if (!SemiFunc.IsMasterClientOrSingleplayer()) return;
            if (!BossRoundService.IsBossLevel) return;
            if (enemyIndex < 0 || damage <= 0) return;

            Enemy enemy = SemiFunc.EnemyGetFromIndex(enemyIndex);
            if (enemy == null || !BossRoundService.IsBoss(enemy)) return;

            int boosted = BossRoundService.BoostHit(damage, __instance.maxHealth);
            Plugin.LogInfo($"[Hit] {enemy.name} hurt other {damage} -> {boosted}");
            damage = boosted;
        }
    }
}
