using System.Collections.Generic;
using Behemoths.Configuration;
using Photon.Pun;
using ScalerCore;
using UnityEngine;

namespace Behemoths.Services
{
    /// <summary>
    /// Host-side brain for boss levels. Decides once per level whether it is a boss
    /// level (climbing chance, map odds, a cooldown), turns its monsters into bosses,
    /// and tracks which monsters are bosses so the damage patches can find them.
    ///
    /// Everything here runs on the master client only. ScalerCore syncs the visual
    /// scaling down to other ScalerCore clients; the cash and damage changes ride
    /// vanilla RPCs that reach every player, modded or not.
    /// </summary>
    internal static class BossRoundService
    {
        /// <summary>
        /// Run stat holding the level number of the last boss level (0 = none yet). It
        /// lives in the game's own run stats, so it is saved with the run and cleared by
        /// the game when a new run starts.
        /// </summary>
        private const string LastBossStat = "behemothsLastBossLevel";

        /// <summary>True while the current level is a boss level.</summary>
        public static bool IsBossLevel { get; private set; }

        // The monsters promoted to bosses this level (for the damage patches).
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
        /// Roll the current level. Called once per gameplay level from the
        /// EnemyDirector.Awake patch. Resets all per-level state first.
        /// </summary>
        public static void DecideForLevel()
        {
            Patches.ReachPatch.Forget();
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

            var rm = RunManager.instance;
            int levelNumber = rm.levelsCompleted + 1; // levelsCompleted is 0 on the first level
            string map = BossOdds.MapName(rm.levelCurrent);

            if (levelNumber < PluginConfig.EarliestLevel.Value)
            {
                Plugin.LogAlways($"[Decide] level {levelNumber} ({map}) below earliest ({PluginConfig.EarliestLevel.Value}), normal level");
                return;
            }

            int lastBoss = LastBossLevel();
            int cooldown = PluginConfig.CooldownLevels.Value;
            if (lastBoss > 0 && levelNumber - lastBoss <= cooldown)
            {
                Plugin.LogAlways($"[Decide] level {levelNumber} ({map}) on cooldown ({levelNumber - lastBoss} since the boss on level {lastBoss}, cooldown {cooldown}), normal level");
                return;
            }

            float chance = BossOdds.ChanceFor(rm.levelCurrent);
            float roll = Random.Range(0f, 100f);

            if (roll < chance)
            {
                IsBossLevel = true;
                // Written straight into the dictionary: PunManager's stat setter reads a
                // field it only fills in Start, and this runs from an Awake in the same
                // scene load. Only the host decides, so nothing needs to reach clients.
                StatsManager.instance.runStats[LastBossStat] = levelNumber;
                Plugin.LogAlways($"[Decide] level {levelNumber} ({map}) is a BOSS LEVEL (rolled {roll:F1} < {chance:F1})");
                BossAnnouncer.Instance?.Trigger();
            }
            else
            {
                Plugin.LogAlways($"[Decide] level {levelNumber} ({map}) normal (rolled {roll:F1} >= {chance:F1})");
            }
        }

        private static int LastBossLevel()
        {
            var stats = StatsManager.instance;
            if (stats == null) return 0;
            return stats.runStats.TryGetValue(LastBossStat, out int level) ? level : 0;
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
                ApplyTremor(parent, enemy);
                Plugin.LogInfo($"[Promote] {parent.enemyName} is now a boss");
            }

            _bosses.Add(enemy);
        }

        /// <summary>True if this monster was promoted to a boss this level.</summary>
        public static bool IsBoss(Enemy enemy) => enemy != null && _bosses.Contains(enemy);

        /// <summary>
        /// A boss hit, after the damage multiplier and the fairness cap. The cap keeps a
        /// hit that was survivable at full health survivable at full health: a boosted hit
        /// never exceeds HitCap percent of the victim's max health unless the vanilla hit
        /// already did.
        /// </summary>
        public static int BoostHit(int damage, int maxHealth)
        {
            float mult = PluginConfig.BossDamageMultiplier.Value;
            if (damage <= 0 || mult <= 1f) return damage;

            int boosted = Mathf.RoundToInt(damage * mult);
            float capPercent = PluginConfig.BossHitCap.Value;
            if (capPercent < 100f)
            {
                int cap = Mathf.RoundToInt(maxHealth * capPercent / 100f);
                boosted = Mathf.Min(boosted, Mathf.Max(damage, cap));
            }
            return boosted;
        }

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
                // A valuable still waiting on its own value roll (its coroutine polls the
                // level state and the view id) has nothing to multiply yet. Writing a value
                // now would also flip dollarValueSet and skip its roll, leaving it at the
                // placeholder 100. It gets boosted when it sets its value.
                if (!valuable.dollarValueSet) continue;
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
        /// Boost a placed valuable that set its value after the level sweep ran. Called from
        /// inside its own DollarValueSetLogic, so the game's coroutine still adds the boosted
        /// value to the running total itself; nothing to fold in here.
        /// </summary>
        public static void BoostLateValuable(ValuableObject valuable)
        {
            if (!IsBossLevel || !LootBoosted) return;

            float mult = PluginConfig.ValuableValueMultiplier.Value;
            if (mult == 1f) return;

            float after = Mathf.Round(valuable.dollarValueCurrent * mult);
            SetValue(valuable, after);
            Plugin.LogInfo($"[Loot] late valuable {valuable.name} x{mult} -> {after}");
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
            // The body scales with the look unless ColliderCap holds it back, so hits, grabs and
            // the monster's own reach line up with the mesh. The nav agent keeps vanilla width so
            // it still fits the doorways the navmesh was baked for.
            options.EnemyPhysicalFactorCap = PluginConfig.BossColliderCap.Value;
            options.EnemyWidthFactorCap = 0f;
            options.EnemyHeightFactorCap = 0f;
            options.EnemyNavRadiusFactorCap = 1f;
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

            // The Tick keeps its normal pool. Its health is its hunger: it only bites while
            // below the full mark its animator holds, and that mark is not networked, so a
            // bigger pool would leave it either never biting or looking full forever to
            // everyone else. Size, resistance, and damage still apply to it.
            if (enemy.GetComponentInChildren<EnemyTick>(true) != null) return;

            int newMax = Mathf.Max(1, Mathf.RoundToInt(enemy.Health.health * mult));
            enemy.Health.health = newMax;
            // OnSpawn ran just before this (sets healthCurrent = health), so lift the
            // live pool to match the new maximum.
            enemy.Health.healthCurrent = newMax;
            Plugin.LogVerbose($"[Promote] health -> {newMax}");
        }

        private static void ApplyTremor(EnemyParent parent, Enemy enemy)
        {
            if (!enemy.HasRigidbody || enemy.Rigidbody == null) return;

            var body = enemy.Rigidbody.gameObject;
            if (body.GetComponent<BehemothTremor>() != null) return;
            body.AddComponent<BehemothTremor>().Setup(parent, enemy.Rigidbody.rb);
        }
    }
}
