using System;
using UnityEngine;
using KingSmash.Core;

namespace KingSmash.UI
{
    public class PauseManager : MonoBehaviour
    {
        public static event Action OnPaused;
        public static event Action OnResumed;

        public static bool IsPaused => Time.timeScale == 0f;

        private void OnEnable() => GameManager.OnStateChanged += HandleStateChanged;
        private void OnDisable() => GameManager.OnStateChanged -= HandleStateChanged;

        private void HandleStateChanged(GameState prev, GameState next)
        {
            if (next == GameState.Paused)
            {
                Time.timeScale = 0f;
                OnPaused?.Invoke();
            }
            else if (prev == GameState.Paused)
            {
                Time.timeScale = 1f;
                OnResumed?.Invoke();
            }
        }

        public void Pause()  => GameManager.Instance?.TransitionTo(GameState.Paused);
        public void Resume() => GameManager.Instance?.TransitionTo(GameState.Gameplay);
    }
}
