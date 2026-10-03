using UnityEngine;
namespace KingSmash.Levels
{
    [CreateAssetMenu(fileName = "World1Config", menuName = "KingSmash/Config/World1Config")]
    public class World1Config : ScriptableObject
    {
        [Header("Identity")]
        public int   worldIndex     = 0;
        public string worldName     = "Forest Kingdom";
        public int   totalLevels    = 10;

        [Header("Environment")]
        public string backgroundKey    = "world1_background";
        public string musicKey         = "world1_music";
        public Color  ambientColor     = new Color(0.85f, 0.95f, 0.75f);

        [Header("Completion")]
        public int    totalStarsMax    = 30;  // 10 levels × 3 stars
        public string completionMessage = "World 1 Complete!\nThe Forest Kingdom is saved!";
    }
}
