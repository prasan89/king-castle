using UnityEngine;
using UnityEngine.UI;
using TMPro;
using KingSmash.Core;
using KingSmash.Gameplay;
using KingSmash.Levels;
using KingSmash.Physics;
using KingSmash.Characters;

namespace KingSmash.UI
{
    /// Minimal in-game HUD.  Wires up to events — no polling in Update.
    public class GameHUD : MonoBehaviour
    {
        [Header("Top Bar")]
        [SerializeField] private TextMeshProUGUI _levelLabel;
        [SerializeField] private Button _pauseButton;

        [Header("Launch Indicators")]
        [SerializeField] private Transform _launchIconContainer;
        [SerializeField] private GameObject _launchIconPrefab;

        [Header("Destruction Bar")]
        [SerializeField] private Slider _destructionSlider;
        [SerializeField] private TextMeshProUGUI _destructionLabel;

        [Header("M2 — Character Info")]
        [SerializeField] private TextMeshProUGUI _enemiesRemainingLabel;
        [SerializeField] private TextMeshProUGUI _queenStatusLabel;
        [SerializeField] private Slider _kingHPSlider;

        [Header("Result Panel")]
        [SerializeField] private GameObject _resultPanel;
        [SerializeField] private TextMeshProUGUI _resultTitle;
        [SerializeField] private TextMeshProUGUI _resultScore;
        [SerializeField] private TextMeshProUGUI _resultDestructionPct;
        [SerializeField] private Button _restartButton;

        private LevelController _levelController;
        private int _enemiesRemaining;

        private void Awake()
        {
            _resultPanel?.SetActive(false);
            _pauseButton?.onClick.AddListener(OnPauseClicked);
            _restartButton?.onClick.AddListener(OnRestartClicked);
        }

        private void OnEnable()
        {
            LaunchController.OnLaunchesRemainingChanged += UpdateLaunchIcons;
            LevelController.OnLevelStateChanged         += HandleStateChanged;
            LevelController.OnLevelEnded                += HandleLevelEnded;
            DestructionController.OnGlobalDestructionChanged += UpdateDestructionBar;
            EnemyController.OnEnemyDied     += HandleEnemyDied;
            KingHealth.OnKingHPChanged      += UpdateKingHP;
            QueenController.OnQueenRescued  += HandleQueenRescued;
        }

        private void OnDisable()
        {
            LaunchController.OnLaunchesRemainingChanged -= UpdateLaunchIcons;
            LevelController.OnLevelStateChanged         -= HandleStateChanged;
            LevelController.OnLevelEnded                -= HandleLevelEnded;
            DestructionController.OnGlobalDestructionChanged -= UpdateDestructionBar;
            EnemyController.OnEnemyDied     -= HandleEnemyDied;
            KingHealth.OnKingHPChanged      -= UpdateKingHP;
            QueenController.OnQueenRescued  -= HandleQueenRescued;
        }

        public void Initialize(int levelIndex, int totalLaunches, LevelController levelController)
        {
            _levelController = levelController;
            if (_levelLabel != null) _levelLabel.text = $"Level {levelIndex + 1}";
            BuildLaunchIcons(totalLaunches);
        }

        public void SetEnemyCount(int total)
        {
            _enemiesRemaining = total;
            UpdateEnemyLabel();
        }

        private void BuildLaunchIcons(int count)
        {
            if (_launchIconContainer == null || _launchIconPrefab == null) return;
            foreach (Transform child in _launchIconContainer) Destroy(child.gameObject);
            for (int i = 0; i < count; i++)
                Instantiate(_launchIconPrefab, _launchIconContainer);
        }

        private void UpdateLaunchIcons(int remaining)
        {
            if (_launchIconContainer == null) return;
            int i = 0;
            foreach (Transform child in _launchIconContainer)
            {
                child.gameObject.SetActive(i < remaining);
                i++;
            }
        }

        private void UpdateDestructionBar(float ratio)
        {
            if (_destructionSlider != null) _destructionSlider.value = ratio;
            if (_destructionLabel  != null) _destructionLabel.text   = $"{ratio * 100f:F0}%";
        }

        private void HandleStateChanged(LevelState prev, LevelState next)
        {
            GameLogger.Debug("GameHUD", $"LevelState: {prev} -> {next}");
        }

        private void HandleLevelEnded(int score, int stars, float destructionRatio)
        {
            _resultPanel?.SetActive(true);
            if (_resultTitle != null)
                _resultTitle.text = stars > 0 ? "SMASHED!" : "FAILED";
            if (_resultScore != null)
                _resultScore.text = $"Score: {score:N0}";
            if (_resultDestructionPct != null)
                _resultDestructionPct.text = $"Destroyed: {destructionRatio * 100f:F0}%";
        }

        private void OnPauseClicked()
        {
            GameManager.Instance?.TransitionTo(GameState.Paused);
            Time.timeScale = 0f;
        }

        private void OnRestartClicked()
        {
            Time.timeScale = 1f;
            _resultPanel?.SetActive(false);
            _levelController?.RestartLevel();
        }

        private void OnDestroy()
        {
            _pauseButton?.onClick.RemoveAllListeners();
            _restartButton?.onClick.RemoveAllListeners();
        }

        private void HandleEnemyDied(EnemyController _)
        {
            _enemiesRemaining = Mathf.Max(0, _enemiesRemaining - 1);
            UpdateEnemyLabel();
        }

        private void UpdateEnemyLabel()
        {
            if (_enemiesRemainingLabel != null)
                _enemiesRemainingLabel.text = $"Enemies: {_enemiesRemaining}";
        }

        private void UpdateKingHP(float current, float max)
        {
            if (_kingHPSlider != null) _kingHPSlider.value = max > 0 ? current / max : 0f;
        }

        private void HandleQueenRescued(QueenController _)
        {
            if (_queenStatusLabel != null) _queenStatusLabel.text = "Queen Rescued!";
        }
    }
}
