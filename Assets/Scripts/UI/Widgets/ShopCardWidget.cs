using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
namespace KingSmash.UI.Widgets
{
    public class ShopCardWidget : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _nameLabel;
        [SerializeField] private TextMeshProUGUI _priceLabel;
        [SerializeField] private Button _buyButton;

        public void Setup(string displayName, string price, Action onBuy)
        {
            if (_nameLabel  != null) _nameLabel.text  = displayName;
            if (_priceLabel != null) _priceLabel.text = price;
            if (_buyButton  != null)
            {
                _buyButton.onClick.RemoveAllListeners();
                _buyButton.onClick.AddListener(() => onBuy?.Invoke());
            }
        }

        private void OnDestroy() => _buyButton?.onClick.RemoveAllListeners();
    }
}
