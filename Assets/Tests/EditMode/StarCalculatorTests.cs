using NUnit.Framework;
using KingSmash.Levels;
namespace KingSmash.Tests.EditMode
{
    public class StarCalculatorTests
    {
        private StarThresholds _default;

        [SetUp]
        public void SetUp()
        {
            _default = new StarThresholds
            {
                oneStar_destructionMin       = 0.30f,
                oneStar_mustRescueQueen      = true,
                twoStar_destructionMin       = 0.60f,
                twoStar_mustRescueQueen      = true,
                twoStar_enemyRatioMin        = 0.50f,
                threeStar_destructionMin     = 0.90f,
                threeStar_mustRescueQueen    = true,
                threeStar_enemyRatioMin      = 1.00f,
                threeStar_attemptsRemaining  = 1,
            };
        }

        [Test] public void NoDestruction_NullThresholds_Returns0()
            => Assert.AreEqual(0, StarCalculator.Calculate(null, 0f, 0f, true, 0));

        [Test] public void QueenNotRescued_Required_Returns0()
            => Assert.AreEqual(0, StarCalculator.Calculate(_default, 1f, 1f, false, 5));

        [Test] public void Destruction30_QueenRescued_Returns1()
            => Assert.AreEqual(1, StarCalculator.Calculate(_default, 0.30f, 0f, true, 0));

        [Test] public void Destruction29_Returns0()
            => Assert.AreEqual(0, StarCalculator.Calculate(_default, 0.29f, 0f, true, 0));

        [Test] public void Destruction60_Enemy50_Returns2()
            => Assert.AreEqual(2, StarCalculator.Calculate(_default, 0.60f, 0.50f, true, 0));

        [Test] public void Destruction60_Enemy49_Returns1()
            => Assert.AreEqual(1, StarCalculator.Calculate(_default, 0.60f, 0.49f, true, 0));

        [Test] public void ThreeStarAllMet_Returns3()
            => Assert.AreEqual(3, StarCalculator.Calculate(_default, 0.90f, 1.00f, true, 1));

        [Test] public void ThreeStar_NotEnoughAttempts_Returns2()
            => Assert.AreEqual(2, StarCalculator.Calculate(_default, 0.90f, 1.00f, true, 0));

        [Test] public void ThreeStar_EnemyRatioShort_Returns2()
            => Assert.AreEqual(2, StarCalculator.Calculate(_default, 0.90f, 0.95f, true, 1));

        [Test] public void QueenRescueNotRequired_DestructionOnly()
        {
            _default.oneStar_mustRescueQueen   = false;
            _default.twoStar_mustRescueQueen   = false;
            _default.threeStar_mustRescueQueen = false;
            Assert.AreEqual(3, StarCalculator.Calculate(_default, 0.95f, 1.00f, false, 1));
        }

        [Test] public void PerfectRun_Returns3()
            => Assert.AreEqual(3, StarCalculator.Calculate(_default, 1.00f, 1.00f, true, 3));

        [Test] public void MinimalOneStarExact_Returns1()
            => Assert.AreEqual(1, StarCalculator.Calculate(_default, 0.30f, 0.00f, true, 0));
    }
}
