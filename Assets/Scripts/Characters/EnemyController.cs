using System;
using UnityEngine;
using KingSmash.Core;
using KingSmash.Gameplay;
using KingSmash.Services;

namespace KingSmash.Characters
{
    // Abstract base for all enemy types.
    [RequireComponent(typeof(EnemyHealth), typeof(Rigidbody2D))]
    public abstract class EnemyController : MonoBehaviour, IDamageReceiver
    {
        [SerializeField] protected EnemyAnimator _animator;

        protected EnemyHealth _health;
        protected Rigidbody2D _rb;

        public static event Action<EnemyController> OnEnemyDied;

        public bool IsAlive => _health != null && _health.IsAlive;

        protected virtual void Awake()
        {
            _health = GetComponent<EnemyHealth>();
            _rb     = GetComponent<Rigidbody2D>();
            ConfigureFromScriptableObject();
        }

        protected virtual void OnEnable()
        {
            EnemyHealth.OnEnemyDefeated += HandleDefeated;
            EnemyHealth.OnEnemyHit      += HandleHit;
        }

        protected virtual void OnDisable()
        {
            EnemyHealth.OnEnemyDefeated -= HandleDefeated;
            EnemyHealth.OnEnemyHit      -= HandleHit;
        }

        // Override to read your specific ScriptableObject config into EnemyHealth
        protected abstract void ConfigureFromScriptableObject();

        public virtual void ReceiveDamage(IDamageSource source)
        {
            if (!IsAlive) return;
            _health.TakeDamage(source.DamageAmount);
            ApplyKnockback(source.ImpactForce);
        }

        protected virtual void ApplyKnockback(Vector2 force)
        {
            if (_rb != null) _rb.AddForce(force, ForceMode2D.Impulse);
        }

        private void HandleHit(EnemyHealth health, float amount, HitReactionSize size)
        {
            if (health != _health) return;
            _animator?.PlayHitReaction(size);
            OnHitReaction(size);
            PlayHitAudio(size);
        }

        private void HandleDefeated(EnemyHealth health)
        {
            if (health != _health) return;
            _animator?.PlayDeath();
            OnDeath();
            OnEnemyDied?.Invoke(this);
        }

        protected virtual void OnHitReaction(HitReactionSize size) { }
        protected virtual void OnDeath() { }

        protected virtual void PlayHitAudio(HitReactionSize size)
        {
            if (ServiceLocator.TryGet<IAudioService>(out var audio))
                audio.PlaySfx("enemy_hit");
        }
    }
}
