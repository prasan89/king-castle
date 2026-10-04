using System.Collections.Generic;
using UnityEngine;
using KingSmash.Core;
using KingSmash.Physics;

namespace KingSmash.PowerUps
{
    public class LightningSmashEffect : PowerUpEffect
    {
        protected override void OnBegin()
        {
            if (Config == null || King == null) return;

            Vector2 origin    = King.transform.position;
            int     remaining = Mathf.Max(Config.chainTargets, 1);
            float   damage    = Config.baseDamage;
            var     struck    = new HashSet<DestructibleObject>();
            Vector2 current   = origin;

            while (remaining > 0)
            {
                DestructibleObject best     = null;
                float              bestDist = float.MaxValue;

                var nearby = Physics2D.OverlapCircleAll(current, 6f);
                foreach (var col in nearby)
                {
                    var dest = col.GetComponent<DestructibleObject>();
                    if (dest == null || dest.IsDestroyed || struck.Contains(dest)) continue;
                    float d = Vector2.Distance(current, dest.transform.position);
                    if (d < bestDist) { bestDist = d; best = dest; }
                }
                if (best == null) break;

                float mult = GetDamageMultiplier(best);
                best.TakeDamage(damage * mult, best.transform.position, Vector2.zero);
                struck.Add(best);
                current = best.transform.position;
                damage *= 0.7f;
                remaining--;
            }

            GameLogger.Debug("LightningSmashEffect", $"Chained {struck.Count} targets");
        }

        protected override void OnEnd() { }
    }
}
