// KingSmashSetup.cs — one-click project setup for local dev play.
// Run via: Tools → King Smash → Setup Project & Play
//
// What this does:
//   1. Sets KING_SMASH_DEV scripting define
//   2. Creates all required ScriptableObject assets in Resources/
//   3. Creates Bootstrap, MainMenu, and Level scenes
//   4. Wires GameObjects and component references in each scene
//   5. Adds all three scenes to Build Settings
//   6. Opens Bootstrap scene and enters Play mode

using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

using KingSmash.Core;
using KingSmash.UI;
using KingSmash.UI.Screens;
using KingSmash.Levels;
using KingSmash.Gameplay;
using KingSmash.Economy;
using KingSmash.Progression;
using KingSmash.Shop;
using KingSmash.Ads;
using KingSmash.Retention;

namespace KingSmash.Editor
{
    public static class KingSmashSetup
    {
        private const string ResourcesPath    = "Assets/Resources";
        private const string ScenesPath       = "Assets/Scenes";
        private const string BootstrapScene   = "Assets/Scenes/Bootstrap.unity";
        private const string MainMenuScene    = "Assets/Scenes/MainMenu.unity";
        private const string LevelScene       = "Assets/Scenes/Level.unity";

        [MenuItem("Tools/King Smash/Setup Project & Play", priority = 1)]
        public static void SetupAndPlay()
        {
            EditorUtility.DisplayProgressBar("King Smash Setup", "Starting...", 0f);
            try
            {
                SetScriptingDefine();
                EditorUtility.DisplayProgressBar("King Smash Setup", "Creating ScriptableObjects...", 0.15f);
                CreateScriptableObjects();
                EditorUtility.DisplayProgressBar("King Smash Setup", "Creating scenes...", 0.40f);
                CreateBootstrapScene();
                CreateMainMenuScene();
                CreateLevelScene();
                EditorUtility.DisplayProgressBar("King Smash Setup", "Configuring Build Settings...", 0.80f);
                SetBuildScenes();
                EditorUtility.DisplayProgressBar("King Smash Setup", "Opening Bootstrap scene...", 0.95f);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
                EditorSceneManager.OpenScene(BootstrapScene);
                EditorUtility.ClearProgressBar();
                Debug.Log("[KingSmashSetup] Setup complete — entering Play mode.");
                EditorApplication.isPlaying = true;
            }
            catch (System.Exception e)
            {
                EditorUtility.ClearProgressBar();
                Debug.LogError($"[KingSmashSetup] Setup failed: {e}");
                throw;
            }
        }

        [MenuItem("Tools/King Smash/Setup Only (no Play)", priority = 2)]
        public static void SetupOnly()
        {
            EditorUtility.DisplayProgressBar("King Smash Setup", "Starting...", 0f);
            try
            {
                SetScriptingDefine();
                EditorUtility.DisplayProgressBar("King Smash Setup", "Creating ScriptableObjects...", 0.2f);
                CreateScriptableObjects();
                EditorUtility.DisplayProgressBar("King Smash Setup", "Creating scenes...", 0.45f);
                CreateBootstrapScene();
                CreateMainMenuScene();
                CreateLevelScene();
                EditorUtility.DisplayProgressBar("King Smash Setup", "Configuring Build Settings...", 0.85f);
                SetBuildScenes();
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
                EditorUtility.ClearProgressBar();
                EditorSceneManager.OpenScene(BootstrapScene);
                Debug.Log("[KingSmashSetup] Setup complete. Press Play to run.");
                EditorUtility.DisplayDialog("King Smash Setup", "Setup complete!\n\nBootstrap scene is open. Press Play (▶) to run the game.", "OK");
            }
            catch (System.Exception e)
            {
                EditorUtility.ClearProgressBar();
                Debug.LogError($"[KingSmashSetup] Setup failed: {e}");
                throw;
            }
        }

        // ── Scripting defines ───────────────────────────────────────────────

