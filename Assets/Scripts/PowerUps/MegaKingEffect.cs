using UnityEngine;
using KingSmash.Core;

namespace KingSmash.PowerUps
{
    public class MegaKingEffect : PowerUpEffect
    {
        private Rigidbody2D _kingBody;
        private float       _originalMass;

        protected override void OnBegin()
        {
            if (King == null || Config == null) return;
            _kingBody = King.GetComponent<Rigidbody2D>();
            if (_kingBody == null) return;
            _originalMass  = _kingBody.mass;
            _kingBody.mass = _originalMass * Config.kingMassMultiplier;
            GameLogger.Debug("MegaKingEffect", $"King mass {_originalMass:F2} → {_kingBody.mass:F2}");
        }

        protected override void OnEnd()
        {
            if (_kingBody == null) return;
            _kingBody.mass = _originalMass;
            _kingBody      = null;
        }
    }
}
