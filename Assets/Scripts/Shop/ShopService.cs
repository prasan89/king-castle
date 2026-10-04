using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using KingSmash.Core;
using KingSmash.Economy;
using KingSmash.PowerUps;
using KingSmash.Services;
using UnityEngine;

namespace KingSmash.Shop
{
    public enum ShopPurchaseError
    {
        None,
        ProductNotFound,
        StoreFailed,
        UserCancelled,
        VerificationFailed,
        RewardFailed,
        Unknown,
    }

    public class PurchaseFlowResult
    {
        public bool Success;
        public string ProductId;
        public ShopPurchaseError Error;
        public string ErrorMessage;
        public bool IsCancelled;
        public bool IsAlreadyOwned;
        public bool IsEntitlement;
        public bool IsRemoveAds;
        public long CoinsGranted;
        public int GemsGranted;

        public bool IsSuccess => Success;
    }

    public sealed class ShopService
    {
        public static event Action<string, PurchaseFlowResult> OnPurchaseCompleted;

        private const string Tag = "ShopService";

        private readonly IStoreService              _store;
        private readonly IReceiptVerificationService _verifier;
        private readonly RewardService              _rewardService;
        private readonly IEntitlementService        _entitlements;
        private readonly CurrencyService            _currency;
        private readonly ISaveService               _save;
        private readonly IAuthService               _auth;
        private readonly ShopProductCatalog         _catalog;
        private readonly PurchaseStateTracker       _stateTracker = new();

        public bool IsInitialized => _store.IsInitialized;

        public ShopService(
            IStoreService               store,
            IReceiptVerificationService  verifier,
            IEntitlementService         entitlements,
            CurrencyService             currency,
            ShopProductCatalog          catalog)
        {
            _store         = store         ?? throw new ArgumentNullException(nameof(store));
            _verifier      = verifier      ?? throw new ArgumentNullException(nameof(verifier));
            _entitlements  = entitlements  ?? throw new ArgumentNullException(nameof(entitlements));
            _currency      = currency      ?? throw new ArgumentNullException(nameof(currency));
            _catalog       = catalog       ?? throw new ArgumentNullException(nameof(catalog));
        }

        public ShopService(
            IStoreService               store,
            IReceiptVerificationService  verifier,
            RewardService               rewardService,
            IEntitlementService         entitlements,
            ISaveService                save,
            IAuthService                auth,
            ShopProductCatalog          catalog)
        {
            _store         = store         ?? throw new ArgumentNullException(nameof(store));
            _verifier      = verifier      ?? throw new ArgumentNullException(nameof(verifier));
            _rewardService = rewardService;
            _entitlements  = entitlements  ?? throw new ArgumentNullException(nameof(entitlements));
            _save          = save;
            _auth          = auth;
            _catalog       = catalog       ?? throw new ArgumentNullException(nameof(catalog));
        }

        public Task InitializeAsync() => _store.InitializeAsync();

        public Task InitAsync() => _store.InitializeAsync();

        public string GetLocalizedPrice(string productId) => _store.GetLocalizedPrice(productId);

        public PurchaseState GetState(string productId) => _stateTracker.GetState(productId);

