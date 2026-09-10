using System.Collections.Generic;
using Behemoths.Configuration;
using UnityEngine;
using UnityEngine.UI;

namespace Behemoths.Services
{
    /// <summary>
    /// Drives the game's own moon-phase popup (MoonUI) to announce a boss level, so the
    /// call-out uses the real styled cinematic instead of injected text. Retries until
    /// MoonUI exists and we are in a level.
    /// </summary>
    internal class BossAnnouncer : MonoBehaviour
    {
        internal static BossAnnouncer? Instance { get; private set; }

        private bool _pending;
        private float _delay = -1f;

        private void Awake() => Instance = this;

        public void Trigger()
        {
            if (PluginConfig.AnnounceBossLevel.Value)
            {
                _pending = true;
                _delay = -1f;
            }
        }

        private void Update()
        {
            if (!_pending) return;

            // Wait until the level has finished generating, then hold a beat, so the popup
            // animates in cleanly instead of arriving mid-load and only playing its exit.
            var lg = LevelGenerator.Instance;
            if (!SemiFunc.RunIsLevel() || lg == null || !lg.Generated)
            {
                _delay = -1f;
                return;
            }
            if (_delay < 0f) _delay = 2f;
            _delay -= Time.deltaTime;
            if (_delay > 0f) return;

            if (ShowPopup())
                _pending = false;
        }

        private static bool ShowPopup()
        {
            var mui = MoonUI.instance;
            var rm = RunManager.instance;
            if (mui == null || rm == null || mui.textTitle == null) return false;

            // Fill the moon graphics from the current moon (cleared when there is none),
            // exactly as the game's own Check() does, so a later real moon popup is fine.
            int ml = Mathf.Clamp(rm.moonLevel, 0, rm.moons.Count);
            SetGraphic(mui.moonGraphicPreviousest, rm.MoonGetIcon(ml - 2));
            SetGraphic(mui.moonGraphicPrevious, rm.MoonGetIcon(ml - 1));
            SetGraphic(mui.moonGraphicCurrent, rm.MoonGetIcon(ml));
            SetGraphic(mui.moonGraphicNext, rm.MoonGetIcon(ml + 1));

            // On a level that is also a moon change this popup stands in for the moon's own,
            // which never runs (MoonUIPatch). The moon's modifiers still apply; the screen is
            // the boss's alone.
            var lines = new List<Moon.MoonAttribute>();
            lines.Add(new Moon.MoonAttribute { text = "Behemoths roam this level" });
            lines.Add(new Moon.MoonAttribute { text = "They hit harder and take more killing" });
            lines.Add(new Moon.MoonAttribute { text = "Their orbs are worth a fortune" });

            mui.textTitle.text = "BOSS LEVEL";
            mui.attributes = lines;
            mui.SetState(MoonUI.State.Start);

            rm.moonLevelChanged = false;

            Plugin.LogAlways("[Announce] BOSS LEVEL popup shown");
            return true;
        }

        private static void SetGraphic(RawImage img, Texture tex)
        {
            if (img == null) return;
            img.texture = tex;
            img.color = tex != null ? Color.white : Color.clear;
        }
    }
}
