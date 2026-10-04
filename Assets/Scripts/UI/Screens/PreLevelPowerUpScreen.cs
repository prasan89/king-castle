using UnityEngine;
using KingSmash.Core;
using KingSmash.PowerUps;

namespace KingSmash.UI.Screens
{
    public class PreLevelPowerUpScreen : UIScreen
    {
        protected override void OnShow()
        {
            GameLogger.Info("PreLevelPowerUpScreen", "Showing power-up selection.");
        }
    }
}
