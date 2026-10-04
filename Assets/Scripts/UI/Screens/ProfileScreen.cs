using UnityEngine;
using UnityEngine.UI;
using TMPro;
using KingSmash.Core;
using KingSmash.Services;

namespace KingSmash.UI.Screens
{
    public class ProfileScreen : UIScreen
    {
        [Header("Display")]
        [SerializeField] private TMP_Text _displayNameLabel;
        [SerializeField] private TMP_Text _playerIdLabel;
        [SerializeField] private TMP_Text _levelLabel;
        [SerializeField] private TMP_Text _coinsLabel;
        [SerializeField] private TMP_Text _gemsLabel;
        [SerializeField] private Image    _avatarImage;

        [Header("Sync")]
        [SerializeField] private Button   _syncButton;
        [SerializeField] private TMP_Text _syncStatusLabel;

        [Header("Auth")]
        [SerializeField] private Button   _linkGoogleButton;
        [SerializeField] private TMP_Text _authLabel;

        protected override void Awake()
        {
            base.Awake();
            _syncButton?.onClick.AddListener(OnSyncClicked);
            _linkGoogleButton?.onClick.AddListener(OnLinkGoogleClicked);
        }

        protected override void OnShow()
        {
            RefreshProfile();
        }

        private void RefreshProfile()
        {
            if (!ServiceLocator.TryGet<ISaveService>(out var save)) return;

            var data = save.Current;

            if (_levelLabel      != null) _levelLabel.text   = $"Level {data.currentLevel}";
            if (_coinsLabel      != null) _coinsLabel.text   = $"{data.coins:N0}";
            if (_gemsLabel       != null) _gemsLabel.text    = $"{data.gems}";
            if (_displayNameLabel!= null) _displayNameLabel.text = "King";

            if (ServiceLocator.TryGet<IAuthService>(out var auth))
            {
                if (_playerIdLabel != null) _playerIdLabel.text = auth.IsAuthenticated
                    ? $"ID: {auth.UserId[..Mathf.Min(8, auth.UserId.Length)]}..."
                    : "Not signed in";

                if (_authLabel != null) _authLabel.text = auth.IsAnonymous
                    ? "Playing as Guest"
                    : "Google Account Linked";
            }
        }

        private async void OnSyncClicked()
        {
            if (_syncStatusLabel != null) _syncStatusLabel.text = "Syncing...";

            if (ServiceLocator.TryGet<CloudSyncService>(out var sync))
                await sync.SyncAsync();

            if (_syncStatusLabel != null) _syncStatusLabel.text = "Synced";
        }

        private async void OnLinkGoogleClicked()
        {
            if (ServiceLocator.TryGet<IAuthService>(out var auth))
                await auth.LinkWithGoogleAsync();

            RefreshProfile();
        }

        private void OnDestroy()
        {
            _syncButton?.onClick.RemoveListener(OnSyncClicked);
            _linkGoogleButton?.onClick.RemoveListener(OnLinkGoogleClicked);
        }
    }
}