        public async Task<PurchaseFlowResult> PurchaseAsync(string productId)
        {
            var product = _catalog.GetProduct(productId);
            if (product == null)
            {
                GameLogger.Warning(Tag, $"Product not found: {productId}");
                return Fail(productId, ShopPurchaseError.ProductNotFound, "Product not found in catalog.");
            }

            SetState(productId, PurchaseState.Purchasing);

            StorePurchaseResult storeResult;
            try
            {
                storeResult = await _store.PurchaseAsync(productId);
            }
            catch (Exception ex)
            {
                GameLogger.Error(Tag, $"Store purchase threw: {ex.Message}");
                SetState(productId, PurchaseState.Failed);
                return Fail(productId, ShopPurchaseError.StoreFailed, ex.Message);
            }

            if (storeResult.UserCancelled)
            {
                SetState(productId, PurchaseState.Cancelled);
                var cancelled = new PurchaseFlowResult { ProductId = productId, IsCancelled = true, Error = ShopPurchaseError.UserCancelled };
                OnPurchaseCompleted?.Invoke(productId, cancelled);
                return cancelled;
            }

            if (!storeResult.Success)
            {
                SetState(productId, PurchaseState.Failed);
                return Fail(productId, ShopPurchaseError.StoreFailed, storeResult.Error);
            }

            SetState(productId, PurchaseState.VerifyingServer);

            string purchaseToken = ExtractPurchaseToken(storeResult.Receipt);
            string playerId = _save?.Current.playerId ?? "unknown";
            string idToken  = "";

            if (_auth?.CurrentUser != null && !string.IsNullOrEmpty(_auth.CurrentUser.UserId))
                idToken = _auth.CurrentUser.UserId;

            VerificationResult vr;
            try
            {
                if (_verifier is ReceiptVerificationServiceMock mock)
                {
                    vr = await mock.VerifyAsync(productId, purchaseToken);
                }
                else
                {
                    vr = await _verifier.VerifyAsync(new VerificationRequest
                    {
                        PlayerId      = playerId,
                        ProductId     = productId,
                        PurchaseToken = purchaseToken,
                        PackageName   = Application.identifier,
                        IdToken       = idToken,
                    });
                }
            }
            catch (Exception ex)
            {
                GameLogger.Error(Tag, $"Verification threw: {ex.Message}");
                SetState(productId, PurchaseState.Failed);
                return Fail(productId, ShopPurchaseError.VerificationFailed, ex.Message);
            }

            if (!vr.IsValid && !vr.IsAlreadyProcessed)
            {
                GameLogger.Warning(Tag, $"Verification failed for {productId}: {vr.Error}");
                SetState(productId, PurchaseState.Failed);
                return Fail(productId, ShopPurchaseError.VerificationFailed, vr.Error);
            }

            if (vr.CoinsGranted > 0)
            {
                if (_rewardService != null)
                    _rewardService.GrantShopCoins(vr.CoinsGranted, productId);
                else if (_currency != null)
                    _currency.TryAdd(vr.CoinsGranted);
            }

            if (vr.GemsGranted > 0)
                _save?.AddGems(vr.GemsGranted);

            if (vr.PowerUpsGranted != null && vr.PowerUpsGranted.Count > 0)
            {
                if (ServiceLocator.TryGet<PowerUpService>(out var powerUpService))
                {
                    foreach (var pu in vr.PowerUpsGranted)
                    {
                        var puType = PowerUpTypeExtensions.FromId(pu.powerUpTypeId);
                        if (puType != PowerUpType.None)
                            powerUpService.Award(puType, pu.count);
                    }
                }
            }

            bool isEntitlement = product.IsNonConsumable;
            if (isEntitlement)
                _entitlements.GrantEntitlement(productId);

#if UNITY_PURCHASING
            if (_store is UnityIAPStoreService unityStore)
                unityStore.AcknowledgePurchase(productId);
#endif

            SetState(productId, PurchaseState.Success);

            var success = new PurchaseFlowResult
            {
                Success       = true,
                ProductId     = productId,
                CoinsGranted  = vr.CoinsGranted,
                GemsGranted   = vr.GemsGranted,
                IsEntitlement = isEntitlement,
                IsRemoveAds   = productId == EntitlementService.RemoveAdsProductId,
            };
            OnPurchaseCompleted?.Invoke(productId, success);
            return success;
        }

        public async Task<bool> RestoreAsync()
        {
            var result = await _store.RestoreAsync();
            if (result.Success && result.RestoredProductIds != null)
                foreach (var id in result.RestoredProductIds)
                    _entitlements.GrantEntitlement(id);
            return result.Success;
        }

        private void SetState(string productId, PurchaseState state)
            => _stateTracker.SetState(productId, state);

        private static PurchaseFlowResult Fail(string productId, ShopPurchaseError error, string msg)
        {
            var result = new PurchaseFlowResult { ProductId = productId, Error = error, ErrorMessage = msg };
            OnPurchaseCompleted?.Invoke(productId, result);
            return result;
        }

        private static string ExtractPurchaseToken(string receipt)
        {
            if (string.IsNullOrEmpty(receipt)) return string.Empty;
            try
            {
                int payloadIdx = receipt.IndexOf("\"Payload\":", StringComparison.Ordinal);
                if (payloadIdx < 0) return string.Empty;

                int colonIdx = receipt.IndexOf(':', payloadIdx);
                if (colonIdx < 0) return string.Empty;

                int quoteStart = receipt.IndexOf('"', colonIdx + 1);
                if (quoteStart < 0) return string.Empty;

                int quoteEnd = receipt.IndexOf('"', quoteStart + 1);
                if (quoteEnd < 0) return string.Empty;

                string payload = receipt.Substring(quoteStart + 1, quoteEnd - quoteStart - 1)
                    .Replace("\\\"", "\"").Replace("\\\\", "\\");

                int tokenIdx = payload.IndexOf("\"purchaseToken\":", StringComparison.Ordinal);
                if (tokenIdx < 0) return string.Empty;

                int tColonIdx = payload.IndexOf(':', tokenIdx);
                if (tColonIdx < 0) return string.Empty;

                int tQuoteStart = payload.IndexOf('"', tColonIdx + 1);
                if (tQuoteStart < 0) return string.Empty;

                int tQuoteEnd = payload.IndexOf('"', tQuoteStart + 1);
                if (tQuoteEnd < 0) return string.Empty;

                return payload.Substring(tQuoteStart + 1, tQuoteEnd - tQuoteStart - 1);
            }
            catch { return string.Empty; }
        }
    }
}
