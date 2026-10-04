using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using KingSmash.Retention;

namespace KingSmash.UI.Widgets
{
    public enum DayState
    {
        Claimed,
        CurrentAvailable,
        CurrentClaimed,
        Upcoming
    }

    public class DayRewardWidget : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _dayLabel;
        [SerializeField] private TextMeshProUGUI _rewardLabel;
        [SerializeField] private GameObject _claimedIndicator;
        [SerializeField] private GameObject _currentGlow;
        [SerializeField] private Image _background;
        [SerializeField] private Image _rewardIcon;
        [SerializeField] private GameObject _checkmarkRoot;
        [SerializeField] private TextMeshProUGUI _coinAmountLabel;
        [SerializeField] private TextMeshProUGUI _gemAmountLabel;

        [SerializeField] private Color _claimedColor          = new Color(0.3f, 0.3f, 0.3f);
        [SerializeField] private Color _currentAvailableColor = new Color(1.0f, 0.8f, 0.1f);
        [SerializeField] private Color _currentClaimedColor   = new Color(0.6f, 0.6f, 0.3f);
        [SerializeField] private Color _upcomingColor         = new Color(0.2f, 0.45f, 0.85f);

        [Header("M13 Theme & Animation")]
        [SerializeField] private KingSmash.UI.KingSmashTheme _themeConfig;
        [SerializeField] private CanvasGroup _widgetCg;
        [SerializeField] private RectTransform _rewardRoot;
        [SerializeField] private Animator _glowAnimator;

        private Coroutine _glowPulseCoroutine;

        public void Setup(DailyRewardEntry entry, DayState state)
        {
            if (_glowPulseCoroutine != null)
            {
                StopCoroutine(_glowPulseCoroutine);
                _glowPulseCoroutine = null;
            }

            if (_dayLabel    != null) _dayLabel.text    = $"DAY {entry.day}";
            if (_rewardLabel != null) _rewardLabel.text = BuildRewardText(entry);

            if (_coinAmountLabel != null)
                _coinAmountLabel.text = entry.coinsReward > 0 ? entry.coinsReward.ToString() : "";
            if (_gemAmountLabel != null)
                _gemAmountLabel.text = entry.gemsReward > 0 ? $"+{entry.gemsReward}" : "";

            bool isClaimed = state == DayState.Claimed || state == DayState.CurrentClaimed;
            if (_claimedIndicator != null) _claimedIndicator.SetActive(isClaimed);
            if (_currentGlow      != null) _currentGlow.SetActive(state == DayState.CurrentAvailable);

            if (_checkmarkRoot != null)
            {
                _checkmarkRoot.SetActive(isClaimed);
                if (isClaimed)
                    StartCoroutine(UIAnimationController.BounceReveal(_checkmarkRoot.transform, 0.30f));
            }

            if (_background != null)
            {
                _background.color = state switch
                {
                    DayState.Claimed          => _claimedColor,
                    DayState.CurrentAvailable => _currentAvailableColor,
                    DayState.CurrentClaimed   => _currentClaimedColor,
                    _                         => _upcomingColor
                };
            }

            switch (state)
            {
                case DayState.CurrentAvailable:
                    if (_widgetCg != null) _widgetCg.alpha = 1f;
                    _glowPulseCoroutine = StartCoroutine(PulseGlow());
                    break;

                case DayState.Claimed:
                case DayState.CurrentClaimed:
                    if (_widgetCg != null) StartCoroutine(FadeWidgetAlpha(1f, 0.7f, 0.25f));
                    break;

                case DayState.Upcoming:
                    if (_widgetCg != null) _widgetCg.alpha = 0.7f;
                    break;
            }

            bool isSpecial = entry.isTreasureChest || entry.day == 7;
            transform.localScale = isSpecial ? Vector3.one * 1.15f : Vector3.one;
        }

        private IEnumerator PulseGlow()
        {
            if (_currentGlow == null) yield break;

            var cg = _currentGlow.GetComponent<CanvasGroup>();
            if (cg == null) cg = _currentGlow.AddComponent<CanvasGroup>();

            float period = 1.5f;

            while (true)
            {
                float elapsed = 0f;
                float half = period * 0.5f;
                while (elapsed < half)
                {
                    elapsed += Time.unscaledDeltaTime;
                    cg.alpha = Mathf.Lerp(0.6f, 1.0f, elapsed / half);
                    yield return null;
                }
                elapsed = 0f;
                while (elapsed < half)
                {
                    elapsed += Time.unscaledDeltaTime;
                    cg.alpha = Mathf.Lerp(1.0f, 0.6f, elapsed / half);
                    yield return null;
                }
            }
        }

        private IEnumerator FadeWidgetAlpha(float from, float to, float duration)
        {
            if (_widgetCg == null) yield break;
            float elapsed = 0f;
            _widgetCg.alpha = from;
            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                _widgetCg.alpha = Mathf.Lerp(from, to, elapsed / duration);
                yield return null;
            }
            _widgetCg.alpha = to;
        }

        private static string BuildRewardText(DailyRewardEntry entry)
        {
            if (!string.IsNullOrEmpty(entry.displayName)) return entry.displayName;
            if (entry.isTreasureChest) return "TREASURE!";
            if (entry.coinsReward > 0 && entry.gemsReward > 0) return $"{entry.coinsReward}C + {entry.gemsReward}G";
            if (entry.gemsReward > 0) return $"{entry.gemsReward} Gems";
            if (!string.IsNullOrEmpty(entry.powerUpTypeId)) return "Power-Up!";
            return entry.coinsReward.ToString();
        }
    }
}
