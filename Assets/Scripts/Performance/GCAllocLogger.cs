// GCAllocLogger.cs — M15 dev-only GC allocation spike detector.
// Compiled only in Editor or when KING_SMASH_DEV is defined.

#if UNITY_EDITOR || KING_SMASH_DEV

using KingSmash.Core;
using UnityEngine;

namespace KingSmash.Performance
{
    /// <summary>
    /// Monitors GC allocation spikes each frame and logs a warning whenever
    /// the managed heap grows by more than <see cref="_gcWarningThresholdKB"/> KB
    /// in a single frame.
    ///
    /// Dev/Editor only.  Attach to the DebugTools GameObject in your dev scene.
    /// </summary>
    public sealed class GCAllocLogger : MonoBehaviour
    {
        // -----------------------------------------------------------------------
        // Inspector
        // -----------------------------------------------------------------------

        [SerializeField] private float _gcWarningThresholdKB = 1f;

        // -----------------------------------------------------------------------
        // State
        // -----------------------------------------------------------------------

        private long _lastGCMemory;

        /// <summary>
        /// Global toggle.  Set to false to silence this monitor without
        /// destroying the component (useful during intentional GC-heavy work).
        /// </summary>
        public static bool Enabled { get; set; } = true;

        // -----------------------------------------------------------------------
        // Unity lifecycle
        // -----------------------------------------------------------------------

        private void Awake()
        {
            _lastGCMemory = System.GC.GetTotalMemory(false);
        }

        private void Update()
        {
            if (!Enabled) return;

            long currentMemory = System.GC.GetTotalMemory(false);
            long delta         = currentMemory - _lastGCMemory;

            if (delta > 0)
            {
                float deltaKB = delta / 1024f;

                if (deltaKB >= _gcWarningThresholdKB)
                {
                    GameLogger.Warning(
                        "GCAllocLogger",
                        $"GC spike: {deltaKB:F1}KB in frame {Time.frameCount}");
                }
            }

            _lastGCMemory = currentMemory;
        }
    }
}

#endif // UNITY_EDITOR || KING_SMASH_DEV
