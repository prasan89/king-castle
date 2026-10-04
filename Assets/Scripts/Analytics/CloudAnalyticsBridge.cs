// CloudAnalyticsBridge.cs — M14 Analytics
// Subscribes to CloudSyncService.OnSyncStatusChanged and maps each SyncStatus
// to the appropriate cloud analytics event.
// RecordError is called via ICrashReportingService only for genuine unexpected
// failures — not for expected conditions such as Offline (no internet).

using UnityEngine;
using KingSmash.Cloud;
using KingSmash.Core;
using KingSmash.Services;

namespace KingSmash.Analytics
{
    /// <summary>
    /// Scene MonoBehaviour that bridges cloud-sync status changes to analytics
    /// and crash reporting.
    /// Distinguishes between Offline (expected, not an error) and Failed
    /// (unexpected, may warrant a crash-report non-fatal event).
    /// </summary>
    public sealed class CloudAnalyticsBridge : MonoBehaviour
    {
        private const string Tag            = "CloudAnalyticsBridge";
        private const string OpSave         = "save";
        private const string OpLoad         = "load";

        // Track the previous status so we can distinguish save vs load direction.
        private SyncStatus _previousStatus = SyncStatus.Idle;

        // ── Lifecycle ─────────────────────────────────────────────────────────

        private void OnEnable()
        {
            CloudSyncService.OnSyncStatusChanged += HandleSyncStatusChanged;
        }

        private void OnDisable()
        {
            CloudSyncService.OnSyncStatusChanged -= HandleSyncStatusChanged;
        }

        // ── Handler ───────────────────────────────────────────────────────────

        private void HandleSyncStatusChanged(SyncStatus status)
        {
            switch (status)
            {
                case SyncStatus.Syncing:
                    TrackSyncing();
                    break;

                case SyncStatus.Synced:
                    TrackSynced();
                    break;

                case SyncStatus.Failed:
                    TrackFailed();
                    break;

                case SyncStatus.Offline:
                    TrackOffline();
                    break;

                // Idle / Connecting — no analytics event needed
                case SyncStatus.Idle:
                case SyncStatus.Connecting:
                    break;
            }

            _previousStatus = status;
        }

        // ── Per-status helpers ────────────────────────────────────────────────

        private void TrackSyncing()
        {
            if (!ServiceLocator.TryGet<IAnalyticsService>(out var analytics)) return;

            // Fire both started events; the service dedups via its rate guard.
            if (AnalyticsRateGuard.Allow(AnalyticsEvents.CloudSaveStarted))
                analytics.TrackCloudEvent(AnalyticsEvents.CloudSaveStarted, OpSave, isOnline: true);

            if (AnalyticsRateGuard.Allow(AnalyticsEvents.CloudLoadStarted))
                analytics.TrackCloudEvent(AnalyticsEvents.CloudLoadStarted, OpLoad, isOnline: true);
        }

        private void TrackSynced()
        {
            if (!ServiceLocator.TryGet<IAnalyticsService>(out var analytics)) return;

            if (AnalyticsRateGuard.Allow(AnalyticsEvents.CloudSaveSuccess))
                analytics.TrackCloudEvent(AnalyticsEvents.CloudSaveSuccess, OpSave, isOnline: true);

            if (AnalyticsRateGuard.Allow(AnalyticsEvents.CloudLoadSuccess))
                analytics.TrackCloudEvent(AnalyticsEvents.CloudLoadSuccess, OpLoad, isOnline: true);
        }

        private void TrackFailed()
        {
            if (ServiceLocator.TryGet<IAnalyticsService>(out var analytics))
            {
                if (AnalyticsRateGuard.Allow(AnalyticsEvents.CloudSaveFailed))
                {
                    analytics.TrackCloudEvent(
                        AnalyticsEvents.CloudSaveFailed,
                        OpSave,
                        isOnline: true,
                        errorCategory: ErrorCategory.Save.ToString());
                }
            }

            // Record a non-fatal crash event for genuinely unexpected sync failures.
            // Offline is NOT recorded here (handled separately in TrackOffline).
            if (ServiceLocator.TryGet<ICrashReportingService>(out var crash))
            {
                crash.RecordError(
                    ErrorCategory.Save,
                    "CloudSyncService reported SyncStatus.Failed");
            }
        }

        private void TrackOffline()
        {
            // Offline is an expected, recoverable condition — do not record an error.
            // Track is_online=false for funnel analysis only.
            if (!ServiceLocator.TryGet<IAnalyticsService>(out var analytics)) return;
            if (!AnalyticsRateGuard.Allow(AnalyticsEvents.CloudSaveFailed)) return;

            analytics.TrackCloudEvent(
                AnalyticsEvents.CloudSaveFailed,
                OpSave,
                isOnline: false,
                errorCategory: "offline");
        }
    }
}
