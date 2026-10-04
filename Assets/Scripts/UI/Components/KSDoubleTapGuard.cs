using UnityEngine;
using UnityEngine.EventSystems;

namespace KingSmash.UI.Components
{
    /// <summary>
    /// Requires two taps within the window to trigger; ignores single rapid taps.
    /// Attach to any UI element that should require confirmation before action.
    /// </summary>
    public class KSDoubleTapGuard : MonoBehaviour, IPointerClickHandler
    {
        [SerializeField] private float _windowSeconds = 0.4f;

        private float _lastTapTime = -999f;
        private int   _tapCount;

        public System.Action OnDoubleTap;

        public void OnPointerClick(PointerEventData eventData)
        {
            float now = Time.unscaledTime;
            if (now - _lastTapTime <= _windowSeconds)
            {
                _tapCount++;
                if (_tapCount >= 2)
                {
                    _tapCount    = 0;
                    _lastTapTime = -999f;
                    OnDoubleTap?.Invoke();
                }
            }
            else
            {
                _tapCount = 1;
            }
            _lastTapTime = now;
        }
    }
}
