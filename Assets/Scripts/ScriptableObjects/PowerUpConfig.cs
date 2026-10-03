using UnityEngine;

namespace KingSmash.Economy
{
    public enum PowerUpType { ExtraLaunch, ExplosiveKing, IronKing, CoinMagnet, TimeFreeze, DoubleCoins }

    [CreateAssetMenu(fileName = "PowerUpConfig", menuName = "KingSmash/Config/PowerUpConfig")]
    public class PowerUpConfig : ScriptableObject
    {
        public PowerUpType powerUpType;
        public string displayName;
        [TextArea] public string description;
        public long coinCost;
        public int gemCost;
        public float duration;
        public bool consumeOnUse = true;
        public Sprite icon;
    }
}
