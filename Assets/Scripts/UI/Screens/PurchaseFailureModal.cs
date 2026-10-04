using UnityEngine;
using UnityEngine.UI;
using TMPro;
using KingSmash.Shop;

namespace KingSmash.UI.Screens
{
    public class PurchaseFailureModal : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _messageLabel;
        [SerializeField] private Button _closeButton;

        private void Awake() => _closeButton?.onClick.AddListener(OnCloseClicked);

        public void Show(PurchaseFlowResult result)
        {
            if (_messageLabel != null)
            {
                _messageLabel.text = result.IsCancelled
                    ? "Purchase cancelled."
                    : "Purchase could not be completed. You were not charged.";
            }
            gameObject.SetActive(true);
        }

        public void Hide() => gameObject.SetActive(false);

        private void OnCloseClicked() => Hide();

        private void OnDestroy() => _closeButton?.onClick.RemoveListener(OnCloseClicked);
    }
}
