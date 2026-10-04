using System.Collections.Generic;
using System.Threading.Tasks;

namespace KingSmash.Shop
{
    public interface IStoreService
    {
        bool IsInitialized { get; }
        Task InitializeAsync();
        Task<StoreInitResult> InitializeStoreAsync(List<string> consumableIds, List<string> nonConsumableIds);
        string GetLocalizedPrice(string productId);
        bool IsOwned(string productId);
        Task<StorePurchaseResult> PurchaseAsync(string productId);
        Task<StoreRestoreResult> RestoreAsync();
    }

    public class StoreInitResult
    {
        public bool Success;
        public string Error;
    }

    public class StorePurchaseResult
    {
        public bool Success;
        public string ProductId;
        public string Receipt;
        public string TransactionId;
        public string Error;
        public bool UserCancelled;
    }

    public class StoreRestoreResult
    {
        public bool Success;
        public List<string> RestoredProductIds;
        public string Error;
    }
}