        private static void SetScriptingDefine()
        {
            var target = BuildTargetGroup.Android;
            PlayerSettings.GetScriptingDefineSymbolsForGroup(target, out string[] existing);
            var list = new List<string>(existing);
            if (!list.Contains("KING_SMASH_DEV"))
            {
                list.Add("KING_SMASH_DEV");
                PlayerSettings.SetScriptingDefineSymbolsForGroup(target, list.ToArray());
                Debug.Log("[KingSmashSetup] Added KING_SMASH_DEV define.");
            }
            // Also set for Standalone so Play-in-Editor works
            var standaloneTarget = BuildTargetGroup.Standalone;
            PlayerSettings.GetScriptingDefineSymbolsForGroup(standaloneTarget, out string[] existingStandalone);
            var standaloneList = new List<string>(existingStandalone);
            if (!standaloneList.Contains("KING_SMASH_DEV"))
            {
                standaloneList.Add("KING_SMASH_DEV");
                PlayerSettings.SetScriptingDefineSymbolsForGroup(standaloneTarget, standaloneList.ToArray());
            }
        }

        // ── ScriptableObject creation ───────────────────────────────────────

        private static void CreateScriptableObjects()
        {
            EnsureDir(ResourcesPath);

            CreateAsset<GameConfig>("GameConfig");
            CreateAsset<EconomyConfig>("EconomyConfig");
            CreateAsset<KingUpgradeConfig>("KingUpgradeConfig");
            CreateAsset<AdConfiguration>("AdConfiguration");

            var catalog = CreateAsset<ShopProductCatalog>("ShopProductCatalog");
            if (catalog != null) catalog.InitializeDefaults();

            var missions = CreateAsset<MissionConfig>("MissionConfig");
            if (missions != null) missions.InitializeDefaults();

            var achievements = CreateAsset<AchievementConfig>("AchievementConfig");
            if (achievements != null) achievements.InitializeDefaults();

            var dailyRewards = CreateAsset<DailyRewardConfig>("DailyRewardConfig");
            if (dailyRewards != null) InitializeDailyRewards(dailyRewards);

            AssetDatabase.SaveAssets();
        }

        private static T CreateAsset<T>(string name) where T : ScriptableObject
        {
            string path = $"{ResourcesPath}/{name}.asset";
            if (File.Exists(path))
            {
                Debug.Log($"[KingSmashSetup] {name}.asset already exists — skipping.");
                return AssetDatabase.LoadAssetAtPath<T>(path);
            }
            var asset = ScriptableObject.CreateInstance<T>();
            AssetDatabase.CreateAsset(asset, path);
            Debug.Log($"[KingSmashSetup] Created {path}");
            return asset;
        }

        private static void InitializeDailyRewards(DailyRewardConfig config)
        {
            config.rewards = new List<DailyRewardEntry>
            {
                new DailyRewardEntry { day = 1, coinsReward = 200,  displayName = "Day 1" },
                new DailyRewardEntry { day = 2, coinsReward = 350,  displayName = "Day 2" },
                new DailyRewardEntry { day = 3, coinsReward = 500,  gemsReward = 5, displayName = "Day 3" },
                new DailyRewardEntry { day = 4, coinsReward = 700,  displayName = "Day 4" },
                new DailyRewardEntry { day = 5, coinsReward = 1000, displayName = "Day 5" },
                new DailyRewardEntry { day = 6, coinsReward = 1500, gemsReward = 10, displayName = "Day 6" },
                new DailyRewardEntry { day = 7, coinsReward = 2500, gemsReward = 20, isTreasureChest = true, displayName = "Day 7 — Treasure!" },
            };
            EditorUtility.SetDirty(config);
        }

        // ── Scene creation ─────────────────────────────────────────────────

        private static void CreateBootstrapScene()
        {
            EnsureDir(ScenesPath);
            if (File.Exists(BootstrapScene))
            {
                Debug.Log("[KingSmashSetup] Bootstrap.unity already exists — skipping.");
                return;
            }

            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            var go = new GameObject("GameBootstrap");
            var bootstrap = go.AddComponent<GameBootstrap>();

            var gameConfig = AssetDatabase.LoadAssetAtPath<GameConfig>($"{ResourcesPath}/GameConfig.asset");
            if (gameConfig != null)
            {
                var so = new SerializedObject(bootstrap);
                so.FindProperty("_gameConfig").objectReferenceValue = gameConfig;
                so.ApplyModifiedProperties();
            }

            EditorSceneManager.SaveScene(scene, BootstrapScene);
            Debug.Log("[KingSmashSetup] Created Bootstrap.unity");
        }

