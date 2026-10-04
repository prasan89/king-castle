using System;
using System.Collections.Generic;
using KingSmash.Services;
using KingSmash.Save;

namespace KingSmash.Shop
{
    public class EntitlementService : IEntitlementService, IAdEntitlementService
    {
        public const string RemoveAdsProductId = "remove_ads";

        private readonly ISaveService _save;

        public static event Action<string> OnEntitlementGranted;

        public EntitlementService(ISaveService save)
        {
            _save = save;
        }

        public bool HasEntitlement(string entitlementId)
        {
            var data = _save.Current;
            return data.ownedProducts != null && data.ownedProducts.Contains(entitlementId);
        }

        public void GrantEntitlement(string entitlementId)
        {
            var data = _save.Current;
            if (data.ownedProducts == null)
                data.ownedProducts = new List<string>();
            if (!data.ownedProducts.Contains(entitlementId))
            {
                data.ownedProducts.Add(entitlementId);
                _save.Save();
                OnEntitlementGranted?.Invoke(entitlementId);
            }
        }

        public void RevokeEntitlement(string entitlementId)
        {
            var data = _save.Current;
            if (data.ownedProducts != null && data.ownedProducts.Remove(entitlementId))
                _save.Save();
        }

        public bool CanShowAds() => !HasEntitlement(RemoveAdsProductId);
    }
}
