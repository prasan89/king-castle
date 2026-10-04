using System;

namespace KingSmash.PowerUps
{
    [Serializable]
    public class PowerUpInventoryEntry
    {
        public string powerUpTypeId;
        public int    count;

        public PowerUpType GetPowerUpType() =>
            (PowerUpType)Enum.Parse(typeof(PowerUpType), powerUpTypeId);
    }
}
