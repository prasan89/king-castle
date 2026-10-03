using UnityEngine;
namespace KingSmash.Characters
{
    [CreateAssetMenu(fileName = "QueenConfig", menuName = "KingSmash/Config/QueenConfig")]
    public class QueenConfig : ScriptableObject
    {
        [Header("Rescue")]
        public float rescueRadius = 3f;
        public int scoreOnRescue = 500;
        public int coinsOnRescue = 25;
        [Header("Visual")]
        public Color capturedColor = Color.red;
        public Color rescuedColor  = Color.yellow;
        public float rescueAnimDuration = 1.2f;
    }
}
