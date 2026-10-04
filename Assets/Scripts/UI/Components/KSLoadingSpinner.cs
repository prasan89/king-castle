using UnityEngine;

namespace KingSmash.UI.Components
{
    /// <summary>
    /// Continuously rotates a RectTransform to display a loading spinner.
    /// Call Show()/Hide() to activate/deactivate.
    /// </summary>
    public class KSLoadingSpinner : MonoBehaviour
    {
        [SerializeField] private RectTransform _spinnerTransform;
        [SerializeField] private float         _rotationSpeed = 360f;

        private void Update()
        {
            if (_spinnerTransform == null) return;
            _spinnerTransform.Rotate(0f, 0f, -_rotationSpeed * Time.unscaledDeltaTime);
        }

        /// <summary>Activates the spinner GameObject.</summary>
        public void Show()
        {
            gameObject.SetActive(true);
        }

        /// <summary>Deactivates the spinner GameObject.</summary>
        public void Hide()
        {
            gameObject.SetActive(false);
        }
    }
}
