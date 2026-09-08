using Behemoths.Services;
using HarmonyLib;
using Photon.Pun;
using UnityEngine;

namespace Behemoths.Patches
{
    /// <summary>
    /// Boss attacks hit harder, part three of three: other players. A monster's
    /// HurtCollider runs on every machine, but PlayerHurt only applies damage to the
    /// avatar that machine owns; for anyone else it bails out. The host still sees the
    /// hit land, so on a boss hit against a remote player it checks the collider's own
    /// per-player cooldown, the same gate the victim's machine uses, and sends just the
    /// extra damage to that player through the game's HurtOtherRPC. The victim's own
    /// game applies the vanilla part as usual, mod or no mod.
    /// </summary>
    [HarmonyPatch(typeof(HurtCollider), "PlayerHurt")]
    internal static class HurtColliderPlayerHurtPatch
    {
        [HarmonyPrefix]
        public static void Prefix(HurtCollider __instance, PlayerAvatar _player)
        {
            if (!GameManager.Multiplayer() || !PhotonNetwork.IsMasterClient) return;
            if (!BossRoundService.IsBossLevel) return;
            if (_player == null || _player.photonView.IsMine) return;
            if (!__instance.playerLogic || __instance.detectionOnly || __instance.playerKill) return;
            if (__instance.ignorePlayers.Contains(_player)) return;

            Enemy enemy = __instance.enemyHost;
            if (enemy == null || !BossRoundService.IsBoss(enemy)) return;

            int vanilla = __instance.playerDamage;
            if (vanilla <= 0) return;

            var health = _player.playerHealth;
            if (health == null || health.health <= 0 || _player.deadSet) return;

            int extra = BossRoundService.BoostHit(vanilla, health.maxHealth) - vanilla;
            if (extra <= 0) return;

            // Same cooldown and wall check the victim's own machine runs, so the extra
            // damage lands once per hit, not once per frame of contact.
            Vector3 target = _player.PlayerVisionTarget.VisionTransform.position;
            if (!__instance.CanHit(_player.gameObject, __instance.playerDamageCooldown, __instance.playerRayCast, target, HurtCollider.HitType.Player))
                return;

            int enemyIndex = SemiFunc.EnemyGetIndex(enemy);
            Plugin.LogInfo($"[Hit] {enemy.name} hurt {_player.playerName} +{extra} on top of {vanilla}");
            // Straight to the RPC rather than HurtOther, so the extra is not multiplied
            // again on the way out. A zero position skips the RPC's distance check.
            health.photonView.RPC("HurtOtherRPC", _player.photonView.Owner, extra, Vector3.zero, true, enemyIndex, false);
        }
    }
}
