using System;

namespace KingSmash.Progression
{
    public readonly struct KingStats
    {
        public readonly float Power;
        public readonly float Speed;
        public readonly float SmashRadius;
        public readonly float Armor;

        public KingStats(float power, float speed, float smashRadius, float armor)
        {
            Power       = power;
            Speed       = speed;
            SmashRadius = smashRadius;
            Armor       = armor;
        }

        public static KingStats Zero => new KingStats(0f, 0f, 0f, 0f);
    }

    [Serializable]
    public class KingProgression
    {
        public const int MaxKingLevel = 20;
        public const int MaxStatLevel = 10;

        public int  KingLevel        = 1;
        public long KingXP           = 0;
        public int  PowerLevel       = 1;
        public int  SpeedLevel       = 1;
        public int  SmashRadiusLevel = 1;
        public int  ArmorLevel       = 1;

        public bool IsMaxKingLevel          => KingLevel >= MaxKingLevel;
        public bool IsMaxStat(int statLevel) => statLevel >= MaxStatLevel;

        public int GetStatLevel(KingStat stat) => stat switch
        {
            KingStat.Power       => PowerLevel,
            KingStat.Speed       => SpeedLevel,
            KingStat.SmashRadius => SmashRadiusLevel,
            KingStat.Armor       => ArmorLevel,
            _                    => 1,
        };

        public void SetStatLevel(KingStat stat, int level)
        {
            switch (stat)
            {
                case KingStat.Power:       PowerLevel       = level; break;
                case KingStat.Speed:       SpeedLevel       = level; break;
                case KingStat.SmashRadius: SmashRadiusLevel = level; break;
                case KingStat.Armor:       ArmorLevel       = level; break;
            }
        }
    }
}
