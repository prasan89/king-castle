using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using KingSmash.Core;

namespace KingSmash.Shop
{
    public enum StorePurchaseOutcome { Success, UserCancelled, Failed }

    public sealed class StoreServiceMock : IStoreService
    {
        private readonly Dictionary<string, string> _prices = new()
        {
            { "coins_1000",     "₹99"  },
            { "coins_5000",     "₹299" },
            { "coins_15000",    "₹499" },
            { "coins_50000",    "₹999" },
            { "gems_small",     "₹99"  },
            { "gems_medium",    "₹299" },
            { "gems_large",     "₹499" },
            { "gems_mega",      "₹999" },
            { "remove_ads",     "₹299" },
            { "starter_bundle", "₹449" },
            { "king_bundle",    "₹749" },
        };

        private readonly HashSet<string> _ownedProducts = new();
        private StorePurchaseOutcome _nextOutcome = StorePurchaseOutcome.Success;

        public bool IsInitialized { get; private set; }

        public Task InitializeAsync()
        {
            IsInitialized = true;
            return Task.CompletedTask;
        }

        public Task<StoreInitResult> InitializeStoreAsync(List<string> consumableIds, List<string> nonConsumableIds)
        {
            IsInitialized = true;
            return Task.FromResult(new StoreInitResult { Success = true });
        }

        public void SimulateNextPurchaseOutcome(StorePurchaseOutcome outcome)
            => _nextOutcome = outcome;

        public void SetLocalizedPrice(string productId, string price)
            => _prices[productId] = price;

        public string GetLocalizedPrice(string productId)
            => _prices.TryGetValue(productId, out var price) ? price : "₹99";

        public bool IsOwned(string productId) => _ownedProducts.Contains(productId);

        public async Task<StorePurchaseResult> PurchaseAsync(string productId)
        {
            await Task.Delay(1);

            var outcome = _nextOutcome;
            _nextOutcome = StorePurchaseOutcome.Success;

            if (outcome == StorePurchaseOutcome.UserCancelled)
            {
                return new StorePurchaseResult
                {
                    Success = false, ProductId = productId,
                    Error = "UserCancelled", UserCancelled = true
                };
            }

            if (outcome == StorePurchaseOutcome.Failed)
            {
                return new StorePurchaseResult
                {
                    Success = false, ProductId = productId,
                    Error = "StoreFailed"
                };
            }

            _ownedProducts.Add(productId);
            var txId = Guid.NewGuid().ToString();
            var receipt = $"{{\"Store\":\"GooglePlay\",\"TransactionID\":\"mock-txid-{txId}\","
                         + $"\"Payload\":\"{{\\\"purchaseToken\\\":\\\"mock-token-{txId}\\\"}}\"}}";
            return new StorePurchaseResult
            {
                Success = true, ProductId = productId,
                Receipt = receipt, TransactionId = $"mock-txid-{txId}"
            };
        }

        public Task<StoreRestoreResult> RestoreAsync()
        {
            var restored = new List<string>(_ownedProducts);
            return Task.FromResult(new StoreRestoreResult { Success = true, RestoredProductIds = restored });
        }
    }
}
