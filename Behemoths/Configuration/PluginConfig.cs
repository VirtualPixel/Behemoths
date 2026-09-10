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
        public static ConfigEntry<float> MaxChance = null!;
        public static ConfigEntry<int> CooldownLevels = null!;
        public static ConfigEntry<int> EarliestLevel = null!;

        // Per-map odds, multipliers on the level chance
        public static ConfigEntry<float> MapManor = null!;
        public static ConfigEntry<float> MapArctic = null!;
        public static ConfigEntry<float> MapWizard = null!;
        public static ConfigEntry<float> MapMuseum = null!;
        public static ConfigEntry<float> MapOther = null!;

        // Boss monsters
        public static ConfigEntry<float> BossSizeMultiplier = null!;
        public static ConfigEntry<float> BossHealthMultiplier = null!;
        public static ConfigEntry<float> BossDamageMultiplier = null!;
        public static ConfigEntry<float> BossHitCap = null!;
        public static ConfigEntry<float> BossDamageResistance = null!;
        public static ConfigEntry<float> BossColliderCap = null!;
        public static ConfigEntry<float> BossHeightCap = null!;
        public static ConfigEntry<bool> BossTremors = null!;

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
                "Call out a boss level once it has loaded, using the game's moon-phase popup retitled BOSS LEVEL. Only the host sees it, since only the host runs the mod. Off makes boss levels arrive silently.");

            BaseChance = config.Bind(
                "Boss Levels",
                "BaseChance",
                10f,
                new ConfigDescription(
                    "Boss-level chance (percent) at the start of a run. The chance climbs from here to MaxChance along the game's own difficulty curve, which tops out at level 10, and then holds.",
                    new AcceptableValueRange<float>(0f, 100f)));

            MaxChance = config.Bind(
                "Boss Levels",
                "MaxChance",
                60f,
                new ConfigDescription(
                    "Boss-level chance (percent) from level 10 onward. Never sits below BaseChance, so a BaseChance of 100 always means a guaranteed boss on every eligible level.",
                    new AcceptableValueRange<float>(0f, 100f)));

            CooldownLevels = config.Bind(
                "Boss Levels",
                "CooldownLevels",
                3,
                new ConfigDescription(
                    "Number of levels that must pass after a boss level before another can roll. 0 allows back-to-back boss levels. Saved with the run, so quitting and loading does not reset it.",
                    new AcceptableValueRange<int>(0, 20)));

            EarliestLevel = config.Bind(
                "Boss Levels",
                "EarliestLevel",
                3,
                new ConfigDescription(
                    "The first level number that can be a boss level. 3 matches how the game holds its nastier monsters back until you have two levels behind you.",
                    new AcceptableValueRange<int>(1, 50)));

            MapManor = config.Bind(
                "Map Odds",
                "Manor",
                1f,
                new ConfigDescription(
                    "Multiplier on the boss-level chance at Headman Manor. 1 is the plain chance, 0 never, 2 double.",
                    new AcceptableValueRange<float>(0f, 3f)));

            MapArctic = config.Bind(
                "Map Odds",
                "Arctic",
                0.75f,
                new ConfigDescription(
                    "Multiplier on the boss-level chance at McJannek Station (the Arctic map). Lower by default: its corridors are tight, and a Behemoth in a tight corridor is a wall.",
                    new AcceptableValueRange<float>(0f, 3f)));

            MapWizard = config.Bind(
                "Map Odds",
                "Wizard",
                1.25f,
                new ConfigDescription(
                    "Multiplier on the boss-level chance at Swiftbroom Academy (the Wizard map). Higher by default: the halls have room for something huge to come around a corner.",
                    new AcceptableValueRange<float>(0f, 3f)));

            MapMuseum = config.Bind(
                "Map Odds",
                "Museum",
                1.25f,
                new ConfigDescription(
                    "Multiplier on the boss-level chance at the Museum of Human Art. Higher by default: big open galleries suit big monsters.",
                    new AcceptableValueRange<float>(0f, 3f)));

            MapOther = config.Bind(
                "Map Odds",
                "OtherMaps",
                1f,
                new ConfigDescription(
                    "Multiplier on the boss-level chance on any map not listed above, including modded ones.",
                    new AcceptableValueRange<float>(0f, 3f)));

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
                    "Boss attack damage relative to normal. 2 means a boss hits twice as hard. Reaches every player, modded or not: the host boosts its own hits directly and sends the extra damage to other players when it sees a hit land.",
                    new AcceptableValueRange<float>(1f, 10f)));

            BossHitCap = config.Bind(
                "Boss Monsters",
                "HitCap",
                75f,
                new ConfigDescription(
                    "Fairness cap: a boosted hit never takes more than this percent of a player's max health, unless the normal hit already did. 75 means a hit you could survive at full health stays survivable at full health; you just leave with a lot less. 100 turns the cap off.",
                    new AcceptableValueRange<float>(10f, 100f)));

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
                0f,
                new ConfigDescription(
                    "The size a boss physically is: its hitbox, its grab body and where its attacks reach from. 0 follows SizeMultiplier, so the body you see is the body you shoot, grab and get hit by. Pathing keeps its vanilla width either way, so a giant still finds its way through doors even if it clips the frame. Set a number to hold the body smaller than the look.",
                    new AcceptableValueRange<float>(0f, 4f)));

            BossHeightCap = config.Bind(
                "Boss Monsters",
                "HeightCap",
                1f,
                new ConfigDescription(
                    "How tall a boss's body is allowed to get, as a multiple of vanilla. 1 keeps it door height so it can still walk through rooms; the width still follows the look. 0 lets the body grow as tall as it looks, which leaves a big one stuck on its side in the first doorway.",
                    new AcceptableValueRange<float>(0f, 4f)));

            BossTremors = config.Bind(
                "Boss Monsters",
                "Tremors",
                true,
                "A Behemoth on the move shakes your camera when it is close, so you feel one coming before you see it. Only the host feels it, since only the host runs the mod.");

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
                "Off: boss-level decisions only. Debug: per-monster, per-hit, and per-orb effects. Verbose: full trace.");
        }
    }
}
