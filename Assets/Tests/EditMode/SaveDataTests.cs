using NUnit.Framework;
using UnityEngine;
using KingSmash.Save;

namespace KingSmash.Tests.EditMode
{
    public class SaveDataTests
    {
        [Test]
        public void CreateNew_GeneratesUniquePlayerIds()
        {
            var a = SaveData.CreateNew();
            var b = SaveData.CreateNew();
            Assert.AreNotEqual(a.playerId, b.playerId, "CreateNew should produce unique player IDs.");
        }

        [Test]
        public void CreateNew_DefaultsToVersionCurrent()
        {
            var data = SaveData.CreateNew();
            Assert.AreEqual(SaveData.CurrentVersion, data.version);
        }

        [Test]
        public void CreateNew_StartsAtLevelZero()
        {
            var data = SaveData.CreateNew();
            Assert.AreEqual(0, data.currentLevel);
            Assert.AreEqual(0, data.coins);
            Assert.AreEqual(0, data.gems);
        }

        [Test]
        public void SaveData_SerializesAndDeserializes_WithJsonUtility()
        {
            var original = SaveData.CreateNew();
            original.coins = 12345;
            original.gems = 7;
            original.kingLevel = 3;
            original.completedLevels.Add(0);
            original.completedLevels.Add(1);

            var json = JsonUtility.ToJson(original);
            var restored = JsonUtility.FromJson<SaveData>(json);

            Assert.AreEqual(original.playerId, restored.playerId);
            Assert.AreEqual(12345, restored.coins);
            Assert.AreEqual(7, restored.gems);
            Assert.AreEqual(3, restored.kingLevel);
            Assert.AreEqual(2, restored.completedLevels.Count);
        }

        [Test]
        public void SaveMigrator_SameVersion_ReturnsUnmodified()
        {
            var data = SaveData.CreateNew();
            data.coins = 999;
            var result = SaveMigrator.Migrate(data);
            Assert.AreEqual(data, result);
            Assert.AreEqual(999, result.coins);
        }

        [Test]
        public void StarsPerLevel_SerializesAndRoundTrips()
        {
            var data = SaveData.CreateNew();
            data.SetStarsForLevel(0, 3);
            data.SetStarsForLevel(1, 2);

            var json = JsonUtility.ToJson(data);
            var restored = JsonUtility.FromJson<SaveData>(json);

            Assert.AreEqual(3, restored.GetStarsForLevel(0));
            Assert.AreEqual(2, restored.GetStarsForLevel(1));
            Assert.AreEqual(0, restored.GetStarsForLevel(99));
        }

        [Test]
        public void GetTotalStars_SumsAllEntries()
        {
            var data = SaveData.CreateNew();
            data.SetStarsForLevel(0, 3);
            data.SetStarsForLevel(1, 2);
            data.SetStarsForLevel(2, 1);
            Assert.AreEqual(6, data.GetTotalStars());
        }
    }
}
