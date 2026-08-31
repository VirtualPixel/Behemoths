using Behemoths.Services;
using HarmonyLib;

namespace Behemoths.Patches
{
    /// <summary>
    /// EnemyParent.SpawnRPC is the host-gated spawn handler (it runs Health.OnSpawn and
    /// Enemy.Spawn). Promoting here means a boss is fully scaled and hardened the moment
    /// it enters the level, and re-runs on respawn are guarded inside the service.
    /// </summary>
    [HarmonyPatch(typeof(EnemyParent), "SpawnRPC")]
    internal static class EnemySpawnPatch
    {
        [HarmonyPostfix]
        public static void Postfix(EnemyParent __instance)
        {
            if (!SemiFunc.IsMasterClientOrSingleplayer()) return;
            if (!SemiFunc.RunIsLevel()) return;
            BossRoundService.Promote(__instance);
        }
    }
}
