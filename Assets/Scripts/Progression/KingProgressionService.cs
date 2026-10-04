using System;
using KingSmash.Save;
using KingSmash.Services;

namespace KingSmash.Progression
{
    public class KingProgressionService
    {
        public static event Action<int, int, KingStats> OnKingLevelUp;
        public static event Action<long, long>          OnXPChanged;

        private readonly ISaveService      _save;
        private readonly KingUpgradeConfig _config;

        public KingProgressionService(ISaveService save, KingUpgradeConfig config)
        {
            _save   = save ?? throw new ArgumentNullException(nameof(save));
            _config = config;
            EnsureProgressionExists();
        }

        public void AddXP(long amount)
        {
            if (amount <= 0) return;
            EnsureProgressionExists();

            var prog     = _save.Current.kingProgression;
            prog.KingXP += amount;

            int  oldLevel  = prog.KingLevel;
            int  maxLevel  = _config != null ? _config.MaxKingLevel : KingProgression.MaxKingLevel;
            bool leveledUp = false;

            while (prog.KingLevel < maxLevel)
            {
                long required = XPRequiredForNextLevel(prog.KingLevel);
                if (prog.KingXP < required) break;
                prog.KingXP   -= required;
                prog.KingLevel++;
                leveledUp      = true;
            }

            if (prog.KingLevel >= maxLevel)
                prog.KingXP = 0;

            _save.Save();

            KingStats newStats = GetCurrentStats();
            if (leveledUp)
                OnKingLevelUp?.Invoke(oldLevel, prog.KingLevel, newStats);

            long xpRequired = XPRequiredForNextLevel(prog.KingLevel);
            OnXPChanged?.Invoke(prog.KingXP, xpRequired);
        }

        public long XPRequiredForNextLevel(int currentLevel)
        {
            if (_config == null) return long.MaxValue;
            var row = _config.GetKingLevelRow(currentLevel + 1);
            return row != null ? row.xpRequired : long.MaxValue;
        }

        public KingStats GetCurrentStats()
        {
            if (_config == null) return KingStats.Zero;
            EnsureProgressionExists();

            var  prog  = _save.Current.kingProgression;
            float power = 0f, speed = 0f, smash = 0f, armor = 0f;

            foreach (var row in _config.powerUpgrades)
                if (row != null && row.statLevel <= prog.PowerLevel)
                    power += row.valueIncrease;

            foreach (var row in _config.speedUpgrades)
                if (row != null && row.statLevel <= prog.SpeedLevel)
                    speed += row.valueIncrease;

            foreach (var row in _config.smashRadiusUpgrades)
                if (row != null && row.statLevel <= prog.SmashRadiusLevel)
                    smash += row.valueIncrease;

            foreach (var row in _config.armorUpgrades)
                if (row != null && row.statLevel <= prog.ArmorLevel)
                    armor += row.valueIncrease;

            return new KingStats(power, speed, smash, armor);
        }

        public int  KingLevel    => _save.Current.kingProgression?.KingLevel ?? 1;
        public long KingXP       => _save.Current.kingProgression?.KingXP    ?? 0;
        public int  MaxKingLevel => _config != null ? _config.MaxKingLevel : KingProgression.MaxKingLevel;

        private void EnsureProgressionExists()
        {
            if (_save.Current.kingProgression == null)
                _save.Current.kingProgression = new KingProgression();
        }
    }
}
