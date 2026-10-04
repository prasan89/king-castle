using System;
using System.Collections.Generic;
using UnityEngine;

namespace KingSmash.Economy
{
    [Serializable]
    public class UpgradeTier
    {
        public int    level;
        public long   coinCost;
        public int    gemCost;
        public string description;
    }

    [Serializable]
    public class DestructionBonus
    {
        public float threshold;
        public long  bonusCoins;
    }

    [Serializable]
    public class WorldReward
    {
        public int    worldIndex;
        public long   coins;
        public int    gems;
        public string powerUpTypeId;
        public int    powerUpCount;
    }

    [Serializable]
    public class PowerUpPrice
    {
        public string powerUpTypeId;
        public long   coinCost;
        public int    gemCost;
    }

    [CreateAssetMenu(fileName = "EconomyConfig", menuName = "KingSmash/Config/EconomyConfig")]
    public class EconomyConfig : ScriptableObject
    {
        [Header("Base Rewards")]
        public long coinsPerStar          = 25;
        public long bonusCoinsThreeStars  = 100;
        public int  gemsFromFirstComplete = 2;

        [Header("King Upgrades")]
        public List<UpgradeTier> kingPowerUpgrades = new();
        public List<UpgradeTier> kingSpeedUpgrades = new();
        public List<UpgradeTier> kingSmashUpgrades = new();
        public List<UpgradeTier> kingArmorUpgrades = new();

        [Header("Coins per Action")]
        public long coinsPerStructureDestroyed = 5;
        public long coinsPerEnemyKilled        = 10;

        [Header("Ad Rewards")]
        public long rewardedAdCoins = 100;
        public long rewardedAdGems  = 1;

        [Header("Level Base Rewards")]
        public long baseLevelCoinsMin         = 100;
        public long baseLevelCoinsMax         = 500;
        public long levelCoinScalePer10Levels = 50;

        [Header("Destruction Bonus")]
        public List<DestructionBonus> destructionBonuses = new()
        {
            new DestructionBonus { threshold = 0f,   bonusCoins = 0   },
            new DestructionBonus { threshold = 0.5f, bonusCoins = 50  },
            new DestructionBonus { threshold = 0.7f, bonusCoins = 150 },
            new DestructionBonus { threshold = 0.9f, bonusCoins = 300 }
        };

        [Header("Enemy Rewards")]
        public long guardKillCoins       = 20;
        public long shieldGuardKillCoins = 30;
        public long archerKillCoins      = 35;
        public long enemyKingKillCoins   = 100;

        [Header("Star Bonuses")]
        public long oneStarBonus   = 0;
        public long twoStarBonus   = 50;
        public long threeStarBonus = 200;

        [Header("Special Rewards")]
        public long queenRescueBonus = 100;

        [Header("World Completion")]
        public List<WorldReward> worldRewards = new()
        {
            new WorldReward { worldIndex = 0, coins = 500,  gems = 5,  powerUpTypeId = "",                 powerUpCount = 0 },
            new WorldReward { worldIndex = 1, coins = 750,  gems = 7,  powerUpTypeId = "powerup_bomb",     powerUpCount = 1 },
            new WorldReward { worldIndex = 2, coins = 1000, gems = 10, powerUpTypeId = "powerup_fire",     powerUpCount = 1 },
            new WorldReward { worldIndex = 3, coins = 1500, gems = 12, powerUpTypeId = "powerup_ice",      powerUpCount = 1 },
            new WorldReward { worldIndex = 4, coins = 2000, gems = 15, powerUpTypeId = "powerup_megaking", powerUpCount = 1 }
        };

        [Header("Power-Up Prices")]
        public List<PowerUpPrice> powerUpPrices = new()
        {
            new PowerUpPrice { powerUpTypeId = "powerup_bomb",      coinCost = 100, gemCost = 0 },
            new PowerUpPrice { powerUpTypeId = "powerup_fire",      coinCost = 150, gemCost = 0 },
            new PowerUpPrice { powerUpTypeId = "powerup_ice",       coinCost = 150, gemCost = 0 },
            new PowerUpPrice { powerUpTypeId = "powerup_lightning", coinCost = 200, gemCost = 0 },
            new PowerUpPrice { powerUpTypeId = "powerup_megaking",  coinCost = 300, gemCost = 0 }
        };

        public long CalculateLevelReward(int starsEarned)
        {
            return starsEarned * coinsPerStar + (starsEarned == 3 ? bonusCoinsThreeStars : 0);
        }

        public long GetDestructionBonus(float ratio)
        {
            long best = 0;
            foreach (var entry in destructionBonuses)
            {
                if (ratio >= entry.threshold)
                    best = entry.bonusCoins;
            }
            return best;
        }

        public long GetEnemyKillReward(string enemyType)
        {
            return enemyType switch
            {
                "Guard"       => guardKillCoins,
                "ShieldGuard" => shieldGuardKillCoins,
                "Archer"      => archerKillCoins,
                "EnemyKing"   => enemyKingKillCoins,
                _             => coinsPerEnemyKilled
            };
        }

        public WorldReward GetWorldReward(int worldIndex)
        {
            foreach (var entry in worldRewards)
                if (entry.worldIndex == worldIndex)
                    return entry;
            return null;
        }

        public PowerUpPrice GetPowerUpPrice(string powerUpTypeId)
        {
            foreach (var entry in powerUpPrices)
                if (entry.powerUpTypeId == powerUpTypeId)
                    return entry;
            return null;
        }
    }
}
