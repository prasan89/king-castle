using System;

namespace KingSmash.UI
{
    /// <summary>
    /// Semantic UI event bridge -- keeps gameplay and economy systems decoupled from UI.
    /// Fire methods from any system; subscribe in UI components to react.
    /// </summary>
    public static class UIEventBridge
    {
        // -- Events -----------------------------------------------------------

        /// <summary>Fired whenever the player's coin balance changes.</summary>
        public static event Action<long> OnCoinsChanged;

        /// <summary>Fired whenever the player's gem balance changes.</summary>
        public static event Action<int> OnGemsChanged;

        /// <summary>Fired when the King levels up. Args: (previousLevel, newLevel).</summary>
        public static event Action<int, int> OnKingLevelChanged;

        /// <summary>Fired when stars are awarded at end of level.</summary>
        public static event Action<int> OnStarsAwarded;

        /// <summary>Fired when an achievement is unlocked. Arg: achievementId.</summary>
        public static event Action<string> OnAchievementUnlocked;

        /// <summary>Fired when a mission is completed. Arg: missionId.</summary>
        public static event Action<string> OnMissionCompleted;

        // -- Fire helpers -----------------------------------------------------

        public static void FireCoinsChanged(long amount)
        {
            OnCoinsChanged?.Invoke(amount);
            ToastService.ShowCoin(amount);
        }

        public static void FireGemsChanged(int amount)
        {
            OnGemsChanged?.Invoke(amount);
            ToastService.ShowGem(amount);
        }

        public static void FireKingLevelChanged(int prev, int next)
        {
            OnKingLevelChanged?.Invoke(prev, next);
        }

        public static void FireStarsAwarded(int count)
        {
            OnStarsAwarded?.Invoke(count);
        }

        public static void FireAchievementUnlocked(string achievementId)
        {
            OnAchievementUnlocked?.Invoke(achievementId);
            if (!string.IsNullOrEmpty(achievementId))
                ToastService.ShowSuccess("Achievement Unlocked!");
        }

        public static void FireMissionCompleted(string missionId)
        {
            OnMissionCompleted?.Invoke(missionId);
            if (!string.IsNullOrEmpty(missionId))
                ToastService.ShowSuccess("Mission Complete!");
        }
    }
}
