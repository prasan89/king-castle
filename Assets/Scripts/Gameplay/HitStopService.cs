using System.Collections;
using UnityEngine;

namespace KingSmash.Gameplay
{
    // ── HitStopConfig ─────────────────────────────────────────────────────────

    /// <summary>
    /// ScriptableObject that holds pre-authored hit-stop preset durations.
    /// Create a single asset and wire it into HitStopService.
    /// </summary>
    [CreateAssetMenu(fileName = "HitStopConfig", menuName = "KingSmash/Gameplay/HitStopConfig")]
    public class HitStopConfig : ScriptableObject
    {
        [Header("Preset Durations (seconds)")]
        public float light  = 0.04f;
        public float medium = 0.08f;
        public float heavy  = 0.12f;
        public float boss   = 0.18f;
    }

    // ── IHitStopService ───────────────────────────────────────────────────────

    /// <summary>
    /// Interface for the hit-stop / time-freeze effect.
    /// Kept in the same file as the implementation (internal use only).
    /// </summary>
    public interface IHitStopService
    {
        /// <summary>
        /// Briefly sets Time.timeScale to near-zero for the given real-time duration,
        /// creating a satisfying impact "freeze frame" effect.
        /// Ignored if a hit-stop is already in progress.
        /// </summary>
        void Trigger(float duration);
    }

    // ── HitStopService ────────────────────────────────────────────────────────

    /// <summary>
    /// MonoBehaviour implementing hit-stop (time-freeze on impact).
    /// Uses WaitForSecondsRealtime so it is unaffected by the scaled time.
    /// Max one active hit-stop at a time.
    /// </summary>
    [DefaultExecutionOrder(-800)]
    public class HitStopService : MonoBehaviour, IHitStopService
    {
        private const float FrozenTimeScale  = 0.05f;
        private const float NormalTimeScale  = 1f;

        [SerializeField] private HitStopConfig _config;

        private bool      _active;
        private Coroutine _hitStopCoroutine;

        // ── IHitStopService ───────────────────────────────────────────────────

        public void Trigger(float duration)
        {
            if (_active) return;
            if (duration <= 0f) return;

            _hitStopCoroutine = StartCoroutine(RunHitStop(duration));
        }

        // ── Convenience preset methods ────────────────────────────────────────

        public void TriggerLight()  => Trigger(_config != null ? _config.light  : 0.04f);
        public void TriggerMedium() => Trigger(_config != null ? _config.medium : 0.08f);
        public void TriggerHeavy()  => Trigger(_config != null ? _config.heavy  : 0.12f);
        public void TriggerBoss()   => Trigger(_config != null ? _config.boss   : 0.18f);

        // ── Internal ──────────────────────────────────────────────────────────

        private IEnumerator RunHitStop(float duration)
        {
            _active = true;
            Time.timeScale = FrozenTimeScale;

            yield return new WaitForSecondsRealtime(duration);

            Time.timeScale    = NormalTimeScale;
            _active           = false;
            _hitStopCoroutine = null;
        }

        private void OnDestroy()
        {
            // Safety: restore timeScale if destroyed mid-hitstop.
            if (_active)
                Time.timeScale = NormalTimeScale;
        }
    }
}
