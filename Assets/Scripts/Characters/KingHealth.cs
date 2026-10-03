using System;
using UnityEngine;
using KingSmash.Core;

namespace KingSmash.Characters
{
    public class KingHealth : MonoBehaviour
    {
        [SerializeField] private KingConfig _config;
        [SerializeField] private int _kingLevel = 1;

        private float _currentHP;

        public static event Action<KingHealth> OnKingDefeated;
        public static event Action<float, float> OnKingHPChanged; // current, max

        public float CurrentHP  => _currentHP;
        public float MaxHP      => _config != null ? _config.GetMaxHPAtLevel(_kingLevel) : 200f;
        public float HPNormalized => MaxHP > 0 ? _currentHP / MaxHP : 0f;
        public bool  IsAlive    => _currentHP > 0f;

        private void Awake()
        {
            _currentHP = MaxHP;
        }

        public void TakeDamage(float amount)
        {
            if (!IsAlive) return;
            float effective = Mathf.Max(0f, amount);
            _currentHP = Mathf.Max(0f, _currentHP - effective);
            GameLogger.Debug("KingHealth", $"King took {effective:F1} dmg. HP={_currentHP:F1}/{MaxHP:F1}");
            OnKingHPChanged?.Invoke(_currentHP, MaxHP);
            if (_currentHP <= 0f)
            {
                GameLogger.Info("KingHealth", "King defeated.");
                OnKingDefeated?.Invoke(this);
            }
        }

        public void Heal(float amount)
        {
            if (!IsAlive) return;
            _currentHP = Mathf.Min(MaxHP, _currentHP + amount);
            OnKingHPChanged?.Invoke(_currentHP, MaxHP);
        }
    }
}
