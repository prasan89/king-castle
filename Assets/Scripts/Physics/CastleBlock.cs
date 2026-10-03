using UnityEngine;
using KingSmash.Core;

namespace KingSmash.Physics
{
    /// Concrete block prefab component.
    /// Composes DestructibleObject + material-driven visual tinting.
    /// Attach to any block prefab alongside a Rigidbody2D and BoxCollider2D.
    [RequireComponent(typeof(DestructibleObject))]
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(BoxCollider2D))]
    public class CastleBlock : MonoBehaviour
    {
        [SerializeField] private MaterialConfig _material;
        [SerializeField] private SpriteRenderer _spriteRenderer;

        private DestructibleObject _destructible;
        private Rigidbody2D _rb;
        private PhysicsMaterial2D _physicsMaterial;

        public MaterialConfig Material => _material;

        private void Awake()
        {
            _destructible = GetComponent<DestructibleObject>();
            _rb = GetComponent<Rigidbody2D>();
            ApplyMaterial();
        }

        private void ApplyMaterial()
        {
            if (_material == null) return;

            // Physics
            _rb.mass = _material.density;
            _physicsMaterial = new PhysicsMaterial2D($"Mat_{_material.materialType}")
            {
                bounciness = _material.bounciness,
                friction = _material.friction
            };
            var col = GetComponent<Collider2D>();
            if (col != null) col.sharedMaterial = _physicsMaterial;

            // Visual tint in editor/dev (production art replaces this)
            if (_spriteRenderer != null)
                _spriteRenderer.color = _material.debugColor;
        }

        private void OnEnable()
        {
            DestructibleObject.OnDestroyed += HandleDestroyed;
        }

        private void OnDisable()
        {
            DestructibleObject.OnDestroyed -= HandleDestroyed;
        }

        private void HandleDestroyed(DestructibleObject obj)
        {
            if (obj != _destructible) return;
            GameLogger.Debug("CastleBlock", $"{name} ({_material?.materialType}) broke.");
        }
    }
}
