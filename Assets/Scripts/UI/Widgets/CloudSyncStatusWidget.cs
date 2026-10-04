using System.Collections;
using TMPro;
using UnityEngine;
using KingSmash.Cloud;

namespace KingSmash.UI.Widgets
{
    public class CloudSyncStatusWidget : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _statusLabel;
        [SerializeField] private GameObject     _spinnerRoot;
        [SerializeField] private GameObject     _checkRoot;
        [SerializeField] private GameObject     _errorRoot;

        private const float SyncedAutoHideDelay = 3f;
        private Coroutine _autoHideCoroutine;

        private void OnEnable()
        {
            CloudSyncService.OnSyncStatusChanged += HandleSyncStatusChanged;
            ApplyStatus(SyncStatus.Idle);
        }

        private void OnDisable()
        {
            CloudSyncService.OnSyncStatusChanged -= HandleSyncStatusChanged;
            StopAutoHide();
        }

        private void HandleSyncStatusChanged(SyncStatus status)
        {
            ApplyStatus(status);
        }

        private void ApplyStatus(SyncStatus status)
        {
            StopAutoHide();
            SetAllInactive();

            switch (status)
            {
                case SyncStatus.Idle:
                    break;

                case SyncStatus.Connecting:
                case SyncStatus.Syncing:
                    SetLabel("Syncing...");
                    if (_spinnerRoot != null) _spinnerRoot.SetActive(true);
                    break;

                case SyncStatus.Synced:
                    SetLabel("Saved");
                    if (_checkRoot != null) _checkRoot.SetActive(true);
                    _autoHideCoroutine = StartCoroutine(AutoHideAfterDelay());
                    break;

                case SyncStatus.Failed:
                    SetLabel("Sync failed");
                    if (_errorRoot != null) _errorRoot.SetActive(true);
                    break;

                case SyncStatus.Offline:
                    SetLabel("Offline");
                    if (_errorRoot != null) _errorRoot.SetActive(true);
                    break;
            }
        }

        private void SetAllInactive()
        {
            if (_spinnerRoot != null) _spinnerRoot.SetActive(false);
            if (_checkRoot   != null) _checkRoot.SetActive(false);
            if (_errorRoot   != null) _errorRoot.SetActive(false);
            if (_statusLabel != null) _statusLabel.text = string.Empty;
        }

        private void SetLabel(string text)
        {
            if (_statusLabel != null) _statusLabel.text = text;
        }

        private IEnumerator AutoHideAfterDelay()
        {
            yield return new WaitForSeconds(SyncedAutoHideDelay);
            SetAllInactive();
            _autoHideCoroutine = null;
        }

        private void StopAutoHide()
        {
            if (_autoHideCoroutine != null)
            {
                StopCoroutine(_autoHideCoroutine);
                _autoHideCoroutine = null;
            }
        }
    }
}
