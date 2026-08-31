using BepInEx.Configuration;

namespace Behemoths.Configuration
{
    public enum VerbosityLevel
    {
        Off = 0,
        Debug = 1,
        Verbose = 2
    }

    /// <summary>
    /// All player-facing settings. Host only: the host's config rules the run, because
    /// every boss decision and effect is applied master-side and synced down.
    /// Ranges are declared so REPOConfig renders proper sliders in the in-game menu.
    /// </summary>
    internal static class PluginConfig
    {
        // General
        public static ConfigEntry<bool> Enabled = null!;
        public static ConfigEntry<bool> AnnounceBossLevel = null!;

        // Boss level odds
        public static ConfigEntry<float> BaseChance = null!;
        public static ConfigEntry<float> ChancePerLevel = null!;
        public static ConfigEntry<float> MaxChance = null!;
        public static ConfigEntry<int> CooldownLevels = null!;
        public static ConfigEntry<int> EarliestLevel = null!;

        // Boss monsters
        public static ConfigEntry<float> BossSizeMultiplier = null!;
        public static ConfigEntry<float> BossHealthMultiplier = null!;
        public static ConfigEntry<float> BossDamageMultiplier = null!;
        public static ConfigEntry<float> BossDamageResistance = null!;
        public static ConfigEntry<float> BossColliderCap = null!;

        // Boss orbs (the valuables bosses drop on death)
        public static ConfigEntry<float> OrbValueMultiplier = null!;
        public static ConfigEntry<float> OrbSizeMultiplier = null!;
        public static ConfigEntry<float> OrbWeightMultiplier = null!;
        public static ConfigEntry<bool> OrbGlow = null!;
        public static ConfigEntry<float> OrbDurability = null!;

        // Boss-round loot (every other valuable in the level)
        public static ConfigEntry<float> ValuableValueMultiplier = null!;

        // Logging
        public static ConfigEntry<VerbosityLevel> LoggingLevel = null!;

