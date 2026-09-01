using System.Collections.Generic;
using Behemoths.Configuration;
using Photon.Pun;
using ScalerCore;
using UnityEngine;

namespace Behemoths.Services
{
    /// <summary>
    /// Host-side brain for boss levels. Decides once per level whether it is a boss
    /// level (climbing chance plus a cooldown), turns its monsters into bosses, and
    /// tracks which monsters are bosses so the damage patch can find them.
    ///
    /// Everything here runs on the master client only. ScalerCore syncs the visual
    /// scaling down to other ScalerCore clients; the cash and damage changes ride
    /// vanilla RPCs that reach every player, modded or not.
    /// </summary>
    internal static class BossRoundService
    {
        /// <summary>True while the current level is a boss level.</summary>
        public static bool IsBossLevel { get; private set; }

        // Levels completed since the last boss level. Drives the cooldown.
        private static int _levelsSinceBoss = int.MaxValue;

        // The monsters promoted to bosses this level (for the damage patch).
        private static readonly HashSet<Enemy> _bosses = new();
        // Health is boosted once per monster; scaling is re-applied on every spawn
        // because ScalerCore restores enemy scale when a monster despawns.
        private static readonly HashSet<EnemyParent> _healthBoosted = new();
        /// <summary>
        /// True once the placed valuables of this boss level have been boosted. A valuable
        /// that sets its value after that point is boosted at set time instead.
        /// </summary>
        public static bool LootBoosted { get; private set; }

        /// <summary>
        /// Clear all run state, including the cooldown. Called when a new run starts so a
        /// fresh game does not inherit the previous run's cooldown (these fields are static
        /// and outlive a single run within the same session).
        /// </summary>
        public static void ResetRun()
        {
            _levelsSinceBoss = int.MaxValue;
            IsBossLevel = false;
            _bosses.Clear();
            _healthBoosted.Clear();
            LootBoosted = false;
            Plugin.LogInfo("[Decide] new run, cooldown reset");
        }

        /// <summary>
        /// Roll the current level. Called once per gameplay level from the
        /// EnemyDirector.Awake patch. Resets all per-level state first.
        /// </summary>
        public static void DecideForLevel()
        {
            // Clear first, so a boss level never leaks its state into the next scene
            // (e.g. the arena) even when that scene never gets to roll.
            IsBossLevel = false;
            _bosses.Clear();
            _healthBoosted.Clear();
            LootBoosted = false;

            if (!SemiFunc.RunIsLevel()) return;

            if (!PluginConfig.Enabled.Value)
            {
                Plugin.LogAlways("[Decide] mod disabled, normal level");
                return;
            }

            int completed = RunManager.instance != null ? RunManager.instance.levelsCompleted : 0;
            int levelNumber = completed + 1; // levelsCompleted is 0 on the first level

            if (levelNumber < PluginConfig.EarliestLevel.Value)
            {
                _levelsSinceBoss++;
                Plugin.LogAlways($"[Decide] level {levelNumber} below earliest ({PluginConfig.EarliestLevel.Value}), normal level");
                return;
            }

            if (_levelsSinceBoss < PluginConfig.CooldownLevels.Value)
            {
                _levelsSinceBoss++;
                Plugin.LogAlways($"[Decide] level {levelNumber} on cooldown ({_levelsSinceBoss}/{PluginConfig.CooldownLevels.Value}), normal level");
                return;
            }

            // BaseChance is always a floor: it never gets capped below itself, so setting
            // it to 100 guarantees a boss every eligible level even if MaxChance is lower.
            float ceiling = Mathf.Max(PluginConfig.BaseChance.Value, PluginConfig.MaxChance.Value);
            float chance = Mathf.Min(ceiling, PluginConfig.BaseChance.Value + PluginConfig.ChancePerLevel.Value * completed);
            float roll = Random.Range(0f, 100f);

            if (roll < chance)
            {
                IsBossLevel = true;
                _levelsSinceBoss = 0;
                Plugin.LogAlways($"[Decide] level {levelNumber} is a BOSS LEVEL (rolled {roll:F1} < {chance:F1})");
                BossAnnouncer.Instance?.Trigger();
            }
            else
            {
                _levelsSinceBoss++;
                Plugin.LogAlways($"[Decide] level {levelNumber} normal (rolled {roll:F1} >= {chance:F1})");
            }
        }

        /// <summary>
        /// Make a monster a boss. Scaling is re-applied every spawn (a despawn restores
        /// the monster to normal size), while health is boosted only the first time so
        /// it never compounds across respawns.
        /// </summary>
        public static void Promote(EnemyParent parent)
        {
            if (!IsBossLevel || parent == null) return;

            Enemy? enemy = parent.Enemy;
            if (enemy == null) return;

            ApplyScale(enemy);
            ApplyResistance(enemy);

            if (_healthBoosted.Add(parent))
            {
                ApplyHealth(enemy);
                Plugin.LogInfo($"[Promote] {parent.enemyName} is now a boss");
            }

            _bosses.Add(enemy);
        }

        /// <summary>True if this monster was promoted to a boss this level.</summary>
        public static bool IsBoss(Enemy enemy) => enemy != null && _bosses.Contains(enemy);

