using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using KingSmash.Audio;
using KingSmash.Core;
using KingSmash.Gameplay;
using KingSmash.Levels;
using KingSmash.Characters;
using KingSmash.Services;
using KingSmash.VFX;

namespace KingSmash.UI
{
    /// Minimal in-game HUD.  Wires up to events — no polling in Update.
    public class GameHUD : MonoBehaviour
    {
        // ── Original fields ──────────────────────────────────────────────────
        [Header("Top Bar")]
        [SerializeField] private TextMeshProUGUI _levelLabel;
        [SerializeField] private Button _pauseButton;

        [Header("Launch Indicators")]
        [SerializeField] private Transform _launchIconContainer;
        [SerializeField] private GameObject _launchIconPrefab;

        [Header("Destruction Bar")]
        [SerializeField] private Slider _destructionSlider;
        [SerializeField] private TextMeshProUGUI _destructionLabel;

        [Header("M2 — Character Info")]
        [SerializeField] private TextMeshProUGUI _enemiesRemainingLabel;
        [SerializeField] private TextMeshProUGUI _queenStatusLabel;
        [SerializeField] private Slider _kingHPSlider;

        [Header("Result Panel")]
        [SerializeField] private GameObject _resultPanel;
        [SerializeField] private TextMeshProUGUI _resultTitle;
        [SerializeField] private TextMeshProUGUI _resultScore;
        [SerializeField] private TextMeshProUGUI _resultDestructionPct;
        [SerializeField] private Button _restartButton;

        // ── M13 Polish fields ────────────────────────────────────────────────
        [Header("M13 Polish")]
        [SerializeField] private KingSmashTheme _themeConfig;
        [SerializeField] private CanvasGroup    _hudCg;
        [SerializeField] private RectTransform  _topBar;
        [SerializeField] private Animator       _kingHPAnimator;
        [SerializeField] private ScreenFlashEffect _screenFlash;

        // ── Private state ────────────────────────────────────────────────────
        private LevelController _levelController;
        private int _enemiesRemaining;

        private float _currentDestructionFill;
        private float _targetDestructionFill;
        private Coroutine _destructionLerpCoroutine;
        private Coroutine _kingHPFlashCoroutine;

        private static readonly int s_LowHP   = Animator.StringToHash("LowHP");
        private static readonly int s_Heal     = Animator.StringToHash("Heal");

        // ── Unity lifecycle ──────────────────────────────────────────────────

        private void Awake()
        {
            _resultPanel?.SetActive(false);
            _pauseButton?.onClick.AddListener(OnPauseClicked);
            _restartButton?.onClick.AddListener(OnRestartClicked);

            // Ensure pause button has adequate touch target (88px)
            EnsureMinTouchTarget(_pauseButton, 88f);
        }

        private void OnEnable()
        {
            LaunchController.OnLaunchesRemainingChanged        += UpdateLaunchIcons;
            LevelController.OnLevelStateChanged                += HandleStateChanged;
            LevelController.OnLevelEnded                       += HandleLevelEnded;
            DestructionController.OnGlobalDestructionChanged   += UpdateDestructionBar;
            EnemyController.OnEnemyDied                        += HandleEnemyDied;
            KingHealth.OnKingHPChanged                         += UpdateKingHP;
            QueenController.OnQueenRescued                     += HandleQueenRescued;
        }

        private void OnDisable()
        {
            LaunchController.OnLaunchesRemainingChanged        -= UpdateLaunchIcons;
            LevelController.OnLevelStateChanged                -= HandleStateChanged;
            LevelController.OnLevelEnded                       -= HandleLevelEnded;
            DestructionController.OnGlobalDestructionChanged   -= UpdateDestructionBar;
            EnemyController.OnEnemyDied                        -= HandleEnemyDied;
            KingHealth.OnKingHPChanged                         -= UpdateKingHP;
            QueenController.OnQueenRescued                     -= HandleQueenRescued;
        }

        // ── Public API ───────────────────────────────────────────────────────

