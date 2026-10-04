// PurchaseAnalyticsBridge.cs — M14 Analytics
// Static entry points called from ShopService or StoreService.
// Does NOT log productId to Crashlytics — IAP product IDs are considered
// potentially privacy-sensitive and must not appear in crash reports.

using UnityEngine;
using KingSmash.Core;
using KingSmash.Services;

namespace KingSmash.Analytics
{
    /// <summary>
    /// Scene MonoBehaviour that exposes static purchase-event entry points.
    /// Called directly from ShopService / StoreService — no scene wiring needed
    /// beyond having this component present in the scene so it is initialized.
    /// </summary>
    public sealed class PurchaseAnalyticsBridge : MonoBehaviour
    {
        private const string Tag = "PurchaseAnalyticsBridge";

        // No instance event subscriptions — all logic is static.

        // ── Static entry points ───────────────────────────────────────────────

        /// <summary>Tracks that the user initiated a purchase flow.</summary>
        public static void TrackPurchaseStarted(string productId, string productType)
        {
            if (!ServiceLocator.TryGet<IAnalyticsService>(out var analytics)) return;
            if (!AnalyticsRateGuard.Allow(AnalyticsEvents.PurchaseStarted)) return;

            // productId is sent to analytics only — NOT to crash reporting (privacy).
            analytics.TrackPurchaseEvent(AnalyticsEvents.PurchaseStarted, productId, productType);
        }

        /// <summary>Tracks a successful IAP transaction.</summary>
        public static void TrackPurchaseSuccess(string productId, string productType)
        {
            if (!ServiceLocator.TryGet<IAnalyticsService>(out var analytics)) return;
            if (!AnalyticsRateGuard.Allow(AnalyticsEvents.PurchaseCompleted)) return;

            analytics.TrackPurchaseEvent(AnalyticsEvents.PurchaseCompleted, productId, productType);
        }

        /// <summary>Tracks a failed IAP transaction.</summary>
        public static void TrackPurchaseFailed(string productId, string productType)
        {
            if (!ServiceLocator.TryGet<IAnalyticsService>(out var analytics)) return;
            if (!AnalyticsRateGuard.Allow(AnalyticsEvents.PurchaseFailed)) return;

            analytics.TrackPurchaseEvent(AnalyticsEvents.PurchaseFailed, productId, productType);
        }

        /// <summary>Tracks that the user cancelled a purchase flow.</summary>
        public static void TrackPurchaseCancelled(string productId, string productType)
        {
            if (!ServiceLocator.TryGet<IAnalyticsService>(out var analytics)) return;
            if (!AnalyticsRateGuard.Allow(AnalyticsEvents.PurchaseCancelled)) return;

            analytics.TrackPurchaseEvent(AnalyticsEvents.PurchaseCancelled, productId, productType);
        }

        /// <summary>Tracks a restored IAP (e.g. from restore purchases flow).</summary>
        public static void TrackPurchaseRestored(string productId, string productType)
        {
            if (!ServiceLocator.TryGet<IAnalyticsService>(out var analytics)) return;
            if (!AnalyticsRateGuard.Allow(AnalyticsEvents.PurchaseRestored)) return;

            analytics.TrackPurchaseEvent(AnalyticsEvents.PurchaseRestored, productId, productType);
        }
    }
}
