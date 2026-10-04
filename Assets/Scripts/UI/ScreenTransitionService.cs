using System;
using System.Collections;
using System.Threading.Tasks;
using UnityEngine;

namespace KingSmash.UI
{
    public class ScreenTransitionService : MonoBehaviour
    {
        public static ScreenTransitionService Instance { get; private set; }

        [SerializeField] private CanvasGroup    _transitionOverlay;
        [SerializeField] private KingSmashTheme _theme;

        private Coroutine _fadeCoroutine;

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;

            if (_transitionOverlay != null)
            {
                _transitionOverlay.alpha          = 0f;
                _transitionOverlay.interactable   = false;
                _transitionOverlay.blocksRaycasts = false;
            }
        }

        private void OnEnable()
        {
            ScreenManager.OnScreenTransition += OnScreenTransition;
        }

        private void OnDisable()
        {
            ScreenManager.OnScreenTransition -= OnScreenTransition;
        }

        private void OnScreenTransition(UIScreen prev, UIScreen next)
        {
            if (_transitionOverlay == null) return;
            float dur = _theme != null ? _theme.durationScreenTransition : 0.22f;
            if (_fadeCoroutine != null) StopCoroutine(_fadeCoroutine);
            _fadeCoroutine = StartCoroutine(QuickFlash(dur));
        }

        private IEnumerator QuickFlash(float halfDuration)
        {
            yield return FadeToBlackCoroutine(halfDuration);
            yield return FadeFromBlackCoroutine(halfDuration);
        }

        // ── Public coroutine API ────────────────────────────────────────────────

        public Coroutine FadeToBlack(float duration = 0.2f)
        {
            if (_fadeCoroutine != null) StopCoroutine(_fadeCoroutine);
            _fadeCoroutine = StartCoroutine(FadeToBlackCoroutine(duration));
            return _fadeCoroutine;
        }

        public Coroutine FadeFromBlack(float duration = 0.2f)
        {
            if (_fadeCoroutine != null) StopCoroutine(_fadeCoroutine);
            _fadeCoroutine = StartCoroutine(FadeFromBlackCoroutine(duration));
            return _fadeCoroutine;
        }

        private IEnumerator FadeToBlackCoroutine(float duration)
        {
            if (_transitionOverlay == null) yield break;
            _transitionOverlay.blocksRaycasts = true;
            float elapsed = 0f;
            float start   = _transitionOverlay.alpha;
            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                _transitionOverlay.alpha = Mathf.Lerp(start, 1f, elapsed / duration);
                yield return null;
            }
            _transitionOverlay.alpha = 1f;
        }

        private IEnumerator FadeFromBlackCoroutine(float duration)
        {
            if (_transitionOverlay == null) yield break;
            float elapsed = 0f;
            float start   = _transitionOverlay.alpha;
            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                _transitionOverlay.alpha = Mathf.Lerp(start, 0f, elapsed / duration);
                yield return null;
            }
            _transitionOverlay.alpha          = 0f;
            _transitionOverlay.interactable   = false;
            _transitionOverlay.blocksRaycasts = false;
        }

        // ── Async Task API ─────────────────────────────────────────────────────

        public async Task TransitionAsync(Action midpoint, float halfDuration = 0.18f)
        {
            var tcs = new TaskCompletionSource<bool>();
            StartCoroutine(TransitionCoroutine(midpoint, halfDuration, tcs));
            await tcs.Task;
        }

        private IEnumerator TransitionCoroutine(
            Action midpoint, float halfDuration, TaskCompletionSource<bool> tcs)
        {
            yield return FadeToBlackCoroutine(halfDuration);
            try { midpoint?.Invoke(); }
            catch (Exception e) { UnityEngine.Debug.LogException(e); }
            yield return FadeFromBlackCoroutine(halfDuration);
            tcs.SetResult(true);
        }
    }
}
