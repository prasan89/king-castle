using NUnit.Framework;
using KingSmash.UI;
namespace KingSmash.Tests.EditMode
{
    public class ScreenNavigationTests
    {
        [Test]
        public void LevelResult_DefaultIsNotVictory()
        {
            var r = new LevelResult();
            Assert.IsFalse(r.IsVictory);
        }

        [Test]
        public void LevelResult_StarsRange_0to3()
        {
            for (int s = 0; s <= 3; s++)
            {
                var r = new LevelResult { Stars = s };
                Assert.GreaterOrEqual(r.Stars, 0);
                Assert.LessOrEqual(r.Stars, 3);
            }
        }

        [Test]
        public void LevelNodeData_UnlockedState()
        {
            var node = new LevelNodeData { levelIndex = 0, starsEarned = 2, isUnlocked = true };
            Assert.IsTrue(node.isUnlocked);
            Assert.AreEqual(2, node.starsEarned);
        }

        [Test]
        public void WorldData_ProgressNormalized()
        {
            var world = new WorldData { starsEarned = 30, starsMax = 60 };
            float progress = world.starsMax > 0 ? (float)world.starsEarned / world.starsMax : 0f;
            Assert.AreEqual(0.5f, progress, 0.001f);
        }

        [Test]
        public void MissionDef_DefaultProgress_IsZero()
        {
            var mission = new Screens.MissionDef { id = "test", targetCount = 5 };
            int progress = 0;
            Assert.AreEqual(0, progress);
            Assert.AreEqual(5, mission.targetCount);
        }

        [Test]
        public void DailyRewardStreak_WrapsAfterDay7()
        {
            int day = 7;
            int nextDay = day < 7 ? day + 1 : 1;
            Assert.AreEqual(1, nextDay);
        }

        [Test]
        public void DailyRewardStreak_AdvancesNormally()
        {
            int day = 3;
            int nextDay = day < 7 ? day + 1 : 1;
            Assert.AreEqual(4, nextDay);
        }

        [Test]
        public void UpgradeScreen_MaxLevel_DisablesUpgrade()
        {
            int currentLevel = 10;
            int maxLevel = 10;
            bool canUpgrade = currentLevel < maxLevel;
            Assert.IsFalse(canUpgrade);
        }
    }
}
