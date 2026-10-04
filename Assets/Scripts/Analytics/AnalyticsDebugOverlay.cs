// AnalyticsDebugOverlay.cs — M14 Analytics
// DEV/EDITOR-only runtime overlay that displays the last 20 tracked analytics
// events in a semi-transparent GUI box in the top-right corner of the screen.
// Never compiled or visible in production builds.

using UnityEngine;
using System.Collections.Generic;

namespace KingSmash.Analytics
{
#if UNITY_EDITOR || KING_SMASH_DEV

    /// <summary>
    /// Displays a scrollable list of recent analytics events in the top-right
    /// corner of the screen. Only compiled and visible in UNITY_EDITOR or
    /// KING_SMASH_DEV builds — stripped entirely from production.
    ///
    /// Usage:
    ///   Attach to any scene GameObject. FirebaseAnalyticsService and
    ///   AnalyticsServiceMock fire the static OnEventTracked event; this overlay
    ///   subscribes and renders the result.
    /// </summary>
    public sealed class AnalyticsDebugOverlay : MonoBehaviour
    {
        // ── Configuration ─────────────────────────────────────────────────────
        private const int   MaxEvents       = 20;
        private const int   OverlayWidth    = 300;
        private const int   OverlayHeight   = 400;
        private const int   PaddingRight    = 10;
        private const int   PaddingTop      = 10;
        private const float BackgroundAlpha = 0.75f;

        [SerializeField] private bool _enabled = true;

        // ── State ─────────────────────────────────────────────────────────────
        private readonly Queue<string> _recentEvents = new Queue<string>();
        private Vector2                _scrollPosition;
        private GUIStyle               _boxStyle;
        private GUIStyle               _labelStyle;
        private GUIStyle               _buttonStyle;
        private bool                   _stylesInitialized;

        // ── Static event — fired by analytics service implementations ─────────
        /// <summary>
        /// Analytics service implementations (FirebaseAnalyticsService,
        /// AnalyticsServiceMock) fire this event each time an event is tracked.
        /// Subscribers must not capture Unity objects in long-lived closures.
        /// </summary>
        public static event System.Action<string, string> OnEventTracked;

        /// <summary>
        /// Fires the <see cref="OnEventTracked"/> event.
        /// Called by analytics service implementations to notify any active overlay.
        /// </summary>
        /// <param name="eventName">The analytics event name (snake_case).</param>
        /// <param name="parameters">Human-readable parameter string ("key=value, ...").</param>
        public static void NotifyEventTracked(string eventName, string parameters)
            => OnEventTracked?.Invoke(eventName, parameters);

        // ── Unity lifecycle ───────────────────────────────────────────────────

        private void OnEnable()
        {
            OnEventTracked += HandleEventTracked;
        }

        private void OnDisable()
        {
            OnEventTracked -= HandleEventTracked;
        }

        // ── Handler ───────────────────────────────────────────────────────────

        private void HandleEventTracked(string eventName, string parameters)
        {
            string entry = string.IsNullOrEmpty(parameters)
                ? eventName
                : $"{eventName} | {parameters}";

            _recentEvents.Enqueue(entry);

            while (_recentEvents.Count > MaxEvents)
                _recentEvents.Dequeue();
        }

        // ── OnGUI ─────────────────────────────────────────────────────────────

        private void OnGUI()
        {
            if (!_enabled) return;

            EnsureStyles();

            float x = Screen.width  - OverlayWidth  - PaddingRight;
            float y = PaddingTop;

            GUILayout.BeginArea(new Rect(x, y, OverlayWidth, OverlayHeight), _boxStyle);

            GUILayout.Label("[Analytics Debug Overlay]", _labelStyle);
            GUILayout.Space(4f);

            _scrollPosition = GUILayout.BeginScrollView(_scrollPosition,
                GUILayout.Width(OverlayWidth - 8f),
                GUILayout.Height(OverlayHeight - 60f));

            foreach (string entry in _recentEvents)
                GUILayout.Label(entry, _labelStyle);

            GUILayout.EndScrollView();

            GUILayout.Space(4f);

            if (GUILayout.Button("Clear", _buttonStyle))
                _recentEvents.Clear();

            GUILayout.EndArea();
        }

        // ── Helpers ───────────────────────────────────────────────────────────

        private void EnsureStyles()
        {
            if (_stylesInitialized) return;
            _stylesInitialized = true;

            var bgTex = new Texture2D(1, 1);
            bgTex.SetPixel(0, 0, new Color(0f, 0f, 0f, BackgroundAlpha));
            bgTex.Apply();

            _boxStyle = new GUIStyle(GUI.skin.box)
            {
                padding = new RectOffset(6, 6, 6, 6)
            };
            _boxStyle.normal.background = bgTex;

            _labelStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize  = 10,
                wordWrap  = true,
                richText  = false,
            };
            _labelStyle.normal.textColor = Color.white;

            _buttonStyle = new GUIStyle(GUI.skin.button)
            {
                fontSize = 10,
            };
        }
    }

#endif // UNITY_EDITOR || KING_SMASH_DEV
}
