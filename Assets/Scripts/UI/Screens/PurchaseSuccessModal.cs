using UnityEngine;
using UnityEngine.UI;
using TMPro;
using KingSmash.Economy;
using KingSmash.Shop;

namespace KingSmash.UI.Screens
{
    public class PurchaseSuccessModal : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _titleLabel;
        [SerializeField] private TextMeshProUGUI _rewardLabel;
        [SerializeField] private Button _closeButton;
        [SerializeField] private GameObject _coinRewardRow;
        [SerializeField] private GameObject _gemRewardRow;
        [SerializeField] private TextMeshProUGUI _coinAmountLabel;
        [SerializeField] private TextMeshProUGUI _gemAmountLabel;

        private void Awake() => _closeButton?.onClick.AddListener(OnCloseClicked);

        public void Show(PurchaseFlowResult result)
        {
            if (_titleLabel != null)
                _titleLabel.text = result.IsEntitlement ? "SUCCESS!" : "PURCHASE COMPLETE!";

            bool showCoinRow = result.CoinsGranted > 0;
            bool showGemRow  = result.GemsGranted  > 0;

            if (_coinRewardRow != null) _coinRewardRow.SetActive(showCoinRow);
            if (_gemRewardRow  != null) _gemRewardRow.SetActive(showGemRow);

            if (showCoinRow && _coinAmountLabel != null)
                _coinAmountLabel.text = $"+{CurrencyFormatter.Format(result.CoinsGranted)} COINS";
            if (showGemRow && _gemAmountLabel != null)
                _gemAmountLabel.text = $"+{result.GemsGranted} GEMS";

            if (_rewardLabel != null)
                _rewardLabel.text = result.IsRemoveAds ? "Ads Removed!" : string.Empty;

            gameObject.SetActive(true);
        }

        public void Hide() => gameObject.SetActive(false);

        private void OnCloseClicked() => Hide();

        private void OnDestroy() => _closeButton?.onClick.RemoveListener(OnCloseClicked);
    }
}
