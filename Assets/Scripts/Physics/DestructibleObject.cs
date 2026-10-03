using System;
using UnityEngine;
using KingSmash.Gameplay;
using KingSmash.Core;

namespace KingSmash.Physics
{
    [RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
    public class DestructibleObject : MonoBehaviour, IDamageable
    {
        [SerializeField] private MaterialConfig _materialConfig;
        [SerializeField] private float _healthOverride = -1f;

        private float _currentHealth;
        private bool _isDestroyed;
        private Rigidbody2D _rb;

        public static event Action<DestructibleObject> OnDestroyed;
        public static event Action<DestructibleObject, float> OnDamaged;

        public float CurrentHealth => _currentHealth;
        public float MaxHealth => _healthOverride > 0f ? _healthOverride : _materialConfig?.maxHealth ?? 100f;
        public bool IsDestroyed => _isDestroyed;
        public MaterialConfig Material => _materialConfig;
        public float HealthNormalized => Mathf.Clamp01(_currentHealth / MaxHealth);

        private void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
            _currentHealth = MaxHealth;
        }

        public void TakeDamage(float damage, Vector2 impactPoint, Vector2 impactForce)
        {
            if (_isDestroyed || damage <= 0f) return;

            _currentHealth -= damage;
            OnDamaged?.Invoke(this, damage);
            GameLogger.Debug("DestructibleObject", $"{name} -{damage:F1} HP → {_currentHealth:F1}/{MaxHealth:F1}");

            if (_rb != null && impactForce.sqrMagnitude > 0f)
                _rb.AddForceAtPosition(impactForce, impactPoint, ForceMode2D.Impulse);

            if (_currentHealth <= 0f)
                InstantDestroy();
        }

        public void InstantDestroy()
        {
            if (_isDestroyed) return;
            _isDestroyed = true;

            // Spawn debris from pool
            if (DebrisPool.Instance != null && _materialConfig != null)
                DebrisPool.Instance.Spawn(transform.position, _materialConfig, _materialConfig.debrisCount);

            GameLogger.Debug("DestructibleObject", $"{name} destroyed.");
            OnDestroyed?.Invoke(this);
            gameObject.SetActive(false);
        }

        public void ResetHealth()
        {
            _isDestroyed = false;
            _currentHealth = MaxHealth;
            gameObject.SetActive(true);
        }
    }
}
