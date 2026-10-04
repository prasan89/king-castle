using UnityEngine;
using KingSmash.Core;
using KingSmash.Levels;
using KingSmash.Save;
using KingSmash.Services;

namespace KingSmash.World
{
    public class LevelAtmosphereInitializer : MonoBehaviour
    {
        [SerializeField] private WorldThemeDatabase   _themeDatabase;
        [SerializeField] private WorldThemeApplicator _applicator;

        private void Start()
        {
            if (_themeDatabase == null)
            {
                GameLogger.Warning("LevelAtmosphereInitializer", "WorldThemeDatabase not assigned.");
                return;
            }

            if (_applicator == null)
            {
                GameLogger.Warning("LevelAtmosphereInitializer", "WorldThemeApplicator not assigned.");
                return;
            }

            int worldIndex = ResolveWorldIndex();
            if (worldIndex <= 0)
            {
                GameLogger.Warning("LevelAtmosphereInitializer", "Could not determine world index — defaulting to 1.");
                worldIndex = 1;
            }

            var theme = _themeDatabase.GetTheme(worldIndex);
            if (theme == null)
            {
                GameLogger.Warning("LevelAtmosphereInitializer",
                    $"No WorldTheme found for world index {worldIndex}.");
                return;
            }

            _applicator.SetTheme(theme);
        }

        private int ResolveWorldIndex()
        {
            var lastResult = LevelProgressionService.LastResult;
            if (lastResult != null && lastResult.LevelIndex >= 0)
            {
                var world = WorldRegistry.GetWorldForLevel(lastResult.LevelIndex);
                if (world != null) return world.WorldIndex;
            }

            var levelController = FindFirstObjectByType<LevelController>();
            if (levelController != null)
            {
                var configField = typeof(LevelController).GetField(
                    "_levelConfig",
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

                if (configField != null)
                {
                    var config = configField.GetValue(levelController) as KingSmash.Levels.LevelConfig;
                    if (config != null && config.worldIndex > 0)
                        return config.worldIndex;
                }
            }

            if (ServiceLocator.TryGet<ISaveService>(out var save))
            {
                int currentLevel = save.Current.currentLevel;
                var world = WorldRegistry.GetWorldForLevel(currentLevel);
                if (world != null) return world.WorldIndex;
            }

            return 1;
        }
    }
}
