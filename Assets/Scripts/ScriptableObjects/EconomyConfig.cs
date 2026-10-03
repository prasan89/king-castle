using System;
using System.Collections.Generic;
using UnityEngine;

namespace KingSmash.Economy
{
    [Serializable]
    public class UpgradeTier
    {
        public int level;
        public long coinCost;
        public int gemCost;
        public string description;
    }

    [CreateAssetMenu(fileName = "EconomyConfig", menuName = "KingSmash/Config/EconomyConfig")]
    public class EconomyConfig : ScriptableObject
    {
        [Header("Base Rewards")]
        public long coinsPerStar = 25;
        public long bonusCoinsThreeStars = 100;
        public int gemsFromFirstComplete = 2;

        [Header("King Upgrades")]
        public List<UpgradeTier> kingPowerUpgrades = new();
        public List<UpgradeTier> kingSpeedUpgrades = new();
        public List<UpgradeTier> kingSmashUpgrades = new();
        public List<UpgradeTier> kingArmorUpgrades = new();

        [Header("Coins per Action")]
        public long coinsPerStructureDestroyed = 5;
        public long coinsPerEnemyKilled = 10;

        [Header("Ad Rewards")]
        public long rewardedAdCoins = 100;
        public long rewardedAdGems = 1;

        public long CalculateLevelReward(int starsEarned)
        {
            return starsEarned * coinsPerStar + (starsEarned == 3 ? bonusCoinsThreeStars : 0);
        }
    }
}
