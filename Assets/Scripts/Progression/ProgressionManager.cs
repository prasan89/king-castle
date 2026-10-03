using System;
using KingSmash.Core;
using KingSmash.Services;
using KingSmash.Save;
using KingSmash.Economy;

namespace KingSmash.Progression
{
    public class ProgressionManager
    {
        private readonly ISaveService _save;
        private readonly EconomyConfig _economyConfig;

        public static event Action<int, int> OnLevelCompleted;
        public static event Action<long> OnCoinsChanged;
        public static event Action<int> OnGemsChanged;
        public static event Action<int> OnKingLeveledUp;

        public ProgressionManager(ISaveService save, EconomyConfig economyConfig)
        {
            _save = save;
            _economyConfig = economyConfig;
        }

        public void HandleLevelComplete(int levelIndex, int starsEarned)
        {
            var reward = _economyConfig.CalculateLevelReward(starsEarned);
            _save.AddCoins(reward);
            _save.RecordLevelComplete(levelIndex, starsEarned);

            OnLevelCompleted?.Invoke(levelIndex, starsEarned);
            OnCoinsChanged?.Invoke(_save.Current.coins);

            GameLogger.Info("ProgressionManager", $"Level {levelIndex} complete: {starsEarned} stars, +{reward} coins");
        }

        public bool TryUpgradeKing(UpgradeTier tier)
        {
            if (!_save.TrySpendCoins(tier.coinCost)) return false;
            var data = _save.Current;
            data.kingLevel++;
            _save.Save();
            OnKingLeveledUp?.Invoke(data.kingLevel);
            ServiceLocator.Get<IAnalyticsService>().LogEvent(AnalyticsEvents.UpgradePurchase,
                ("king_level", data.kingLevel), ("coin_cost", tier.coinCost));
            return true;
        }

        public int GetTotalPlayerStars() => _save.Current.GetTotalStars();
    }
}
