using System;
using KingSmash.Core;
using KingSmash.Economy;
using KingSmash.Services;
using KingSmash.Characters;

namespace KingSmash.PowerUps
{
    public class PowerUpService
    {
        public static event Action<PowerUpType, int> OnInventoryChanged;
        public static event Action<PowerUpType>      OnPowerUpActivated;
        public static event Action<PowerUpType>      OnPowerUpPurchased;
        public static event Action<PowerUpType, int> OnPowerUpAwarded;

        private readonly ISaveService    _save;
        private readonly CurrencyService _currency;
        private PowerUpType?             _selected;

        public PowerUpType? Selected => _selected;

        public PowerUpService(ISaveService save, CurrencyService currency)
        {
            _save     = save     ?? throw new ArgumentNullException(nameof(save));
            _currency = currency ?? throw new ArgumentNullException(nameof(currency));
            EnsureInventoryExists();
        }

        public int  GetCount(PowerUpType type) { EnsureInventoryExists(); return _save.Current.powerUpInventory.GetCount(type); }
        public bool Has(PowerUpType type)      => GetCount(type) > 0;
        public void Select(PowerUpType type)   => _selected = type;
        public void ClearSelection()           => _selected = null;

        public bool TryPurchase(PowerUpType type, PowerUpConfig config)
        {
            if (config == null) return false;
            if (!_currency.TrySpend(config.purchaseCost, $"powerup_purchase_{type}")) return false;

            EnsureInventoryExists();
            _save.Current.powerUpInventory.Add(type, 1);
            _save.Save();

            int newCount = GetCount(type);
            OnInventoryChanged?.Invoke(type, newCount);
            OnPowerUpPurchased?.Invoke(type);
            GameLogger.Info("PowerUpService", $"Purchased {type}, count now {newCount}");
            return true;
        }

        public void Award(PowerUpType type, int amount)
        {
            if (amount <= 0) return;
            EnsureInventoryExists();
            _save.Current.powerUpInventory.Add(type, amount);
            _save.Save();

            int newCount = GetCount(type);
            OnInventoryChanged?.Invoke(type, newCount);
            OnPowerUpAwarded?.Invoke(type, amount);
        }

        public bool TryActivateSelected(KingProjectile king, PowerUpController controller)
        {
            if (_selected == null) return false;
            var type = _selected.Value;

            EnsureInventoryExists();
            if (!_save.Current.powerUpInventory.TryConsume(type))
            {
                GameLogger.Warning("PowerUpService", $"Cannot activate {type} — none in inventory.");
                return false;
            }

            _save.Save();
            _selected = null;

            int remaining = GetCount(type);
            OnInventoryChanged?.Invoke(type, remaining);
            OnPowerUpActivated?.Invoke(type);

            controller.ApplyEffect(type, king);
            return true;
        }

        private void EnsureInventoryExists()
        {
            if (_save.Current.powerUpInventory == null)
                _save.Current.powerUpInventory = new PowerUpInventory();
        }
    }
}
