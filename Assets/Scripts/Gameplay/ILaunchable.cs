using UnityEngine;

namespace KingSmash.Gameplay
{
    public interface ILaunchable
    {
        void Launch(Vector2 direction, float power);
        bool IsLaunched { get; }
        bool HasLanded { get; }
    }
}
