using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using KingSmash.Ads;

namespace KingSmash.UI.Widgets
{
    public class RewardedAdButton : MonoBehaviour
    {
        [SerializeField] private Button            _button;
        [SerializeField] private TextMeshProUGUI   _ctaLabel;
        [SerializeField] private TextMeshProUGUI   _unavailableLabel;
        [SerializeField] private GameObject        _loadingSpinner;
        [SerializeField] private GameObject        _availableState;
        [SerializeField] private GameObject        _unavailableState;

        private Action _onAccepted;
        private Action _onDeclined;

        public void Setup(string ctaText, AdAvailability availability, Action onAccepted, Action onDeclined)
        {
            _onAccepted = onAccepted;
            _onDeclined = onDeclined;

            if (_button != null)
            {
                _button.onClick.RemoveAllListeners();
                _button.onClick.AddListener(HandleAccepted);
            }

            if (_ctaLabel != null)
                _ctaLabel.text = ctaText;

            Refresh(availability);
        }

        public void Refresh(AdAvailability availability)
        {
            bool available   = availability == AdAvailability.Available;
            bool loading     = availability == AdAvailability.Loading;
            bool unavailable = !available && !loading;

            if (_availableState   != null) _availableState.SetActive(available);
            if (_loadingSpinner   != null) _loadingSpinner.SetActive(loading);
            if (_unavailableState != null) _unavailableState.SetActive(unavailable);

            if (_button != null)
                _button.interactable = available;

            if (_unavailableLabel != null && unavailable)
                _unavailableLabel.text = "Reward unavailable";
        }

        private void HandleAccepted() => _onAccepted?.Invoke();

        private void OnDestroy()
        {
            if (_button != null)
                _button.onClick.RemoveAllListeners();
        }
    }
}
