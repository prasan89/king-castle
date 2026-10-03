using UnityEngine;
using KingSmash.Gameplay;
using KingSmash.Core;

namespace KingSmash.Characters
{
    /// Placeholder enemy for M1. Tracks hits and reports defeat.
    [RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
    public class EnemyPlaceholder : MonoBehaviour, IDamageable
    {
        [SerializeField] private float _maxHealth = 60f;
        [SerializeField] private SpriteRenderer _spriteRenderer;
        [SerializeField] private Color _damagedColor = Color.red;

        private float _currentHealth;
        private bool _isDefeated;

        public float CurrentHealth => _currentHealth;
        public float MaxHealth     => _maxHealth;
        public bool  IsDestroyed   => _isDefeated;

        public static System.Action<EnemyPlaceholder> OnEnemyDefeated;

        private void Awake() => _currentHealth = _maxHealth;

        public void TakeDamage(float damage, Vector2 impactPoint, Vector2 impactForce)
        {
            if (_isDefeated) return;
            _currentHealth -= damage;
            if (_spriteRenderer != null) _spriteRenderer.color = _damagedColor;
            if (_currentHealth <= 0f) Defeat();
        }

        public void InstantDestroy() => Defeat();

        private void Defeat()
        {
            if (_isDefeated) return;
            _isDefeated = true;
            GameLogger.Info("EnemyPlaceholder", $"{name} defeated.");
            OnEnemyDefeated?.Invoke(this);
            gameObject.SetActive(false);
        }
    }
}
