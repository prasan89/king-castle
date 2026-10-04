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

        [SerializeField] private Color _claimedColor = new Color(0.3f, 0.3f, 0.3f);
        [SerializeField] private Color _currentAvailableColor = new Color(1.0f, 0.8f, 0.1f);
        [SerializeField] private Color _currentClaimedColor = new Color(0.6f, 0.6f, 0.3f);
        [SerializeField] private Color _upcomingColor = new Color(0.2f, 0.45f, 0.85f);

        public void Setup(DailyRewardEntry entry, DayState state)
        {
            if (_dayLabel != null) _dayLabel.text = $"DAY {entry.day}";

            string rewardText = BuildRewardText(entry);
            if (_rewardLabel != null) _rewardLabel.text = rewardText;
            if (_coinAmountLabel != null) _coinAmountLabel.text = entry.coinsReward > 0 ? entry.coinsReward.ToString() : "";
            if (_gemAmountLabel != null) _gemAmountLabel.text = entry.gemsReward > 0 ? $"+{entry.gemsReward}" : "";

            bool isClaimed = state == DayState.Claimed || state == DayState.CurrentClaimed;
            if (_claimedIndicator != null) _claimedIndicator.SetActive(isClaimed);
            if (_checkmarkRoot != null) _checkmarkRoot.SetActive(isClaimed);
            if (_currentGlow != null) _currentGlow.SetActive(state == DayState.CurrentAvailable);

            if (_background != null)
            {
                _background.color = state switch
                {
                    DayState.Claimed => _claimedColor,
                    DayState.CurrentAvailable => _currentAvailableColor,
                    DayState.CurrentClaimed => _currentClaimedColor,
                    _ => _upcomingColor
                };
            }

            bool isSpecial = entry.isTreasureChest || entry.day == 7;
            transform.localScale = isSpecial ? Vector3.one * 1.15f : Vector3.one;
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
