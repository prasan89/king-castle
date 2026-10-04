// AdRewardGuardTests.cs — EditMode tests for ad reward integrity, interstitial policy,
// and analytics rate guarding.

using System;
using System.Reflection;
using NUnit.Framework;
using KingSmash.Ads;
using KingSmash.Analytics;
using KingSmash.Config;
using KingSmash.Services;
using UnityEngine;

namespace KingSmash.Tests.EditMode
{
    [TestFixture]
    public class AdRewardGuardTests
    {
        // ── AdRewardResult contract ───────────────────────────────────────────

        [Test]
        public void AdRewardResult_Completed_IsRewarded()
        {
            var result = new AdRewardResult
            {
                WasRewarded  = true,
                CoinsGranted = 100L
            };

            Assert.IsTrue(result.WasRewarded);
            Assert.AreEqual(100L, result.CoinsGranted);
        }

        [Test]
        public void AdRewardResult_NotCompleted_NoReward()
        {
            var result = new AdRewardResult
            {
                WasRewarded  = false,
                CoinsGranted = 0L
            };

            Assert.IsFalse(result.WasRewarded);
            Assert.AreEqual(0L, result.CoinsGranted);
        }

        // ── InterstitialPolicy — level-gap enforcement ────────────────────────

        [Test]
        public void InterstitialPolicy_BelowMinLevels_Blocked()
        {
            // Default AdConfiguration has interstitialMinLevels = 3.
            var config        = ScriptableObject.CreateInstance<AdConfiguration>();
            var configService = new RemoteConfigServiceMock();
            var policy        = new InterstitialPolicy(config, configService);

            // levelsSinceLastAd = 1 (< 3), shownThisSession = 0, time = well above min
            bool canShow = policy.CanShow(
                levelsSinceLastAd:   1,
                shownThisSession:    0,
                sessionTimeSeconds:  policy.MinimumSessionTimeSeconds + 10f);

            Assert.IsFalse(canShow, "Should be blocked when levelsSinceLastAd < MinimumLevelsBetweenAds.");
            UnityEngine.Object.DestroyImmediate(config);
        }

        [Test]
        public void InterstitialPolicy_DuringGameplay_MustBeBlocked()
        {
            // Design contract: interstitials are never shown during active gameplay.
            // Verified by: the game only calls CanShow / ShowInterstitialAsync from
            // LevelCompleteScreen or level-transition hooks — never from within an
            // active LevelStateMachine tick.
            //
            // This test documents the assumption and verifies that CanShow returns
            // false when the session time budget has not been satisfied (proxy for
            // "gameplay just started"), which is the nearest testable expression of
            // the "no interstitial during active play" contract.

            var config        = ScriptableObject.CreateInstance<AdConfiguration>();
            var configService = new RemoteConfigServiceMock();
            var policy        = new InterstitialPolicy(config, configService);

            // Simulate a session that is only 5 s old (well below the 60 s minimum).
            bool canShow = policy.CanShow(
                levelsSinceLastAd:   policy.MinimumLevelsBetweenAds,
                shownThisSession:    0,
                sessionTimeSeconds:  5f);

            Assert.IsFalse(canShow,
                "Interstitial must be blocked when the session time minimum has not been reached.");
            UnityEngine.Object.DestroyImmediate(config);
        }

        [Test]
        public void InterstitialPolicy_AfterMaxPerSession_Blocked()
        {
            var config        = ScriptableObject.CreateInstance<AdConfiguration>();
            var configService = new RemoteConfigServiceMock();
            var policy        = new InterstitialPolicy(config, configService);

            // shownThisSession == MaxAdsPerSession → must be blocked regardless of other criteria.
            bool canShow = policy.CanShow(
                levelsSinceLastAd:   policy.MinimumLevelsBetweenAds,
                shownThisSession:    policy.MaxAdsPerSession,
                sessionTimeSeconds:  policy.MinimumSessionTimeSeconds + 10f);

            Assert.IsFalse(canShow, "Should be blocked once the per-session cap is reached.");
            UnityEngine.Object.DestroyImmediate(config);
        }

        // ── AnalyticsRateGuard ────────────────────────────────────────────────

        [Test]
        public void AnalyticsRateGuard_Over200EventsBlocked()
        {
            AnalyticsRateGuard.Reset();

            // Fill the 200-event budget.
            for (int i = 0; i < 200; i++)
                AnalyticsRateGuard.Allow("test_event");

            // The 201st event must be rejected.
            bool result = AnalyticsRateGuard.Allow("overflow_event");

            Assert.IsFalse(result, "The 201st event within the window must be rate-limited.");
        }

        [Test]
        public void AnalyticsRateGuard_ResetClearsCount()
        {
            AnalyticsRateGuard.Reset();

            for (int i = 0; i < 200; i++)
                AnalyticsRateGuard.Allow("test");

            // After reset the counter starts from zero again.
            AnalyticsRateGuard.Reset();
            bool result = AnalyticsRateGuard.Allow("after_reset");

            Assert.IsTrue(result, "First event after Reset() must be allowed.");
        }

        // ── AdPlacement enum coverage ─────────────────────────────────────────

        [Test]
        public void AdPlacement_AllPlacementsHaveValue()
        {
            var values = Enum.GetValues(typeof(AdPlacement));

            // At minimum: LevelCompleteDoubleReward, LevelFailedExtraAttempt, FreePowerUp
            Assert.GreaterOrEqual(values.Length, 3,
                "AdPlacement enum must define at least 3 placement values.");

            // Every value must have a non-null, non-empty string representation.
            foreach (var v in values)
            {
                string name = v.ToString();
                Assert.IsFalse(string.IsNullOrEmpty(name),
                    $"AdPlacement value {v} must have a valid name.");
            }
        }

        // ── AdSessionStats default state ──────────────────────────────────────

        [Test]
        public void AdSessionStats_Default_ZeroCounts()
        {
            // A newly constructed AdSessionStats must have all-zero counters
            // so that a fresh session starts safely without stale ad state.
            var stats = new AdSessionStats();

            Assert.AreEqual(0,  stats.interstitialsShownThisSession,
                "interstitialsShownThisSession must default to 0.");
            Assert.AreEqual(0,  stats.totalInterstitialsShown,
                "totalInterstitialsShown must default to 0.");
            Assert.AreEqual(0L, stats.sessionStartTimestamp,
                "sessionStartTimestamp must default to 0.");
            // lastInterstitialLevelIndex defaults to -1 (no ad shown yet).
            Assert.AreEqual(-1, stats.lastInterstitialLevelIndex,
                "lastInterstitialLevelIndex must default to -1.");
        }
    }
}
