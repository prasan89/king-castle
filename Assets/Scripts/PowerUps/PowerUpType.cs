using System;

namespace KingSmash.PowerUps
{
    public enum PowerUpType
    {
        None      = 0,
        Bomb      = 1,
        Fire      = 2,
        Ice       = 3,
        Lightning = 4,
        MegaKing  = 5
    }

    public static class PowerUpTypeExtensions
    {
        public static string ToId(this PowerUpType t) => t switch
        {
            PowerUpType.Bomb      => "powerup_bomb",
            PowerUpType.Fire      => "powerup_fire",
            PowerUpType.Ice       => "powerup_ice",
            PowerUpType.Lightning => "powerup_lightning",
            PowerUpType.MegaKing  => "powerup_megaking",
            _                     => string.Empty
        };

        public static PowerUpType FromId(string id) => id switch
        {
            "powerup_bomb"      => PowerUpType.Bomb,
            "powerup_fire"      => PowerUpType.Fire,
            "powerup_ice"       => PowerUpType.Ice,
            "powerup_lightning" => PowerUpType.Lightning,
            "powerup_megaking"  => PowerUpType.MegaKing,
            _                   => PowerUpType.None
        };

        public static string DisplayName(this PowerUpType t) => t switch
        {
            PowerUpType.Bomb      => "Bomb",
            PowerUpType.Fire      => "Fire Blaze",
            PowerUpType.Ice       => "Ice Freeze",
            PowerUpType.Lightning => "Lightning Strike",
            PowerUpType.MegaKing  => "Mega King",
            _                     => "None"
        };

        public static string Description(this PowerUpType t) => t switch
        {
            PowerUpType.Bomb      => "Triggers a massive area explosion on impact, destroying blocks in a wide radius.",
            PowerUpType.Fire      => "Ignites nearby blocks on landing, causing them to burn and crumble over time.",
            PowerUpType.Ice       => "Freezes nearby enemies solid on contact, leaving them immobile and vulnerable.",
            PowerUpType.Lightning => "Unleashes a chain of lightning bolts that stun multiple enemies in sequence.",
            PowerUpType.MegaKing  => "Transforms the King into a giant with greatly increased mass and destructive force.",
            _                     => string.Empty
        };
    }

    [Serializable]
    public class PowerUpSlot
    {
        public PowerUpType Type;
        public int Count;
    }
}
