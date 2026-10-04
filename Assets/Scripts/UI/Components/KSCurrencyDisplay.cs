using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using KingSmash.Economy;

namespace KingSmash.UI.Components
{
    public class KSCurrencyDisplay : MonoBehaviour
    {
        [SerializeField] private Image           _icon;
        [SerializeField] private TextMeshProUGUI _amountLabel;
        [SerializeField] private CurrencyType    _currencyType;
        [SerializeField] private KingSmashTheme  _theme;

        private Coroutine _countCoroutine;
        private long      _currentAmount;

        private void Awake()
        {
            ApplyTheme();
        }

        private void OnEnable()
        {
            CurrencyService.OnCoinsChanged += OnCurrencyChanged;
        }

        private void OnDisable()
        {
            CurrencyService.OnCoinsChanged -= OnCurrencyChanged;
        }

        public void SetAmount(long amount, bool animated = false, long fromAmount = -1)
        {
            if (_countCoroutine != null) StopCoroutine(_countCoroutine);

            if (animated && fromAmount >= 0 && gameObject.activeInHierarchy)
            {
                float duration = _theme != null ? _theme.durationCountUp : 0.8f;
                _countCoroutine = StartCoroutine(
                    UIAnimationController.CountUp(_amountLabel, fromAmount, amount, duration));
            }
            else
            {
                if (_amountLabel != null)
                    _amountLabel.text = amount.ToString("N0");
            }
            _currentAmount = amount;
        }

        private void ApplyTheme()
        {
            if (_theme == null || _amountLabel == null) return;

            _amountLabel.fontSize = _theme.fontCurrency;
            _amountLabel.color    = _currencyType == CurrencyType.Coins
                ? _theme.coinGold
                : _theme.gemPurple;

            if (_icon != null)
            {
                _icon.sprite = _currencyType == CurrencyType.Coins
                    ? _theme.iconCoin
                    : _theme.iconGem;
            }
        }

        private void OnCurrencyChanged(long newBalance, CurrencyTransaction tx)
        {
            // Only react to coin changes for now; gem events would use a separate service
            if (_currencyType == CurrencyType.Coins)
                SetAmount(newBalance, animated: true, fromAmount: _currentAmount);
        }
    }
}
