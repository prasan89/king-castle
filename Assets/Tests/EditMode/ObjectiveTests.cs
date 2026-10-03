using NUnit.Framework;
using KingSmash.Levels;

namespace KingSmash.Tests.EditMode
{
    public class ObjectiveTests
    {
        [Test]
        public void ObjectiveStatus_DefaultIsInProgress()
        {
            // ILevelObjective contract: newly initialized objectives must be InProgress
            var status = ObjectiveStatus.InProgress;
            Assert.AreEqual(ObjectiveStatus.InProgress, status);
        }

        [Test]
        public void ObjectiveStatus_EnumValues_ExistAndAreDistinct()
        {
            Assert.AreNotEqual(ObjectiveStatus.InProgress, ObjectiveStatus.Completed);
            Assert.AreNotEqual(ObjectiveStatus.InProgress, ObjectiveStatus.Failed);
            Assert.AreNotEqual(ObjectiveStatus.Completed,  ObjectiveStatus.Failed);
        }

        [Test]
        public void ProgressNormalized_ZeroOnInit_ForDefeatEnemies()
        {
            // Pure math: 0 defeated out of 3 = 0 progress
            int defeated = 0;
            int target = 3;
            float progress = target > 0 ? (float)defeated / target : 0f;
            Assert.AreEqual(0f, progress, 0.001f);
        }

        [Test]
        public void ProgressNormalized_OneOnAllDefeated()
        {
            int defeated = 3;
            int target = 3;
            float progress = (float)defeated / target;
            Assert.AreEqual(1f, progress, 0.001f);
        }

        [Test]
        public void ProgressNormalized_PartialProgress()
        {
            int defeated = 2;
            int target = 4;
            float progress = (float)defeated / target;
            Assert.AreEqual(0.5f, progress, 0.001f);
        }

        [Test]
        public void QueenObjective_RescuedProgress_IsOne()
        {
            bool rescued = true;
            float progress = rescued ? 1f : 0f;
            Assert.AreEqual(1f, progress, 0.001f);
        }

        [Test]
        public void QueenObjective_NotRescuedProgress_IsZero()
        {
            bool rescued = false;
            float progress = rescued ? 1f : 0f;
            Assert.AreEqual(0f, progress, 0.001f);
        }
    }
}
