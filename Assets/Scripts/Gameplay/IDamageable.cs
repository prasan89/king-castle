using UnityEngine;

namespace KingSmash.Gameplay
{
    public interface IDamageable
    {
        float CurrentHealth { get; }
        float MaxHealth { get; }
        bool IsDestroyed { get; }
        void TakeDamage(float damage, Vector2 impactPoint, Vector2 impactForce);
        void InstantDestroy();
    }
}
