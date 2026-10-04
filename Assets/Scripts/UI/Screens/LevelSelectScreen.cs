using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using KingSmash.Core;
using KingSmash.Levels;
using KingSmash.Services;
using KingSmash.UI.Components;

namespace KingSmash.UI.Screens
{
    public class LevelSelectScreen : UIScreen
    {
        // ── Existing fields (preserved) ────────────────────────────────────────

        [Header("Navigation")]
        [SerializeField] private Button _backButton;
        [SerializeField] private Button _playButton;

        [Header("Level Info")]
        [SerializeField] private TextMeshProUGUI _levelNumberLabel;
        [SerializeField] private TextMeshProUGUI _worldNameLabel;

        [Header("Stars")]
        [SerializeField] private List<GameObject> _starIcons;

        [Header("Objectives")]
        [SerializeField] private Transform          _objectivesContainer;
        [SerializeField] private TextMeshProUGUI    _objectiveItemPrefab;

        [Header("Power-ups")]
        [SerializeField] private Transform          _powerUpContainer;
        [SerializeField] private PowerUpCardWidget  _powerUpCardPrefab;

        // ── M13 Polish fields ──────────────────────────────────────────────

        [Header("M13 Polish")]
        [SerializeField] private KingSmashTheme _themeConfig;
        [SerializeField] private CanvasGroup    _contentCg;
        [SerializeField] private RectTransform  _levelPreviewPanel;
        [SerializeField] private KSStarDisplay  _starDisplay;
        [SerializeField] private TextMeshProUGUI _bestScoreLabel;

        private int _currentLevelIndex;
        private Coroutine _playPulseCoroutine;

        // ── Lifecycle ──────────────────────────────────────────────────

        protected override void Awake()
        {
            base.Awake();
            _backButton?.onClick.AddListener(OnBackClicked);
            _playButton?.onClick.AddListener(OnPlayClicked);
        }

        public void SetLevel(int levelIndex)
        {
            _currentLevelIndex = levelIndex;
            PopulateLevel(levelIndex);

            // Animate panel slide in from bottom
            if (_levelPreviewPanel != null)
                StartCoroutine(UIAnimationController.SlideIn(_levelPreviewPanel, 60f,
                    _themeConfig != null ? _themeConfig.durationNormal : 0.25f));

            // Animate stars sequentially
            if (_starDisplay != null)
            {
                ServiceLocator.TryGet<ISaveService>(out var save2);
                int s = save2?.Current.GetStarsForLevel(levelIndex) ?? 0;
                _starDisplay.SetStars(s, animated: true);
            }
        }

        protected override void OnShow()
        {
            // Fade in content
            if (_contentCg != null)
                StartCoroutine(UIAnimationController.Fade(_contentCg, 0f, 1f,
                    _themeConfig != null ? _themeConfig.durationNormal : 0.25f));

            // Slide level preview panel from bottom
            if (_levelPreviewPanel != null)
                StartCoroutine(UIAnimationController.SlideIn(_levelPreviewPanel, 80f,
                    _themeConfig != null ? _themeConfig.durationNormal : 0.25f));

            // Start attention pulse on play button after 2s delay
            if (_playPulseCoroutine != null) StopCoroutine(_playPulseCoroutine);
            _playPulseCoroutine = StartCoroutine(DelayedPlayPulse());
        }

        private void OnDisable()
        {
            if (_playPulseCoroutine != null) { StopCoroutine(_playPulseCoroutine); _playPulseCoroutine = null; }
        }

        // ── Population ──────────────────────────────────────────────────

        private void PopulateLevel(int levelIndex)
        {
            if (_levelNumberLabel != null) _levelNumberLabel.text = $"Level {levelIndex + 1}";

            // World name
            var worldDef = WorldRegistry.GetWorldForLevel(levelIndex);
            if (_worldNameLabel != null && worldDef != null)
                _worldNameLabel.text = worldDef.DisplayName;

            // Stars from save
            ServiceLocator.TryGet<ISaveService>(out var save);
            int stars = save?.Current.GetStarsForLevel(levelIndex) ?? 0;

            // Legacy star icons (show if no KSStarDisplay assigned)
            if (_starDisplay == null)
            {
                for (int i = 0; i < _starIcons.Count; i++)
                    if (_starIcons[i] != null) _starIcons[i].SetActive(i < stars);
            }

            // Best score label
            if (_bestScoreLabel != null)
                _bestScoreLabel.text = stars > 0 ? $"Best: {stars} ★" : "Not played";

            // Objectives
            ClearContainer(_objectivesContainer);
            AddObjective("Rescue the Queen");
            AddObjective("Defeat all enemies");
            AddObjective("Destroy 70% of castle");
        }

        private void AddObjective(string text)
        {
            if (_objectiveItemPrefab == null || _objectivesContainer == null) return;
            var item = Instantiate(_objectiveItemPrefab, _objectivesContainer);
            item.text = text;
        }

        private void ClearContainer(Transform container)
        {
            if (container == null) return;
            foreach (Transform child in container) Destroy(child.gameObject);
        }

        // ── Button handlers ──────────────────────────────────────────────

        private void OnPlayClicked()
        {
            PlaySound(Audio.SoundId.ButtonConfirm);
            if (_playButton != null)
                StartCoroutine(UIAnimationController.ButtonPress(_playButton.transform));
            var screen = ScreenManager.Instance.Show<LevelStartScreen>();
            screen?.SetLevel(_currentLevelIndex);
        }

        private void OnBackClicked()
        {
            PlaySound(Audio.SoundId.Back);
            ScreenManager.Instance.Back();
        }

        // ── Idle animation ───────────────────────────────────────────────

        private IEnumerator DelayedPlayPulse()
        {
            yield return new WaitForSecondsRealtime(2f);
            while (true)
            {
                Transform target = _playButton != null ? _playButton.transform : null;
                if (target != null)
                    yield return StartCoroutine(PulseScale(target, 1.08f, 0.28f));
                yield return new WaitForSecondsRealtime(4f);
            }
        }

        private IEnumerator PulseScale(Transform target, float peakScale, float duration)
        {
            if (target == null) yield break;
            Vector3 original = target.localScale;
            float half = duration * 0.5f;
            float e = 0f;
            while (e < half)
            {
                e += Time.unscaledDeltaTime;
                target.localScale = Vector3.Lerp(original, original * peakScale, e / half);
                yield return null;
            }
            e = 0f;
            while (e < half)
            {
                e += Time.unscaledDeltaTime;
                target.localScale = Vector3.Lerp(original * peakScale, original, e / half);
                yield return null;
            }
            target.localScale = original;
        }

        // ── Utilities ────────────────────────────────────────────────────

        private static void PlaySound(Audio.SoundId id)
        {
            if (ServiceLocator.TryGet<IAudioService>(out var audio))
                audio.Play(id);
        }

        private void OnDestroy()
        {
            _backButton?.onClick.RemoveListener(OnBackClicked);
            _playButton?.onClick.RemoveListener(OnPlayClicked);
        }
    }
}
