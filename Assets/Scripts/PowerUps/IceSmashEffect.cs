using System.Collections.Generic;
using UnityEngine;

namespace KingSmash.PowerUps
{
    public class IceSmashEffect : PowerUpEffect
    {
        private Rigidbody2D[] _frozenBodies;
        private float[]       _origAngDamping;
        private float[]       _origLinDamping;

        protected override void OnBegin()
        {
            if (Config == null || King == null) return;
            Vector2 center = King.transform.position;
            var hits   = Physics2D.OverlapCircleAll(center, Mathf.Max(Config.blastRadius, 1f));
            var bodies = new List<Rigidbody2D>();
            var angD   = new List<float>();
            var linD   = new List<float>();

            foreach (var hit in hits)
            {
                var rb = hit.GetComponent<Rigidbody2D>();
                if (rb == null) continue;
                bodies.Add(rb);
                angD.Add(rb.angularDamping);
                linD.Add(rb.linearDamping);
                rb.angularDamping = 100f;
                rb.linearDamping  = 100f;
            }

            _frozenBodies   = bodies.ToArray();
            _origAngDamping = angD.ToArray();
            _origLinDamping = linD.ToArray();
        }

        protected override void OnEnd()
        {
            if (_frozenBodies == null) return;
            for (int i = 0; i < _frozenBodies.Length; i++)
            {
                if (_frozenBodies[i] == null) continue;
                _frozenBodies[i].angularDamping = _origAngDamping[i];
                _frozenBodies[i].linearDamping  = _origLinDamping[i];
            }
            _frozenBodies = null;
        }
    }
}
