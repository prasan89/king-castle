using System.Collections.Generic;
using UnityEngine;
namespace KingSmash.UI
{
    // Simple data bag for world map display — populated from LevelConfig assets at runtime.
    [System.Serializable]
    public class WorldData
    {
        public int worldIndex;
        public string worldName;
        public int levelsInWorld;
        public int starsEarned;
        public int starsMax;
        public bool isUnlocked;
        public List<LevelNodeData> levels = new();
    }

    [System.Serializable]
    public class LevelNodeData
    {
        public int levelIndex;
        public int starsEarned;   // 0-3
        public bool isUnlocked;
        public bool isCurrent;
    }
}
