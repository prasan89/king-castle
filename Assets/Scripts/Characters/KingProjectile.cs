using System;
using UnityEngine;
using KingSmash.Gameplay;
using KingSmash.Physics;
using KingSmash.Core;

namespace KingSmash.Characters
{
    /// The King as a launched projectile.
    /// Handles flight, collision damage, and landing detection.
    [RequireComponent(typeof(Rigidbody2D), typeof(CircleCollider2D))]
    public class KingProjectile : MonoBehaviour, ILaunchable
    {
        [SerializeField] private ProjectileConfig _config;
        [SerializeField] private TrailRenderer _trailRenderer;

        private Rigidbody2D _rb;
        private int _bounceCount;
        private bool _isLaunched;
        private bool _hasLanded;

        public static event Action<KingProjectile, Collision2D> OnKingCollision;
        public static event Action<KingProjectile> OnKingLanded;

        public bool IsLaunched => _isLaunched;
        public bool HasLanded  => _hasLanded;

        public Rigidbody2D Body => _rb;

        private void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
            _rb.isKinematic = true;
            if (_config != null)
            {
                _rb.gravityScale   = _config.gravityScale;
                _rb.linearDamping  = _config.airDrag;
            }
            if (_trailRenderer != null) _trailRenderer.enabled = false;
        }

        public void Launch(Vector2 direction, float power)
        {
            if (_isLaunched) return;
            _isLaunched = true;
            _rb.isKinematic = false;
            _rb.linearVelocity = direction * power;
            if (_trailRenderer != null)
            {
                _trailRenderer.enabled = true;
                _trailRenderer.time = _config != null ? _config.trailTime : 0.4f;
            }
            GameLogger.Debug("KingProjectile", $"Launched dir={direction} power={power:F1}");
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            if (!_isLaunched) return;
            OnKingCollision?.Invoke(this, collision);

            var contact = collision.contacts[0];
            float relSpeed = collision.relativeVelocity.magnitude;

            var damageable = collision.gameObject.GetComponent<IDamageable>();
            if (damageable != null)
            {
                var material = collision.gameObject.GetComponent<DestructibleObject>()?.Material;
                float damage = DamageCalculator.Calculate(relSpeed, material, _rb.mass);
                Vector2 impulse = DamageCalculator.ImpulseForce(
                    collision.relativeVelocity,
                    _rb.mass,
                    material?.bounciness ?? 0.3f);
                damageable.TakeDamage(damage, contact.point, impulse);
            }

            _bounceCount++;
            if (_config != null && _bounceCount > _config.maxBounces)
                Land();
        }

        private void Land()
        {
            if (_hasLanded) return;
            _hasLanded = true;
            _rb.linearDamping  = 6f;
            _rb.angularDamping = 6f;
            if (_trailRenderer != null) _trailRenderer.enabled = false;
            OnKingLanded?.Invoke(this);
            GameLogger.Debug("KingProjectile", "King landed.");
        }

        private void OnBecameInvisible()
        {
            // King flew off screen — count as landed to allow next launch
            if (_isLaunched && !_hasLanded) Land();
        }
    }
}
