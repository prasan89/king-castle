using UnityEngine;
using KingSmash.Core;
using KingSmash.PowerUps;

namespace KingSmash.UI
{
    public class PowerUpHUD : MonoBehaviour
    {
        private PowerUpService _service;

        private void OnEnable()
        {
            PowerUpService.OnPowerUpActivated += HandleActivated;
            PowerUpService.OnInventoryChanged += HandleInventoryChanged;
        }

        private void OnDisable()
        {
            PowerUpService.OnPowerUpActivated -= HandleActivated;
            PowerUpService.OnInventoryChanged -= HandleInventoryChanged;
        }

        private void Start()
        {
            ServiceLocator.TryGet<PowerUpService>(out _service);
        }

        private void HandleActivated(PowerUpType type)
        {
            GameLogger.Debug("PowerUpHUD", $"Activated: {type}");
        }

        private void HandleInventoryChanged(PowerUpType type, int newCount)
        {
            GameLogger.Debug("PowerUpHUD", $"Inventory changed: {type} → {newCount}");
        }
    }
}
