using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using KingSmash.Core;
using KingSmash.Levels;
using KingSmash.Services;
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

        [Header("Buttons")]
        [SerializeField] private Button _homeButton;
        [SerializeField] private Button _nextLevelButton;
        [SerializeField] private Button _replayButton;

        private LevelResult _result;
        private int _currentLevelIndex;

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
        }

        private void OnDisable()
        {
            LevelController.OnLevelEnded -= HandleLevelEnded;
        }

        private void HandleLevelEnded(int score, int stars, float destructionRatio)
        {
            if (!GameManager.Instance || GameManager.Instance.CurrentState != GameState.LevelComplete) return;
            _result = new LevelResult
            {
                Stars            = stars,
                Score            = score,
                DestructionRatio = destructionRatio,
                IsVictory        = true,
                CoinsEarned      = stars * 25L
            };
            Show();
        }

        protected override void OnShow()
        {
            if (_result == null) return;
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

            // Populate stats
            if (_destructionLabel != null) _destructionLabel.text = $"{_result.DestructionRatio * 100f:F0}%";
            if (_enemiesLabel     != null) _enemiesLabel.text     = $"{_result.EnemiesDefeated}/{_result.TotalEnemies}";
            if (_queenLabel       != null) _queenLabel.text       = _result.QueenRescued ? "✓" : "✗";

            // Animate coin count-up
            if (_coinsEarnedLabel != null)
                yield return StartCoroutine(UIAnimationController.CountUp(_coinsEarnedLabel, 0, _result.CoinsEarned, 1.2f));
        }

        private void OnHomeClicked()       => SceneLoader.LoadScene(SceneNames.MainMenu);
        private void OnNextLevelClicked()  => SceneLoader.LoadScene(SceneNames.Level);
        private void OnReplayClicked()     => SceneLoader.LoadScene(SceneNames.Level);

        private void OnDestroy()
        {
            _homeButton?.onClick.RemoveListener(OnHomeClicked);
            _nextLevelButton?.onClick.RemoveListener(OnNextLevelClicked);
            _replayButton?.onClick.RemoveListener(OnReplayClicked);
        }
    }
}
