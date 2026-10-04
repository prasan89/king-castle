using System.Threading.Tasks;

namespace KingSmash.Shop
{
    public interface IReceiptVerificationService
    {
        Task<VerificationResult> VerifyAsync(VerificationRequest request);
    }

    public class VerificationRequest
    {
        public string PlayerId;
        public string ProductId;
        public string PurchaseToken;
        public string PackageName;
        public string IdToken;
    }

    public class VerificationResult
    {
        public bool IsValid;
        public string TransactionId;
        public string Error;
        public long CoinsGranted;
        public int GemsGranted;
        public System.Collections.Generic.List<PowerUpReward> PowerUpsGranted;
        public bool IsAlreadyProcessed;
    }
}
