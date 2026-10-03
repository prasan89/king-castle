using UnityEngine;
namespace KingSmash.Gameplay
{
    public interface IDamageReceiver
    {
        void ReceiveDamage(IDamageSource source);
        bool IsAlive { get; }
    }
}
