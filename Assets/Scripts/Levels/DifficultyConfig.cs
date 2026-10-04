using System;
using System.Collections.Generic;
using UnityEngine;
namespace KingSmash.Levels
{
    public enum DifficultyTier { Tutorial, Easy, Normal, Hard, VeryHard, Boss }

    [Serializable]
    public class DifficultyProfile
    {
        public DifficultyTier tier;
        public int            ratingMin;    // 1-10 scale
        public int            ratingMax;
        public string         displayName;
        public Color          uiColor;

        // Design guidance (no runtime effect — drives level designer decisions)
        public int    RecommendedKingLaunches;
        public int    RecommendedEnemyCount;
        public bool   AllowEnemyKing;
        public bool   AllowExplosiveChains;
        public int    RecommendedCastleFloors;
        public float  TargetDestructionFor3Star; // 0..1
    }

    [CreateAssetMenu(fileName = "DifficultyConfig", menuName = "KingSmash/Config/DifficultyConfig")]
    public class DifficultyConfig : ScriptableObject
    {
        [SerializeField] private List<DifficultyProfile> _profiles = new()
        {
            new DifficultyProfile { tier = DifficultyTier.Tutorial, ratingMin = 1, ratingMax = 1,
                displayName = "Tutorial", uiColor = new Color(0.5f, 1f, 0.5f),
                RecommendedKingLaunches = 5, RecommendedEnemyCount = 1, AllowEnemyKing = false,
                AllowExplosiveChains = false, RecommendedCastleFloors = 1, TargetDestructionFor3Star = 0.80f },
            new DifficultyProfile { tier = DifficultyTier.Easy, ratingMin = 2, ratingMax = 3,
                displayName = "Easy", uiColor = new Color(0.6f, 0.9f, 0.4f),
                RecommendedKingLaunches = 4, RecommendedEnemyCount = 2, AllowEnemyKing = false,
                AllowExplosiveChains = false, RecommendedCastleFloors = 2, TargetDestructionFor3Star = 0.85f },
            new DifficultyProfile { tier = DifficultyTier.Normal, ratingMin = 4, ratingMax = 5,
                displayName = "Normal", uiColor = new Color(1f, 0.85f, 0.2f),
                RecommendedKingLaunches = 4, RecommendedEnemyCount = 3, AllowEnemyKing = false,
                AllowExplosiveChains = true, RecommendedCastleFloors = 2, TargetDestructionFor3Star = 0.88f },
            new DifficultyProfile { tier = DifficultyTier.Hard, ratingMin = 6, ratingMax = 7,
                displayName = "Hard", uiColor = new Color(1f, 0.55f, 0.1f),
                RecommendedKingLaunches = 3, RecommendedEnemyCount = 4, AllowEnemyKing = false,
                AllowExplosiveChains = true, RecommendedCastleFloors = 3, TargetDestructionFor3Star = 0.90f },
            new DifficultyProfile { tier = DifficultyTier.VeryHard, ratingMin = 8, ratingMax = 9,
                displayName = "Very Hard", uiColor = new Color(0.9f, 0.2f, 0.2f),
                RecommendedKingLaunches = 3, RecommendedEnemyCount = 5, AllowEnemyKing = true,
                AllowExplosiveChains = true, RecommendedCastleFloors = 3, TargetDestructionFor3Star = 0.90f },
            new DifficultyProfile { tier = DifficultyTier.Boss, ratingMin = 10, ratingMax = 10,
                displayName = "Boss", uiColor = new Color(0.6f, 0.0f, 0.8f),
                RecommendedKingLaunches = 5, RecommendedEnemyCount = 6, AllowEnemyKing = true,
                AllowExplosiveChains = true, RecommendedCastleFloors = 4, TargetDestructionFor3Star = 0.85f },
        };

        public DifficultyProfile GetProfile(int difficultyRating)
        {
            DifficultyProfile best = _profiles.Count > 0 ? _profiles[0] : null;
            foreach (var p in _profiles)
                if (difficultyRating >= p.ratingMin && difficultyRating <= p.ratingMax) return p;
            return best;
        }

        public DifficultyTier GetTier(int difficultyRating)
            => GetProfile(difficultyRating)?.tier ?? DifficultyTier.Normal;
    }
}
