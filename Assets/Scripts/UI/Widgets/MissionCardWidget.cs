using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using KingSmash.Retention;
using KingSmash.Economy;
using KingSmash.Save;

namespace KingSmash.UI.Widgets
{
    public class MissionCardWidget : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _titleLabel;
        [SerializeField] private TextMeshProUGUI _descriptionLabel;
        [SerializeField] private TextMeshProUGUI _progressLabel;
        [SerializeField] private Slider _progressBar;
        [SerializeField] private TextMeshProUGUI _rewardLabel;
        [SerializeField] private Button _claimButton;
        [SerializeField] private TextMeshProUGUI _claimButtonLabel;
        [SerializeField] private GameObject _claimedBadge;
        [SerializeField] private Image _iconImage;

        [Header("M13 Theme & Animation")]
        [SerializeField] private KingSmash.UI.KingSmashTheme _themeConfig;
        [SerializeField] private CanvasGroup _cardCg;
        [SerializeField] private Image _cardBackground;
        [SerializeField] private Image _progressBarFill;

        public event Action<string> OnClaimPressed;

        private string _currentMissionId;
        private Coroutine _pulseCoroutine;

        public void Setup(MissionDefinition def, MissionProgress progress)
        {
            _currentMissionId = def.missionId;

            bool completed = progress.progress >= def.target;
            bool claimed   = progress.claimed;

            if (_titleLabel       != null) _titleLabel.text       = def.displayName;
            if (_descriptionLabel != null) _descriptionLabel.text = def.description;
            if (_progressLabel    != null) _progressLabel.text    = $"{progress.progress} / {def.target}";
            if (_rewardLabel      != null) _rewardLabel.text      = CurrencyFormatter.Format(def.coinReward);
            if (_iconImage        != null && def.icon != null) _iconImage.sprite = def.icon;

            if (_progressBar != null)
            {
                float target = Mathf.Clamp01((float)progress.progress / Mathf.Max(1, def.target));
                _progressBar.value = 0f;
                StartCoroutine(AnimateProgressBar(target, 0.4f));
            }

            if (_claimButton != null)
            {
                _claimButton.gameObject.SetActive(completed && !claimed);
                _claimButton.onClick.RemoveAllListeners();
                _claimButton.onClick.AddListener(OnClaimButtonClicked);
            }

            if (_claimButtonLabel != null) _claimButtonLabel.text = "CLAIM";

            if (_claimedBadge != null)
            {
                _claimedBadge.SetActive(claimed);
                if (claimed)
                    StartCoroutine(UIAnimationController.BounceReveal(_claimedBadge.transform, 0.30f));
            }

            ApplyStateVisuals(completed, claimed);
        }

        private void ApplyStateVisuals(bool completed, bool claimed)
        {
            if (_pulseCoroutine != null) { StopCoroutine(_pulseCoroutine); _pulseCoroutine = null; }

            if (claimed)
            {
                if (_cardCg != null) StartCoroutine(FadeAlpha(_cardCg, _cardCg.alpha, 0.6f, 0.25f));
                if (_cardBackground != null && _themeConfig != null)
                    _cardBackground.color = _themeConfig.disabledColor;
            }
            else if (completed)
            {
                if (_cardBackground != null && _themeConfig != null)
                    _cardBackground.color = new Color(_themeConfig.goldAccent.r, _themeConfig.goldAccent.g, _themeConfig.goldAccent.b, 0.25f);
                if (_claimButton != null)
                    _pulseCoroutine = StartCoroutine(PulseButton(_claimButton.transform));
            }
            else
            {
                if (_cardBackground != null && _themeConfig != null)
                    _cardBackground.color = _themeConfig.panelBackground;
            }
        }

        public void Refresh(MissionProgress progress)
        {
            if (_progressLabel != null)
                _progressLabel.text = $"{progress.progress} / {progress.progress}";
            if (_progressBar != null)
            {
                float target = _progressBar.maxValue > 0
                    ? Mathf.Clamp01((float)progress.progress / _progressBar.maxValue)
                    : 0f;
                StartCoroutine(AnimateProgressBar(target, 0.3f));
            }
        }

        private IEnumerator AnimateProgressBar(float targetValue, float duration)
        {
            if (_progressBar == null) yield break;
            float startValue = _progressBar.value;
            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                _progressBar.value = Mathf.Lerp(startValue, targetValue, elapsed / duration);
                yield return null;
            }
            _progressBar.value = targetValue;
        }

        private static IEnumerator FadeAlpha(CanvasGroup cg, float from, float to, float duration)
        {
            float elapsed = 0f;
            cg.alpha = from;
            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                cg.alpha = Mathf.Lerp(from, to, elapsed / duration);
                yield return null;
            }
            cg.alpha = to;
        }

        private static IEnumerator PulseButton(Transform t)
        {
            if (t == null) yield break;
            Vector3 original = t.localScale;
            float pulseInterval = 2f;

            while (true)
            {
                yield return new WaitForSecondsRealtime(pulseInterval);
                float elapsed = 0f;
                float half = 0.15f;
                while (elapsed < half)
                {
                    elapsed += Time.unscaledDeltaTime;
                    t.localScale = Vector3.Lerp(original, original * 1.08f, elapsed / half);
                    yield return null;
                }
                elapsed = 0f;
                while (elapsed < half)
                {
                    elapsed += Time.unscaledDeltaTime;
                    t.localScale = Vector3.Lerp(original * 1.08f, original, elapsed / half);
                    yield return null;
                }
                t.localScale = original;
            }
        }

        private void OnClaimButtonClicked() => OnClaimPressed?.Invoke(_currentMissionId);

        private void OnDestroy()
        {
            _claimButton?.onClick.RemoveAllListeners();
        }
    }
}
