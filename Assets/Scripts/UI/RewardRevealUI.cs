using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using KingSmash.Economy;
using KingSmash.PowerUps;

namespace KingSmash.UI
{
    public class RewardRevealUI : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _titleLabel;
        [SerializeField] private TextMeshProUGUI _coinsLabel;
        [SerializeField] private TextMeshProUGUI _gemsLabel;
        [SerializeField] private TextMeshProUGUI _powerUpLabel;
        [SerializeField] private GameObject _coinsRoot;
        [SerializeField] private GameObject _gemsRoot;
        [SerializeField] private GameObject _powerUpRoot;
        [SerializeField] private Button _continueButton;
        [SerializeField] private CanvasGroup _canvasGroup;

        private static RewardRevealUI _instance;
        private bool _waitingForContinue;

        private void Awake()
        {
            _instance = this;
            gameObject.SetActive(false);
        }

        private void OnDestroy()
        {
            if (_instance == this) _instance = null;
        }

        public static RewardRevealUI Show(long coins, int gems, PowerUpType powerUp, int powerUpCount, string title = "REWARD!")
        {
            if (_instance == null)
            {
                var prefab = Resources.Load<RewardRevealUI>("RewardRevealUI");
                if (prefab != null)
                    _instance = Instantiate(prefab);
                else
                {
                    var go = new GameObject("RewardRevealUI");
                    _instance = go.AddComponent<RewardRevealUI>();
                }
            }

            _instance.gameObject.SetActive(true);
            _instance.StartCoroutine(_instance.RevealSequence(coins, gems, powerUp, powerUpCount, title));
            return _instance;
        }

        private IEnumerator RevealSequence(long coins, int gems, PowerUpType powerUp, int powerUpCount, string title)
        {
            _waitingForContinue = false;

            if (_continueButton != null)
            {
                _continueButton.onClick.RemoveAllListeners();
                _continueButton.onClick.AddListener(OnContinueClicked);
            }

            if (_canvasGroup != null) _canvasGroup.alpha = 0f;
            if (_titleLabel != null) _titleLabel.text = title;

            if (_coinsRoot != null) _coinsRoot.SetActive(false);
            if (_gemsRoot != null) _gemsRoot.SetActive(false);
            if (_powerUpRoot != null) _powerUpRoot.SetActive(false);

            yield return UIAnimationController.Fade(_canvasGroup, 0f, 1f, 0.3f);

            yield return UIAnimationController.BounceReveal(_titleLabel != null ? _titleLabel.transform : transform, 0.3f);

            if (coins > 0 && _coinsRoot != null)
            {
                _coinsRoot.SetActive(true);
                if (_coinsLabel != null)
                    yield return UIAnimationController.CountUp(_coinsLabel, 0, coins, 0.8f, "+");
            }

            if (gems > 0 && _gemsRoot != null)
            {
                _gemsRoot.SetActive(true);
                if (_gemsLabel != null) _gemsLabel.text = $"+{gems}";
                yield return UIAnimationController.BounceReveal(_gemsRoot.transform, 0.2f);
            }

            if (powerUp != PowerUpType.None && powerUpCount > 0 && _powerUpRoot != null)
            {
                _powerUpRoot.SetActive(true);
                if (_powerUpLabel != null) _powerUpLabel.text = $"{powerUp.DisplayName()} x{powerUpCount}";
                yield return UIAnimationController.BounceReveal(_powerUpRoot.transform, 0.2f);
            }

            _waitingForContinue = true;
            yield return new WaitUntil(() => !_waitingForContinue);

            yield return UIAnimationController.Fade(_canvasGroup, 1f, 0f, 0.2f);
            gameObject.SetActive(false);
        }

        private void OnContinueClicked()
        {
            _waitingForContinue = false;
        }
    }
}
