// StructuredLogEntry.cs — M14 structured log payload for backend Cloud Run logging.
// NOT intended for per-frame use — only emit on errors/warnings in production.
// SECURITY: never populate any field with passwords, tokens, auth data, or PII.

using UnityEngine;
using KingSmash.Services.Firebase;

namespace KingSmash.Analytics
{
    [System.Serializable]
    public class StructuredLogEntry
    {
        public string timestamp;   // ISO-8601 UTC, e.g. "2026-10-04T12:34:56.789Z"
        public string level;       // "DEBUG" | "INFO" | "WARNING" | "ERROR" | "FATAL"
        public string tag;         // source tag matching GameLogger convention
        public string message;     // human-readable log body
        public string environment; // from FirebaseEnvironmentConfig.Environment
        public string appVersion;  // from Application.version

        /// <summary>
        /// Create a populated entry.  Timestamp and environment metadata are
        /// injected automatically; caller supplies level, tag, and message.
        /// </summary>
        public static StructuredLogEntry Create(string level, string tag, string message)
        {
            return new StructuredLogEntry
            {
                timestamp   = System.DateTime.UtcNow.ToString("o"),
                level       = level       ?? string.Empty,
                tag         = tag         ?? string.Empty,
                message     = message     ?? string.Empty,
                environment = FirebaseEnvironmentConfig.Environment,
                appVersion  = Application.version
            };
        }

        /// <summary>
        /// Serialise to a compact JSON string suitable for Cloud Run log ingestion.
        /// </summary>
        public string ToJson() => JsonUtility.ToJson(this);
    }
}
