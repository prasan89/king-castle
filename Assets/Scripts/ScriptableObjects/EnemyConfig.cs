using UnityEngine;

namespace KingSmash.Enemies
{
    public enum EnemyType { Grunt, Guard, Knight, Wizard, BossKnight }

    [CreateAssetMenu(fileName = "EnemyConfig", menuName = "KingSmash/Config/EnemyConfig")]
    public class EnemyConfig : ScriptableObject
    {
        public EnemyType enemyType;
        public string displayName;
        public int maxHealth = 100;
        public float armor = 1f;
        public int coinsOnDeath = 10;
        public int scoreOnDeath = 100;
        public float knockbackResistance = 0f;
        [TextArea] public string description;
    }
}
