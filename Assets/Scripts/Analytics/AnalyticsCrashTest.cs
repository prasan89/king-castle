// AnalyticsCrashTest.cs — M14 Analytics
// DEV/EDITOR-only utility to manually trigger a forced crash for validating
// Crashlytics integration. Never compiled or included in production builds.

using UnityEngine;
using KingSmash.Core;
using KingSmash.Services;

namespace KingSmash.Analytics
{
#if UNITY_EDITOR || KING_SMASH_DEV

    /// <summary>
    /// Attaches to a scene GameObject. When _enableCrashTest is true, pressing
    /// Left Shift + C calls <see cref="ICrashReportingService.ForceCrash"/> so
    /// the Crashlytics pipeline can be validated end-to-end.
    ///
    /// Only compiled and active in UNITY_EDITOR or KING_SMASH_DEV builds.
    /// Never ship with _enableCrashTest = true.
    /// </summary>
    public sealed class AnalyticsCrashTest : MonoBehaviour
    {
        [SerializeField] private bool _enableCrashTest = false;

        private GUIStyle _warningStyle;
        private bool     _styleInitialized;

        // ── Update ────────────────────────────────────────────────────────────

        private void Update()
        {
            if (!_enableCrashTest) return;

            if (Input.GetKey(KeyCode.LeftShift) && Input.GetKeyDown(KeyCode.C))
            {
                GameLogger.Warning("AnalyticsCrashTest",
                    "Force crash triggered by developer input (Left Shift + C).");

                if (ServiceLocator.TryGet<ICrashReportingService>(out var crash))
                {
                    crash.ForceCrash();
                }
                else
                {
                    GameLogger.Error("AnalyticsCrashTest",
                        "ICrashReportingService not registered — cannot force crash.");
                }
            }
        }

        // ── OnGUI ─────────────────────────────────────────────────────────────

        private void OnGUI()
        {
            if (!_enableCrashTest) return;

            EnsureStyle();

            GUI.Label(
                new Rect(10f, Screen.height - 40f, 400f, 30f),
                "CRASH TEST ENABLED — Shift+C to force crash",
                _warningStyle);
        }

        // ── Helpers ───────────────────────────────────────────────────────────

        private void EnsureStyle()
        {
            if (_styleInitialized) return;
            _styleInitialized = true;

            _warningStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize   = 14,
                fontStyle  = FontStyle.Bold,
            };
            _warningStyle.normal.textColor = Color.red;
        }
    }

#endif // UNITY_EDITOR || KING_SMASH_DEV
}
