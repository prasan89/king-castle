// MemoryWatchdog.cs — M15 dev-only memory usage monitor.
// Compiled only in Editor or when KING_SMASH_DEV is defined.

#if UNITY_EDITOR || KING_SMASH_DEV

using KingSmash.Core;
using UnityEngine;
using UnityEngine.Profiling;

namespace KingSmash.Performance
{
    /// <summary>
    /// Periodically samples Unity's native allocator and the managed (Mono) heap,
    /// logging warnings/errors when configured budget limits are exceeded.
    ///
    /// Dev/Editor only.  Attach to the DebugTools GameObject in your dev scene.
    /// Checks are throttled to every 5 seconds to avoid measurement overhead.
    /// </summary>
    public sealed class MemoryWatchdog : MonoBehaviour
    {
        // -----------------------------------------------------------------------
        // Constants
        // -----------------------------------------------------------------------

        private const float  CheckIntervalSeconds = 5f;
        private const long   TotalMemoryErrorThresholdBytes = 600L * 1024L * 1024L; // 600 MB

        // -----------------------------------------------------------------------
        // State
        // -----------------------------------------------------------------------

        private float _lastCheckTime;

        // -----------------------------------------------------------------------
        // Unity lifecycle
        // -----------------------------------------------------------------------

        private void Awake()
        {
            // Check immediately on first frame so first-frame leaks are visible.
            _lastCheckTime = -CheckIntervalSeconds;
        }

        private void Update()
        {
            float now = Time.realtimeSinceStartup;
            if (now - _lastCheckTime < CheckIntervalSeconds) return;

            _lastCheckTime = now;
            CheckMemory();
        }

        // -----------------------------------------------------------------------
        // Memory checks
        // -----------------------------------------------------------------------

        private void CheckMemory()
        {
            long managedBytes = Profiler.GetMonoUsedSizeLong();
            long totalBytes   = Profiler.GetTotalAllocatedMemoryLong();

            long managedLimitBytes = (long)PerformanceBudget.MaxManagedHeapMB * 1024L * 1024L;

            if (managedBytes > managedLimitBytes)
            {
                GameLogger.Warning(
                    "MemoryWatchdog",
                    $"Managed heap {ManagedHeapMB} MB exceeds budget of "
                    + $"{PerformanceBudget.MaxManagedHeapMB} MB "
                    + $"(total allocated: {TotalAllocatedMB} MB)");
            }

            if (totalBytes > TotalMemoryErrorThresholdBytes)
            {
                GameLogger.Error(
                    "MemoryWatchdog",
                    $"Total allocated memory {TotalAllocatedMB} MB exceeds 600 MB threshold "
                    + $"(managed heap: {ManagedHeapMB} MB)");
            }
        }

        // -----------------------------------------------------------------------
        // Public accessors
        // -----------------------------------------------------------------------

        /// <summary>Total memory allocated by Unity's native allocators (MB).</summary>
        public long TotalAllocatedMB => Profiler.GetTotalAllocatedMemoryLong() / (1024L * 1024L);

        /// <summary>Managed (Mono/IL2CPP GC) heap in use (MB).</summary>
        public long ManagedHeapMB => Profiler.GetMonoUsedSizeLong() / (1024L * 1024L);
    }
}

#endif // UNITY_EDITOR || KING_SMASH_DEV
