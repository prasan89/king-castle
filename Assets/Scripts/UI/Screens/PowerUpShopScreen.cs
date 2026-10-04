using UnityEngine;
using KingSmash.Core;
using KingSmash.PowerUps;

namespace KingSmash.UI.Screens
{
    public class PowerUpShopScreen : UIScreen
    {
        private PowerUpService _service;

        protected override void OnShow()
        {
            ServiceLocator.TryGet<PowerUpService>(out _service);
            if (_service == null)
                GameLogger.Warning("PowerUpShopScreen", "PowerUpService not registered.");
        }
    }
}
