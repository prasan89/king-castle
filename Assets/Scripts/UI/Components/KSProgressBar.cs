using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace KingSmash.UI.Components
{
    public class KSProgressBar : MonoBehaviour
    {
        [SerializeField] private Slider          _slider;
        [SerializeField] private Image           _fill;
        [SerializeField] private TextMeshProUGUI _label;
        [SerializeField] private Color           _fillColor  = Color.green;
        [SerializeField] private Color           _emptyColor = new Color(0.2f, 0.2f, 0.2f);

        private Coroutine _progressCoroutine;

        private void Awake()
        {
            if (_fill != null) _fill.color = _fillColor;
        }

        public void SetProgress(float value, bool animated = true, float duration = 0.3f)
        {
            value = Mathf.Clamp01(value);
            if (_progressCoroutine != null) StopCoroutine(_progressCoroutine);

            if (animated && gameObject.activeInHierarchy)
                _progressCoroutine = StartCoroutine(LerpProgress(value, duration));
            else
                if (_slider != null) _slider.value = value;
        }

        public void SetLabel(string text)
        {
            if (_label != null) _label.text = text;
        }

        public void SetColors(Color fill, Color empty)
        {
            _fillColor  = fill;
            _emptyColor = empty;
            if (_fill != null) _fill.color = _fillColor;
        }

        private IEnumerator LerpProgress(float targetValue, float duration)
        {
            if (_slider == null) yield break;

            float startValue = _slider.value;
            float elapsed    = 0f;

            while (elapsed < duration)
            {
                elapsed       += Time.unscaledDeltaTime;
                _slider.value  = Mathf.Lerp(startValue, targetValue, elapsed / duration);
                yield return null;
            }
            _slider.value = targetValue;
        }
    }
}
