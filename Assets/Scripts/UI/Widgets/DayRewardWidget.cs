using UnityEngine;
using UnityEngine.UI;
using TMPro;
namespace KingSmash.UI.Widgets
{
    public class DayRewardWidget : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _dayLabel;
        [SerializeField] private TextMeshProUGUI _rewardLabel;
        [SerializeField] private GameObject _claimedIndicator;
        [SerializeField] private GameObject _currentIndicator;
        [SerializeField] private Image _background;
        [SerializeField] private Color _claimedColor  = new Color(0.3f, 0.3f, 0.3f);
        [SerializeField] private Color _currentColor  = new Color(1.0f, 0.8f, 0.1f);
        [SerializeField] private Color _upcomingColor = new Color(0.2f, 0.45f, 0.85f);

        public void Setup(int day, string rewardText, bool isCurrent, bool isClaimed, bool isSpecial)
        {
            if (_dayLabel    != null) _dayLabel.text    = $"Day {day}";
            if (_rewardLabel != null) _rewardLabel.text = rewardText;
            if (_claimedIndicator != null) _claimedIndicator.SetActive(isClaimed);
            if (_currentIndicator != null) _currentIndicator.SetActive(isCurrent);
            if (_background != null)
            {
                _background.color = isClaimed  ? _claimedColor
                                  : isCurrent  ? _currentColor
                                               : _upcomingColor;
            }
            // Special day (day 7) scales the whole widget up slightly
            if (isSpecial) transform.localScale = Vector3.one * 1.15f;
        }
    }
}
