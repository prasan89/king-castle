using KingSmash.Characters;

namespace KingSmash.PowerUps
{
    public interface IPowerUp
    {
        PowerUpType Type        { get; }
        string      DisplayName { get; }
        bool        IsActive    { get; }
        void Activate(KingProjectile king);
        void Deactivate();
    }
}
