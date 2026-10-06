using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using KingSmash.Retention;
using KingSmash.Save;

namespace KingSmash.UI.Widgets
{
    public class AchievementCardWidget : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _titleLabel;
        [SerializeField] private TextMeshProUGUI _descriptionLabel;
        [SerializeField] private TextMeshProUGUI _progressLabel;
        [SerializeField] private Slider _progressBar;
        [SerializeField] private Button _claimButton;
        [SerializeField] private TextMeshProUGUI _claimButtonLabel;
        [SerializeField] private GameObject _claimedBadge;
        [SerializeField] private List<GameObject> _tierIndicators;
        [SerializeField] private Image _iconImage;

        public event Action<string> OnClaimPressed;

        private string _currentAchievementId;

        public void Setup(AchievementDefinition def, AchievementProgress progress)
        {
            _currentAchievementId = def.achievementId;

            if (_titleLabel != null) _titleLabel.text = def.displayName;
            if (_descriptionLabel != null) _descriptionLabel.text = def.description;
            if (_iconImage != null && def.icon != null) _iconImage.sprite = def.icon;

            int claimedCount = progress.claimedTierCount;
            int currentTierIndex = Mathf.Clamp(claimedCount, 0, def.tiers.Count - 1);
            AchievementTier nextTier = claimedCount < def.tiers.Count ? def.tiers[claimedCount] : null;

            if (_progressBar != null)
                _progressBar.value = nextTier != null ? Mathf.Clamp01((float)progress.progress / Mathf.Max(1, nextTier.targetCount)) : 1f;

            if (_progressLabel != null)
            {
                if (nextTier != null)
                    _progressLabel.text = $"{progress.progress} / {nextTier.targetCount}";
                else
                    _progressLabel.text = "COMPLETE";
            }

            UpdateTierIndicators(def, claimedCount, progress.progress);

            bool canClaim = nextTier != null && progress.progress >= nextTier.targetCount;
            if (_claimButton != null)
            {
                _claimButton.gameObject.SetActive(canClaim);
                _claimButton.onClick.RemoveAllListeners();
                _claimButton.onClick.AddListener(OnClaimButtonClicked);
            }

            if (_claimedBadge != null) _claimedBadge.SetActive(claimedCount >= def.tiers.Count);
            if (_claimButtonLabel != null) _claimButtonLabel.text = "CLAIM TIER";
        }

        private void UpdateTierIndicators(AchievementDefinition def, int claimedCount, int currentProgress)
        {
            if (_tierIndicators == null) return;
            for (int i = 0; i < _tierIndicators.Count; i++)
            {
                if (_tierIndicators[i] == null) continue;
                bool hasTier = i < def.tiers.Count;
                _tierIndicators[i].SetActive(hasTier);
                // Filled for claimed tiers; current for next claimable; empty for future
                var img = _tierIndicators[i].GetComponent<Image>();
                if (img == null) continue;
                if (i < claimedCount)
                    img.color = new Color(1f, 0.8f, 0.1f);
                else if (i == claimedCount && hasTier && currentProgress >= def.tiers[i].targetCount)
                    img.color = new Color(0.2f, 1f, 0.2f);
                else
                    img.color = new Color(0.4f, 0.4f, 0.4f);
            }
        }

        private void OnClaimButtonClicked() => OnClaimPressed?.Invoke(_currentAchievementId);

        private void OnDestroy() => _claimButton?.onClick.RemoveAllListeners();
    }
}
