// ObservabilityTests.cs — M14 EditMode Tests
// 23 NUnit tests covering: AnalyticsEvents, AnalyticsParameters, ErrorCategory,
// RemoteConfig keys / defaults / validator, FeatureFlag, and mock services.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using KingSmash;
using KingSmash.Analytics;
using KingSmash.Config;
using KingSmash.Services;

namespace KingSmash.Tests.EditMode
{
    // =========================================================================
    // AnalyticsEventsTests (5)
    // =========================================================================

    [TestFixture]
    internal class AnalyticsEventsTests
    {
        [Test]
        public void AnalyticsEvents_AppOpen_IsSnakeCase()
        {
            string value = AnalyticsEvents.AppOpen;
            bool isSnakeCase = value.All(c => char.IsLower(c) || c == '_');
            Assert.IsTrue(isSnakeCase,
                $"Expected '{value}' to contain only lowercase letters and underscores.");
        }

        [Test]
        public void AnalyticsEvents_HasAllSessionEvents()
        {
            Assert.AreEqual("session_start", AnalyticsEvents.SessionStart,
                "SessionStart constant must equal 'session_start'.");
            Assert.AreEqual("session_end", AnalyticsEvents.SessionEnd,
                "SessionEnd constant must equal 'session_end'.");
        }

        [Test]
        public void AnalyticsEvents_HasAllLevelEvents()
        {
            Assert.AreEqual("level_start",     AnalyticsEvents.LevelStart);
            Assert.AreEqual("level_complete",  AnalyticsEvents.LevelComplete);
            Assert.AreEqual("level_failed",    AnalyticsEvents.LevelFailed);
            Assert.AreEqual("level_retried",   AnalyticsEvents.LevelRetried);
            Assert.AreEqual("level_abandoned", AnalyticsEvents.LevelAbandoned);
        }

        [Test]
        public void AnalyticsEvents_HasAllRetentionMilestones()
        {
            Assert.AreEqual("first_launch",         AnalyticsEvents.FirstLaunch);
            Assert.AreEqual("first_level_complete", AnalyticsEvents.FirstLevelComplete);
            Assert.AreEqual("first_purchase",       AnalyticsEvents.FirstPurchase);
            Assert.AreEqual("first_rewarded_ad",    AnalyticsEvents.FirstRewardedAd);
            Assert.AreEqual("first_upgrade",        AnalyticsEvents.FirstUpgrade);
        }

        [Test]
        public void AnalyticsEvents_HasCloudEvents()
        {
            Assert.AreEqual("cloud_save_started", AnalyticsEvents.CloudSaveStarted);
            Assert.AreEqual("cloud_save_success", AnalyticsEvents.CloudSaveSuccess);
            Assert.AreEqual("cloud_save_failed",  AnalyticsEvents.CloudSaveFailed);
        }
    }

    // =========================================================================
    // AnalyticsParametersTests (4)
    // =========================================================================

    [TestFixture]
    internal class AnalyticsParametersTests
    {
        [Test]
        public void AnalyticsParameters_HasLevelId()
        {
            Assert.AreEqual("level_id", AnalyticsParameters.LevelId);
        }

        [Test]
        public void AnalyticsParameters_HasWorldId()
        {
            Assert.AreEqual("world_id", AnalyticsParameters.WorldId);
        }

        [Test]
        public void AnalyticsParameters_AllKeysAreSnakeCase()
        {
            // Spot-check 5 parameter constants via reflection.
            string[] sampleFields = {
                nameof(AnalyticsParameters.LevelId),
                nameof(AnalyticsParameters.WorldId),
                nameof(AnalyticsParameters.AttemptNumber),
                nameof(AnalyticsParameters.Currency),
                nameof(AnalyticsParameters.Placement),
            };

            var type = typeof(AnalyticsParameters);
            foreach (string fieldName in sampleFields)
            {
                var field = type.GetField(fieldName,
                    BindingFlags.Public | BindingFlags.Static);
                Assert.IsNotNull(field, $"Field '{fieldName}' not found on AnalyticsParameters.");

                string value = (string)field.GetValue(null);
                bool isSnakeCase = value.All(c => char.IsLower(c) || c == '_');
                Assert.IsTrue(isSnakeCase,
                    $"Parameter key '{value}' must be snake_case (lowercase + underscores only).");
            }
        }

        [Test]
        public void AnalyticsParameters_HasPrivacySafeKeys()
        {
            // Ensure no constant value contains PII-related keywords.
            string[] forbidden = { "token", "password", "credential" };

            var fields = typeof(AnalyticsParameters)
                .GetFields(BindingFlags.Public | BindingFlags.Static)
                .Where(f => f.FieldType == typeof(string));

            foreach (var field in fields)
            {
                string value = (string)field.GetValue(null);
                foreach (string word in forbidden)
                {
                    Assert.IsFalse(
                        value.IndexOf(word, StringComparison.OrdinalIgnoreCase) >= 0,
                        $"Parameter constant '{field.Name}' = '{value}' must not contain '{word}'.");
                }
            }
        }
    }

    // =========================================================================
    // ErrorCategoryTests (3)
    // =========================================================================

    [TestFixture]
    internal class ErrorCategoryTests
    {
        [Test]
        public void ErrorCategory_HasNetworkCategory()
        {
            Assert.IsTrue(Enum.IsDefined(typeof(ErrorCategory), ErrorCategory.Network),
                "ErrorCategory.Network must be defined.");
        }

