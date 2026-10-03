using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace KingSmash.Core
{
    public static class SceneNames
    {
        public const string Bootstrap    = "Bootstrap";
        public const string MainMenu     = "MainMenu";
        public const string Level        = "Level";
        public const string WorldMap     = "WorldMap";
        public const string LevelSelect  = "LevelSelect";
        public const string Shop         = "Shop";
        public const string Upgrade      = "Upgrade";
        public const string DailyRewards = "DailyRewards";
        public const string Missions     = "Missions";
        public const string Settings     = "Settings";
    }

    public static class SceneLoader
    {
        public static event Action<string> OnSceneLoadStarted;
        public static event Action<string> OnSceneLoaded;

        private static MonoBehaviour _coroutineRunner;

        public static void SetCoroutineRunner(MonoBehaviour runner) => _coroutineRunner = runner;

        public static void LoadScene(string sceneName)
        {
            GameLogger.Info("SceneLoader", $"Loading scene: {sceneName}");
            OnSceneLoadStarted?.Invoke(sceneName);
            SceneManager.LoadScene(sceneName);
            OnSceneLoaded?.Invoke(sceneName);
        }

        public static void LoadSceneAsync(string sceneName, Action onComplete = null)
        {
            if (_coroutineRunner == null)
            {
                GameLogger.Warning("SceneLoader", "No coroutine runner set, falling back to sync load.");
                LoadScene(sceneName);
                onComplete?.Invoke();
                return;
            }
            _coroutineRunner.StartCoroutine(LoadAsync(sceneName, onComplete));
        }

        private static IEnumerator LoadAsync(string sceneName, Action onComplete)
        {
            OnSceneLoadStarted?.Invoke(sceneName);
            var op = SceneManager.LoadSceneAsync(sceneName);
            while (!op.isDone) yield return null;
            OnSceneLoaded?.Invoke(sceneName);
            onComplete?.Invoke();
        }
    }
}
