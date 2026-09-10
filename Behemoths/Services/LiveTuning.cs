using System;
using System.IO;
using BepInEx.Configuration;
using UnityEngine;

namespace Behemoths.Services
{
    /// <summary>
    /// Config changes land on the bosses that are already in the level. A slider moved in the
    /// in-game config menu fires SettingChanged on its own; an edit to the cfg file on disk is
    /// picked up by watching its write time, so a value can be tuned with the game running.
    /// </summary>
    internal class LiveTuning : MonoBehaviour
    {
        private const float PollSeconds = 1f;
        private const float SettleSeconds = 0.3f;

        private ConfigFile _config = null!;
        private DateTime _seenWrite;
        private float _nextPoll;
        private float _applyAt = -1f;

        public void Setup(ConfigFile config)
        {
            _config = config;
            _seenWrite = WriteTime();
            _config.SettingChanged += OnSettingChanged;
        }

        private void OnSettingChanged(object sender, SettingChangedEventArgs args)
        {
            // Several entries change in one reload; wait for the last one before applying.
            _applyAt = Time.unscaledTime + SettleSeconds;
        }

        private void Update()
        {
            if (Time.unscaledTime >= _nextPoll)
            {
                _nextPoll = Time.unscaledTime + PollSeconds;
                DateTime now = WriteTime();
                if (now != _seenWrite)
                {
                    _seenWrite = now;
                    _config.Reload();
                    // A reload saves the file back, so take that write as already seen.
                    _seenWrite = WriteTime();
                    Plugin.LogAlways("[Tune] config file changed, reloaded");
                }
            }

            if (_applyAt >= 0f && Time.unscaledTime >= _applyAt)
            {
                _applyAt = -1f;
                BossRoundService.Retune();
            }
        }

        private DateTime WriteTime()
        {
            return File.Exists(_config.ConfigFilePath) ? File.GetLastWriteTimeUtc(_config.ConfigFilePath) : DateTime.MinValue;
        }
    }
}
