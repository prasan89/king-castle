using UnityEngine;

namespace KingSmash.Core
{
    [CreateAssetMenu(fileName = "GameConfig", menuName = "KingSmash/Config/GameConfig")]
    public class GameConfig : ScriptableObject
    {
        [Header("Application")]
        public string gameVersion = "1.0.0";
        public int targetFrameRate = 60;
        public LogLevel logLevel = LogLevel.Info;

        [Header("World")]
        public int totalWorlds = 5;
        public int levelsPerWorld = 20;

        [Header("Physics")]
        public float globalGravityScale = 1.5f;
        public float destructionThreshold = 10f;

        [Header("Audio")]
        [Range(0f, 1f)] public float defaultMusicVolume = 0.6f;
        [Range(0f, 1f)] public float defaultSfxVolume = 0.8f;
    }
}
