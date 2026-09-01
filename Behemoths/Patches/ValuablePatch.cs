using Behemoths.Configuration;
using Behemoths.Services;
using HarmonyLib;
using ScalerCore;
using UnityEngine;

namespace Behemoths.Patches
{
    /// <summary>
    /// A boss's dropped orb (it carries an EnemyValuable) is worth more, bigger, and
    /// far heavier than a normal one. This fires on the orb's own value setup; orbs
    /// spawn mid-level from a kill, so unlike placed valuables they aren't part of the
    /// level's spawn-value budget and can be inflated here directly.
    ///
    /// Every other valuable in a boss level is handled by RoundDirector.StartRoundLogic,
    /// after the extraction goal is locked, so boosting them stays pure profit. One that
    /// sets its value after that sweep gets the same multiplier here.
    /// </summary>
    [HarmonyPatch(typeof(ValuableObject), nameof(ValuableObject.DollarValueSetLogic))]
    internal static class ValuablePatch
    {
        // Only act on the call that actually sets the value, never a later no-op call.
        [HarmonyPrefix]
        public static void Prefix(ValuableObject __instance, out bool __state)
        {
            __state = __instance.dollarValueSet;
        }

        [HarmonyPostfix]
        public static void Postfix(ValuableObject __instance, bool __state)
        {
            if (__state) return; // value was already set before this call
            if (!SemiFunc.IsMasterClientOrSingleplayer()) return;
            if (!SemiFunc.RunIsLevel()) return;
            if (!BossRoundService.IsBossLevel) return;
            if (!BossRoundService.IsEnemyOrb(__instance))
            {
                BossRoundService.BoostLateValuable(__instance);
                return;
            }

            float mult = PluginConfig.OrbValueMultiplier.Value;
            if (mult != 1f)
            {
                float value = Mathf.Round(__instance.dollarValueCurrent * mult);
                BossRoundService.SetValue(__instance, value);
                Plugin.LogVerbose($"[Loot] orb value x{mult} -> {value}");
            }

            GrowOrb(__instance);
            BossRoundService.TintOrb(__instance);
        }

        private static void GrowOrb(ValuableObject valuable)
        {
            var pgo = valuable.physGrabObject;
            if (pgo == null) return;

            var rb = pgo.GetComponent<Rigidbody>();
            float baseMass = rb != null ? rb.mass : 0f;

            // A big, heavy orb can tunnel through the floor. Continuous detection against
            // the static floor stops it (also set at spawn in EnemyValuableSpawnPatch).
            if (rb != null && !rb.isKinematic)
                rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;

            float size = PluginConfig.OrbSizeMultiplier.Value;
            if (size > 1f)
            {
                ScaleOptions options = ScaleOptions.Growth;
                options.Factor = size;
                options.AllowedTargets = ScaleTargets.Valuables;
                options.RejectExternalApply = true;     // shrink rays can't shrink a boss orb
                options.IgnoreBonkExpand = true;        // bumps and hits don't restore its size
                options.SuppressValueDropExpand = true; // losing value doesn't restore its size
                ScaleManager.ApplyIfNotScaled(pgo.gameObject, options);

                // Growing the collider in place can leave it embedded in the floor, which
                // the solver resolves by flinging it through. Lift it clear so it settles
                // on top instead.
                if (rb != null)
                    rb.position += Vector3.up * 0.5f * (size - 1f);
            }

            // Weight is set apart from size: ScalerCore couples mass to the scale factor,
            // so we override the rigidbody mass (and the mass the game resets back to)
            // to get a heavy orb without making it any bigger.
            float weight = PluginConfig.OrbWeightMultiplier.Value;
            if (weight > 1f && rb != null && baseMass > 0f)
            {
                float mass = baseMass * weight;
                rb.mass = mass;
                pgo.massOriginal = mass;
                Plugin.LogVerbose($"[Loot] boss orb size x{size}, weight x{weight} ({mass:F1})");
            }

            // Durability: lower the break-force scaler so the orb shrugs off knocks and
            // keeps its value. fragilityMultiplier scales the impact that registers as a
            // break, so 1/durability means it takes that many times the abuse to chip.
            float durability = PluginConfig.OrbDurability.Value;
            if (durability > 1f)
            {
                var det = pgo.GetComponentInChildren<PhysGrabObjectImpactDetector>();
                if (det != null) det.fragilityMultiplier = 1f / durability;
            }
        }
    }
}
