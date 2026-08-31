using Behemoths.Services;
using HarmonyLib;
using UnityEngine;

namespace Behemoths.Patches
{
    /// <summary>
    /// A boss orb is big and heavy, so on a thin floor it can tunnel straight through with
    /// the default discrete collision check. Switch the orb to continuous collision the
    /// moment it spawns, before it has a chance to fall, on boss levels.
    /// </summary>
    [HarmonyPatch(typeof(EnemyValuable), "Start")]
    internal static class EnemyValuableSpawnPatch
    {
        [HarmonyPostfix]
        public static void Postfix(EnemyValuable __instance)
        {
            if (!SemiFunc.IsMasterClientOrSingleplayer()) return;
            if (!BossRoundService.IsBossLevel) return;

            var rb = __instance.GetComponentInChildren<Rigidbody>();
            if (rb != null && !rb.isKinematic)
                rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        }
    }
}
