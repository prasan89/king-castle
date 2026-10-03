using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
namespace KingSmash.UI.Widgets
{
    public class MissionCardWidget : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _titleLabel;
        [SerializeField] private TextMeshProUGUI _progressLabel;
        [SerializeField] private Slider _progressBar;
        [SerializeField] private TextMeshProUGUI _rewardLabel;
        [SerializeField] private Button _claimButton;
        [SerializeField] private GameObject _claimedBadge;

        public void Setup(string title, int progress, int target, long coinReward, bool claimed)
        {
            if (_titleLabel    != null) _titleLabel.text    = title;
            if (_progressLabel != null) _progressLabel.text = $"{progress}/{target}";
            if (_progressBar   != null) _progressBar.value  = target > 0 ? (float)progress / target : 0f;
            if (_rewardLabel   != null) _rewardLabel.text   = coinReward.ToString("N0");
            bool canClaim = progress >= target && !claimed;
            if (_claimButton   != null) _claimButton.interactable = canClaim;
            if (_claimedBadge  != null) _claimedBadge.SetActive(claimed);
        }

        private void OnDestroy() => _claimButton?.onClick.RemoveAllListeners();
    }
}