        private static void CreateMainMenuScene()
        {
            EnsureDir(ScenesPath);
            if (File.Exists(MainMenuScene))
            {
                Debug.Log("[KingSmashSetup] MainMenu.unity already exists — skipping.");
                return;
            }

            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            // Canvas
            var canvasGo = new GameObject("Canvas");
            var canvas = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasGo.AddComponent<UnityEngine.UI.CanvasScaler>();
            canvasGo.AddComponent<UnityEngine.UI.GraphicRaycaster>();

            // EventSystem
            var eventSystemGo = new GameObject("EventSystem");
            eventSystemGo.AddComponent<UnityEngine.EventSystems.EventSystem>();
            eventSystemGo.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();

            // ScreenManager on Canvas
            var screenManager = canvasGo.AddComponent<ScreenManager>();

            // HomeScreen child
            var homeGo = new GameObject("HomeScreen");
            homeGo.transform.SetParent(canvasGo.transform, false);
            var homeRect = homeGo.AddComponent<RectTransform>();
            homeRect.anchorMin = Vector2.zero;
            homeRect.anchorMax = Vector2.one;
            homeRect.offsetMin = Vector2.zero;
            homeRect.offsetMax = Vector2.zero;
            var homeScreen = homeGo.AddComponent<HomeScreen>();

            // SplashScreen child
            var splashGo = new GameObject("SplashScreen");
            splashGo.transform.SetParent(canvasGo.transform, false);
            var splashRect = splashGo.AddComponent<RectTransform>();
            splashRect.anchorMin = Vector2.zero;
            splashRect.anchorMax = Vector2.one;
            splashRect.offsetMin = Vector2.zero;
            splashRect.offsetMax = Vector2.zero;
            var splashScreen = splashGo.AddComponent<SplashScreen>();

            // Register screens in ScreenManager
            var so = new SerializedObject(screenManager);
            var screensProp = so.FindProperty("_screens");
            screensProp.arraySize = 2;
            screensProp.GetArrayElementAtIndex(0).objectReferenceValue = splashScreen;
            screensProp.GetArrayElementAtIndex(1).objectReferenceValue = homeScreen;
            so.ApplyModifiedProperties();

            EditorSceneManager.SaveScene(scene, MainMenuScene);
            Debug.Log("[KingSmashSetup] Created MainMenu.unity");
        }

        private static void CreateLevelScene()
        {
            EnsureDir(ScenesPath);
            if (File.Exists(LevelScene))
            {
                Debug.Log("[KingSmashSetup] Level.unity already exists — skipping.");
                return;
            }

            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            var launchControllerGo = new GameObject("LaunchController");
            var launchController = launchControllerGo.AddComponent<LaunchController>();

            var levelControllerGo = new GameObject("LevelController");
            var levelController = levelControllerGo.AddComponent<LevelController>();

            // Wire LaunchController into LevelController
            var so = new SerializedObject(levelController);
            so.FindProperty("_launchController").objectReferenceValue = launchController;
            so.ApplyModifiedProperties();

            EditorSceneManager.SaveScene(scene, LevelScene);
            Debug.Log("[KingSmashSetup] Created Level.unity");
        }

        // ── Build Settings ─────────────────────────────────────────────────

        private static void SetBuildScenes()
        {
            var scenes = new[]
            {
                new EditorBuildSettingsScene(BootstrapScene, true),
                new EditorBuildSettingsScene(MainMenuScene, true),
                new EditorBuildSettingsScene(LevelScene, true),
            };
            EditorBuildSettings.scenes = scenes;
            Debug.Log("[KingSmashSetup] Build Settings updated with 3 scenes.");
        }

        // ── Helpers ────────────────────────────────────────────────────────

        private static void EnsureDir(string path)
        {
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
        }
    }
}
