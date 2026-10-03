using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
namespace KingSmash.UI.Widgets
{
    public class StarDisplayWidget : MonoBehaviour
    {
        [SerializeField] private List<Image> _stars;
        [SerializeField] private Sprite _starFilled;
        [SerializeField] private Sprite _starEmpty;

        public void SetStars(int count)
        {
            for (int i = 0; i < _stars.Count; i++)
                if (_stars[i] != null) _stars[i].sprite = i < count ? _starFilled : _starEmpty;
        }

        public IEnumerator RevealStars(int count)
        {
            for (int i = 0; i < _stars.Count; i++)
            {
                if (_stars[i] != null) _stars[i].sprite = _starEmpty;
                _stars[i]?.gameObject.SetActive(true);
            }
            yield return new WaitForSecondsRealtime(0.3f);
            for (int i = 0; i < count && i < _stars.Count; i++)
            {
                if (_stars[i] != null) _stars[i].sprite = _starFilled;
                yield return StartCoroutine(UIAnimationController.BounceReveal(_stars[i].transform, 0.25f));
                yield return new WaitForSecondsRealtime(0.12f);
            }
        }
    }
}
