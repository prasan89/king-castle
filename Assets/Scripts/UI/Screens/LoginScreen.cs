using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using KingSmash.Cloud;
using KingSmash.Core;
using KingSmash.Services;

namespace KingSmash.UI.Screens
{
    public class LoginScreen : UIScreen
    {
        [Header("Buttons")]
        [SerializeField] private Button _signInGoogleButton;
        [SerializeField] private Button _continueAnonymouslyButton;

        [Header("Status")]
        [SerializeField] private TextMeshProUGUI _errorLabel;
        [SerializeField] private GameObject      _loadingSpinner;

        protected override void Awake()
        {
            base.Awake();
            _signInGoogleButton?.onClick.AddListener(OnSignInGoogleClicked);
            _continueAnonymouslyButton?.onClick.AddListener(OnContinueAnonymouslyClicked);
        }

        protected override void OnShow()
        {
            ClearError();
            SetLoading(false);
        }

        private async void OnSignInGoogleClicked()
        {
            SetLoading(true);
            ClearError();

            try
            {
                if (!ServiceLocator.TryGet<IAuthService>(out var auth))
                    throw new Exception("Auth service unavailable.");

                await auth.SignInWithGoogleAsync();

                if (ServiceLocator.TryGet<CloudSyncService>(out var sync))
                    await sync.FullSyncAsync();

                ServiceLocator.TryGet<IAnalyticsService>(out var analytics);
                analytics?.LogEvent(AnalyticsEvents.AppOpen);

                ScreenManager.Instance.Show<HomeScreen>();
            }
            catch (Exception ex)
            {
                GameLogger.Error("LoginScreen", $"Google sign-in failed: {ex.Message}");
                ShowError(TranslateError(ex));
            }
            finally
            {
                SetLoading(false);
            }
        }

        private async void OnContinueAnonymouslyClicked()
        {
            SetLoading(true);
            ClearError();

            try
            {
                if (!ServiceLocator.TryGet<IAuthService>(out var auth))
                    throw new Exception("Auth service unavailable.");

                await auth.SignInAnonymouslyAsync();

                ServiceLocator.TryGet<IAnalyticsService>(out var analytics);
                analytics?.LogEvent(AnalyticsEvents.AppOpen);

                ScreenManager.Instance.Show<HomeScreen>();
            }
            catch (Exception ex)
            {
                GameLogger.Error("LoginScreen", $"Anonymous sign-in failed: {ex.Message}");
                ShowError(TranslateError(ex));
            }
            finally
            {
                SetLoading(false);
            }
        }

        private void SetLoading(bool loading)
        {
            if (_loadingSpinner          != null) _loadingSpinner.SetActive(loading);
            if (_signInGoogleButton       != null) _signInGoogleButton.interactable       = !loading;
            if (_continueAnonymouslyButton != null) _continueAnonymouslyButton.interactable = !loading;
        }

        private void ShowError(string message)
        {
            if (_errorLabel != null)
            {
                _errorLabel.text = message;
                _errorLabel.gameObject.SetActive(true);
            }
        }

        private void ClearError()
        {
            if (_errorLabel != null)
            {
                _errorLabel.text = string.Empty;
                _errorLabel.gameObject.SetActive(false);
            }
        }

        private static string TranslateError(Exception ex)
        {
            var msg = ex.Message ?? string.Empty;

            if (msg.Contains("network", StringComparison.OrdinalIgnoreCase)
                || msg.Contains("timeout", StringComparison.OrdinalIgnoreCase)
                || msg.Contains("offline", StringComparison.OrdinalIgnoreCase))
                return "No internet connection. Please check your network and try again.";

            if (msg.Contains("cancelled", StringComparison.OrdinalIgnoreCase)
                || msg.Contains("canceled", StringComparison.OrdinalIgnoreCase))
                return "Sign-in was cancelled.";

            if (msg.Contains("credential", StringComparison.OrdinalIgnoreCase)
                || msg.Contains("password", StringComparison.OrdinalIgnoreCase))
                return "Incorrect email or password.";

            if (msg.Contains("too-many-requests", StringComparison.OrdinalIgnoreCase))
                return "Too many attempts. Please try again later.";

            return "Something went wrong. Please try again.";
        }

        private void OnDestroy()
        {
            _signInGoogleButton?.onClick.RemoveListener(OnSignInGoogleClicked);
            _continueAnonymouslyButton?.onClick.RemoveListener(OnContinueAnonymouslyClicked);
        }
    }
}
