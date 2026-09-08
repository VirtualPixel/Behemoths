using Behemoths.Services;
using HarmonyLib;

namespace Behemoths.Patches
{
    /// <summary>
    /// Boss attacks hit harder, part two of three: the host's own avatar. Nearly every
    /// monster attack is a HurtCollider that damages the player it touches on that
    /// player's own machine, through Hurt. On the host that player is the host, so the
    /// multiplier goes on here. Hurt already ignores calls for avatars it does not own,
    /// and every other path (HurtOther in singleplayer, the RPC when the host is the
    /// victim) ends in this one call, so nothing is boosted twice.
    /// </summary>
    [HarmonyPatch(typeof(PlayerHealth), nameof(PlayerHealth.Hurt))]
    internal static class PlayerHealthHurtPatch
    {
        [HarmonyPrefix]
        public static void Prefix(PlayerHealth __instance, ref int damage, int enemyIndex)
        {
            if (!SemiFunc.IsMasterClientOrSingleplayer()) return;
            if (!BossRoundService.IsBossLevel) return;
            if (enemyIndex < 0 || damage <= 0) return;
            if (GameManager.Multiplayer() && !__instance.photonView.IsMine) return;

            Enemy enemy = SemiFunc.EnemyGetFromIndex(enemyIndex);
            if (enemy == null || !BossRoundService.IsBoss(enemy)) return;

            int boosted = BossRoundService.BoostHit(damage, __instance.maxHealth);
            Plugin.LogInfo($"[Hit] {enemy.name} hurt host {damage} -> {boosted}");
            damage = boosted;
        }
    }
}
