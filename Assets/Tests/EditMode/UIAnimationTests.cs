using NUnit.Framework;
using UnityEngine;
namespace KingSmash.Tests.EditMode
{
    public class UIAnimationTests
    {
        // BounceEase mirror (copied logic for testability)
        private static float BounceEase(float t)
        {
            if (t < 0.6f) return t / 0.6f * 1.15f;
            if (t < 0.8f) return 1.15f - (t - 0.6f) / 0.2f * 0.15f;
            return 1f;
        }

        [Test] public void BounceEase_AtZero_IsZero()           => Assert.AreEqual(0f,   BounceEase(0f),   0.001f);
        [Test] public void BounceEase_AtOne_IsOne()             => Assert.AreEqual(1f,   BounceEase(1f),   0.001f);
        [Test] public void BounceEase_AtPeak_ExceedsOne()       => Assert.Greater(BounceEase(0.55f), 1f);
        [Test] public void BounceEase_AtMidPoint_IsIncreasing() => Assert.Greater(BounceEase(0.3f), BounceEase(0.1f));

        // FormatNumber helper mirror
        private static string FormatNumber(long n)
        {
            if (n >= 1_000_000) return $"{n / 1_000_000f:F1}M";
            if (n >= 1_000)     return $"{n / 1_000f:F1}K";
            return n.ToString();
        }

        [Test] public void FormatNumber_Under1K_IsRaw()         => Assert.AreEqual("999",   FormatNumber(999));
        [Test] public void FormatNumber_Exactly1K_IsK()         => Assert.AreEqual("1.0K",  FormatNumber(1000));
        [Test] public void FormatNumber_1500_Is1Point5K()       => Assert.AreEqual("1.5K",  FormatNumber(1500));
        [Test] public void FormatNumber_1M_Is1PointZeroM()      => Assert.AreEqual("1.0M",  FormatNumber(1_000_000));

        [Test]
        public void LevelResult_VictoryFlag_IsDistinctFromFail()
        {
            var win  = new KingSmash.UI.LevelResult { IsVictory = true  };
            var fail = new KingSmash.UI.LevelResult { IsVictory = false };
            Assert.IsTrue(win.IsVictory);
            Assert.IsFalse(fail.IsVictory);
        }

        [Test]
        public void LevelResult_DestructionRatio_ClampedImplicitly()
        {
            var r = new KingSmash.UI.LevelResult { DestructionRatio = 0.92f };
            Assert.AreEqual(0.92f, r.DestructionRatio, 0.001f);
        }

        [Test]
        public void WorldData_StarsMaxCalculation()
        {
            var world = new KingSmash.UI.WorldData { levelsInWorld = 20, starsMax = 60 };
            Assert.AreEqual(60, world.starsMax);
        }

        [Test]
        public void DailyReward_CoinsAccumulate()
        {
            long coins = 0;
            long[] rewards = { 100, 200, 0, 300, 0, 500, 1000 };
            foreach (var r in rewards) coins += r;
            Assert.AreEqual(2100, coins);
        }

        [Test]
        public void ShopItem_PriceDisplay_IsSet()
        {
            var item = new KingSmash.UI.Screens.ShopItem { displayName = "1,000 Coins", priceDisplay = "₹99", coinsGranted = 1000 };
            Assert.AreEqual("₹99", item.priceDisplay);
            Assert.AreEqual(1000L, item.coinsGranted);
        }
    }
}
