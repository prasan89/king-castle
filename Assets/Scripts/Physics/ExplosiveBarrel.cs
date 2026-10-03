using System.Collections;
using UnityEngine;
using KingSmash.Core;
using KingSmash.Services;

namespace KingSmash.Physics
{
    [RequireComponent(typeof(DestructibleObject))]
    public class ExplosiveBarrel : MonoBehaviour
    {
        [Header("Explosion Config")]
        [SerializeField] private float _explosionRadius  = 3.5f;
        [SerializeField] private float _explosionForce   = 800f;
        [SerializeField] private float _explosionDamage  = 120f;
        [SerializeField] private float _fuseDelay        = 0.12f;  // short delay for VFX
        [SerializeField] private LayerMask _affectedLayers;

        [Header("VFX / Audio")]
        [SerializeField] private GameObject _explosionVFXPrefab;
        [SerializeField] private string _explosionSfxKey = "explosion";

        private DestructibleObject _destructible;
        private bool _exploded;

        private void Awake() => _destructible = GetComponent<DestructibleObject>();

        private void OnEnable()  => DestructibleObject.OnDestroyed += HandleDestructibleDestroyed;
        private void OnDisable() => DestructibleObject.OnDestroyed -= HandleDestructibleDestroyed;

        private void HandleDestructibleDestroyed(DestructibleObject obj)
        {
            if (obj != _destructible || _exploded) return;
            StartCoroutine(ExplodeAfterDelay());
        }

        private IEnumerator ExplodeAfterDelay()
        {
            yield return new WaitForSeconds(_fuseDelay);
            Explode();
        }

        private void Explode()
        {
            if (_exploded) return;
            _exploded = true;

            Vector2 origin = transform.position;

            // Spawn VFX
            if (_explosionVFXPrefab != null)
                Instantiate(_explosionVFXPrefab, origin, Quaternion.identity);

            // Play audio
            if (ServiceLocator.TryGet<IAudioService>(out var audio))
                audio.PlaySfx(_explosionSfxKey);

            // Apply radial impulse + damage to everything in radius
            var colliders = UnityEngine.Physics2D.OverlapCircleAll(origin, _explosionRadius, _affectedLayers);
            foreach (var col in colliders)
            {
                // Rigidbody impulse
                var rb = col.GetComponent<Rigidbody2D>();
                if (rb != null)
                {
                    Vector2 dir = ((Vector2)col.transform.position - origin).normalized;
                    float dist  = Vector2.Distance(col.transform.position, origin);
                    float falloff = 1f - Mathf.Clamp01(dist / _explosionRadius);
                    rb.AddForce(dir * _explosionForce * falloff, ForceMode2D.Impulse);
                }

                // Damage destructibles
                var destructible = col.GetComponent<DestructibleObject>();
                if (destructible != null && !destructible.IsDestroyed)
                {
                    float dist = Vector2.Distance(col.transform.position, origin);
                    float falloff = 1f - Mathf.Clamp01(dist / _explosionRadius);
                    Vector2 impulseDir = ((Vector2)col.transform.position - origin).normalized;
                    destructible.TakeDamage(_explosionDamage * falloff, origin, impulseDir * _explosionForce * falloff);
                }
            }

            GameLogger.Info("ExplosiveBarrel", $"Exploded at {origin}. Radius={_explosionRadius}, affected={colliders.Length}.");
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = new Color(1f, 0.4f, 0f, 0.35f);
            Gizmos.DrawWireSphere(transform.position, _explosionRadius);
        }
    }
}
