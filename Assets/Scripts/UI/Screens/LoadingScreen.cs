using System.Collections;
using UnityEngine;
using TMPro;
using KingSmash.UI.Components;

namespace KingSmash.UI.Screens
{
    /// <summary>
    /// Full-screen loading screen with spinner, hint label, progress bar, and floating king art.
    /// </summary>
    public class LoadingScreen : UIScreen
    {
        [SerializeField] private KingSmashTheme   _themeConfig;
        [SerializeField] private KSProgressBar    _progressBar;
        [SerializeField] private TextMeshProUGUI  _hintLabel;
        [SerializeField] private KSLoadingSpinner _spinner;
        [SerializeField] private RectTransform    _kingArtTransform;

        private static readonly string[] Hints =
        {
            "Aim for the weak spots!",
            "Power-ups can change everything!",
            "Rescue the Queen to complete levels!",
            "Upgrade your King to smash harder!",
            "Chain explosions for maximum destruction!",
            "3 stars means perfect destruction!",
            "Daily rewards grow with your streak!",
            "Complete missions to earn bonus coins!",
            "Ice smash freezes enemies!",
            "The Final Castle awaits..."
        };

        private Coroutine _floatCoroutine;
        private Coroutine _progressCoroutine;

        protected override void OnShow()
        {
            // Show spinner
            if (_spinner != null)
                _spinner.Show();

            // Show a random hint
            if (_hintLabel != null)
                _hintLabel.text = Hints[UnityEngine.Random.Range(0, Hints.Length)];

            // Reset progress bar to 0
            if (_progressBar != null)
                _progressBar.SetProgress(0f, animated: false);

            // Start floating king art
            if (_kingArtTransform != null)
            {
                if (_floatCoroutine != null) StopCoroutine(_floatCoroutine);
                _floatCoroutine = StartCoroutine(FloatKingArt());
            }
        }

        protected override void OnHide()
        {
            if (_spinner != null)
                _spinner.Hide();

            if (_floatCoroutine != null)
            {
                StopCoroutine(_floatCoroutine);
                _floatCoroutine = null;
            }

            if (_progressCoroutine != null)
            {
                StopCoroutine(_progressCoroutine);
                _progressCoroutine = null;
            }
        }

        /// <summary>Updates the progress bar (0-1).</summary>
        public void SetProgress(float normalized)
        {
            if (_progressBar != null)
                _progressBar.SetProgress(Mathf.Clamp01(normalized));
        }

        // Sinusoidal float for king art
        private IEnumerator FloatKingArt()
        {
            if (_kingArtTransform == null) yield break;

            Vector2 origin    = _kingArtTransform.anchoredPosition;
            float   amplitude = 12f;
            float   speed     = 1.2f;
            float   elapsed   = 0f;

            while (true)
            {
                elapsed += Time.unscaledDeltaTime * speed;
                float offset = Mathf.Sin(elapsed) * amplitude;
                _kingArtTransform.anchoredPosition = origin + new Vector2(0f, offset);
                yield return null;
            }
        }
    }
}
