using System.Collections;
using UnityEngine;

namespace KingSmash.VFX
{
    [RequireComponent(typeof(CanvasGroup))]
    public class ScreenFlashEffect : MonoBehaviour
    {
        [SerializeField] private CanvasGroup _canvasGroup;

        private Coroutine _flashCoroutine;

        private void Awake()
        {
            if (_canvasGroup == null)
                _canvasGroup = GetComponent<CanvasGroup>();

            if (_canvasGroup != null)
            {
                _canvasGroup.alpha          = 0f;
                _canvasGroup.blocksRaycasts = false;
                _canvasGroup.interactable   = false;
            }
        }

        public void Flash(Color color, float duration)
        {
            if (_canvasGroup == null) return;

            var img = GetComponent<UnityEngine.UI.Image>();
            if (img != null) img.color = color;

            if (_flashCoroutine != null)
                StopCoroutine(_flashCoroutine);

            _flashCoroutine = StartCoroutine(RunFlash(duration));
        }

        private IEnumerator RunFlash(float duration)
        {
            const float StartAlpha = 0.6f;

            _canvasGroup.alpha = StartAlpha;

            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                _canvasGroup.alpha = Mathf.Lerp(StartAlpha, 0f, elapsed / duration);
                yield return null;
            }

            _canvasGroup.alpha = 0f;
            _flashCoroutine    = null;
        }
    }
}
