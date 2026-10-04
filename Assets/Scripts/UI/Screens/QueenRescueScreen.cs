using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using KingSmash.Audio;
using KingSmash.Characters;
using KingSmash.Core;
using KingSmash.Services;

namespace KingSmash.UI.Screens
{
    public class QueenRescueScreen : UIScreen
    {
        // ── M13 Fields ───────────────────────────────────────────────────────
        [Header("Theme")]
        [SerializeField] private KingSmashTheme _themeConfig;
        [SerializeField] private CanvasGroup    _screenCg;

        [Header("Character Art")]
        [SerializeField] private RectTransform  _kingImage;
        [SerializeField] private RectTransform  _queenImage;

        [Header("Particles")]
        [SerializeField] private ParticleSystem _heartsParticles;
        [SerializeField] private ParticleSystem _starsParticles;

        [Header("Labels")]
        [SerializeField] private TextMeshProUGUI _rescuedLabel;
        [SerializeField] private TextMeshProUGUI _worldCompleteLabel;
        [SerializeField] private TextMeshProUGUI _rewardLabel;

        [Header("Panels")]
        [SerializeField] private GameObject _worldUnlockPanel;

        [Header("Buttons")]
        [SerializeField] private Button _continueButton;

        // ── Reward data ──────────────────────────────────────────────────────
        private long _rewardCoins    = 50L;
        private bool _isWorldComplete;

        // ── Lifecycle ────────────────────────────────────────────────────────

        protected override void Awake()
        {
            base.Awake();
            _continueButton?.onClick.AddListener(OnContinueClicked);
            if (_worldUnlockPanel != null) _worldUnlockPanel.SetActive(false);
        }

        private void OnEnable()  => QueenController.OnQueenRescued += HandleQueenRescued;
        private void OnDisable() => QueenController.OnQueenRescued -= HandleQueenRescued;

        // ── Public API ───────────────────────────────────────────────────────

        private void HandleQueenRescued(QueenController _) => Show();

        public void SetRewardData(long coins, bool worldComplete)
        {
            _rewardCoins     = coins;
            _isWorldComplete = worldComplete;
        }

        // ── Show sequence ────────────────────────────────────────────────────

        protected override void OnShow()
        {
            StartCoroutine(RescueSequence());
        }

        private IEnumerator RescueSequence()
        {
            // Prep: hide elements at zero scale
            if (_rescuedLabel != null)
                _rescuedLabel.transform.localScale = Vector3.zero;
            if (_continueButton != null)
                _continueButton.transform.localScale = Vector3.zero;
            if (_worldUnlockPanel != null)
                _worldUnlockPanel.SetActive(false);

            // Position characters off-screen laterally
            Vector2 kingStartPos  = Vector2.zero;
            Vector2 queenStartPos = Vector2.zero;
            Vector2 kingEndPos    = Vector2.zero;
            Vector2 queenEndPos   = Vector2.zero;

            if (_kingImage != null)
            {
                kingEndPos   = _kingImage.anchoredPosition;
                kingStartPos = kingEndPos + new Vector2(-400f, 0f);
                _kingImage.anchoredPosition = kingStartPos;
            }
            if (_queenImage != null)
            {
                queenEndPos   = _queenImage.anchoredPosition;
                queenStartPos = queenEndPos + new Vector2(400f, 0f);
                _queenImage.anchoredPosition = queenStartPos;
            }

            // 1. Fade screen in (0.3s) with dark overlay
            if (_screenCg != null)
            {
                _screenCg.alpha = 0f;
                yield return StartCoroutine(UIAnimationController.Fade(_screenCg, 0f, 1f, 0.3f));
            }

            // 2. Play QueenRescue sound
            if (ServiceLocator.TryGet<IAudioService>(out var audio))
                audio.Play(SoundId.QueenRescue);

            // 3. Slide King from left, Queen from right (0.4s, cubic ease out)
            if (_kingImage != null || _queenImage != null)
            {
                float elapsed = 0f;
                const float slideDur = 0.4f;
                while (elapsed < slideDur)
                {
                    elapsed += Time.unscaledDeltaTime;
                    float t     = Mathf.Clamp01(elapsed / slideDur);
                    float eased = 1f - Mathf.Pow(1f - t, 3f); // cubic ease out
                    if (_kingImage  != null) _kingImage.anchoredPosition  = Vector2.Lerp(kingStartPos,  kingEndPos,  eased);
                    if (_queenImage != null) _queenImage.anchoredPosition = Vector2.Lerp(queenStartPos, queenEndPos, eased);
                    yield return null;
                }
                if (_kingImage  != null) _kingImage.anchoredPosition  = kingEndPos;
                if (_queenImage != null) _queenImage.anchoredPosition = queenEndPos;
            }

            // 4. BounceReveal both characters
            if (_kingImage != null)
                StartCoroutine(UIAnimationController.BounceReveal(_kingImage, 0.3f));
            if (_queenImage != null)
            {
                yield return new WaitForSecondsRealtime(0.08f);
                StartCoroutine(UIAnimationController.BounceReveal(_queenImage, 0.3f));
            }

            yield return new WaitForSecondsRealtime(0.3f);

            // 5. Start hearts particles
            if (_heartsParticles != null && !_heartsParticles.isPlaying)
                _heartsParticles.Play();

            // 6. BounceReveal rescued label
            if (_rescuedLabel != null)
            {
                _rescuedLabel.text = "The Queen is Rescued!";
                yield return StartCoroutine(UIAnimationController.BounceReveal(_rescuedLabel.transform, 0.35f));
            }

            // 7. Wait then CountUp reward coins, start stars particles
            yield return new WaitForSecondsRealtime(0.8f);

            if (_rewardLabel != null)
            {
                if (ServiceLocator.TryGet<IAudioService>(out var coinSfx))
                    coinSfx.Play(SoundId.Coins);
                yield return StartCoroutine(UIAnimationController.CountUp(
                    _rewardLabel, 0, _rewardCoins, 0.8f, prefix: "+", suffix: " Coins"));
            }

            if (_starsParticles != null && !_starsParticles.isPlaying)
                _starsParticles.Play();

            // 8. If world complete: slide in world unlock panel
            if (_isWorldComplete && _worldUnlockPanel != null)
            {
                _worldUnlockPanel.SetActive(true);
                var rt = _worldUnlockPanel.GetComponent<RectTransform>();
                if (rt != null)
                    yield return StartCoroutine(UIAnimationController.SlideIn(rt, 60f, 0.35f));
                if (_worldCompleteLabel != null)
                    _worldCompleteLabel.text = "World Complete!";
            }

            yield return new WaitForSecondsRealtime(0.2f);

            // 9. BounceReveal continue button
            if (_continueButton != null)
                yield return StartCoroutine(UIAnimationController.BounceReveal(_continueButton.transform, 0.3f));
        }

        // ── Button handlers ──────────────────────────────────────────────────

        private void OnContinueClicked()
        {
            if (ScreenManager.Instance != null)
                ScreenManager.Instance.Show<LevelCompleteScreen>();
            else
                Hide();
        }

        protected override void OnHide()
        {
            if (_heartsParticles != null) _heartsParticles.Stop();
            if (_starsParticles  != null) _starsParticles.Stop();
        }

        private void OnDestroy() => _continueButton?.onClick.RemoveListener(OnContinueClicked);
    }
}
