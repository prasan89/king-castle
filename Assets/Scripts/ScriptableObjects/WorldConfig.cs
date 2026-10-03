using System.Collections.Generic;
using UnityEngine;
using KingSmash.Levels;

namespace KingSmash.Levels
{
    [CreateAssetMenu(fileName = "WorldConfig", menuName = "KingSmash/Config/WorldConfig")]
    public class WorldConfig : ScriptableObject
    {
        public int worldIndex;
        public string worldName;
        [TextArea(1, 3)] public string worldDescription;
        public Color themeColor = Color.white;
        public List<LevelConfig> levels = new();
        public int requiredStarsToUnlock = 0;
        public Sprite worldIcon;

        public int TotalLevels => levels.Count;
        public bool IsUnlocked(int totalPlayerStars) => totalPlayerStars >= requiredStarsToUnlock;
    }
}
