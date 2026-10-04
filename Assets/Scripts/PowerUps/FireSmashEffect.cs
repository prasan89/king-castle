using System.Collections.Generic;
using UnityEngine;
using KingSmash.Physics;

namespace KingSmash.PowerUps
{
    public class FireSmashEffect : PowerUpEffect
    {
        private DestructibleObject[] _targets;
        private float                _tickTimer;
        private const float          TickInterval = 0.3f;

        protected override void OnBegin()
        {
            if (Config == null || King == null) return;
            Vector2 center = King.transform.position;
            var hits = Physics2D.OverlapCircleAll(center, Mathf.Max(Config.blastRadius, 1f));
            var list = new List<DestructibleObject>();
            foreach (var hit in hits)
            {
                var dest = hit.GetComponent<DestructibleObject>();
                if (dest != null && !dest.IsDestroyed) list.Add(dest);
            }
            _targets   = list.ToArray();
            _tickTimer = 0f;
        }

        private void Update()
        {
            if (!IsRunning || _targets == null) return;
            _tickTimer += Time.deltaTime;
            if (_tickTimer < TickInterval) return;
            _tickTimer -= TickInterval;
            foreach (var dest in _targets)
            {
                if (dest == null || dest.IsDestroyed) continue;
                float mult   = GetDamageMultiplier(dest);
                float damage = Config.dotDamagePerSecond * TickInterval * mult;
                dest.TakeDamage(damage, dest.transform.position, Vector2.zero);
            }
        }

        protected override void OnEnd() => _targets = null;
    }
}
