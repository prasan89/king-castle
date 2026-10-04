// FrameTimingMonitor.cs — M15 dev-only frame-time tracker.
// Compiled only in Editor or when KING_SMASH_DEV is defined.

#if UNITY_EDITOR || KING_SMASH_DEV

using KingSmash.Core;
using UnityEngine;

namespace KingSmash.Performance
{
    /// <summary>
    /// Monitors frame times using a circular buffer and logs warnings/errors
    /// when the rolling average breaches configurable thresholds.
    ///
    /// Dev/Editor only.  Attach to the DebugTools GameObject in your dev scene.
    /// </summary>
    public sealed class FrameTimingMonitor : MonoBehaviour
    {
        // -----------------------------------------------------------------------
        // Inspector
        // -----------------------------------------------------------------------

        [SerializeField] [Tooltip("Rolling-average threshold that triggers a Warning log (ms).")]
        private float _warningThresholdMs = 33f;   // 30 fps

        [SerializeField] [Tooltip("Rolling-average threshold that triggers an Error log (ms).")]
        private float _criticalThresholdMs = 66f;  // 15 fps

        [SerializeField] [Tooltip("Number of frames in the rolling sample window.")]
        private int _sampleWindow = 60;

        // -----------------------------------------------------------------------
        // State
        // -----------------------------------------------------------------------

        private float[] _frameTimes;
        private int     _frameIndex;
        private bool    _bufferFilled;

        // -----------------------------------------------------------------------
        // Unity lifecycle
        // -----------------------------------------------------------------------

        private void Awake()
        {
            _sampleWindow = Mathf.Max(1, _sampleWindow);
            _frameTimes   = new float[_sampleWindow];
            _frameIndex   = 0;
            _bufferFilled = false;
        }

        private void Update()
        {
            float frameMs = Time.deltaTime * 1000f;

            _frameTimes[_frameIndex] = frameMs;
            _frameIndex              = (_frameIndex + 1) % _sampleWindow;

            if (!_bufferFilled && _frameIndex == 0)
                _bufferFilled = true;

            // Only evaluate once the window is fully populated.
            if (!_bufferFilled) return;

            float avg = AverageFrameTimeMs;

            if (avg > _criticalThresholdMs)
            {
                GameLogger.Error(
                    "FrameTimingMonitor",
                    $"CRITICAL frame time: avg={avg:F1}ms ({CurrentFPS:F1}fps) worst={WorstFrameTimeMs:F1}ms "
                    + $"over last {_sampleWindow} frames");
            }
            else if (avg > _warningThresholdMs)
            {
                GameLogger.Warning(
                    "FrameTimingMonitor",
                    $"Slow frame time: avg={avg:F1}ms ({CurrentFPS:F1}fps) worst={WorstFrameTimeMs:F1}ms "
                    + $"over last {_sampleWindow} frames");
            }
        }

        // -----------------------------------------------------------------------
        // Public accessors
        // -----------------------------------------------------------------------

        /// <summary>Rolling average frame time in milliseconds over the sample window.</summary>
        public float AverageFrameTimeMs
        {
            get
            {
                if (_frameTimes == null || _frameTimes.Length == 0) return 0f;

                float sum   = 0f;
                int   count = _bufferFilled ? _sampleWindow : _frameIndex;

                if (count == 0) return 0f;

                for (int i = 0; i < count; i++)
                    sum += _frameTimes[i];

                return sum / count;
            }
        }

        /// <summary>Worst (highest) frame time in the current sample window.</summary>
        public float WorstFrameTimeMs
        {
            get
            {
                if (_frameTimes == null || _frameTimes.Length == 0) return 0f;

                float worst = 0f;
                int   count = _bufferFilled ? _sampleWindow : _frameIndex;

                for (int i = 0; i < count; i++)
                {
                    if (_frameTimes[i] > worst)
                        worst = _frameTimes[i];
                }

                return worst;
            }
        }

        /// <summary>Instantaneous FPS derived from the rolling average frame time.</summary>
        public float CurrentFPS
        {
            get
            {
                float avg = AverageFrameTimeMs;
                return avg > 0f ? 1000f / avg : 0f;
            }
        }
    }
}

#endif // UNITY_EDITOR || KING_SMASH_DEV
