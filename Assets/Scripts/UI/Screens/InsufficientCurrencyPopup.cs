using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using KingSmash.Economy;

namespace KingSmash.UI.Screens
{
    public class InsufficientCurrencyPopup : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _titleLabel;
        [SerializeField] private TextMeshProUGUI _currentLabel;
        [SerializeField] private TextMeshProUGUI _requiredLabel;
        [SerializeField] private TextMeshProUGUI _shortfallLabel;
        [SerializeField] private Button          _earnCoinsButton;
        [SerializeField] private Button          _closeButton;

        public static event Action OnEarnCoinsRequested;

        private void Awake()
        {
            if (_earnCoinsButton != null)
                _earnCoinsButton.onClick.AddListener(OnEarnCoinsClicked);

            if (_closeButton != null)
                _closeButton.onClick.AddListener(OnCloseClicked);
        }

        private void OnDestroy()
        {
            if (_earnCoinsButton != null)
                _earnCoinsButton.onClick.RemoveListener(OnEarnCoinsClicked);

            if (_closeButton != null)
                _closeButton.onClick.RemoveListener(OnCloseClicked);
        }

        public void Show(CurrencyType type, long current, long required)
        {
            if (_titleLabel    != null) _titleLabel.text    = "Not Enough " + type.ToString();
            if (_currentLabel  != null) _currentLabel.text  = "You have: "  + CurrencyFormatter.Format(current);
            if (_requiredLabel != null) _requiredLabel.text  = "You need: "  + CurrencyFormatter.Format(required);
            if (_shortfallLabel != null) _shortfallLabel.text = "Short by: " + CurrencyFormatter.Format(required - current);

            gameObject.SetActive(true);
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }

        private void OnEarnCoinsClicked()
        {
            OnEarnCoinsRequested?.Invoke();
            Hide();
        }

        private void OnCloseClicked()
        {
            Hide();
        }
    }
}
