using System;
using System.Collections.Generic;
using KingSmash.Save;
using KingSmash.Services;

namespace KingSmash.Economy
{
    [Serializable]
    public class CurrencyTransaction
    {
        public string type;
        public long amount;
        public string reason;
        public long sessionTick;

        private static long _tickCounter;

        public CurrencyTransaction(string type, long amount, string reason)
        {
            this.type = type;
            this.amount = amount;
            this.reason = reason;
            this.sessionTick = ++_tickCounter;
        }
    }

    public interface ICurrencyService
    {
        long Balance { get; }
        bool TryAdd(long amount, string reason, out CurrencyTransaction tx);
        bool TrySpend(long amount, string reason, out CurrencyTransaction tx);
        CurrencyTransaction[] GetHistory();
    }

    public static class CurrencyBalance
    {
        private static ISaveService _saveService;

        public static void Initialize(ISaveService saveService)
        {
            _saveService = saveService;
        }

        public static long Balance
        {
            get => _saveService?.Current?.coins ?? 0L;
        }
    }

    public sealed class CurrencyService : ICurrencyService
    {
        public static event Action<long, CurrencyTransaction> OnCoinsChanged;
        public static event Action<long, long> OnInsufficientFunds;

        private const int MaxHistorySize = 50;

        private readonly ISaveService _saveService;
        private readonly List<CurrencyTransaction> _history = new List<CurrencyTransaction>();

        public CurrencyService(ISaveService saveService)
        {
            _saveService = saveService ?? throw new ArgumentNullException(nameof(saveService));
        }

        public long Balance => _saveService.Current.coins;

        public bool TryAdd(long amount, string reason, out CurrencyTransaction tx)
        {
            tx = null;

            if (amount <= 0)
                return false;

            _saveService.AddCoins(amount);
            _saveService.Save();

            tx = new CurrencyTransaction("add", amount, reason);
            RecordTransaction(tx);

            OnCoinsChanged?.Invoke(Balance, tx);
            return true;
        }

        public bool TrySpend(long amount, string reason, out CurrencyTransaction tx)
        {
            tx = null;

            if (amount <= 0)
                return false;

            long current = Balance;
            if (current < amount)
            {
                OnInsufficientFunds?.Invoke(amount, current);
                return false;
            }

            _saveService.SpendCoins(amount);
            _saveService.Save();

            tx = new CurrencyTransaction("spend", amount, reason);
            RecordTransaction(tx);

            OnCoinsChanged?.Invoke(Balance, tx);
            return true;
        }

        public CurrencyTransaction[] GetHistory()
        {
            return _history.ToArray();
        }

        // Convenience overloads used by upgrade services
        public bool TryAdd(long amount)
        {
            return TryAdd(amount, "unspecified", out _);
        }

        public bool TrySpend(long amount, string reason)
        {
            return TrySpend(amount, reason, out _);
        }

        public bool CanAfford(long amount) => Balance >= amount;

        private void RecordTransaction(CurrencyTransaction tx)
        {
            _history.Add(tx);
            if (_history.Count > MaxHistorySize)
                _history.RemoveAt(0);
        }
    }
}