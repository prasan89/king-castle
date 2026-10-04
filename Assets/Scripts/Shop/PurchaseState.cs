using System;

namespace KingSmash.Shop
{
    public enum PurchaseState
    {
        Idle,
        Loading,
        Purchasing,
        Pending,
        VerifyingServer,
        Success,
        Failed,
        Cancelled,
        Unavailable,
        Available,
        Owned,
    }

    [Serializable]
    public class PurchaseRecord
    {
        public string purchaseToken;
        public string productId;
        public string orderId;
        public string transactionId;
        public long timestampUtc;
        public bool verified;
        public bool rewarded;
    }
}