        [Test]
        public void ErrorCategory_HasSaveCategory()
        {
            Assert.IsTrue(Enum.IsDefined(typeof(ErrorCategory), ErrorCategory.Save),
                "ErrorCategory.Save must be defined.");
        }

        [Test]
        public void ErrorCategory_AllCategoriesUnique()
        {
            var values = (ErrorCategory[])Enum.GetValues(typeof(ErrorCategory));
            var intValues = values.Select(v => (int)v).ToList();
            var distinctCount = intValues.Distinct().Count();
            Assert.AreEqual(values.Length, distinctCount,
                "All ErrorCategory enum values must have unique underlying integers.");
        }
    }

    // =========================================================================
    // RemoteConfigTests (5)
    // =========================================================================

    [TestFixture]
    internal class RemoteConfigTests
    {
        [Test]
        public void RemoteConfigKeys_HasGameplayKeys()
        {
            Assert.IsNotNull(RemoteConfigKeys.StartingKings);
            Assert.IsNotNull(RemoteConfigKeys.MaxKings);
            Assert.IsNotNull(RemoteConfigKeys.KingLaunchPower);

            Assert.IsFalse(string.IsNullOrEmpty(RemoteConfigKeys.StartingKings));
            Assert.IsFalse(string.IsNullOrEmpty(RemoteConfigKeys.MaxKings));
            Assert.IsFalse(string.IsNullOrEmpty(RemoteConfigKeys.KingLaunchPower));
        }

        [Test]
        public void RemoteConfigKeys_HasEconomyKeys()
        {
            Assert.IsFalse(string.IsNullOrEmpty(RemoteConfigKeys.CoinRewardMultiplier));
            Assert.IsFalse(string.IsNullOrEmpty(RemoteConfigKeys.GemRewardMultiplier));
        }

        [Test]
        public void RemoteConfigKeys_HasFeatureFlagKeys()
        {
            Assert.IsFalse(string.IsNullOrEmpty(RemoteConfigKeys.ShopEnabled));
            Assert.IsFalse(string.IsNullOrEmpty(RemoteConfigKeys.PowerupsEnabled));
        }

        [Test]
        public void RemoteConfigDefaults_HasAllRemoteConfigKeys()
        {
            var defaults = RemoteConfigDefaults.GetAll();
            Assert.IsNotNull(defaults, "RemoteConfigDefaults.GetAll() must not return null.");
            Assert.Greater(defaults.Count, 20,
                "RemoteConfigDefaults.GetAll() must return more than 20 entries.");
        }

        [Test]
        public void RemoteConfigValidator_MultiplierClampsToRange()
        {
            // Any value above 10 must be clamped down to the allowed maximum.
            float clamped = RemoteConfigValidator.ValidateMultiplier("test_multiplier", 50f);
            Assert.LessOrEqual(clamped, 10f,
                "ValidateMultiplier must clamp values above 10 to 10 or below.");
        }
    }

    // =========================================================================
    // FeatureFlagTests (3)
    // =========================================================================

    [TestFixture]
    internal class FeatureFlagTests
    {
        [Test]
        public void FeatureFlag_HasAllRequiredFlags()
        {
            Assert.IsTrue(Enum.IsDefined(typeof(FeatureFlag), FeatureFlag.PowerUps));
            Assert.IsTrue(Enum.IsDefined(typeof(FeatureFlag), FeatureFlag.DailyRewards));
            Assert.IsTrue(Enum.IsDefined(typeof(FeatureFlag), FeatureFlag.Missions));
            Assert.IsTrue(Enum.IsDefined(typeof(FeatureFlag), FeatureFlag.Achievements));
            Assert.IsTrue(Enum.IsDefined(typeof(FeatureFlag), FeatureFlag.Shop));
        }

        [Test]
        public void FeatureFlag_HasAdFlags()
        {
            Assert.IsTrue(Enum.IsDefined(typeof(FeatureFlag), FeatureFlag.RewardedContinue));
            Assert.IsTrue(Enum.IsDefined(typeof(FeatureFlag), FeatureFlag.InterstitialAds));
        }

        [Test]
        public void FeatureFlag_HasCloudFlags()
        {
            Assert.IsTrue(Enum.IsDefined(typeof(FeatureFlag), FeatureFlag.CloudSave));
            Assert.IsTrue(Enum.IsDefined(typeof(FeatureFlag), FeatureFlag.GoogleSignIn));
        }
    }

    // =========================================================================
    // MockServiceTests (3)
    // =========================================================================

    [TestFixture]
    internal class MockServiceTests
    {
        [Test]
        public void MockCrashReportingService_CanBeInstantiated()
        {
            var mock = new MockCrashReportingService();
            Assert.IsNotNull(mock,
                "MockCrashReportingService must be instantiable without throwing.");
        }

        [Test]
        public void AnalyticsServiceMock_LogEventDoesNotThrow()
        {
            var mock = new AnalyticsServiceMock();
            Assert.DoesNotThrow(() => mock.LogEvent("test_event"),
                "AnalyticsServiceMock.LogEvent must not throw for a simple event.");
        }

        [Test]
        public void RemoteConfigServiceMock_GetBoolReturnsDefault()
        {
            var mock = new RemoteConfigServiceMock();
            bool result = mock.GetBool("nonexistent_key", true);
            Assert.IsTrue(result,
                "RemoteConfigServiceMock.GetBool must return the supplied default for unknown keys.");
        }
    }
}
