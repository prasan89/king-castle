using System.Collections;
using UnityEngine;
using KingSmash.Core;
namespace KingSmash.UI.Screens
{
    public class SplashScreen : UIScreen
    {
        [SerializeField] private float _splashDuration = 2f;
        [SerializeField] private CanvasGroup _logoGroup;

        protected override void OnShow()
        {
            StartCoroutine(SplashSequence());
        }

        private IEnumerator SplashSequence()
        {
            if (_logoGroup != null) yield return StartCoroutine(UIAnimationController.Fade(_logoGroup, 0f, 1f, 0.6f));
            float elapsed = 0f;
            while (elapsed < _splashDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began) break;
                yield return null;
            }
            Advance();
        }

        private void Advance()
        {
            if (ServiceLocator.TryGet<KingSmash.Services.ISaveService>(out var save) && save.Current.playerId == "")
                ScreenManager.Instance.Show<LoginScreen>();
            else
                ScreenManager.Instance.Show<HomeScreen>();
        }
    }
}
