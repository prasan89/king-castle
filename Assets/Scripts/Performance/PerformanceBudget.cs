// PerformanceBudget.cs — M15 King Smash performance constants.
// Static class: no allocations, no MonoBehaviour, safe to access from any context.

namespace KingSmash.Performance
{
    /// <summary>
    /// Central repository for all performance budget constants used by monitoring
    /// components, tests, and runtime logic.  All values are compile-time constants
    /// or static readonly fields so accessing them in hot paths is allocation-free.
    /// </summary>
    public static class PerformanceBudget
    {
        // -----------------------------------------------------------------------
        // Frame-time budgets (milliseconds)
        // -----------------------------------------------------------------------

        /// <summary>Target frame time for 60 fps (16.67 ms).</summary>
        public const float TargetFrameTime = 1000f / 60f;   // ≈ 16.667 ms

        /// <summary>Acceptable frame time for 30 fps on low-end devices (33.33 ms).</summary>
        public const float LowEndFrameTime = 1000f / 30f;   // ≈ 33.333 ms

        // -----------------------------------------------------------------------
        // Simulation limits (per-frame stagger guards)
        // -----------------------------------------------------------------------

        /// <summary>
        /// Maximum number of DestructibleObjects that may be destroyed in a single
        /// frame.  Large castle collapses stagger their remaining objects across
        /// subsequent frames to avoid GC and physics spikes.
        /// </summary>
        public const int MaxDestructiblesPerFrame = 5;

        /// <summary>
        /// Soft cap on simultaneously active debris particle systems.
        /// VFXService respects this when deciding whether to suppress low-priority
        /// debris effects.
        /// </summary>
        public const int MaxDebrisParticlesActive = 50;

        /// <summary>
        /// Maximum number of active Rigidbody2D physics bodies permitted at once.
        /// Exceeding this is a signal to pool or sleep idle bodies.
        /// </summary>
        public const int MaxActivePhysicsBodies = 100;

        // -----------------------------------------------------------------------
        // Audio
        // -----------------------------------------------------------------------

        /// <summary>
        /// Number of AudioSource instances in AudioService's fixed pool.
        /// Must match AudioService._sources array length exactly.
        /// </summary>
        public const int MaxAudioSourcesActive = 8;

        // -----------------------------------------------------------------------
        // Startup / scene-load budgets (milliseconds, wall-clock)
        // -----------------------------------------------------------------------

        /// <summary>5-second cold-launch budget (bootstrap → main-menu visible).</summary>
        public const int StartupBudgetMs = 5000;

        /// <summary>3-second level-load budget (tap Play → game-play interactive).</summary>
        public const int SceneLoadBudgetMs = 3000;

        // -----------------------------------------------------------------------
        // Memory budgets
        // -----------------------------------------------------------------------

        /// <summary>
        /// Soft limit for the managed (Mono) heap in megabytes.
        /// MemoryWatchdog logs a warning when this is exceeded.
        /// </summary>
        public const int MaxManagedHeapMB = 256;

        // -----------------------------------------------------------------------
        // Hot-path tags (for FrameTimingMonitor annotations and tooling)
        // -----------------------------------------------------------------------

        /// <summary>
        /// Names of methods considered hot paths.  Used by dev tooling and
        /// static analysis helpers; never accessed in Update/FixedUpdate loops.
        /// </summary>
        public static readonly string[] HotPaths =
        {
            "FixedUpdate",
            "OnCollisionEnter2D",
            "Update",
        };
    }
}
