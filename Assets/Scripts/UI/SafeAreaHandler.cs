using UnityEngine;

namespace KingSmash.UI
{
    [RequireComponent(typeof(RectTransform))]
    public class SafeAreaHandler : MonoBehaviour
    {
        [Header("Safe Area Axes")]
        [SerializeField] private bool _applyTop    = true;
        [SerializeField] private bool _applyBottom = true;
        [SerializeField] private bool _applyLeft   = true;
        [SerializeField] private bool _applyRight  = true;

        private RectTransform _rt;
        private Rect _lastSafeArea  = Rect.zero;
        private Vector2Int _lastScreenSize;
        private ScreenOrientation _lastOrientation;

        private void Awake()
        {
            _rt = GetComponent<RectTransform>();
            _lastSafeArea    = Rect.zero;
            _lastScreenSize  = Vector2Int.zero;
            _lastOrientation = Screen.orientation;
        }

        private void Start() => ApplySafeArea();

        private void Update()
        {
            // Reapply on any screen change: safe area rect, resolution, or orientation
            var currentSize   = new Vector2Int(Screen.width, Screen.height);
            var currentOrient = Screen.orientation;

            if (Screen.safeArea != _lastSafeArea
                || currentSize   != _lastScreenSize
                || currentOrient != _lastOrientation)
            {
                ApplySafeArea();
            }
        }

        /// <summary>
        /// Applies the safe area insets to the supplied RectTransform.
        /// Call this manually to apply safe area to a specific panel at runtime.
        /// </summary>
        public void ApplySafeArea(RectTransform panel)
        {
            if (panel == null) return;

            Rect safe = Screen.safeArea;
            int  sw   = Screen.width;
            int  sh   = Screen.height;

            if (sw <= 0 || sh <= 0) return;

            // Normalise safe-area rect to 0–1 anchor space
            Vector2 anchorMin = new Vector2(safe.x / sw, safe.y / sh);
            Vector2 anchorMax = new Vector2((safe.x + safe.width) / sw, (safe.y + safe.height) / sh);

            // Clamp so we never push anchors outside [0,1]
            anchorMin.x = Mathf.Clamp01(anchorMin.x);
            anchorMin.y = Mathf.Clamp01(anchorMin.y);
            anchorMax.x = Mathf.Clamp01(anchorMax.x);
            anchorMax.y = Mathf.Clamp01(anchorMax.y);

            // Selectively block insets per axis
            if (!_applyLeft)   anchorMin.x = 0f;
            if (!_applyBottom) anchorMin.y = 0f;
            if (!_applyRight)  anchorMax.x = 1f;
            if (!_applyTop)    anchorMax.y = 1f;

            panel.anchorMin = anchorMin;
            panel.anchorMax = anchorMax;

            // Reset offsets so there is no additional padding beyond what the anchors encode
            panel.offsetMin = Vector2.zero;
            panel.offsetMax = Vector2.zero;
        }

        // ── Private convenience overload used by this component ────────────────

        private void ApplySafeArea()
        {
            if (_rt == null) return;

            _lastSafeArea    = Screen.safeArea;
            _lastScreenSize  = new Vector2Int(Screen.width, Screen.height);
            _lastOrientation = Screen.orientation;

            ApplySafeArea(_rt);
        }
    }
}
