using UnityEngine;
using TMPro;
namespace KingSmash.UI.Widgets
{
    public class CurrencyDisplayWidget : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _coinsLabel;
        [SerializeField] private TextMeshProUGUI _gemsLabel;

        public void SetCoins(long coins) { if (_coinsLabel != null) _coinsLabel.text = Format(coins); }
        public void SetGems(int gems)    { if (_gemsLabel  != null) _gemsLabel.text  = gems.ToString(); }

        public void AnimateCoins(long from, long to)
        {
            if (_coinsLabel != null)
                StartCoroutine(UIAnimationController.CountUp(_coinsLabel, from, to, 0.8f));
        }

        private static string Format(long n)
        {
            if (n >= 1_000_000) return $"{n / 1_000_000f:F1}M";
            if (n >= 1_000)     return $"{n / 1_000f:F1}K";
            return n.ToString();
        }
    }
}
