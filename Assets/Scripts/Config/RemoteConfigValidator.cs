// RemoteConfigValidator — validates and clamps Remote Config values before use.
// Prevents bad server-side values from crashing the game.
// M14 Remote Config + Feature Flags system.

using UnityEngine;

namespace KingSmash.Config
{
    public static class RemoteConfigValidator
    {
        // ── Nested result struct ─────────────────────────────────────────────

        public struct ValidatedFloat
        {
            public float Value;
            public bool  IsValid;
            public float UsedDefault;

            public ValidatedFloat(float value, bool isValid, float usedDefault)
            {
                Value        = value;
                IsValid      = isValid;
                UsedDefault  = usedDefault;
            }
        }

        // ── Float validation ─────────────────────────────────────────────────

        /// <summary>
        /// Validates <paramref name="rawValue"/> against [<paramref name="min"/>, <paramref name="max"/>].
        /// Returns <paramref name="defaultValue"/> if the raw value is NaN or Infinity.
        /// Clamps and logs a warning if the value is merely out of range.
        /// </summary>
        public static ValidatedFloat ValidateFloat(
            string key,
            float  rawValue,
            float  defaultValue,
            float  min,
            float  max)
        {
            if (float.IsNaN(rawValue) || float.IsInfinity(rawValue))
            {
                Debug.LogWarning(
                    $"[RemoteConfigValidator] Key '{key}': invalid float ({rawValue}), using default {defaultValue}.");
                return new ValidatedFloat(defaultValue, false, defaultValue);
            }

            if (rawValue < min || rawValue > max)
            {
                float clamped = Mathf.Clamp(rawValue, min, max);
                Debug.LogWarning(
                    $"[RemoteConfigValidator] Key '{key}': value {rawValue} out of range [{min},{max}], clamped to {clamped}.");
                return new ValidatedFloat(clamped, true, defaultValue);
            }

            return new ValidatedFloat(rawValue, true, defaultValue);
        }

        // ── Int validation ───────────────────────────────────────────────────

        /// <summary>
        /// Validates <paramref name="rawValue"/> against [<paramref name="min"/>, <paramref name="max"/>].
        /// Returns <paramref name="defaultValue"/> if the raw value equals <c>int.MinValue</c>
        /// or <c>int.MaxValue</c> (sentinel for a failed parse upstream).
        /// Clamps and logs a warning if the value is merely out of range.
        /// </summary>
        public static int ValidateInt(
            string key,
            int    rawValue,
            int    defaultValue,
            int    min,
            int    max)
        {
            // Treat extreme sentinels as completely invalid.
            if (rawValue == int.MinValue || rawValue == int.MaxValue)
            {
                Debug.LogWarning(
                    $"[RemoteConfigValidator] Key '{key}': invalid int ({rawValue}), using default {defaultValue}.");
                return defaultValue;
            }

            if (rawValue < min || rawValue > max)
            {
                int clamped = Mathf.Clamp(rawValue, min, max);
                Debug.LogWarning(
                    $"[RemoteConfigValidator] Key '{key}': value {rawValue} out of range [{min},{max}], clamped to {clamped}.");
                return clamped;
            }

            return rawValue;
        }

        // ── Bool validation ──────────────────────────────────────────────────

        /// <summary>
        /// Parses a string into a bool. Falls back to <paramref name="defaultValue"/>
        /// and logs a warning if parsing fails.
        /// </summary>
        public static bool ValidateBool(string key, string rawValue, bool defaultValue)
        {
            if (bool.TryParse(rawValue, out bool result))
                return result;

            // Also accept "0" / "1" for convenience.
            if (rawValue == "1") return true;
            if (rawValue == "0") return false;

            Debug.LogWarning(
                $"[RemoteConfigValidator] Key '{key}': cannot parse '{rawValue}' as bool, using default {defaultValue}.");
            return defaultValue;
        }

        // ── Convenience: multiplier ──────────────────────────────────────────

        /// <summary>
        /// Validates a generic multiplier key with a range of [0.1, 10] and a default of 1.0.
        /// </summary>
        public static ValidatedFloat ValidateMultiplier(string key, float rawValue)
            => ValidateFloat(key, rawValue, defaultValue: 1.0f, min: 0.1f, max: 10.0f);
    }
}
