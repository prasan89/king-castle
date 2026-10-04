using System;
using System.Collections.Generic;
using UnityEngine;

namespace KingSmash.Progression
{
    [CreateAssetMenu(fileName = "KingUpgradeConfig", menuName = "KingSmash/Config/KingUpgradeConfig")]
    public class KingUpgradeConfig : ScriptableObject
    {
        [Serializable]
        public class KingLevelRow
        {
            public int   kingLevel;
            public long  xpRequired;
            public long  upgradeCostCoins;
            public float power;
            public float speed;
            public float smashRadius;
            public float armor;
        }

        [Serializable]
        public class StatUpgradeRow
        {
            public int    statLevel;
            public long   coinCost;
            public float  valueIncrease;
            public string displayLabel;
        }

        [Header("King Level Table")]
        public List<KingLevelRow> kingLevels = new List<KingLevelRow>();

        [Header("Stat Upgrade Tables")]
        public List<StatUpgradeRow> powerUpgrades       = new List<StatUpgradeRow>();
        public List<StatUpgradeRow> speedUpgrades       = new List<StatUpgradeRow>();
        public List<StatUpgradeRow> smashRadiusUpgrades = new List<StatUpgradeRow>();
        public List<StatUpgradeRow> armorUpgrades       = new List<StatUpgradeRow>();

        [Header("Limits")]
        public int maxKingLevel = 20;
        public int maxStatLevel = 10;

        // Properties used by services (alias the inspector fields)
        public int MaxKingLevel => maxKingLevel;
        public int MaxStatLevel => maxStatLevel;

        [Header("XP Rewards")]
        public long xpPerLevelComplete        = 200;
        public long xpPerEnemyKilled          = 25;
        public long xpPerQueenRescued         = 100;
        public long xpBonusOneStar            = 50;
        public long xpBonusTwoStar            = 150;
        public long xpBonusThreeStar          = 300;
        public long xpDestructionPercentBonus = 1;

        public KingLevelRow GetKingLevelRow(int kingLevel)
        {
            foreach (var row in kingLevels)
                if (row.kingLevel == kingLevel) return row;
            return null;
        }

        public StatUpgradeRow GetStatRow(List<StatUpgradeRow> list, int currentStatLevel)
        {
            if (currentStatLevel >= maxStatLevel) return null;
            int target = currentStatLevel + 1;
            foreach (var row in list)
                if (row.statLevel == target) return row;
            return null;
        }

        public StatUpgradeRow GetUpgradeRow(KingStat stat, int currentStatLevel)
        {
            return stat switch
            {
                KingStat.Power       => GetStatRow(powerUpgrades,       currentStatLevel),
                KingStat.Speed       => GetStatRow(speedUpgrades,       currentStatLevel),
                KingStat.SmashRadius => GetStatRow(smashRadiusUpgrades, currentStatLevel),
                KingStat.Armor       => GetStatRow(armorUpgrades,       currentStatLevel),
                _                    => null,
            };
        }

        public List<StatUpgradeRow> GetStatList(KingStat stat) => stat switch
        {
            KingStat.Power       => powerUpgrades,
            KingStat.Speed       => speedUpgrades,
            KingStat.SmashRadius => smashRadiusUpgrades,
            KingStat.Armor       => armorUpgrades,
            _                    => null,
        };

        public KingStats GetKingStatsAtLevel(int kingLevel)
        {
            var row = GetKingLevelRow(kingLevel);
            if (row == null) return KingStats.Zero;
            return new KingStats(row.power, row.speed, row.smashRadius, row.armor);
        }
    }
}
