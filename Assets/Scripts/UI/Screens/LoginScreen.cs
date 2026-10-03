using UnityEngine;
using UnityEngine.UI;
using TMPro;
using KingSmash.Core;
using KingSmash.Services;
namespace KingSmash.UI.Screens
{
    public class LoginScreen : UIScreen
    {
        [Header("Buttons")]
        [SerializeField] private Button _googleButton;
        [SerializeField] private Button _playGamesButton;
        [SerializeField] private Button _emailButton;
        [SerializeField] private Button _guestButton;

        [Header("Status")]
        [SerializeField] private TextMeshProUGUI _statusLabel;
        [SerializeField] private GameObject _loadingSpinner;

        protected override void Awake()
        {
            base.Awake();
            _googleButton?.onClick.AddListener(OnGoogleClicked);
            _playGamesButton?.onClick.AddListener(OnPlayGamesClicked);
            _emailButton?.onClick.AddListener(OnEmailClicked);
            _guestButton?.onClick.AddListener(OnGuestClicked);
        }

        protected override void OnShow()
        {
            SetStatus("", false);
        }

        private void OnGoogleClicked()    => AttemptSignIn("google");
        private void OnPlayGamesClicked() => AttemptSignIn("play_games");
        private void OnEmailClicked()     => AttemptSignIn("email");
        private void OnGuestClicked()     => AttemptSignIn("guest");

        private async void AttemptSignIn(string method)
        {
            SetStatus("Signing in...", true);
            if (ServiceLocator.TryGet<IAuthService>(out var auth))
            {
                await auth.SignInAnonymouslyAsync();
            }
            if (ServiceLocator.TryGet<IAnalyticsService>(out var analytics))
                analytics.LogEvent("login_attempt", ("method", method));
            SetStatus("", false);
            ScreenManager.Instance.Show<HomeScreen>();
        }

        private void SetStatus(string msg, bool showSpinner)
        {
            if (_statusLabel != null) _statusLabel.text = msg;
            if (_loadingSpinner != null) _loadingSpinner.SetActive(showSpinner);
        }

        private void OnDestroy()
        {
            _googleButton?.onClick.RemoveListener(OnGoogleClicked);
            _playGamesButton?.onClick.RemoveListener(OnPlayGamesClicked);
            _emailButton?.onClick.RemoveListener(OnEmailClicked);
            _guestButton?.onClick.RemoveListener(OnGuestClicked);
        }
    }
}
