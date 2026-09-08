using System;
using Behemoths.Configuration;
using UnityEngine;

namespace Behemoths.Services
{
    /// <summary>
    /// The boss-level chance for a given level. Two ingredients: the run's own difficulty
    /// curve (the same one the game uses to add monsters, from level 1 up to level 10)
    /// sets how far the chance has climbed from BaseChance toward MaxChance, and the map
    /// scales the result, so some maps are Behemoth country and others rarely are.
    /// </summary>
    internal static class BossOdds
    {
        /// <summary>Chance in percent that the current level is a boss level.</summary>
        public static float ChanceFor(Level level)
        {
            // BaseChance is a floor: it never gets capped below itself, so 100 guarantees
            // a boss every eligible level even if MaxChance is lower.
            float floor = PluginConfig.BaseChance.Value;
            float ceiling = Mathf.Max(floor, PluginConfig.MaxChance.Value);
            float climb = Mathf.Lerp(floor, ceiling, SemiFunc.RunGetDifficultyMultiplier1());
            return Mathf.Clamp(climb * MapWeight(level), 0f, 100f);
        }

        /// <summary>The map's multiplier on the chance. Modded maps fall under OtherMaps.</summary>
        public static float MapWeight(Level level)
        {
            string name = level != null ? level.name : string.Empty;
            if (Has(name, "Manor")) return PluginConfig.MapManor.Value;
            if (Has(name, "Arctic")) return PluginConfig.MapArctic.Value;
            if (Has(name, "Wizard")) return PluginConfig.MapWizard.Value;
            if (Has(name, "Museum")) return PluginConfig.MapMuseum.Value;
            return PluginConfig.MapOther.Value;
        }

        /// <summary>Short map name for the log, from the asset name "Level - Manor".</summary>
        public static string MapName(Level level)
        {
            if (level == null) return "unknown map";
            const string prefix = "Level - ";
            string name = level.name;
            return name.StartsWith(prefix, StringComparison.Ordinal) ? name.Substring(prefix.Length) : name;
        }

        private static bool Has(string name, string token) =>
            name.IndexOf(token, StringComparison.OrdinalIgnoreCase) >= 0;
    }
}
