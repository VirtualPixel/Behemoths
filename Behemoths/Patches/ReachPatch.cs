using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using Behemoths.Configuration;
using Behemoths.Services;
using HarmonyLib;
using UnityEngine;

namespace Behemoths.Patches
{
    // Every melee monster decides "close enough to swing" with a metre count written into its
    // state code, measured from its own centre. A body two and a half times wider never lets a
    // player get that close, so a boss would stand there swinging at air. Each of those numbers
    // gets multiplied by the boss's size right where the game reads it.
    [HarmonyPatch]
    internal static class ReachPatch
    {
        private static readonly (string Type, string Method, float[] Metres)[] Targets =
        {
            ("EnemyRobe", "StateTargetPlayer", new[] { 2f }),
            ("EnemyRobe", "StateLookUnder", new[] { 2.5f }),
            ("EnemyAnimal", "StateGoToPlayer", new[] { 3f }),
            ("EnemyFloater", "StateNotice", new[] { 2.5f }),
            ("EnemySlowWalker", "StateLookUnder", new[] { 3f }),
            ("EnemyUpscream", "StateGoToPlayer", new[] { 1.5f }),
            ("EnemyValuableThrower", "StateTargetPlayer", new[] { 3f }),
        };

        private static readonly Dictionary<MonoBehaviour, Enemy?> owners = new();

        private static IEnumerable<MethodBase> TargetMethods()
        {
            foreach ((string type, string method, float[] _) in Targets)
            {
                MethodBase? m = AccessTools.Method(AccessTools.TypeByName(type), method);
                if (m != null)
                    yield return m;
                else
                    Plugin.LogAlways($"[Reach] {type}.{method} is not in this build, its reach stays vanilla");
            }
        }

        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, MethodBase original)
        {
            float[] metres = Targets.First(t => t.Type == original.DeclaringType!.Name && t.Method == original.Name).Metres;
            MethodInfo scale = AccessTools.Method(typeof(ReachPatch), nameof(Scale));
            int swapped = 0;
            foreach (CodeInstruction code in instructions)
            {
                yield return code;
                if (code.opcode == OpCodes.Ldc_R4 && code.operand is float f && metres.Any(m => Mathf.Approximately(m, f)))
                {
                    yield return new CodeInstruction(OpCodes.Ldarg_0);
                    yield return new CodeInstruction(OpCodes.Call, scale);
                    swapped++;
                }
            }
            Plugin.LogInfo($"[Reach] {original.DeclaringType!.Name}.{original.Name}: {swapped} distance(s) now follow boss size");
        }

        public static float Scale(float metres, MonoBehaviour self)
        {
            if (!BossRoundService.IsBossLevel || self == null)
                return metres;
            if (!owners.TryGetValue(self, out Enemy? enemy))
            {
                enemy = self.GetComponent<Enemy>() ?? self.GetComponentInParent<Enemy>();
                owners[self] = enemy;
            }
            if (enemy == null || !BossRoundService.IsBoss(enemy))
                return metres;
            float factor = PluginConfig.BossSizeMultiplier.Value;
            float cap = PluginConfig.BossColliderCap.Value;
            if (cap > 0f)
                factor = Mathf.Min(factor, cap);
            return metres * Mathf.Max(1f, factor);
        }

        internal static void Forget() => owners.Clear();
    }
}
