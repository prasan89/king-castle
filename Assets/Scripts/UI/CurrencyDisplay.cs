using TMPro;
using UnityEngine;
using UnityEngine.UI;
using KingSmash.Economy;
using KingSmash.Core;
using KingSmash.Services;

namespace KingSmash.UI
{
    public class CurrencyDisplay : MonoBehaviour
    {
        [SerializeField] private CurrencyType    _currencyType     = CurrencyType.Coins;
        [SerializeField] private TextMeshProUGUI _label;
        [SerializeField] private Image           _icon;
        [SerializeField] private bool            _useCompactFormat = true;

        private void OnEnable()
        {
            CurrencyService.OnCoinsChanged += OnCoinsChangedHandler;
            Refresh();
        }

        private void OnDisable()
        {
            CurrencyService.OnCoinsChanged -= OnCoinsChangedHandler;
        }

        private void OnCoinsChangedHandler(long newBalance, CurrencyTransaction tx)
        {
            Refresh();
        }

        private void Refresh()
        {
            if (_label == null) return;
            if (!ServiceLocator.TryGet<ISaveService>(out var saveService)) return;

            if (_currencyType == CurrencyType.Coins)
            {
                long coins = saveService.Current.coins;
                _label.text = _useCompactFormat
                    ? CurrencyFormatter.Format(coins)
                    : CurrencyFormatter.FormatExact(coins);
            }
            else
            {
                int gems = saveService.Current.gems;
                _label.text = _useCompactFormat
                    ? CurrencyFormatter.FormatGems(gems)
                    : CurrencyFormatter.FormatExact(gems);
            }
        }
    }
}
