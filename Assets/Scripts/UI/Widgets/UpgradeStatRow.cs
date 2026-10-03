using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
namespace KingSmash.UI.Widgets
{
    public class UpgradeStatRow : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _statNameLabel;
        [SerializeField] private TextMeshProUGUI _levelLabel;
        [SerializeField] private Slider _progressBar;
        [SerializeField] private TextMeshProUGUI _costLabel;
        [SerializeField] private Button _upgradeButton;

        private const int MaxStatLevel = 10;

        public void SetupButton(Action callback)
        {
            _upgradeButton?.onClick.AddListener(() => callback?.Invoke());
        }

        public void Refresh(string statName, int currentLevel, long cost, long playerCoins)
        {
            if (_statNameLabel != null) _statNameLabel.text = statName;
            if (_levelLabel    != null) _levelLabel.text    = $"Lv {currentLevel}";
            if (_progressBar   != null) _progressBar.value  = Mathf.Clamp01((float)currentLevel / MaxStatLevel);
            if (_costLabel     != null) _costLabel.text     = currentLevel >= MaxStatLevel ? "MAX" : $"{cost:N0}";
            if (_upgradeButton != null) _upgradeButton.interactable = playerCoins >= cost && currentLevel < MaxStatLevel;
        }

        private void OnDestroy()
        {
            _upgradeButton?.onClick.RemoveAllListeners();
        }
    }
}
