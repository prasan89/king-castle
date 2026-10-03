using UnityEngine;
namespace KingSmash.Gameplay
{
    public interface IDamageSource
    {
        float DamageAmount { get; }
        Vector2 ImpactPoint { get; }
        Vector2 ImpactForce { get; }
        GameObject SourceObject { get; }
    }
}
