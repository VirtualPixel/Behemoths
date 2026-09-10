using BepInEx;
using BepInEx.Logging;
using Behemoths.Configuration;
using Behemoths.Services;
using HarmonyLib;
using UnityEngine;

namespace Behemoths
{
    [BepInPlugin("Vippy.Behemoths", "Behemoths", BuildInfo.Version)]
    [BepInDependency("Vippy.ScalerCore", BepInDependency.DependencyFlags.HardDependency)]
    public class Plugin : BaseUnityPlugin
    {
        internal static Plugin Instance { get; private set; } = null!;
        internal new static ManualLogSource Logger => Instance.BaseLogger;
        private ManualLogSource BaseLogger => base.Logger;
        internal Harmony? Harmony { get; set; }

        private void Awake()
        {
            Instance = this;
            gameObject.transform.parent = null;
            gameObject.hideFlags = HideFlags.HideAndDontSave;

            PluginConfig.Init(Config);

            gameObject.AddComponent<BossAnnouncer>();
            gameObject.AddComponent<LiveTuning>().Setup(Config);

            Harmony ??= new Harmony(Info.Metadata.GUID);
            Harmony.PatchAll();

            Logger.LogInfo($"{Info.Metadata.GUID} v{Info.Metadata.Version} loaded. LogLevel={PluginConfig.LoggingLevel.Value}");
        }

        internal static void LogAlways(string msg) => Logger.LogInfo(msg);

        internal static void LogInfo(string msg)
        {
            if (PluginConfig.LoggingLevel.Value >= VerbosityLevel.Debug)
                Logger.LogInfo(msg);
        }

        internal static void LogVerbose(string msg)
        {
            if (PluginConfig.LoggingLevel.Value >= VerbosityLevel.Verbose)
                Logger.LogDebug(msg);
        }
    }
}
