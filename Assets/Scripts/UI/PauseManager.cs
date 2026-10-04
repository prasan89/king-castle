using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using KingSmash.Audio;
using KingSmash.Core;
using KingSmash.Services;

namespace KingSmash.UI
{
    public class PauseManager : MonoBehaviour
    {
        // ── Original public API ──────────────────────────────────────────────
        public static event Action OnPaused;
        public static event Action OnResumed;

        public static bool IsPaused => Time.timeScale == 0f;

        // ── M13 Polish fields ────────────────────────────────────────────────
        [Header("M13 Polish")]
        [SerializeField] private KingSmashTheme _themeConfig;
        [SerializeField] private CanvasGroup    _pauseOverlayCg;
        [SerializeField] private RectTransform  _pausePanel;

        [Header("Buttons")]
        [SerializeField] private Button _resumeButton;
        [SerializeField] private Button _homeButton;
        [SerializeField] private Button _restartButton;

        // ── Private state ────────────────────────────────────────────────────
        private Coroutine _animCoroutine;
        private Vector2   _panelRestPos;
        private bool      _panelPosRecorded;

        // ── Lifecycle ────────────────────────────────────────────────────────

        private void Awake()
        {
            _resumeButton?.onClick.AddListener(OnResumeClicked);
            _homeButton?.onClick.AddListener(OnHomeClicked);
            _restartButton?.onClick.AddListener(OnRestartClicked);

            // Hide overlay initially
            if (_pauseOverlayCg != null)
            {
                _pauseOverlayCg.alpha          = 0f;
                _pauseOverlayCg.interactable   = false;
                _pauseOverlayCg.blocksRaycasts = false;
            }
        }

        private void OnEnable()  => GameManager.OnStateChanged += HandleStateChanged;
        private void OnDisable() => GameManager.OnStateChanged -= HandleStateChanged;

        private void Start()
        {
            // Record resting position of pause panel after layout is complete
            if (_pausePanel != null)
            {
                _panelRestPos      = _pausePanel.anchoredPosition;
                _panelPosRecorded  = true;
                // Start hidden above screen
                _pausePanel.anchoredPosition = _panelRestPos + Vector2.up * 200f;
            }
        }

        // ── State handler ────────────────────────────────────────────────────

        private void HandleStateChanged(GameState prev, GameState next)
        {
            if (next == GameState.Paused)
            {
                Time.timeScale = 0f;
                OnPaused?.Invoke();
                if (_animCoroutine != null) StopCoroutine(_animCoroutine);
                _animCoroutine = StartCoroutine(AnimatePauseIn());
            }
            else if (prev == GameState.Paused)
            {
                Time.timeScale = 1f;
                OnResumed?.Invoke();
                if (_animCoroutine != null) StopCoroutine(_animCoroutine);
                _animCoroutine = StartCoroutine(AnimatePauseOut());
            }
        }

        // ── Animations ───────────────────────────────────────────────────────

        private IEnumerator AnimatePauseIn()
        {
            if (_pauseOverlayCg != null)
            {
                _pauseOverlayCg.interactable   = true;
                _pauseOverlayCg.blocksRaycasts = true;
            }

            float elapsed = 0f;
            const float dur = 0.15f;

            Vector2 panelStart = _panelPosRecorded
                ? _panelRestPos + Vector2.up * 80f
                : Vector2.up * 80f;
            Vector2 panelEnd = _panelPosRecorded ? _panelRestPos : Vector2.zero;

            if (_pausePanel != null) _pausePanel.anchoredPosition = panelStart;

            while (elapsed < dur)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / dur);
                if (_pauseOverlayCg != null) _pauseOverlayCg.alpha = Mathf.Lerp(0f, 1f, t);
                if (_pausePanel != null)     _pausePanel.anchoredPosition = Vector2.Lerp(panelStart, panelEnd, t);
                yield return null;
            }

            if (_pauseOverlayCg != null) _pauseOverlayCg.alpha = 1f;
            if (_pausePanel != null) _pausePanel.anchoredPosition = panelEnd;

            // Slide in panel from slightly above (second animation phase: 0.2s)
            if (_pausePanel != null)
            {
                elapsed = 0f;
                Vector2 slideStart = panelEnd + Vector2.up * 40f;
                _pausePanel.anchoredPosition = slideStart;
                const float slideDur = 0.2f;
                while (elapsed < slideDur)
                {
                    elapsed += Time.unscaledDeltaTime;
                    float t = Mathf.Clamp01(elapsed / slideDur);
                    // Ease out back
                    float eased = EaseOutBack(t);
                    _pausePanel.anchoredPosition = Vector2.Lerp(slideStart, panelEnd, eased);
                    yield return null;
                }
                _pausePanel.anchoredPosition = panelEnd;
            }
        }

        private IEnumerator AnimatePauseOut()
        {
            float elapsed = 0f;
            const float dur = 0.18f;

            Vector2 panelStart = _panelPosRecorded ? _panelRestPos : Vector2.zero;
            Vector2 panelEnd   = panelStart + Vector2.up * 80f;
            float alphaStart   = _pauseOverlayCg != null ? _pauseOverlayCg.alpha : 1f;

            while (elapsed < dur)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / dur);
                if (_pauseOverlayCg != null) _pauseOverlayCg.alpha = Mathf.Lerp(alphaStart, 0f, t);
                if (_pausePanel != null)     _pausePanel.anchoredPosition = Vector2.Lerp(panelStart, panelEnd, t);
                yield return null;
            }

            if (_pauseOverlayCg != null)
            {
                _pauseOverlayCg.alpha          = 0f;
                _pauseOverlayCg.interactable   = false;
                _pauseOverlayCg.blocksRaycasts = false;
            }
        }

        // ── Public API ───────────────────────────────────────────────────────

        public void Pause()  => GameManager.Instance?.TransitionTo(GameState.Paused);
        public void Resume() => GameManager.Instance?.TransitionTo(GameState.Gameplay);

        // ── Button handlers ──────────────────────────────────────────────────

        private void OnResumeClicked()
        {
            PlayButtonClick();
            Resume();
        }

        private void OnHomeClicked()
        {
            PlayButtonClick();
            Time.timeScale = 1f;
            SceneLoader.LoadScene(SceneNames.MainMenu);
        }

        private void OnRestartClicked()
        {
            PlayButtonClick();
            Time.timeScale = 1f;
            SceneLoader.LoadScene(SceneNames.Level);
        }

        private void OnDestroy()
        {
            _resumeButton?.onClick.RemoveListener(OnResumeClicked);
            _homeButton?.onClick.RemoveListener(OnHomeClicked);
            _restartButton?.onClick.RemoveListener(OnRestartClicked);
        }

        // ── Helpers ──────────────────────────────────────────────────────────

        private static void PlayButtonClick()
        {
            if (ServiceLocator.TryGet<IAudioService>(out var audio))
                audio.Play(SoundId.ButtonClick);
        }

        private static float EaseOutBack(float t)
        {
            const float c1 = 1.70158f;
            const float c3 = c1 + 1f;
            return 1f + c3 * Mathf.Pow(t - 1f, 3f) + c1 * Mathf.Pow(t - 1f, 2f);
        }
    }
}
