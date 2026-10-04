using UnityEngine;
using KingSmash.Core;
using KingSmash.Physics;

namespace KingSmash.PowerUps
{
    public class BombSmashEffect : PowerUpEffect
    {
        protected override void OnBegin()
        {
            if (Config == null || King == null) return;
            Vector2 center = King.transform.position;
            var hits = Physics2D.OverlapCircleAll(center, Config.blastRadius);
            foreach (var hit in hits)
            {
                var dest = hit.GetComponent<DestructibleObject>();
                if (dest == null || dest.IsDestroyed) continue;
                float mult    = GetDamageMultiplier(dest);
                float damage  = Config.baseDamage * mult;
                Vector2 dir   = ((Vector2)hit.transform.position - center).normalized;
                float dist    = Vector2.Distance(center, hit.transform.position);
                float falloff = Mathf.Clamp01(1f - dist / Mathf.Max(Config.blastRadius, 0.01f));
                dest.TakeDamage(damage * falloff, center, dir * damage * falloff);
            }
            GameLogger.Debug("BombSmashEffect", $"Exploded at {center} r={Config.blastRadius} hits={hits.Length}");
        }

        protected override void OnEnd() { }
    }
}
