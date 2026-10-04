#if UNITY_PURCHASING
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using KingSmash.Core;
using UnityEngine.Purchasing;
using UnityEngine.Purchasing.Extension;

namespace KingSmash.Shop
{
    public sealed class UnityIAPStoreService : IStoreService
    {
        private IStoreController _controller;
        private TaskCompletionSource<StoreInitResult> _initTcs;
        private TaskCompletionSource<StorePurchaseResult> _purchaseTcs;
        private readonly ShopProductCatalog _catalog;

        public bool IsInitialized => _controller != null;

        public UnityIAPStoreService(ShopProductCatalog catalog)
        {
            _catalog = catalog ?? throw new ArgumentNullException(nameof(catalog));
        }

        public Task InitializeAsync()
            => InitializeStoreAsync(_catalog.GetConsumableIds(), _catalog.GetNonConsumableIds());

        public Task<StoreInitResult> InitializeStoreAsync(List<string> consumableIds, List<string> nonConsumableIds)
        {
            if (IsInitialized)
                return Task.FromResult(new StoreInitResult { Success = true });

            _initTcs = new TaskCompletionSource<StoreInitResult>();

            var builder = ConfigurationBuilder.Instance(StandardPurchasingModule.Instance());
            foreach (var id in consumableIds)
                builder.AddProduct(id, ProductType.Consumable);
            foreach (var id in nonConsumableIds)
                builder.AddProduct(id, ProductType.NonConsumable);

            UnityPurchasing.Initialize(this, builder);
            return _initTcs.Task;
        }

        public string GetLocalizedPrice(string productId)
            => _controller?.products.WithID(productId)?.metadata?.localizedPriceString ?? "";

        public bool IsOwned(string productId)
            => _controller?.products.WithID(productId)?.hasReceipt ?? false;

        public Task<StorePurchaseResult> PurchaseAsync(string productId)
        {
            if (_purchaseTcs != null && !_purchaseTcs.Task.IsCompleted)
                return Task.FromResult(new StorePurchaseResult
                    { Success = false, ProductId = productId, Error = "A purchase is already in progress." });

            _purchaseTcs = new TaskCompletionSource<StorePurchaseResult>();
            _controller.InitiatePurchase(productId);
            return _purchaseTcs.Task;
        }

        public Task<StoreRestoreResult> RestoreAsync()
        {
            var restored = new List<string>();
            if (_controller != null)
                foreach (var product in _controller.products.all)
                    if (product.hasReceipt)
                        restored.Add(product.definition.id);
            return Task.FromResult(new StoreRestoreResult { Success = true, RestoredProductIds = restored });
        }

        public void AcknowledgePurchase(string productId)
        {
            var product = _controller?.products.WithID(productId);
            if (product != null)
                _controller.ConfirmPendingPurchase(product);
        }

        void IStoreListener.OnInitialized(IStoreController controller, IExtensionProvider extensions)
        {
            _controller = controller;
            GameLogger.Info("UnityIAPStoreService", "Store initialized successfully.");
            _initTcs?.TrySetResult(new StoreInitResult { Success = true });
        }

        void IStoreListener.OnInitializeFailed(InitializationFailureReason error)
        {
            GameLogger.Error("UnityIAPStoreService", $"Store init failed: {error}");
            _initTcs?.TrySetResult(new StoreInitResult { Success = false, Error = error.ToString() });
        }

        void IStoreListener.OnInitializeFailed(InitializationFailureReason error, string message)
        {
            GameLogger.Error("UnityIAPStoreService", $"Store init failed: {error} — {message}");
            _initTcs?.TrySetResult(new StoreInitResult { Success = false, Error = message ?? error.ToString() });
        }

        PurchaseProcessingResult IStoreListener.ProcessPurchase(PurchaseEventArgs args)
        {
            var result = new StorePurchaseResult
            {
                Success       = true,
                ProductId     = args.purchasedProduct.definition.id,
                Receipt       = args.purchasedProduct.receipt,
                TransactionId = args.purchasedProduct.transactionID,
            };
            _purchaseTcs?.TrySetResult(result);
            return PurchaseProcessingResult.Pending;
        }

        void IStoreListener.OnPurchaseFailed(Product product, PurchaseFailureReason failureReason)
        {
            bool cancelled = failureReason == PurchaseFailureReason.UserCancelled;
            GameLogger.Warning("UnityIAPStoreService", $"Purchase failed: {product.definition.id} — {failureReason}");
            _purchaseTcs?.TrySetResult(new StorePurchaseResult
            {
                Success       = false,
                ProductId     = product.definition.id,
                Error         = failureReason.ToString(),
                UserCancelled = cancelled,
            });
        }
    }
}
#endif
