using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace KingSmash.UI.Components
{
    public class KSPanel : MonoBehaviour
    {
        [SerializeField] private KingSmashTheme _theme;
        [SerializeField] private Image          _background;
        [SerializeField] private bool           _hasShadow = true;
        [SerializeField] private Shadow         _shadowComponent;

        private Coroutine _animCoroutine;

        private void Awake()
        {
            ApplyTheme();
        }

        public void ApplyTheme()
        {
            if (_theme == null) return;

            if (_background != null)
                _background.color = _theme.panelBackground;

            if (_hasShadow && _shadowComponent != null)
            {
                _shadowComponent.effectColor  = _theme.shadowColor;
                _shadowComponent.effectDistance = _theme.shadowOffset;
            }
        }

        public void Show(bool animated = true)
        {
            gameObject.SetActive(true);
            if (animated)
            {
                if (_animCoroutine != null) StopCoroutine(_animCoroutine);
                var rt = GetComponent<RectTransform>();
                if (rt != null)
                    _animCoroutine = StartCoroutine(
                        UIAnimationController.SlideIn(rt, _theme != null ? _theme.panelSlideDistance : 60f,
                            _theme != null ? _theme.durationNormal : 0.25f));
            }
        }

        public void Hide(bool animated = true)
        {
            if (animated)
            {
                if (_animCoroutine != null) StopCoroutine(_animCoroutine);
                var rt = GetComponent<RectTransform>();
                if (rt != null)
                    _animCoroutine = StartCoroutine(SlideOutThenDeactivate(rt));
                else
                    gameObject.SetActive(false);
            }
            else
            {
                gameObject.SetActive(false);
            }
        }

        private IEnumerator SlideOutThenDeactivate(RectTransform rt)
        {
            yield return UIAnimationController.SlideOut(rt,
                _theme != null ? _theme.panelSlideDistance : 60f,
                _theme != null ? _theme.durationFast : 0.20f);
            gameObject.SetActive(false);
        }
    }
}
