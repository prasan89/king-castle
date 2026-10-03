using System;
using UnityEngine;
using KingSmash.Services;

namespace KingSmash.Core
{
    public enum GameState { Boot, MainMenu, Gameplay, Paused, LevelComplete, LevelFailed, Upgrading }

    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        public GameState CurrentState { get; private set; } = GameState.Boot;

        public static event Action<GameState, GameState> OnStateChanged;

        private void Awake()
        {
            if (Instance != null) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        public void TransitionTo(GameState newState)
        {
            if (CurrentState == newState) return;
            var prev = CurrentState;
            CurrentState = newState;
            GameLogger.Info("GameManager", $"State: {prev} -> {newState}");
            OnStateChanged?.Invoke(prev, newState);
        }

        public void StartLevel(int levelIndex)
        {
            TransitionTo(GameState.Gameplay);
            ServiceLocator.Get<IAnalyticsService>().LogEvent(AnalyticsEvents.LevelStart,
                ("level_index", levelIndex));
        }

        public void CompleteLevel(int levelIndex, int starsEarned)
        {
            TransitionTo(GameState.LevelComplete);
            ServiceLocator.Get<IAnalyticsService>().LogEvent(AnalyticsEvents.LevelComplete,
                ("level_index", levelIndex), ("stars", starsEarned));
        }

        public void FailLevel(int levelIndex, string reason)
        {
            TransitionTo(GameState.LevelFailed);
            ServiceLocator.Get<IAnalyticsService>().LogEvent(AnalyticsEvents.LevelFailed,
                ("level_index", levelIndex), ("reason", reason));
        }
    }
}
