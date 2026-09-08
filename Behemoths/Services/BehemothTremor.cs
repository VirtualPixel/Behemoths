using Behemoths.Configuration;
using UnityEngine;

namespace Behemoths.Services
{
    /// <summary>
    /// Rides on a boss's body and shakes the host's camera while the boss moves nearby, so
    /// a Behemoth is felt through the walls before it is seen. Only the host has the mod
    /// and camera shake is local, so only the host feels it. Lives on the enemy's own
    /// object, so it goes away with the level.
    /// </summary>
    internal class BehemothTremor : MonoBehaviour
    {
        private const float Interval = 0.45f;
        private const float MinSpeed = 1f;
        private const float NearDistance = 3f;
        private const float FarDistance = 14f;
        private const float Duration = 0.3f;

        private EnemyParent? _parent;
        private Rigidbody? _rb;
        private float _timer;

        public void Setup(EnemyParent parent, Rigidbody rb)
        {
            _parent = parent;
            _rb = rb;
        }

        private void Update()
        {
            if (_parent == null || _rb == null || !_parent.Spawned) return;

            _timer -= Time.deltaTime;
            if (_timer > 0f) return;
            _timer = Interval;

            if (!PluginConfig.BossTremors.Value) return;
            if (_rb.velocity.sqrMagnitude < MinSpeed * MinSpeed) return;

            var director = GameDirector.instance;
            if (director == null || director.CameraShake == null) return;

            // Bigger boss, heavier footfall.
            float strength = 1.2f * PluginConfig.BossSizeMultiplier.Value;
            director.CameraShake.ShakeDistance(strength, NearDistance, FarDistance, _rb.position, Duration);
        }
    }
}
