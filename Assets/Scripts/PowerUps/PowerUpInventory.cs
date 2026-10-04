using System;
using System.Collections.Generic;

namespace KingSmash.PowerUps
{
    [Serializable]
    public class PowerUpInventory
    {
        public List<PowerUpInventoryEntry> entries = new List<PowerUpInventoryEntry>();

        public int GetCount(PowerUpType type)
        {
            string id = type.ToString();
            foreach (var entry in entries)
                if (entry.powerUpTypeId == id) return entry.count;
            return 0;
        }

        public void Add(PowerUpType type, int amount)
        {
            if (amount <= 0) return;
            string id = type.ToString();
            foreach (var entry in entries)
            {
                if (entry.powerUpTypeId == id) { entry.count += amount; return; }
            }
            entries.Add(new PowerUpInventoryEntry { powerUpTypeId = id, count = amount });
        }

        public bool TryConsume(PowerUpType type)
        {
            string id = type.ToString();
            foreach (var entry in entries)
            {
                if (entry.powerUpTypeId != id) continue;
                if (entry.count <= 0) return false;
                entry.count--;
                return true;
            }
            return false;
        }

        public bool Has(PowerUpType type) => GetCount(type) > 0;
    }
}
