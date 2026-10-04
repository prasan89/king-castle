using UnityEngine;
using TMPro;

namespace KingSmash.UI.Components
{
    public class KSBadge : MonoBehaviour
    {
        [SerializeField] private GameObject      _dot;
        [SerializeField] private TextMeshProUGUI _countLabel;

        public void SetCount(int count)
        {
            bool hasAny  = count > 0;
            bool showNum = count > 1;

            if (_dot != null)
                _dot.SetActive(hasAny);

            if (_countLabel != null)
            {
                _countLabel.gameObject.SetActive(showNum);
                if (showNum) _countLabel.text = count > 99 ? "99+" : count.ToString();
            }
        }

        public void Show()
        {
            gameObject.SetActive(true);
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }
    }
}
