using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using KingSmash.Core;
using KingSmash.Levels;
using KingSmash.Services;
namespace KingSmash.UI.Screens
{
    public class WorldCompleteScreen : UIScreen
    {
        [Header("World Info")]
        [SerializeField] private TextMeshProUGUI _worldNameLabel;
        [SerializeField] private TextMeshProUGUI _completionMessage;
        [SerializeField] private TextMeshProUGUI _starsLabel;
        [SerializeField] private TextMeshProUGUI _nextWorldLabel;
        [SerializeField] private GameObject      _nextWorldPanel;

        [Header("Buttons")]
        [SerializeField] private Button _continueButton;
        [SerializeField] private Button _homeButton;

        private int _completedWorldIndex;

        protected override void Awake()
        {
            base.Awake();
            _continueButton?.onClick.AddListener(OnContinueClicked);
            _homeButton?.onClick.AddListener(OnHomeClicked);
        }

        private void OnEnable()
        {
            WorldProgressionService.OnWorldCompleted += HandleWorldCompleted;
            WorldProgressionService.OnWorldUnlocked  += HandleWorldUnlocked;
        }

        private void OnDisable()
        {
            WorldProgressionService.OnWorldCompleted -= HandleWorldCompleted;
            WorldProgressionService.OnWorldUnlocked  -= HandleWorldUnlocked;
        }

        private void HandleWorldCompleted(int worldIndex)
        {
            _completedWorldIndex = worldIndex;
            var world = WorldRegistry.GetWorld(worldIndex);
            if (world == null) return;

            ServiceLocator.TryGet<ISaveService>(out var save);
            int starsEarned = 0;
            if (save != null)
                for (int i = world.LevelStart; i <= world.LevelEnd; i++)
                    starsEarned += save.Current.GetStarsForLevel(i);

            if (_worldNameLabel    != null) _worldNameLabel.text    = $"{world.DisplayName} Complete!";
            if (_completionMessage != null) _completionMessage.text = $"{world.LevelsPerWorld} / {world.LevelsPerWorld} Levels";
            if (_starsLabel        != null) _starsLabel.text        = $"{starsEarned} / {world.LevelsPerWorld * 3} ⭐";

            _nextWorldPanel?.SetActive(false);

            // Check if world 5 complete (final)
            if (worldIndex == 4)
            {
                if (_completionMessage != null) _completionMessage.text = "The campaign is complete!\nThe King has returned!";
            }

            Show();
            StartCoroutine(UIAnimationController.BounceReveal(transform, 0.4f));
        }

        private void HandleWorldUnlocked(int worldIndex)
        {
            var nextWorld = WorldRegistry.GetWorld(worldIndex);
            if (nextWorld == null) return;
            _nextWorldPanel?.SetActive(true);
            if (_nextWorldLabel != null) _nextWorldLabel.text = $"New World Unlocked:\n{nextWorld.DisplayName}";
        }

        private void OnContinueClicked()
        {
            // Advance to first level of next world (or world map if final)
            var nextWorld = WorldRegistry.GetWorld(_completedWorldIndex + 1);
            if (nextWorld != null)
                ScreenManager.Instance.Show<WorldMapScreen>();
            else
                ScreenManager.Instance.Show<HomeScreen>();
        }

        private void OnHomeClicked() => SceneLoader.LoadScene(SceneNames.MainMenu);

        private void OnDestroy()
        {
            _continueButton?.onClick.RemoveListener(OnContinueClicked);
            _homeButton?.onClick.RemoveListener(OnHomeClicked);
        }
    }
}
