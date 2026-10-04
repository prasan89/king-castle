using UnityEngine;

namespace KingSmash.Camera
{
    /// <summary>
    /// ScriptableObject profile that describes a camera shake preset.
    /// Create instances via the editor (e.g. Subtle, Small, Medium, Large, Boss, PowerUp, QueenRescue).
    /// </summary>
    [CreateAssetMenu(fileName = "CameraShakeProfile", menuName = "KingSmash/Camera/CameraShakeProfile")]
    public class CameraShakeProfile : ScriptableObject
    {
        [Tooltip("Total shake duration in seconds.")]
        public float duration = 0.2f;

        [Tooltip("Peak magnitude (world units) of the displacement.")]
        public float magnitude = 0.15f;

        [Tooltip("Maps normalised time (0–1) to a 0–1 multiplier applied to magnitude.")]
        public AnimationCurve envelope = AnimationCurve.EaseInOut(0f, 1f, 1f, 0f);
    }
}