        public void Initialize(int levelIndex, int totalLaunches, LevelController levelController)
        {
            _levelController = levelController;
            if (_levelLabel != null) _levelLabel.text = $"Level {levelIndex + 1}";
            BuildLaunchIcons(totalLaunches);
            ShowHUD();
        }

        public void SetEnemyCount(int total)
        {
            _enemiesRemaining = total;
            UpdateEnemyLabel();
        }

        // ── Show animation ───────────────────────────────────────────────────

        private void ShowHUD()
        {
            if (_topBar != null)
                StartCoroutine(SlideTopBarIn());
        }

        private IEnumerator SlideTopBarIn()
        {
            if (_topBar == null) yield break;
            Vector2 start = _topBar.anchoredPosition;
            start.y = -80f;
            _topBar.anchoredPosition = start;

            float elapsed = 0f;
            const float dur = 0.2f;
            while (elapsed < dur)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / dur);
                Vector2 pos = _topBar.anchoredPosition;
                pos.y = Mathf.Lerp(-80f, 0f, t);
                _topBar.anchoredPosition = pos;
                yield return null;
            }
            Vector2 final = _topBar.anchoredPosition;
            final.y = 0f;
            _topBar.anchoredPosition = final;
        }

        // ── Launch icons ─────────────────────────────────────────────────────

        private void BuildLaunchIcons(int count)
        {
            if (_launchIconContainer == null || _launchIconPrefab == null) return;
            foreach (Transform child in _launchIconContainer) Destroy(child.gameObject);
            for (int i = 0; i < count; i++)
            {
                var icon = Instantiate(_launchIconPrefab, _launchIconContainer);
                StartCoroutine(UIAnimationController.BounceReveal(icon.transform, 0.25f + i * 0.05f));
            }
        }

        private void UpdateLaunchIcons(int remaining)
        {
            if (_launchIconContainer == null) return;
            int i = 0;
            foreach (Transform child in _launchIconContainer)
            {
                bool active = i < remaining;
                child.gameObject.SetActive(true); // keep visible, just dim used ones
                var cg = child.GetComponent<CanvasGroup>();
                if (cg == null) cg = child.gameObject.AddComponent<CanvasGroup>();
                if (!active)
                {
                    StartCoroutine(DimIcon(child, cg));
                }
                i++;
            }
        }

        private IEnumerator DimIcon(Transform icon, CanvasGroup cg)
        {
            // Shrink and dim used icon
            float elapsed = 0f;
            const float dur = 0.15f;
            Vector3 originalScale = icon.localScale;
            while (elapsed < dur)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = elapsed / dur;
                icon.localScale = Vector3.Lerp(originalScale, originalScale * 0.7f, t);
                cg.alpha = Mathf.Lerp(1f, 0.35f, t);
                yield return null;
            }
            icon.localScale = originalScale * 0.7f;
            cg.alpha = 0.35f;
        }

        // ── Destruction bar ──────────────────────────────────────────────────

        private void UpdateDestructionBar(float ratio)
        {
            _targetDestructionFill = ratio;
            if (_destructionLerpCoroutine != null) StopCoroutine(_destructionLerpCoroutine);
            _destructionLerpCoroutine = StartCoroutine(LerpDestructionBar(ratio));
        }

        private IEnumerator LerpDestructionBar(float target)
        {
            if (_destructionSlider == null) yield break;
            float start   = _destructionSlider.value;
            float elapsed = 0f;
            const float dur = 0.3f;
            while (elapsed < dur)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = elapsed / dur;
                float val = Mathf.Lerp(start, target, t);
                _destructionSlider.value = val;
                if (_destructionLabel != null)
                    _destructionLabel.text = $"{val * 100f:F0}%";
                yield return null;
            }
            _destructionSlider.value = target;
            if (_destructionLabel != null)
                _destructionLabel.text = $"{target * 100f:F0}%";
        }

        // ── King HP ──────────────────────────────────────────────────────────

        private void UpdateKingHP(float current, float max)
        {
            if (_kingHPSlider == null) return;
            float ratio = max > 0 ? current / max : 0f;
            float prev  = _kingHPSlider.value;
            _kingHPSlider.value = ratio;

            if (_kingHPFlashCoroutine != null) StopCoroutine(_kingHPFlashCoroutine);
            _kingHPFlashCoroutine = StartCoroutine(FlashKingHP(prev, ratio));

            if (_kingHPAnimator != null)
            {
                _kingHPAnimator.SetBool(s_LowHP, ratio < 0.3f);
                if (current > prev * max) _kingHPAnimator.SetTrigger(s_Heal);
            }
        }

        private IEnumerator FlashKingHP(float prev, float next)
        {
            if (_kingHPSlider == null) yield break;
            var fill = _kingHPSlider.fillRect?.GetComponent<Image>();
            if (fill == null) yield break;

            bool isLow   = next < 0.3f;
            bool isHeal  = next > prev;
            Color target = isLow ? (_themeConfig != null ? _themeConfig.error : Color.red)
                         : isHeal ? (_themeConfig != null ? _themeConfig.success : Color.green)
                         : Color.white;

            Color original = fill.color;
            fill.color = target;
            yield return new WaitForSecondsRealtime(0.12f);
            float elapsed = 0f;
            while (elapsed < 0.2f)
            {
                elapsed += Time.unscaledDeltaTime;
                fill.color = Color.Lerp(target, original, elapsed / 0.2f);
                yield return null;
            }
            fill.color = original;
        }

        // ── Enemy defeat flash ────────────────────────────────────────────────

        private void HandleEnemyDied(EnemyController _)
        {
            _enemiesRemaining = Mathf.Max(0, _enemiesRemaining - 1);
            UpdateEnemyLabel();

            // Brief white flash on enemy defeat
            if (_screenFlash != null)
                _screenFlash.Flash(new Color(1f, 1f, 1f, 0.6f), 0.05f);
        }

        private void UpdateEnemyLabel()
        {
            if (_enemiesRemainingLabel != null)
                _enemiesRemainingLabel.text = $"Enemies: {_enemiesRemaining}";
        }

        // ── State / level end ────────────────────────────────────────────────

        private void HandleStateChanged(LevelState prev, LevelState next)
        {
            GameLogger.Debug("GameHUD", $"LevelState: {prev} -> {next}");
        }

        private void HandleLevelEnded(int score, int stars, float destructionRatio)
        {
            _resultPanel?.SetActive(true);
            if (_resultTitle != null)
                _resultTitle.text = stars > 0 ? "SMASHED!" : "FAILED";
            if (_resultScore != null)
                _resultScore.text = $"Score: {score:N0}";
            if (_resultDestructionPct != null)
                _resultDestructionPct.text = $"Destroyed: {destructionRatio * 100f:F0}%";
        }

        private void HandleQueenRescued(QueenController _)
        {
            if (_queenStatusLabel != null) _queenStatusLabel.text = "Queen Rescued!";
        }

        // ── Button handlers ──────────────────────────────────────────────────

        private void OnPauseClicked()
        {
            if (ServiceLocator.TryGet<IAudioService>(out var audio))
                audio.Play(SoundId.ButtonClick);
            GameManager.Instance?.TransitionTo(GameState.Paused);
            Time.timeScale = 0f;
        }

        private void OnRestartClicked()
        {
            Time.timeScale = 1f;
            _resultPanel?.SetActive(false);
            _levelController?.RestartLevel();
        }

        private void OnDestroy()
        {
            _pauseButton?.onClick.RemoveAllListeners();
            _restartButton?.onClick.RemoveAllListeners();
        }

        // ── Helpers ──────────────────────────────────────────────────────────

        private static void EnsureMinTouchTarget(Button button, float minPx)
        {
            if (button == null) return;
            var rt = button.GetComponent<RectTransform>();
            if (rt == null) return;
            Vector2 size = rt.sizeDelta;
            if (size.x < minPx) size.x = minPx;
            if (size.y < minPx) size.y = minPx;
            rt.sizeDelta = size;
        }
    }
}
