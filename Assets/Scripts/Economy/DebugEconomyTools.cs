#if UNITY_EDITOR
using KingSmash.Core;
using KingSmash.PowerUps;
using KingSmash.Services;

namespace KingSmash.Economy
{
    public static class DebugEconomyTools
    {
        [UnityEditor.MenuItem("KingSmash/Debug/Economy/Add Coins (1000)")]
        public static void AddCoins()
        {
            AddCoins(1000);
        }

        public static void AddCoins(long amount = 1000)
        {
            if (!ServiceLocator.TryGet<CurrencyService>(out var currency))
            {
                GameLogger.Warning("DebugEconomyTools", "CurrencyService not registered.");
                return;
            }
            currency.TryAdd(amount, "DEBUG", out _);
            GameLogger.Info("DebugEconomyTools", $"Added {amount} coins.");
        }

        [UnityEditor.MenuItem("KingSmash/Debug/Economy/Add Big Coins (10000)")]
        public static void AddBigCoins()
        {
            AddBigCoins(10000);
        }

        public static void AddBigCoins(long amount = 10000)
        {
            if (!ServiceLocator.TryGet<CurrencyService>(out var currency))
            {
                GameLogger.Warning("DebugEconomyTools", "CurrencyService not registered.");
                return;
            }
            currency.TryAdd(amount, "DEBUG", out _);
            GameLogger.Info("DebugEconomyTools", $"Added {amount} coins (big).");
        }

        [UnityEditor.MenuItem("KingSmash/Debug/Economy/Add Gems (10)")]
        public static void AddGems()
        {
            AddGems(10);
        }

        public static void AddGems(int amount = 10)
        {
            if (!ServiceLocator.TryGet<ISaveService>(out var save))
            {
                GameLogger.Warning("DebugEconomyTools", "ISaveService not registered.");
                return;
            }
            save.AddGems(amount);
            save.Save();
            GameLogger.Info("DebugEconomyTools", $"Added {amount} gems.");
        }

        [UnityEditor.MenuItem("KingSmash/Debug/Economy/Clear Coins")]
        public static void ClearCoins()
        {
            if (!ServiceLocator.TryGet<ISaveService>(out var save))
            {
                GameLogger.Warning("DebugEconomyTools", "ISaveService not registered.");
                return;
            }
            save.Current.coins = 0;
            save.Save();
            GameLogger.Info("DebugEconomyTools", "Cleared coins.");
        }

        [UnityEditor.MenuItem("KingSmash/Debug/Economy/Clear Gems")]
        public static void ClearGems()
        {
            if (!ServiceLocator.TryGet<ISaveService>(out var save))
            {
                GameLogger.Warning("DebugEconomyTools", "ISaveService not registered.");
                return;
            }
            save.Current.gems = 0;
            save.Save();
            GameLogger.Info("DebugEconomyTools", "Cleared gems.");
        }

        [UnityEditor.MenuItem("KingSmash/Debug/Economy/Reset Economy")]
        public static void ResetEconomy()
        {
            if (!ServiceLocator.TryGet<ISaveService>(out var save))
            {
                GameLogger.Warning("DebugEconomyTools", "ISaveService not registered.");
                return;
            }
            ClearCoins();
            ClearGems();
            save.Current.processedLevelRewards.Clear();
            save.Save();
            GameLogger.Info("DebugEconomyTools", "Economy reset complete.");
        }

        [UnityEditor.MenuItem("KingSmash/Debug/Economy/Grant Test Reward")]
        public static void GrantTestReward()
        {
            if (!ServiceLocator.TryGet<CurrencyService>(out var currency))
            {
                GameLogger.Warning("DebugEconomyTools", "CurrencyService not registered.");
                return;
            }
            currency.TryAdd(500, "DEBUG_TEST_REWARD", out _);
            GameLogger.Info("DebugEconomyTools", "Granted test reward: 500 coins.");

            if (ServiceLocator.TryGet<PowerUpService>(out var powerUpService))
            {
                powerUpService.Award(PowerUpType.Bomb, 1);
                GameLogger.Info("DebugEconomyTools", "Granted test reward: 1 Bomb powerup.");
            }
        }
    }
}
#endif
