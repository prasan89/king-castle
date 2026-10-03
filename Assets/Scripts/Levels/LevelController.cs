using System;
using System.Collections;
using UnityEngine;
using KingSmash.Core;
using KingSmash.Gameplay;
using KingSmash.Physics;
using KingSmash.Services;
using KingSmash.Economy;

namespace KingSmash.Levels
{
    /// Orchestrates a single level: state machine, score, restart.
    public class LevelController : MonoBehaviour
    {
        [Header("Config")]
        [SerializeField] private LevelConfig _levelConfig;
        [SerializeField] private EconomyConfig _economyConfig;

        [Header("Scene References")]
        [SerializeField] private LaunchController _launchController;
        [SerializeField] private DestructionController _destructionController;
        [SerializeField] private CastleStructure[] _castles;
        [SerializeField] private StarThresholds _starThresholds;
        [SerializeField] private GameObject _tutorialHintRoot; // enable only for level 1

        [Header("Timing")]
        [SerializeField] private float _resolveDelay = 2.5f;
        [SerializeField] private float _levelStartDelay = 0.5f;

        private LevelStateMachine _stateMachine;
        private int _score;
        private int _totalBlocks;
        private int _destroyedBlocks;
        private int _enemiesDefeated;
        private int _totalEnemies;
        private bool _queenRescued;
        private int _attemptsUsed;

        public static event Action<LevelState, LevelState> OnLevelStateChanged;
        public static event Action<int, int, float> OnLevelEnded; // score, stars, destructionRatio

        public LevelState CurrentState => _stateMachine?.Current ?? LevelState.LevelStart;
        public float DestructionRatio => _totalBlocks > 0 ? (float)_destroyedBlocks / _totalBlocks : 0f;

        private void Awake()
        {
            _stateMachine = new LevelStateMachine();
            LevelStateMachine.OnStateChanged += (prev, next) => OnLevelStateChanged?.Invoke(prev, next);
        }

        private void CountTotalBlocks()
        {
            _totalBlocks = 0;
            foreach (var c in _castles)
                if (c != null) _totalBlocks += c.TotalPieces;
        }

        private void Start()
        {
            // Count blocks after all Awake() calls have run
            CountTotalBlocks();
            _totalEnemies = FindObjectsByType<KingSmash.Characters.EnemyController>(FindObjectsSortMode.None).Length;
            _launchController?.Initialize(_levelConfig != null ? _levelConfig.kingLaunches : 3);
            GameManager.Instance?.StartLevel(_levelConfig != null ? _levelConfig.levelIndex : 0);
            StartCoroutine(StartSequence());
        }

        private IEnumerator StartSequence()
        {
            yield return new WaitForSeconds(_levelStartDelay);
            _stateMachine.TryTransition(LevelState.Playing);
        }

        private void OnEnable()
        {
            DestructionController.OnAllCastlesDestroyed += HandleAllCastlesDestroyed;
            LaunchController.OnAllLaunchesExpended    += HandleAllLaunchesExpended;
            LaunchController.OnKingLaunched           += HandleKingLaunched;
            DestructibleObject.OnDestroyed            += HandleBlockDestroyed;
            KingSmash.Characters.EnemyController.OnEnemyDied += HandleEnemyDied;
            KingSmash.Characters.QueenController.OnQueenRescued += HandleQueenRescued;
        }

        private void OnDisable()
        {
            DestructionController.OnAllCastlesDestroyed -= HandleAllCastlesDestroyed;
            LaunchController.OnAllLaunchesExpended    -= HandleAllLaunchesExpended;
            LaunchController.OnKingLaunched           -= HandleKingLaunched;
            DestructibleObject.OnDestroyed            -= HandleBlockDestroyed;
            KingSmash.Characters.EnemyController.OnEnemyDied -= HandleEnemyDied;
            KingSmash.Characters.QueenController.OnQueenRescued -= HandleQueenRescued;
        }

        private void HandleKingLaunched(Vector2 _, float __)
        {
            _stateMachine.TryTransition(LevelState.KingFlying);
        }

        private void HandleBlockDestroyed(DestructibleObject block)
        {
            _destroyedBlocks++;
            AddScore(50);
        }

        private void AddScore(int amount)
        {
            _score += amount;
        }

        private void HandleAllCastlesDestroyed()
        {
            if (CurrentState == LevelState.LevelComplete || CurrentState == LevelState.LevelFailed) return;
            _stateMachine.TryTransition(LevelState.LevelComplete);
            EndLevel(won: true);
        }

        private void HandleAllLaunchesExpended()
        {
            if (CurrentState == LevelState.LevelComplete || CurrentState == LevelState.LevelFailed) return;
            _stateMachine.TryTransition(LevelState.Resolving);
            StartCoroutine(ResolveAfterDelay());
        }

        private IEnumerator ResolveAfterDelay()
        {
            yield return new WaitForSeconds(_resolveDelay);
            if (CurrentState == LevelState.Resolving)
            {
                bool won = _destructionController != null && _destructionController.AllCastlesDestroyed;
                if (won) _stateMachine.TryTransition(LevelState.LevelComplete);
                else _stateMachine.TryTransition(LevelState.LevelFailed);
                EndLevel(won);
            }
        }

        private void HandleEnemyDied(KingSmash.Characters.EnemyController _) => _enemiesDefeated++;
        private void HandleQueenRescued(KingSmash.Characters.QueenController _) => _queenRescued = true;

        private void EndLevel(bool won)
        {
            int attemptsRemaining = _launchController != null ? _launchController.LaunchesRemaining : 0;
            string failReason = won ? null : "No launches remaining";

            var result = LevelRewardCalculator.Build(
                levelIndex:        _levelConfig != null ? _levelConfig.levelIndex : 0,
                score:             _score,
                destructionRatio:  DestructionRatio,
                enemiesDefeated:   _enemiesDefeated,
                totalEnemies:      _totalEnemies,
                queenRescued:      _queenRescued,
                attemptsRemaining: attemptsRemaining,
                isVictory:         won,
                failReason:        failReason,
                starThresholds:    _starThresholds,
                economy:           _economyConfig);

            LevelProgressionService.CommitResult(result);

            if (won)
                GameManager.Instance?.CompleteLevel(result.LevelIndex, result.Stars);
            else
                GameManager.Instance?.FailLevel(result.LevelIndex, failReason);

            OnLevelEnded?.Invoke(result.Score, result.Stars, result.DestructionRatio);
        }

        public void RestartLevel()
        {
            GameLogger.Info("LevelController", "Restarting level.");
            _stateMachine.ForceTransition(LevelState.LevelStart);
            SceneLoader.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
        }
    }
}
