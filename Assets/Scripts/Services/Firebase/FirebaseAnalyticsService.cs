// FirebaseAnalyticsService — implements IAnalyticsService.
// Real Firebase Analytics calls are guarded by #if FIREBASE_ENABLED.
// Without the flag the service logs to GameLogger only, so the project
// compiles in CI and editor builds that lack the Firebase SDK.
//
// SETUP REQUIRED:
//   1. Import Firebase Unity SDK (Analytics package)
//   2. Add google-services.json to Assets/ (Android)
//   3. In GameBootstrap replace AnalyticsServiceMock with this service
//   4. Add FIREBASE_ENABLED to Player Settings → Scripting Define Symbols
//
// PRIVACY NOTE:
//   SetUserId hashes the raw UID with SHA-256 (truncated to 16 hex chars)
//   before sending it to Firebase Analytics. Never send raw Firebase UIDs.

using System;
using System.Security.Cryptography;
using System.Text;
using KingSmash.Analytics;
using KingSmash.Core;
using UnityEngine;

namespace KingSmash.Services.Firebase
{
    public class FirebaseAnalyticsService : IAnalyticsService
    {
        private const string Tag            = "FirebaseAnalytics";
        private const int    RateLimit      = 200;   // events per minute
        private const float  RateWindowSecs = 60f;

        private int   _eventCount;
        private float _windowStart;

        // ── Constructor ───────────────────────────────────────────────────────

        public FirebaseAnalyticsService()
        {
            _windowStart = Time.realtimeSinceStartup;
            _eventCount  = 0;
        }

        // ── Rate guard ────────────────────────────────────────────────────────

        private bool CheckRateLimit(string eventName)
        {
            float now = Time.realtimeSinceStartup;
            if (now - _windowStart > RateWindowSecs)
            {
                _windowStart = now;
                _eventCount  = 0;
            }
            if (_eventCount >= RateLimit)
            {
                GameLogger.Warning(Tag, $"Rate limit reached — dropping event: {eventName}");
                return false;
            }
            _eventCount++;
            return true;
        }

        // ── Low-level ─────────────────────────────────────────────────────────

        public void LogEvent(string eventName, params (string key, object value)[] parameters)
        {
            if (!CheckRateLimit(eventName)) return;

#if FIREBASE_ENABLED
            try
            {
                if (parameters == null || parameters.Length == 0)
                {
                    global::Firebase.Analytics.FirebaseAnalytics.LogEvent(eventName);
                    return;
                }

                var fbParams = new global::Firebase.Analytics.Parameter[parameters.Length];
                for (int i = 0; i < parameters.Length; i++)
                {
                    var (key, value) = parameters[i];
                    switch (value)
                    {
                        case int    iv: fbParams[i] = new global::Firebase.Analytics.Parameter(key, (long)iv);   break;
                        case long   lv: fbParams[i] = new global::Firebase.Analytics.Parameter(key, lv);         break;
                        case float  fv: fbParams[i] = new global::Firebase.Analytics.Parameter(key, (double)fv); break;
                        case double dv: fbParams[i] = new global::Firebase.Analytics.Parameter(key, dv);         break;
                        case bool   bv: fbParams[i] = new global::Firebase.Analytics.Parameter(key, bv ? 1L : 0L); break;
                        default:        fbParams[i] = new global::Firebase.Analytics.Parameter(key, value?.ToString() ?? ""); break;
                    }
                }
                global::Firebase.Analytics.FirebaseAnalytics.LogEvent(eventName, fbParams);
            }
            catch (Exception ex)
            {
                GameLogger.Error(Tag, $"LogEvent failed for '{eventName}': {ex.Message}");
            }
#else
            if (parameters == null || parameters.Length == 0)
            {
                GameLogger.Debug(Tag, $"[STUB] Event: {eventName}");
                return;
            }
            var sb = new StringBuilder();
            for (int i = 0; i < parameters.Length; i++)
            {
                if (i > 0) sb.Append(", ");
                sb.Append(parameters[i].key);
                sb.Append('=');
                sb.Append(parameters[i].value);
            }
            GameLogger.Debug(Tag, $"[STUB] Event: {eventName} | {sb}");
#endif
        }

        public void SetUserProperty(string key, string value)
        {
#if FIREBASE_ENABLED
            try
            {
                global::Firebase.Analytics.FirebaseAnalytics.SetUserProperty(key, value);
            }
            catch (Exception ex)
            {
                GameLogger.Error(Tag, $"SetUserProperty failed: {ex.Message}");
            }
#else
            GameLogger.Debug(Tag, $"[STUB] SetUserProperty: {key}={value}");
#endif
        }

        /// <summary>
        /// Hashes the userId with SHA-256 and sends only the first 16 hex chars to
        /// Firebase Analytics to avoid sending raw internal UIDs.
        /// </summary>
        public void SetUserId(string userId)
        {
            if (string.IsNullOrEmpty(userId)) return;

            string hashed = HashUserId(userId);

#if FIREBASE_ENABLED
            try
            {
                global::Firebase.Analytics.FirebaseAnalytics.SetUserId(hashed);
            }
            catch (Exception ex)
            {
                GameLogger.Error(Tag, $"SetUserId failed: {ex.Message}");
            }
#else
            GameLogger.Debug(Tag, $"[STUB] SetUserId (hashed): {hashed}");
#endif
        }

        // ── Level ─────────────────────────────────────────────────────────────

        public void TrackLevelStarted(int levelId, int worldId, int attemptNumber)
        {
            LogEvent(AnalyticsEvents.LevelStart,
                (AnalyticsParameters.LevelId,       levelId),
                (AnalyticsParameters.WorldId,        worldId),
                (AnalyticsParameters.AttemptNumber,  attemptNumber));
        }

