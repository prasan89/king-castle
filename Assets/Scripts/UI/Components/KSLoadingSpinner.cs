using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace KingSmash.UI.Components
{
    /// <summary>
    /// Animated spinner overlay. Activate/Deactivate the GameObject to show/hide.
    /// </summary>
    public class KSLoadingSpinner : MonoBehaviour
    {
        [SerializeField] private RectTransform _spinnerTransform;
        [SerializeField] private CanvasGroup   _canvasGroup;
        [SerializeField] private float         _rotationSpeed = 360f;
        [SerializeField] private float         _fadeInDuration  = 0.2f;
        [SerializeField] private float         _fadeOutDuration = 0.15f;

        private Coroutine _fadeCoroutine;
        private bool      _spinning;

        private void OnEnable()
        {
            _spinning = true;
            if (_canvasGroup != null)
            {
                if (_fadeCoroutine != null) StopCoroutine(_fadeCoroutine);
                _fadeCoroutine = StartCoroutine(FadeTo(1f, _fadeInDuration));
            }
        }

        private void OnDisable()
        {
            _spinning = false;
        }

        private void Update()
        {
            if (!_spinning || _spinnerTransform == null) return;
            _spinnerTransform.Rotate(0f, 0f, -_rotationSpeed * Time.unscaledDeltaTime);
        }

        public void Hide(System.Action onDone = null)
        {
            if (_canvasGroup == null)
            {
                gameObject.SetActive(false);
                onDone?.Invoke();
                return;
            }

            if (_fadeCoroutine != null) StopCoroutine(_fadeCoroutine);
            _fadeCoroutine = StartCoroutine(FadeOutAndDisable(onDone));
        }

        private IEnumerator FadeTo(float target, float duration)
        {
            float start   = _canvasGroup.alpha;
            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                _canvasGroup.alpha = Mathf.Lerp(start, target, elapsed / duration);
                yield return null;
            }
            _canvasGroup.alpha = target;
            _fadeCoroutine = null;
        }

        private IEnumerator FadeOutAndDisable(System.Action onDone)
        {
            yield return FadeTo(0f, _fadeOutDuration);
            gameObject.SetActive(false);
            onDone?.Invoke();
        }
    }
}
