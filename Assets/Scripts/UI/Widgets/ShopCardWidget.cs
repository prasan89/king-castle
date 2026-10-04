using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using KingSmash.Shop;
using KingSmash.Economy;

namespace KingSmash.UI.Widgets
{
    public class ShopCardWidget : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _titleLabel;
        [SerializeField] private TextMeshProUGUI _quantityLabel;
        [SerializeField] private TextMeshProUGUI _priceLabel;
        [SerializeField] private TextMeshProUGUI _bonusLabel;
        [SerializeField] private Button _buyButton;
        [SerializeField] private GameObject _bonusBadge;
        [SerializeField] private GameObject _ownedOverlay;
        [SerializeField] private GameObject _loadingSpinner;
        [SerializeField] private Image _productIcon;

        private string _productId;
        private Action<string> _onBuy;

        public void Setup(ShopProduct product, string localizedPrice, Action<string> onBuy)
        {
            _productId = product.ProductId;
            _onBuy     = onBuy;

            if (_titleLabel != null)
                _titleLabel.text = product.DisplayName;

            if (_quantityLabel != null)
                _quantityLabel.text = BuildQuantityLabel(product);

            if (_priceLabel != null)
                _priceLabel.text = localizedPrice;

            bool hasBonus = !string.IsNullOrEmpty(product.BonusLabel);
            if (_bonusBadge != null) _bonusBadge.SetActive(hasBonus);
            if (_bonusLabel != null) _bonusLabel.text = hasBonus ? product.BonusLabel : string.Empty;

            if (_productIcon != null && product.Icon != null)
                _productIcon.sprite = product.Icon;

            _buyButton?.onClick.RemoveAllListeners();
            _buyButton?.onClick.AddListener(OnBuyClicked);

            RefreshState(PurchaseState.Idle);
        }

        public void RefreshState(PurchaseState state)
        {
            switch (state)
            {
                case PurchaseState.Idle:
                case PurchaseState.Available:
                    if (_buyButton != null) _buyButton.interactable = true;
                    _loadingSpinner?.SetActive(false);
                    _ownedOverlay?.SetActive(false);
                    if (_buyButton != null) _buyButton.gameObject.SetActive(true);
                    break;

                case PurchaseState.Purchasing:
                case PurchaseState.VerifyingServer:
                    if (_buyButton != null) _buyButton.interactable = false;
                    _loadingSpinner?.SetActive(true);
                    break;

                case PurchaseState.Owned:
                    _ownedOverlay?.SetActive(true);
                    if (_buyButton != null) _buyButton.gameObject.SetActive(false);
                    _loadingSpinner?.SetActive(false);
                    break;

                case PurchaseState.Failed:
                case PurchaseState.Cancelled:
                    if (_buyButton != null) _buyButton.interactable = true;
                    _loadingSpinner?.SetActive(false);
                    break;
            }
        }

        private void OnEnable()  => PurchaseStateTracker.OnStateChanged += HandleStateChanged;
        private void OnDisable() => PurchaseStateTracker.OnStateChanged -= HandleStateChanged;

        private void HandleStateChanged(string productId, PurchaseState state)
        {
            if (productId != _productId) return;
            RefreshState(state);
        }

        private void OnBuyClicked() => _onBuy?.Invoke(_productId);

        private static string BuildQuantityLabel(ShopProduct product)
        {
            if (product.CoinsGranted > 0) return $"{CurrencyFormatter.FormatExact(product.CoinsGranted)} COINS";
            if (product.GemsGranted  > 0) return $"{product.GemsGranted} GEMS";
            return product.DisplayName.ToUpper();
        }

        private void OnDestroy() => _buyButton?.onClick.RemoveAllListeners();
    }
}
