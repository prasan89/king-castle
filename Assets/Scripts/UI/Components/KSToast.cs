using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace KingSmash.UI.Components
{
    public class KSToast : MonoBehaviour
    {
        [SerializeField] private CanvasGroup     _cg;
        [SerializeField] private RectTransform   _rt;
        [SerializeField] private TextMeshProUGUI _messageLabel;
        [SerializeField] private Image           _iconImage;
        [SerializeField] private Image           _background;

        private Coroutine _showCoroutine;

        private void Awake()
        {
            if (_cg == null) _cg = GetComponent<CanvasGroup>();
            if (_rt == null) _rt = GetComponent<RectTransform>();
        }

        public void Show(string message, Sprite icon, Color bgColor, float duration = 2.5f)
        {
            if (_showCoroutine != null) StopCoroutine(_showCoroutine);

            if (_messageLabel != null) _messageLabel.text = message;
            if (_iconImage != null)
            {
                _iconImage.sprite  = icon;
                _iconImage.enabled = icon != null;
            }
            if (_background != null) _background.color = bgColor;

            gameObject.SetActive(true);
            _showCoroutine = StartCoroutine(ShowSequence(duration));
        }

        private IEnumerator ShowSequence(float holdDuration)
        {
            const float slideInDuration  = 0.22f;
            const float slideOutDuration = 0.22f;
            const float slideDistance    = 80f;

            Vector2 shownPos  = _rt.anchoredPosition;
            Vector2 hiddenPos = shownPos + Vector2.down * slideDistance;
            _rt.anchoredPosition = hiddenPos;
            if (_cg != null) _cg.alpha = 0f;

            // Slide in + fade in
            float elapsed = 0f;
            while (elapsed < slideInDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / slideInDuration);
                _rt.anchoredPosition = Vector2.Lerp(hiddenPos, shownPos, t);
                if (_cg != null) _cg.alpha = t;
                yield return null;
            }
            _rt.anchoredPosition = shownPos;
            if (_cg != null) _cg.alpha = 1f;

            // Hold
            yield return new WaitForSecondsRealtime(holdDuration);

            // Slide out + fade out
            elapsed = 0f;
            while (elapsed < slideOutDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / slideOutDuration);
                _rt.anchoredPosition = Vector2.Lerp(shownPos, hiddenPos, t);
                if (_cg != null) _cg.alpha = 1f - t;
                yield return null;
            }

            _rt.anchoredPosition = shownPos; // reset for pool reuse
            if (_cg != null) _cg.alpha = 0f;
            gameObject.SetActive(false);
        }
    }
}
