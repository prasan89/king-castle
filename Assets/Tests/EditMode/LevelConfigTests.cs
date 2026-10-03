using NUnit.Framework;
using UnityEngine;
using KingSmash.Levels;

namespace KingSmash.Tests.EditMode
{
    public class LevelConfigTests
    {
        private LevelConfig MakeValidConfig(int levelIndex = 0, int worldIndex = 0)
        {
            var cfg = ScriptableObject.CreateInstance<LevelConfig>();
            cfg.levelIndex = levelIndex;
            cfg.worldIndex = worldIndex;
            cfg.kingLaunches = 3;
            cfg.objective = new LevelObjective
            {
                requiredScore = 1000,
                twoStarScore = 2000,
                threeStarScore = 3000,
                mustFreeQueen = true
            };
            return cfg;
        }

        [Test]
        public void ValidConfig_IsValid()
        {
            var cfg = MakeValidConfig();
            Assert.IsTrue(cfg.IsValid());
            Object.DestroyImmediate(cfg);
        }

        [Test]
        public void ZeroLaunches_IsInvalid()
        {
            var cfg = MakeValidConfig();
            cfg.kingLaunches = 0;
            Assert.IsFalse(cfg.IsValid());
            Object.DestroyImmediate(cfg);
        }

        [Test]
        public void InvertedStarThresholds_IsInvalid()
        {
            var cfg = MakeValidConfig();
            cfg.objective.twoStarScore = 5000;
            cfg.objective.threeStarScore = 1000;
            Assert.IsFalse(cfg.IsValid());
            Object.DestroyImmediate(cfg);
        }

        [Test]
        public void GetStarsForScore_ReturnsCorrectStars()
        {
            var cfg = MakeValidConfig();
            Assert.AreEqual(0, cfg.GetStarsForScore(500));
            Assert.AreEqual(1, cfg.GetStarsForScore(1000));
            Assert.AreEqual(2, cfg.GetStarsForScore(2000));
            Assert.AreEqual(3, cfg.GetStarsForScore(3000));
            Object.DestroyImmediate(cfg);
        }
    }
}
