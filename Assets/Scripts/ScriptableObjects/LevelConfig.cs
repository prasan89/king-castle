using System;
using System.Collections.Generic;
using UnityEngine;

namespace KingSmash.Levels
{
    [Serializable]
    public class LevelObjective
    {
        public string description;
        public int requiredScore;
        public int threeStarScore;
        public int twoStarScore;
        public bool mustFreeQueen = true;
    }

    [CreateAssetMenu(fileName = "LevelConfig", menuName = "KingSmash/Config/LevelConfig")]
    public class LevelConfig : ScriptableObject
    {
        [Header("Identity")]
        public int levelIndex;
        public int worldIndex;
        public string displayName;
        [TextArea(2, 4)] public string description;

        [Header("Objectives")]
        public LevelObjective objective = new();

        [Header("Resources")]
        public int kingLaunches = 3;
        public int startingCoins = 0;
        public List<string> availablePowerUps = new();

        [Header("Scene")]
        public string sceneName;

        [Header("Difficulty")]
        [Range(1, 10)] public int difficultyRating = 1;

        [Header("Unlock")]
        public int requiredStarsToUnlock = 0;

        public int GetStarsForScore(int score)
        {
            if (score >= objective.threeStarScore) return 3;
            if (score >= objective.twoStarScore) return 2;
            if (score >= objective.requiredScore) return 1;
            return 0;
        }

        public bool IsValid()
        {
            return levelIndex >= 0
                && worldIndex >= 0
                && objective != null
                && objective.threeStarScore >= objective.twoStarScore
                && objective.twoStarScore >= objective.requiredScore
                && objective.requiredScore > 0
                && kingLaunches > 0;
        }
    }
}
