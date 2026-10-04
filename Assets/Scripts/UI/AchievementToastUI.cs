using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using KingSmash.Economy;
using KingSmash.Retention;

namespace KingSmash.UI
{
    public class AchievementToastUI : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _titleLabel;
        [SerializeField] private TextMeshProUGUI _subtitleLabel;
        [SerializeField] private TextMeshProUGUI _rewardLabel;
        [SerializeField] private CanvasGroup _canvasGroup;
        [SerializeField] private float _displayDuration = 3f;

        private struct ToastData
        {
            public string achievementName;
            public string rewardText;
        }

        private static readonly Queue<ToastData> _pendingToasts = new();
        private bool _showing;

        public static void QueueToast(string achievementName, string rewardText)
        {
            _pendingToasts.Enqueue(new ToastData { achievementName = achievementName, rewardText = rewardText });
        }

        private void OnEnable()
        {
            MissionService.OnAchievementTierClaimed += HandleAchievementTierClaimed;
        }

        private void OnDisable()
        {
            MissionService.OnAchievementTierClaimed -= HandleAchievementTierClaimed;
        }

        private void HandleAchievementTierClaimed(string achievementId, int tierIndex, RewardResult reward)
        {
            string rewardText = reward.coins > 0 ? $"+{CurrencyFormatter.Format(reward.coins)}" : "";
            if (reward.gems > 0) rewardText += (rewardText.Length > 0 ? " " : "") + $"+{reward.gems} Gems";
            QueueToast(achievementId, rewardText);
            if (!_showing) StartCoroutine(ShowNextToast());
        }

        private IEnumerator ShowNextToast()
        {
            while (_pendingToasts.Count > 0)
            {
                _showing = true;
                var data = _pendingToasts.Dequeue();

                if (_titleLabel != null) _titleLabel.text = "Achievement Unlocked!";
                if (_subtitleLabel != null) _subtitleLabel.text = data.achievementName;
                if (_rewardLabel != null) _rewardLabel.text = data.rewardText;

                if (_canvasGroup != null)
                {
                    gameObject.SetActive(true);
                    yield return UIAnimationController.Fade(_canvasGroup, 0f, 1f, 0.25f);
                    yield return new WaitForSeconds(_displayDuration);
                    yield return UIAnimationController.Fade(_canvasGroup, 1f, 0f, 0.25f);
                    gameObject.SetActive(false);
                }
                else
                {
                    yield return new WaitForSeconds(_displayDuration);
                }
            }

            _showing = false;
        }
    }
}
