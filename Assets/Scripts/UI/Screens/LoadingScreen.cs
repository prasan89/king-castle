using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using KingSmash.Core;
using KingSmash.Services;
using KingSmash.Audio;

namespace KingSmash.UI.Screens
{
    public class LoadingScreen : UIScreen
    {
        [SerializeField] private UI.Components.KSLoadingSpinner _spinner;
        [SerializeField] private TMP_Text  _tipText;
        [SerializeField] private Slider    _progressBar;
        [SerializeField] private CanvasGroup _canvasGroup;

        private static readonly string[] Tips =
        {
            "Aim for weak points to deal more damage!",
            "Upgrade the King to unlock new abilities.",
            "Chaining combos multiplies your score.",
            "Daily rewards grow the longer you play!",
            "Boss levels drop rare materials."
        };

        protected override void OnShow()
        {
            if (_tipText != null)
                _tipText.text = Tips[Random.Range(0, Tips.Length)];

            if (_progressBar != null)
                _progressBar.value = 0f;

            if (_spinner != null)
                _spinner.gameObject.SetActive(true);
        }

        protected override void OnHide()
        {
            if (_spinner != null)
                _spinner.gameObject.SetActive(false);
        }

        public void SetProgress(float normalized)
        {
            if (_progressBar != null)
                _progressBar.value = Mathf.Clamp01(normalized);
        }

        public IEnumerator FadeOutAndHide()
        {
            if (_canvasGroup == null) { Hide(); yield break; }

            float elapsed = 0f;
            const float dur = 0.25f;
            while (elapsed < dur)
            {
                elapsed += Time.unscaledDeltaTime;
                _canvasGroup.alpha = Mathf.Lerp(1f, 0f, elapsed / dur);
                yield return null;
            }
            _canvasGroup.alpha = 0f;
            Hide();
        }
    }
}
