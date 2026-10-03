using UnityEngine;
namespace KingSmash.UI
{
    [RequireComponent(typeof(RectTransform))]
    public class SafeAreaHandler : MonoBehaviour
    {
        private RectTransform _rt;
        private Rect _lastSafeArea = Rect.zero;

        private void Awake() => _rt = GetComponent<RectTransform>();

        private void Start() => ApplySafeArea();

        private void Update()
        {
            // Reapply on orientation change (screen safe area may change)
            if (Screen.safeArea != _lastSafeArea) ApplySafeArea();
        }

        private void ApplySafeArea()
        {
            var safe = Screen.safeArea;
            _lastSafeArea = safe;

            Vector2 anchorMin = safe.position;
            Vector2 anchorMax = safe.position + safe.size;
            anchorMin.x /= Screen.width;
            anchorMin.y /= Screen.height;
            anchorMax.x /= Screen.width;
            anchorMax.y /= Screen.height;

            _rt.anchorMin = anchorMin;
            _rt.anchorMax = anchorMax;
        }
    }
}
