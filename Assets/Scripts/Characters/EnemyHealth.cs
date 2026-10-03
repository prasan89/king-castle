using System;
using UnityEngine;
using KingSmash.Core;

namespace KingSmash.Characters
{
    public class EnemyHealth : MonoBehaviour
    {
        [SerializeField] private float _maxHP = 60f;
        [SerializeField] private float _armor = 0.5f;  // damage multiplier reduction: effective = dmg * (1 - armor)

        private float _currentHP;

        public static event Action<EnemyHealth> OnEnemyDefeated;
        public static event Action<EnemyHealth, float, HitReactionSize> OnEnemyHit;

        public float CurrentHP     => _currentHP;
        public float MaxHP         => _maxHP;
        public float HPNormalized  => _maxHP > 0f ? _currentHP / _maxHP : 0f;
        public bool  IsAlive       => _currentHP > 0f;

        // Config thresholds — set by EnemyController after reading its config
        public float SmallThreshold  { get; set; } = 10f;
        public float MediumThreshold { get; set; } = 30f;
        public float LargeThreshold  { get; set; } = 60f;

        public void Initialize(float maxHP, float armor)
        {
            _maxHP     = maxHP;
            _armor     = Mathf.Clamp01(armor);
            _currentHP = _maxHP;
        }

        // Awake intentionally omitted: Initialize() is always called by EnemyController.Awake()
        // via ConfigureFromScriptableObject() before Unity would seed _currentHP from _maxHP.
        private void Awake() { /* HP seeded by Initialize() */ }

        public void TakeDamage(float rawAmount)
        {
            if (!IsAlive) return;
            float effective = rawAmount * (1f - _armor);
            _currentHP = Mathf.Max(0f, _currentHP - effective);
            var size = ClassifyHit(effective);
            GameLogger.Debug("EnemyHealth", $"{name} took {effective:F1} dmg ({size}). HP={_currentHP:F1}/{_maxHP:F1}");
            OnEnemyHit?.Invoke(this, effective, size);
            if (_currentHP <= 0f)
            {
                GameLogger.Info("EnemyHealth", $"{name} defeated.");
                OnEnemyDefeated?.Invoke(this);
            }
        }

        private HitReactionSize ClassifyHit(float effective)
        {
            if (effective >= LargeThreshold)  return HitReactionSize.Large;
            if (effective >= MediumThreshold) return HitReactionSize.Medium;
            return HitReactionSize.Small;
        }
    }
}
