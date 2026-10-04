using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace KingSmash.UI.Components
{
    public class KSStarDisplay : MonoBehaviour
    {
        [SerializeField] private List<Image>    _starImages = new List<Image>();
        [SerializeField] private KingSmashTheme _theme;

        private Coroutine _revealCoroutine;

        public void SetStars(int count, bool animated = true)
        {
            count = Mathf.Clamp(count, 0, _starImages.Count);

            if (_revealCoroutine != null) StopCoroutine(_revealCoroutine);

            for (int i = 0; i < _starImages.Count; i++)
            {
                if (_starImages[i] == null) continue;
                bool filled = i < count;
                _starImages[i].color = filled
                    ? (_theme != null ? _theme.starFilled : Color.yellow)
                    : (_theme != null ? _theme.starEmpty  : Color.gray);
                _starImages[i].transform.localScale = Vector3.one;
            }

            if (animated && gameObject.activeInHierarchy)
                _revealCoroutine = StartCoroutine(AnimateStars(count));
        }

        private IEnumerator AnimateStars(int count)
        {
            for (int i = 0; i < count && i < _starImages.Count; i++)
            {
                if (_starImages[i] != null)
                    _starImages[i].transform.localScale = Vector3.zero;
            }

            float stagger = 0.1f;
            for (int i = 0; i < count && i < _starImages.Count; i++)
            {
                if (_starImages[i] == null) continue;
                StartCoroutine(UIAnimationController.BounceReveal(
                    _starImages[i].transform,
                    _theme != null ? _theme.durationBounce : 0.35f));
                if (i < count - 1)
                    yield return new WaitForSecondsRealtime(stagger);
            }
        }
    }
}