        public static void Init(ConfigFile config)
        {
            Enabled = config.Bind(
                "General",
                "Enabled",
                true,
                "Master switch. Off disables boss levels entirely.");

            AnnounceBossLevel = config.Bind(
                "General",
                "AnnounceBossLevel",
                true,
                "Call out a boss level on arrival: a BOSS LEVEL banner flashes on screen, the taxman posts a warning to the truck screen (which everyone sees), and a stinger plays. Off makes boss levels arrive silently.");

            BaseChance = config.Bind(
                "Boss Levels",
                "BaseChance",
                10f,
                new ConfigDescription(
                    "Starting chance (percent) that any eligible level is a boss level, before the per-level ramp.",
                    new AcceptableValueRange<float>(0f, 100f)));

            ChancePerLevel = config.Bind(
                "Boss Levels",
                "ChancePerLevel",
                5f,
                new ConfigDescription(
                    "Extra boss-level chance (percent) added for each level already completed, so the deeper you push the more likely a boss becomes.",
                    new AcceptableValueRange<float>(0f, 100f)));

            MaxChance = config.Bind(
                "Boss Levels",
                "MaxChance",
                60f,
                new ConfigDescription(
                    "Upper cap (percent) the climbing boss-level chance can reach. Never caps below BaseChance, so a BaseChance of 100 always means a guaranteed boss.",
                    new AcceptableValueRange<float>(0f, 100f)));

            CooldownLevels = config.Bind(
                "Boss Levels",
                "CooldownLevels",
                3,
                new ConfigDescription(
                    "Number of levels that must pass after a boss level before another can roll. 0 allows back-to-back boss levels.",
                    new AcceptableValueRange<int>(0, 20)));

            EarliestLevel = config.Bind(
                "Boss Levels",
                "EarliestLevel",
                1,
                new ConfigDescription(
                    "The first level number that can be a boss level. 1 means a boss can appear from the very first level.",
                    new AcceptableValueRange<int>(1, 50)));

            BossSizeMultiplier = config.Bind(
                "Boss Monsters",
                "SizeMultiplier",
                1.4f,
                new ConfigDescription(
                    "How large boss monsters grow. 1.4 is the sweet spot REPO's rooms allow: noticeably bigger, but the body and collider stay matched (no sinking) and tall monsters keep their heads out of the ceiling so they can still see and reach you. Going higher than ColliderCap starts to sink the look and clip ceilings.",
                    new AcceptableValueRange<float>(1f, 4f)));

            BossHealthMultiplier = config.Bind(
                "Boss Monsters",
                "HealthMultiplier",
                3f,
                new ConfigDescription(
                    "Boss health relative to a normal monster of the same kind. 3 means three times the hits to kill.",
                    new AcceptableValueRange<float>(1f, 20f)));

            BossDamageMultiplier = config.Bind(
                "Boss Monsters",
                "DamageMultiplier",
                2f,
                new ConfigDescription(
                    "Boss attack damage relative to normal. 2 means a boss hits twice as hard.",
                    new AcceptableValueRange<float>(1f, 10f)));

            BossDamageResistance = config.Bind(
                "Boss Monsters",
                "DamageResistance",
                0.5f,
                new ConfigDescription(
                    "Fraction of incoming damage a boss shrugs off, on top of its extra health, so it feels durable. 0.5 means it takes half damage. 0 is no resistance.",
                    new AcceptableValueRange<float>(0f, 0.9f)));

            BossColliderCap = config.Bind(
                "Boss Monsters",
                "ColliderCap",
                1f,
                new ConfigDescription(
                    "The size a boss physically behaves at: its collision, pathing, attack range, and where its attacks reach from, kept apart from how big it looks (SizeMultiplier). 1 keeps the body fully vanilla, so the monster fits doors, sees and attacks you normally, and just looks huge. Raise it for a bigger physical body that wedges in doorways and reaches its attacks from higher up.",
                    new AcceptableValueRange<float>(1f, 4f)));

            OrbValueMultiplier = config.Bind(
                "Boss Orbs",
                "ValueMultiplier",
                2.5f,
                new ConfigDescription(
                    "Cash value of the orb a boss drops, relative to a normal monster's orb. Synced to everyone, including players without the mod.",
                    new AcceptableValueRange<float>(1f, 10f)));

            OrbSizeMultiplier = config.Bind(
                "Boss Orbs",
                "SizeMultiplier",
                2f,
                new ConfigDescription(
                    "Visual size of a boss orb. 2 makes it twice as big. Boss orbs never shrink back down. Size is visible only to players who have ScalerCore installed.",
                    new AcceptableValueRange<float>(1f, 6f)));

            OrbWeightMultiplier = config.Bind(
                "Boss Orbs",
                "WeightMultiplier",
                2.5f,
                new ConfigDescription(
                    "Weight of a boss orb, set apart from its size so it can be a real hauling problem. 2.5 makes it two and a half times heavier than a normal orb.",
                    new AcceptableValueRange<float>(1f, 20f)));

            OrbDurability = config.Bind(
                "Boss Orbs",
                "Durability",
                5f,
                new ConfigDescription(
                    "How tough a boss orb is against losing value when it gets knocked around. 5 means it takes roughly five times the abuse a normal valuable would before it chips. 1 leaves it as fragile as normal.",
                    new AcceptableValueRange<float>(1f, 20f)));

            OrbGlow = config.Bind(
                "Boss Orbs",
                "Glow",
                true,
                "Give boss orbs an amber glow so the big prizes stand out across the map. Visible only to the host who runs the mod.");

            ValuableValueMultiplier = config.Bind(
                "Boss Round Loot",
                "ValuableMultiplier",
                1.5f,
                new ConfigDescription(
                    "Cash value of every other valuable in a boss level, on top of the boss orbs. 1.5 is fifty percent richer. 1 leaves normal loot untouched. Synced to everyone.",
                    new AcceptableValueRange<float>(1f, 5f)));

            LoggingLevel = config.Bind(
                "General",
                "LogLevel",
                VerbosityLevel.Off,
                "Off: boss-level decisions only. Debug: per-monster and per-orb effects. Verbose: full trace.");
        }
    }
}
