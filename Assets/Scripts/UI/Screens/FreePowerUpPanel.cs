using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using KingSmash.Ads;
using KingSmash.PowerUps;

namespace KingSmash.UI.Screens
{
    public class FreePowerUpPanel : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _titleLabel;
        [SerializeField] private TextMeshProUGUI _powerUpNameLabel;
        [SerializeField] private Button          _watchAdButton;
        [SerializeField] private Button          _noThanksButton;
        [SerializeField] private GameObject      _loadingSpinner;
        [SerializeField] private GameObject      _availableRoot;
        [SerializeField] private GameObject      _unavailableRoot;

        public static event Action OnFreePowerUpAccepted;
        public static event Action OnFreePowerUpDeclined;

        private Coroutine _autoHideCoroutine;

        private void Awake()
        {
            _watchAdButton?.onClick.AddListener(HandleAccepted);
            _noThanksButton?.onClick.AddListener(HandleDeclined);
        }

        public void Show(PowerUpType powerUpType, AdAvailability availability)
        {
            if (availability == AdAvailability.RemoveAdsPurchased)
            {
                gameObject.SetActive(false);
                return;
            }

            gameObject.SetActive(true);

            if (_autoHideCoroutine != null)
            {
                StopCoroutine(_autoHideCoroutine);
                _autoHideCoroutine = null;
            }

            bool isUnavailable = availability == AdAvailability.Unavailable
                              || availability == AdAvailability.Offline
                              || availability == AdAvailability.Disabled;

            if (isUnavailable)
            {
                SetRoots(available: false, unavailable: true);
                if (_loadingSpinner != null) _loadingSpinner.SetActive(false);
                _autoHideCoroutine = StartCoroutine(AutoHideAfterDelay(2f));
                return;
            }

            if (availability == AdAvailability.Loading)
            {
                SetRoots(available: false, unavailable: false);
                if (_loadingSpinner != null) _loadingSpinner.SetActive(true);
                return;
            }

            SetRoots(available: true, unavailable: false);
            if (_loadingSpinner != null) _loadingSpinner.SetActive(false);

            if (_titleLabel != null)
                _titleLabel.text = "FREE POWER-UP!";

            if (_powerUpNameLabel != null)
                _powerUpNameLabel.text = "1x " + powerUpType.DisplayName();
        }

        public void Hide() => gameObject.SetActive(false);

        private void SetRoots(bool available, bool unavailable)
        {
            if (_availableRoot   != null) _availableRoot.SetActive(available);
            if (_unavailableRoot != null) _unavailableRoot.SetActive(unavailable);
        }

        private IEnumerator AutoHideAfterDelay(float seconds)
        {
            yield return new WaitForSecondsRealtime(seconds);
            Hide();
            OnFreePowerUpDeclined?.Invoke();
        }

        private void HandleAccepted()
        {
            if (_autoHideCoroutine != null)
            {
                StopCoroutine(_autoHideCoroutine);
                _autoHideCoroutine = null;
            }
            OnFreePowerUpAccepted?.Invoke();
        }

        private void HandleDeclined()
        {
            if (_autoHideCoroutine != null)
            {
                StopCoroutine(_autoHideCoroutine);
                _autoHideCoroutine = null;
            }
            OnFreePowerUpDeclined?.Invoke();
        }

        private void OnDestroy()
        {
            _watchAdButton?.onClick.RemoveListener(HandleAccepted);
            _noThanksButton?.onClick.RemoveListener(HandleDeclined);
        }
    }
}
