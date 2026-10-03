using System.Threading.Tasks;

namespace KingSmash.Services
{
    public class PurchaseResult
    {
        public bool Success { get; set; }
        public string ProductId { get; set; }
        public string TransactionId { get; set; }
        public string ErrorMessage { get; set; }
    }

    public interface IPurchaseService
    {
        Task InitializeAsync();
        Task<PurchaseResult> PurchaseAsync(string productId);
        Task<bool> RestorePurchasesAsync();
        bool IsProductOwned(string productId);
    }
}
