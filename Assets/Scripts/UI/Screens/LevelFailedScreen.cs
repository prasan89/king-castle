using UnityEngine;
using UnityEngine.UI;
using TMPro;
using KingSmash.Core;
using KingSmash.Levels;
namespace KingSmash.UI.Screens
{
    public class LevelFailedScreen : UIScreen
    {
        [Header("Info")]
        [SerializeField] private TextMeshProUGUI _reasonLabel;
        [SerializeField] private TextMeshProUGUI _attemptsLabel;

        [Header("Buttons")]
        [SerializeField] private Button _retryButton;
        [SerializeField] private Button _homeButton;

        private LevelResult _result;

        protected override void Awake()
        {
            base.Awake();
            _retryButton?.onClick.AddListener(OnRetryClicked);
            _homeButton?.onClick.AddListener(OnHomeClicked);
        }

        private void OnEnable()  => LevelController.OnLevelEnded += HandleLevelEnded;
        private void OnDisable() => LevelController.OnLevelEnded -= HandleLevelEnded;

        private void HandleLevelEnded(int score, int stars, float destructionRatio)
        {
            if (!GameManager.Instance || GameManager.Instance.CurrentState != GameState.LevelFailed) return;
            _result = new LevelResult { Stars = stars, Score = score, IsVictory = false, FailReason = "No launches remaining" };
            Show();
        }

        protected override void OnShow()
        {
            if (_result == null) return;
            if (_reasonLabel   != null) _reasonLabel.text   = _result.FailReason ?? "No launches remaining!";
            if (_attemptsLabel != null) _attemptsLabel.text = _result.AttemptsRemaining > 0
                ? $"{_result.AttemptsRemaining} attempts remaining"
                : "No attempts remaining!";
        }

        private void OnRetryClicked() => SceneLoader.LoadScene(SceneNames.Level);
        private void OnHomeClicked()  => SceneLoader.LoadScene(SceneNames.MainMenu);

        private void OnDestroy()
        {
            _retryButton?.onClick.RemoveListener(OnRetryClicked);
            _homeButton?.onClick.RemoveListener(OnHomeClicked);
        }
    }
}
