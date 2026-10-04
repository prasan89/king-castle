using System;
using System.Collections.Generic;

namespace KingSmash.Shop
{
    [Serializable]
    public class PurchaseStateEntry
    {
        public string productId;
        public PurchaseState state;
    }

    public class PurchaseStateTracker
    {
        private readonly List<PurchaseStateEntry> _entries = new();

        public static event Action<string, PurchaseState> OnStateChanged;

        public PurchaseState GetState(string productId)
        {
            foreach (var entry in _entries)
                if (entry.productId == productId) return entry.state;
            return PurchaseState.Idle;
        }

        public void SetState(string productId, PurchaseState state)
        {
            foreach (var entry in _entries)
            {
                if (entry.productId == productId)
                {
                    entry.state = state;
                    OnStateChanged?.Invoke(productId, state);
                    return;
                }
            }
            _entries.Add(new PurchaseStateEntry { productId = productId, state = state });
            OnStateChanged?.Invoke(productId, state);
        }
    }
}
