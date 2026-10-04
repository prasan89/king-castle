using System;
using KingSmash.Core;
using KingSmash.Services;
using KingSmash.Economy;

namespace KingSmash.Progression
{
    public enum KingStat
    {
        Power,
        Speed,
        SmashRadius,
        Armor
    }

    public enum UpgradeResult
    {
        Success,
        InsufficientCoins,
        AlreadyMaxLevel,
        InvalidStat
    }

    public class KingUpgradeService
    {
        public static event Action<KingStat, int, int, float, float, long> OnStatUpgraded;

        private readonly ISaveService          _save;
        private readonly KingUpgradeConfig     _config;
        private readonly CurrencyService       _currency;
        private readonly KingProgressionService _progression;

        public KingUpgradeService(ISaveService save, KingUpgradeConfig config, CurrencyService currency)
        {
            _save     = save;
            _config   = config;
            _currency = currency;
        }

        // Lazy-resolve KingProgressionService since it is registered after KingUpgradeService
        private KingProgressionService ProgressionService
        {
            get
            {
                if (_progression != null) return _progression;
                ServiceLocator.TryGet<KingProgressionService>(out var svc);
                return svc;
            }
        }

        public UpgradeResult TryUpgradeStat(KingStat stat)
        {
            if (_save == null) return UpgradeResult.InvalidStat;
            if (_config == null) return UpgradeResult.InvalidStat;

            var prog = _save.Current.kingProgression;
            if (prog == null) return UpgradeResult.InvalidStat;

            int currentLevel = prog.GetStatLevel(stat);

            if (currentLevel >= _config.MaxStatLevel)
                return UpgradeResult.AlreadyMaxLevel;

            var row = _config.GetUpgradeRow(stat, currentLevel);
            if (row == null)
                return UpgradeResult.InvalidStat;

            float oldValue = GetCurrentStatValue(stat);

            if (_currency == null || !_currency.TrySpend(row.coinCost, $"upgrade_{stat}"))
                return UpgradeResult.InsufficientCoins;

            int oldLevel = currentLevel;
            int newLevel = currentLevel + 1;
            prog.SetStatLevel(stat, newLevel);
            _save.Save();

            float newValue = oldValue + row.valueIncrease;
            OnStatUpgraded?.Invoke(stat, oldLevel, newLevel, oldValue, newValue, row.coinCost);

            if (ServiceLocator.TryGet<IAnalyticsService>(out var analytics))
                analytics.LogEvent(AnalyticsEvents.KingStatUpgrade,
                    ("stat",      stat.ToString()),
                    ("old_level", (object)oldLevel),
                    ("new_level", (object)newLevel),
                    ("cost",      (object)row.coinCost));

            return UpgradeResult.Success;
        }

        public (bool canAfford, int currentLevel, int nextLevel, float currentValue, float nextValue, long cost, bool isMax)
            GetUpgradePreview(KingStat stat)
        {
            if (_config == null || _save?.Current?.kingProgression == null)
                return (false, 1, 1, 0f, 0f, 0L, false);

            var prog         = _save.Current.kingProgression;
            int currentLevel = prog.GetStatLevel(stat);
            bool isMax       = currentLevel >= _config.MaxStatLevel;
            float currentVal = GetCurrentStatValue(stat);

            if (isMax)
                return (false, currentLevel, currentLevel, currentVal, currentVal, 0L, true);

            var row = _config.GetUpgradeRow(stat, currentLevel);
            if (row == null)
                return (false, currentLevel, currentLevel, currentVal, currentVal, 0L, false);

            float nextValue = currentVal + row.valueIncrease;
            bool canAfford  = _currency != null && _currency.CanAfford(row.coinCost);

            return (canAfford, currentLevel, currentLevel + 1, currentVal, nextValue, row.coinCost, false);
        }

        private float GetCurrentStatValue(KingStat stat)
        {
            var svc = ProgressionService;
            if (svc == null) return 0f;
            var stats = svc.GetCurrentStats();
            switch (stat)
            {
                case KingStat.Power:       return stats.Power;
                case KingStat.Speed:       return stats.Speed;
                case KingStat.SmashRadius: return stats.SmashRadius;
                case KingStat.Armor:       return stats.Armor;
                default:                   return 0f;
            }
        }
    }
}
