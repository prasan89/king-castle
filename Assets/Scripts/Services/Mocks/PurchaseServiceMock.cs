using System.Collections.Generic;
using System.Threading.Tasks;
using KingSmash.Core;

namespace KingSmash.Services
{
    public class PurchaseServiceMock : IPurchaseService
    {
        private readonly HashSet<string> _ownedProducts = new();

        public Task InitializeAsync()
        {
            GameLogger.Info("PurchaseServiceMock", "Purchase service initialized.");
            return Task.CompletedTask;
        }

        public Task<PurchaseResult> PurchaseAsync(string productId)
        {
            GameLogger.Info("PurchaseServiceMock", $"Mock purchase: {productId}");
            _ownedProducts.Add(productId);
            return Task.FromResult(new PurchaseResult
            {
                Success = true,
                ProductId = productId,
                TransactionId = System.Guid.NewGuid().ToString()
            });
        }

        public Task<bool> RestorePurchasesAsync()
        {
            GameLogger.Info("PurchaseServiceMock", "Restore purchases (mock).");
            return Task.FromResult(true);
        }

        public bool IsProductOwned(string productId) => _ownedProducts.Contains(productId);
    }
}
