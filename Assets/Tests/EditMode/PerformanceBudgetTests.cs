// PerformanceBudgetTests.cs — M15 edit-mode sanity tests for PerformanceBudget constants.

using KingSmash.Performance;
using NUnit.Framework;

namespace KingSmash.Tests.EditMode
{
    [TestFixture]
    public class PerformanceBudgetTests
    {
        [Test]
        public void Budget_TargetFrameTimeIs16ms()
        {
            Assert.Greater(PerformanceBudget.TargetFrameTime, 16f,
                "TargetFrameTime should be greater than 16 ms (≈16.667 ms for 60 fps).");
            Assert.Less(PerformanceBudget.TargetFrameTime, 17f,
                "TargetFrameTime should be less than 17 ms (≈16.667 ms for 60 fps).");
        }

        [Test]
        public void Budget_LowEndFrameTimeIs33ms()
        {
            Assert.Greater(PerformanceBudget.LowEndFrameTime, 33f,
                "LowEndFrameTime should be greater than 33 ms (≈33.333 ms for 30 fps).");
            Assert.Less(PerformanceBudget.LowEndFrameTime, 34f,
                "LowEndFrameTime should be less than 34 ms (≈33.333 ms for 30 fps).");
        }

        [Test]
        public void Budget_LowEndSlowerThanTarget()
        {
            Assert.Greater(PerformanceBudget.LowEndFrameTime, PerformanceBudget.TargetFrameTime,
                "Low-end frame budget must be longer (slower) than the high-end target.");
        }

        [Test]
        public void Budget_MaxDebrisReasonable()
        {
            Assert.Greater(PerformanceBudget.MaxDebrisParticlesActive, 0,
                "MaxDebrisParticlesActive must allow at least one particle.");
            Assert.LessOrEqual(PerformanceBudget.MaxDebrisParticlesActive, 200,
                "MaxDebrisParticlesActive should not exceed 200 (mobile performance risk).");
        }

        [Test]
        public void Budget_AudioSourcePoolMatchesService()
        {
            Assert.AreEqual(8, PerformanceBudget.MaxAudioSourcesActive,
                "MaxAudioSourcesActive must be exactly 8 to match AudioService's fixed pool size.");
        }

        [Test]
        public void Budget_StartupBudgetReasonable()
        {
            Assert.GreaterOrEqual(PerformanceBudget.StartupBudgetMs, 3000,
                "StartupBudgetMs should allow at least 3 s for cold launch.");
            Assert.LessOrEqual(PerformanceBudget.StartupBudgetMs, 10000,
                "StartupBudgetMs should not exceed 10 s (user experience threshold).");
        }
    }
}
