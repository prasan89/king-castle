using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using KingSmash.Retention;
using KingSmash.Economy;

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

        public event Action<string> OnClaimPressed;

        private string _currentMissionId;

        public void Setup(MissionDefinition def, MissionProgress progress)
        {
            _currentMissionId = def.missionId;

            bool completed = progress.progress >= def.target;
            bool claimed = progress.claimed;

            if (_titleLabel != null) _titleLabel.text = def.displayName;
            if (_descriptionLabel != null) _descriptionLabel.text = def.description;
            if (_progressLabel != null) _progressLabel.text = $"{progress.progress} / {def.target}";
            if (_progressBar != null) _progressBar.value = Mathf.Clamp01((float)progress.progress / Mathf.Max(1, def.target));
            if (_rewardLabel != null) _rewardLabel.text = CurrencyFormatter.Format(def.coinReward);
            if (_iconImage != null && def.icon != null) _iconImage.sprite = def.icon;

            if (_claimButton != null)
            {
                _claimButton.gameObject.SetActive(completed && !claimed);
                _claimButton.onClick.RemoveAllListeners();
                _claimButton.onClick.AddListener(OnClaimButtonClicked);
            }

            if (_claimedBadge != null) _claimedBadge.SetActive(claimed);
            if (_claimButtonLabel != null) _claimButtonLabel.text = "CLAIM";
        }

        public void Refresh(MissionProgress progress)
        {
            if (_progressLabel != null) _progressLabel.text = $"{progress.progress} / {progress.progress}";
            if (_progressBar != null)
            {
                float target = _progressBar.maxValue > 0 ? _progressBar.maxValue : 1f;
                _progressBar.value = Mathf.Clamp01((float)progress.progress / target);
            }
        }

        private void OnClaimButtonClicked() => OnClaimPressed?.Invoke(_currentMissionId);

        private void OnDestroy() => _claimButton?.onClick.RemoveAllListeners();
    }
}
