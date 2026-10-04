using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using KingSmash.Core;
using KingSmash.Services;

namespace KingSmash.UI.Screens
{
    public class SplashScreen : UIScreen
    {
        [Header("Logo / Art")]
        [SerializeField] private CanvasGroup _logoGroup;
        [SerializeField] private CanvasGroup _kingArtGroup;
        [SerializeField] private TextMeshProUGUI _gameTitleLabel;

        [Header("Loading Bar")]
        [SerializeField] private RectTransform _loadingBarFill;
        [SerializeField] private CanvasGroup   _loadingBarGroup;
        [SerializeField] private float         _progressDuration = 2f;

        [Header("Floating Particles")]
        [SerializeField] private RectTransform[] _floatingParticles;   // crown/star decorations
        [SerializeField] private float           _floatAmplitude = 12f;
        [SerializeField] private float           _floatPeriod    = 2.8f;

        [Header("Theme")]
        [SerializeField] private KingSmashTheme _themeConfig;

        private float _barMaxWidth = 600f;

        protected override void Awake()
        {
            base.Awake();
            // Cache full width of the bar from its parent rect if possible
            if (_loadingBarFill != null && _loadingBarFill.parent is RectTransform parentRect)
                _barMaxWidth = parentRect.rect.width;
        }

        protected override void OnShow()
        {
            // Background gradient
            if (Camera.main != null && _themeConfig != null)
                Camera.main.backgroundColor = _themeConfig.skyTop;

            // Hide everything initially
            if (_logoGroup    != null) { _logoGroup.alpha    = 0f; }
            if (_kingArtGroup != null) { _kingArtGroup.alpha = 0f; }
            if (_loadingBarGroup != null) { _loadingBarGroup.alpha = 0f; }
            SetBarProgress(0f);

            StartCoroutine(SplashSequence());
            StartCoroutine(FloatParticles());
        }

        private IEnumerator SplashSequence()
        {
            // 1 — Lerp background color
            float bgElapsed = 0f;
            float bgDuration = 0.8f;
            Color skyStart  = _themeConfig != null ? _themeConfig.skyTop    : new Color(0.08f, 0.18f, 0.45f);
            Color skyEnd    = _themeConfig != null ? _themeConfig.skyBottom : new Color(0.12f, 0.28f, 0.65f);
            while (bgElapsed < bgDuration)
            {
                bgElapsed += Time.unscaledDeltaTime;
                if (Camera.main != null)
                    Camera.main.backgroundColor = Color.Lerp(skyStart, skyEnd, bgElapsed / bgDuration);
                yield return null;
            }

            // 2 — Fade in king art
            if (_kingArtGroup != null)
                yield return StartCoroutine(UIAnimationController.Fade(_kingArtGroup, 0f, 1f, 0.4f));

            // 3 — Bounce reveal game title
            if (_gameTitleLabel != null)
                yield return StartCoroutine(UIAnimationController.BounceReveal(_gameTitleLabel.transform, 0.35f));

            // 4 — Fade in logo
            if (_logoGroup != null)
                yield return StartCoroutine(UIAnimationController.Fade(_logoGroup, 0f, 1f, 0.3f));

            // 5 — Fade in loading bar
            if (_loadingBarGroup != null)
                yield return StartCoroutine(UIAnimationController.Fade(_loadingBarGroup, 0f, 1f, 0.25f));

            // 6 — Animate progress bar 0→100%
            float elapsed = 0f;
            while (elapsed < _progressDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                // Allow touch to skip bar but still show at least 80%
                if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
                    elapsed = _progressDuration;

                SetBarProgress(Mathf.Clamp01(elapsed / _progressDuration));
                yield return null;
            }
            SetBarProgress(1f);

            // 7 — Short pause, then transition
            yield return new WaitForSecondsRealtime(0.3f);
            Advance();
        }

        private void SetBarProgress(float t)
        {
            if (_loadingBarFill == null) return;
            var sd = _loadingBarFill.sizeDelta;
            sd.x = Mathf.Lerp(0f, _barMaxWidth, t);
            _loadingBarFill.sizeDelta = sd;
        }

        private IEnumerator FloatParticles()
        {
            if (_floatingParticles == null || _floatingParticles.Length == 0) yield break;

            // Store original Y positions
            float[] originY = new float[_floatingParticles.Length];
            for (int i = 0; i < _floatingParticles.Length; i++)
                if (_floatingParticles[i] != null)
                    originY[i] = _floatingParticles[i].anchoredPosition.y;

            float time = 0f;
            while (true)
            {
                time += Time.unscaledDeltaTime;
                for (int i = 0; i < _floatingParticles.Length; i++)
                {
                    if (_floatingParticles[i] == null) continue;
                    float offset = i * (Mathf.PI * 2f / Mathf.Max(1, _floatingParticles.Length));
                    float y = originY[i] + Mathf.Sin(time * (Mathf.PI * 2f / _floatPeriod) + offset) * _floatAmplitude;
                    var pos = _floatingParticles[i].anchoredPosition;
                    pos.y = y;
                    _floatingParticles[i].anchoredPosition = pos;
                }
                yield return null;
            }
        }

        private void Advance()
        {
            bool isSignedIn = true;
            if (ServiceLocator.TryGet<IAuthService>(out var auth))
                isSignedIn = auth.IsSignedIn;

            if (!isSignedIn)
            {
                ScreenManager.Instance.Show<LoginScreen>();
                return;
            }

            if (ServiceLocator.TryGet<ISaveService>(out var save) && save.Current.playerId == "")
                ScreenManager.Instance.Show<LoginScreen>();
            else
                ScreenManager.Instance.Show<HomeScreen>();
        }
    }
}