        public void TrackLevelCompleted(int levelId, int worldId, int stars, int attemptNumber,
                                        float completionTime, int remainingKings)
        {
            LogEvent(AnalyticsEvents.LevelComplete,
                (AnalyticsParameters.LevelId,         levelId),
                (AnalyticsParameters.WorldId,          worldId),
                (AnalyticsParameters.Stars,            stars),
                (AnalyticsParameters.AttemptNumber,    attemptNumber),
                (AnalyticsParameters.CompletionTime,   completionTime),
                (AnalyticsParameters.RemainingKings,   remainingKings));
        }

        public void TrackLevelFailed(int levelId, int worldId, int attemptNumber, string reason)
        {
            LogEvent(AnalyticsEvents.LevelFailed,
                (AnalyticsParameters.LevelId,       levelId),
                (AnalyticsParameters.WorldId,        worldId),
                (AnalyticsParameters.AttemptNumber,  attemptNumber),
                (AnalyticsParameters.Source,         reason));
        }

        public void TrackLevelAbandoned(int levelId, int worldId, int attemptNumber)
        {
            LogEvent(AnalyticsEvents.LevelAbandoned,
                (AnalyticsParameters.LevelId,       levelId),
                (AnalyticsParameters.WorldId,        worldId),
                (AnalyticsParameters.AttemptNumber,  attemptNumber));
        }

        public void TrackLevelRetried(int levelId, int worldId, int attemptNumber)
        {
            LogEvent(AnalyticsEvents.LevelRetried,
                (AnalyticsParameters.LevelId,       levelId),
                (AnalyticsParameters.WorldId,        worldId),
                (AnalyticsParameters.AttemptNumber,  attemptNumber));
        }

        // ── Power-Ups ─────────────────────────────────────────────────────────

        public void TrackPowerUpActivated(string powerupId, int levelId, int worldId, int quantityBefore)
        {
            LogEvent(AnalyticsEvents.PowerUpActivated,
                (AnalyticsParameters.PowerupId,      powerupId),
                (AnalyticsParameters.LevelId,        levelId),
                (AnalyticsParameters.WorldId,        worldId),
                (AnalyticsParameters.QuantityBefore, quantityBefore));
        }

        // ── Progression ───────────────────────────────────────────────────────

        public void TrackKingUpgrade(string statName, float oldValue, float newValue,
                                     long cost, int playerLevel)
        {
            LogEvent(AnalyticsEvents.KingStatUpgrade,
                (AnalyticsParameters.StatName,    statName),
                (AnalyticsParameters.OldValue,    oldValue),
                (AnalyticsParameters.NewValue,    newValue),
                (AnalyticsParameters.Cost,        cost),
                (AnalyticsParameters.PlayerLevel, playerLevel));
        }

        // ── Economy ───────────────────────────────────────────────────────────

        public void TrackCurrencyEvent(string eventName, string currency, long amount, string source)
        {
            LogEvent(eventName,
                (AnalyticsParameters.Currency, currency),
                (AnalyticsParameters.Amount,   amount),
                (AnalyticsParameters.Source,   source));
        }

        public void TrackRewardClaimed(string rewardType, long amount, string source)
        {
            LogEvent(AnalyticsEvents.RewardClaimed,
                (AnalyticsParameters.RewardType,   rewardType),
                (AnalyticsParameters.RewardAmount, amount),
                (AnalyticsParameters.Source,       source));
        }

        // ── Ads ───────────────────────────────────────────────────────────────

        public void TrackAdEvent(string eventName, string adType, string placement)
        {
            LogEvent(eventName,
                (AnalyticsParameters.AdType,    adType),
                (AnalyticsParameters.Placement, placement));
        }

        public void TrackAdRewardGranted(string placement, string rewardType)
        {
            LogEvent(AnalyticsEvents.RewardedAdRewardGranted,
                (AnalyticsParameters.Placement,  placement),
                (AnalyticsParameters.RewardType, rewardType));
        }

        // ── IAP ───────────────────────────────────────────────────────────────

        public void TrackPurchaseEvent(string eventName, string productId, string productType)
        {
            LogEvent(eventName,
                (AnalyticsParameters.ProductId,   productId),
                (AnalyticsParameters.ProductType, productType));
        }

        // ── Network / Cloud ───────────────────────────────────────────────────

        public void TrackCloudEvent(string eventName, string operation, bool isOnline,
                                    string errorCategory = "")
        {
            LogEvent(eventName,
                (AnalyticsParameters.Operation,     operation),
                (AnalyticsParameters.IsOnline,      isOnline),
                (AnalyticsParameters.ErrorCategory, errorCategory));
        }

        // ── Retention ─────────────────────────────────────────────────────────

        public void TrackRetentionEvent(string eventName)
        {
            LogEvent(eventName);
        }

        // ── Config ────────────────────────────────────────────────────────────

        public void SetEnvironment(string environment)
        {
            SetUserProperty(AnalyticsParameters.Environment, environment);
        }

        public void SetConfigVersion(string version)
        {
            SetUserProperty(AnalyticsParameters.ConfigVersion, version);
        }

        // ── Helpers ───────────────────────────────────────────────────────────

        /// <summary>
        /// SHA-256 hash of the input, truncated to 16 lowercase hex characters (~64 bits).
        /// Used to anonymise user IDs before sending to Firebase Analytics.
        /// </summary>
        private static string HashUserId(string rawId)
        {
            using var sha = SHA256.Create();
            byte[] hashBytes = sha.ComputeHash(Encoding.UTF8.GetBytes(rawId));
            // 8 bytes = 16 hex chars
            var sb = new StringBuilder(16);
            for (int i = 0; i < 8; i++)
                sb.Append(hashBytes[i].ToString("x2"));
            return sb.ToString();
        }
    }
}
