using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using KingSmash.Core;
using KingSmash.Levels;
using KingSmash.Services;
using KingSmash.Progression;

namespace KingSmash.UI.Screens
{
    public class LevelCompleteScreen : UIScreen
    {
        [Header("Stars")]
        [SerializeField] private List<GameObject> _starObjects;  // 3 star GameObjects

        [Header("Stats")]
        [SerializeField] private TextMeshProUGUI _destructionLabel;
        [SerializeField] private TextMeshProUGUI _enemiesLabel;
        [SerializeField] private TextMeshProUGUI _queenLabel;
        [SerializeField] private TextMeshProUGUI _coinsEarnedLabel;

        [Header("XP / King Level-Up")]
        [SerializeField] private TextMeshProUGUI _xpEarnedLabel;
        [SerializeField] private GameObject      _levelUpPanel;
        [SerializeField] private TextMeshProUGUI _levelUpLabel;

        [Header("Buttons")]
        [SerializeField] private Button _homeButton;
        [SerializeField] private Button _nextLevelButton;
        [SerializeField] private Button _replayButton;

        private LevelResult _result;
        private int         _currentLevelIndex;

        // Level-up state captured from KingProgressionService.OnKingLevelUp
        private bool _leveledUpThisSession;
        private int  _levelUpFromLevel;
        private int  _levelUpToLevel;

        // -------------------------------------------------------------------------
        // Lifecycle
        // -------------------------------------------------------------------------

        protected override void Awake()
        {
            base.Awake();
            _homeButton?.onClick.AddListener(OnHomeClicked);
            _nextLevelButton?.onClick.AddListener(OnNextLevelClicked);
            _replayButton?.onClick.AddListener(OnReplayClicked);
        }

        private void OnEnable()
        {
            LevelController.OnLevelEnded += HandleLevelEnded;
            KingProgressionService.OnKingLevelUp += HandleKingLevelUp;
        }

        private void OnDisable()
        {
            LevelController.OnLevelEnded -= HandleLevelEnded;
            KingProgressionService.OnKingLevelUp -= HandleKingLevelUp;
        }

        // -------------------------------------------------------------------------
        // Event handlers
        // -------------------------------------------------------------------------

        private void HandleLevelEnded(int score, int stars, float destructionRatio)
        {
            if (!GameManager.Instance || GameManager.Instance.CurrentState != GameState.LevelComplete) return;

            // Reset level-up state for this new result session
            _leveledUpThisSession = false;
            _levelUpFromLevel     = 0;
            _levelUpToLevel       = 0;

            // Prefer the full result committed by LevelProgressionService
            LevelResult lastResult = LevelProgressionService.LastResult;
            bool resultIsFresh = lastResult != null
                                 && lastResult.Stars == stars
                                 && lastResult.Score == score;

            if (resultIsFresh)
            {
                _result = lastResult;
            }
            else
            {
                // Fallback: construct a minimal result from the signal parameters
                _result = new LevelResult
                {
                    Stars            = stars,
                    Score            = score,
                    DestructionRatio = destructionRatio,
                    IsVictory        = true,
                    CoinsEarned      = stars * 25L
                };
            }

            Show();
        }

        private void HandleKingLevelUp(int fromLevel, int toLevel, KingStats stats)
        {
            _leveledUpThisSession = true;
            _levelUpFromLevel     = fromLevel;
            _levelUpToLevel       = toLevel;
        }

        // -------------------------------------------------------------------------
        // Show / reveal sequence
        // -------------------------------------------------------------------------

        protected override void OnShow()
        {
            if (_result == null) return;

            // Hide level-up panel until we know whether a level-up happened
            if (_levelUpPanel != null) _levelUpPanel.SetActive(false);

            StartCoroutine(PlayRevealSequence());
        }

        private IEnumerator PlayRevealSequence()
        {
            // Hide all stars first
            foreach (var s in _starObjects) if (s != null) s.SetActive(false);

            yield return new WaitForSecondsRealtime(0.3f);

            // Reveal stars one by one
            for (int i = 0; i < _starObjects.Count; i++)
            {
                if (i < _result.Stars && _starObjects[i] != null)
                {
                    _starObjects[i].SetActive(true);
                    yield return StartCoroutine(UIAnimationController.BounceReveal(_starObjects[i].transform, 0.3f));
                    yield return new WaitForSecondsRealtime(0.15f);
                }
            }

            // Populate stat labels
            if (_destructionLabel != null)
                _destructionLabel.text = $"{_result.DestructionRatio * 100f:F0}%";
            if (_enemiesLabel != null)
                _enemiesLabel.text = $"{_result.EnemiesDefeated}/{_result.TotalEnemies}";
            if (_queenLabel != null)
                _queenLabel.text = _result.QueenRescued ? "✓" : "✗";

            // Animate coin count-up
            if (_coinsEarnedLabel != null)
                yield return StartCoroutine(UIAnimationController.CountUp(_coinsEarnedLabel, 0, _result.CoinsEarned, 1.2f));

            // Animate XP count-up
            long xpEarned = _result.XPEarned;
            if (_xpEarnedLabel != null)
            {
                _xpEarnedLabel.text = "+0 XP";
                yield return StartCoroutine(UIAnimationController.CountUp(_xpEarnedLabel, 0, xpEarned, 1.0f,
                    prefix: "+", suffix: " XP"));
            }

            // Show level-up panel if a level-up was triggered during this result
            if (_leveledUpThisSession && _levelUpPanel != null)
            {
                if (_levelUpLabel != null)
                    _levelUpLabel.text = $"King Lv {_levelUpFromLevel} → Lv {_levelUpToLevel}";

                _levelUpPanel.SetActive(true);
                yield return StartCoroutine(UIAnimationController.BounceReveal(_levelUpPanel.transform, 0.4f));
            }
        }

        // -------------------------------------------------------------------------
        // Button handlers
        // -------------------------------------------------------------------------

        private void OnHomeClicked()      => SceneLoader.LoadScene(SceneNames.MainMenu);
        private void OnNextLevelClicked() => SceneLoader.LoadScene(SceneNames.Level);
        private void OnReplayClicked()    => SceneLoader.LoadScene(SceneNames.Level);

        private void OnDestroy()
        {
            _homeButton?.onClick.RemoveListener(OnHomeClicked);
            _nextLevelButton?.onClick.RemoveListener(OnNextLevelClicked);
            _replayButton?.onClick.RemoveListener(OnReplayClicked);
        }
    }
}