        /// <summary>
        /// Boost the cash value of every placed valuable in a boss level. Runs after the
        /// extraction goal is locked, so it is pure profit: it does not feed the value
        /// budget that decides how many valuables spawn, nor the goal you have to hit.
        /// Boss orbs are handled separately, on spawn.
        /// </summary>
        public static void BoostLevelValuables()
        {
            if (!IsBossLevel || LootBoosted) return;
            LootBoosted = true;

            float mult = PluginConfig.ValuableValueMultiplier.Value;
            if (mult == 1f) return;

            int boosted = 0;
            float added = 0f;
            foreach (var valuable in Object.FindObjectsOfType<ValuableObject>())
            {
                if (valuable == null || IsEnemyOrb(valuable)) continue;
                float before = valuable.dollarValueCurrent;
                float after = Mathf.Round(before * mult);
                SetValue(valuable, after);
                added += after - before;
                boosted++;
            }

            // Fold the extra value into the level's running total so map-value trackers
            // and the truck reflect the real haul. The extraction goal was already locked
            // in StartRoundLogic before this runs, so the goal itself stays put.
            if (added != 0f && RoundDirector.instance != null)
                RoundDirector.instance.haulGoalMax += (int)added;

            Plugin.LogAlways($"[Loot] boosted {boosted} valuable(s) x{mult} (+{added:F0})");
        }

        /// <summary>
        /// Set a valuable's value and sync it to other players. Mirrors the game's own
        /// DollarValueSet path so the truck total and extraction tracking stay correct.
        /// </summary>
        public static void SetValue(ValuableObject valuable, float value)
        {
            // Route through DollarValueSetRPC (locally too) so value-tracking mods that
            // hook it see the new price on the host, not just on remote clients. The
            // game's own RPC sets the fields; in singleplayer we call it directly.
            if (SemiFunc.IsMultiplayer())
            {
                if (valuable.photonView != null && valuable.photonView.ViewID != 0)
                    valuable.photonView.RPC("DollarValueSetRPC", RpcTarget.All, value);
            }
            else
            {
                valuable.DollarValueSetRPC(value, default);
            }
        }

        public static bool IsEnemyOrb(ValuableObject valuable) =>
            valuable.GetComponentInParent<EnemyValuable>() != null
            || valuable.GetComponentInChildren<EnemyValuable>() != null;

        // Amber so the big prizes read at a glance.
        private static readonly Color OrbColor = new(1f, 0.55f, 0.05f);

        /// <summary>Tint a boss orb's fresnel glow so it stands out. Host-local visual.</summary>
        public static void TintOrb(ValuableObject valuable)
        {
            if (!PluginConfig.OrbGlow.Value) return;
            var ev = valuable.GetComponentInParent<EnemyValuable>()
                  ?? valuable.GetComponentInChildren<EnemyValuable>();
            if (ev == null) return;

            // Set both the default and indestructible colors so the startup lerp lands
            // on our color instead of fading back to the vanilla glow.
            ev.fresnelColorDefault = OrbColor;
            ev.fresnelColorIndestructible = OrbColor;
            if (ev.outerMaterial != null)
                ev.outerMaterial.SetColor("_FresnelColor", OrbColor);
        }

        private static void ApplyScale(Enemy enemy)
        {
            if (!enemy.HasRigidbody || enemy.Rigidbody == null) return;

            float factor = PluginConfig.BossSizeMultiplier.Value;
            if (factor <= 1f) return;

            ScaleOptions options = ScaleOptions.Growth;
            options.Factor = factor;
            options.AllowedTargets = ScaleTargets.Enemies;
            // The mesh scales to Factor; the collider and nav agent stop at ColliderCap.
            // This is ScalerCore's grow-gun behaviour: the cap sits close to the visual so
            // the body stays grounded (a far-smaller collider sinks the mesh) while still
            // fitting the doorways the navmesh was baked for.
            options.EnemyPhysicalFactorCap = PluginConfig.BossColliderCap.Value;
            options.EnemyWidthFactorCap = 0f;
            options.EnemyHeightFactorCap = 0f;
            options.EnemyNavRadiusFactorCap = 0f;
            // A boss stays a boss: a shrink ray can't shrink it and taking a hit
            // doesn't snap it back to normal size.
            options.RejectExternalApply = true;
            options.IgnoreBonkExpand = true;

            // A despawn restores the scale (ScaleController.OnDisable), so a respawn needs
            // a fresh apply. If the controller is somehow still scaled, leave it: a second
            // apply at the same factor is ScalerCore's toggle and would shrink the boss.
            ScaleManager.ApplyIfNotScaled(enemy.Rigidbody.gameObject, options);
        }

        private static void ApplyResistance(Enemy enemy)
        {
            if (!enemy.HasHealth || enemy.Health == null) return;

            float resist = PluginConfig.BossDamageResistance.Value;
            if (resist <= 0f) return;

            // Long timer because the game zeroes resistance once it lapses; re-applied
            // every spawn, so a respawned boss stays tough.
            enemy.Health.OverrideDamageResistance(Mathf.Clamp(resist, 0f, 0.95f), 1e9f);
        }

        private static void ApplyHealth(Enemy enemy)
        {
            if (!enemy.HasHealth || enemy.Health == null) return;

            float mult = PluginConfig.BossHealthMultiplier.Value;
            if (mult <= 1f) return;

            int newMax = Mathf.Max(1, Mathf.RoundToInt(enemy.Health.health * mult));
            enemy.Health.health = newMax;
            // OnSpawn ran just before this (sets healthCurrent = health), so lift the
            // live pool to match the new maximum.
            enemy.Health.healthCurrent = newMax;
            Plugin.LogVerbose($"[Promote] health -> {newMax}");
        }
    }
}
