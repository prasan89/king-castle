using System.Collections.Generic;
namespace KingSmash.Levels
{
    public static class WorldRegistry
    {
        public static IReadOnlyList<WorldDefinition> All { get; } = BuildAll();

        public static WorldDefinition GetWorld(int worldIndex)
        {
            foreach (var w in All)
                if (w.WorldIndex == worldIndex) return w;
            return null;
        }

        public static WorldDefinition GetWorldForLevel(int levelIndex0Based)
        {
            foreach (var w in All)
                if (w.ContainsLevel(levelIndex0Based)) return w;
            return null;
        }

        public static bool IsBossLevel(int levelIndex0Based)
        {
            foreach (var w in All)
                if (w.BossLevelIndex == levelIndex0Based) return true;
            return false;
        }

        private static List<WorldDefinition> BuildAll() => new List<WorldDefinition>
        {
            new WorldDefinition
            {
                WorldIndex            = 0,
                DisplayName           = "Forest Kingdom",
                Description           = "A lush medieval kingdom under siege. Master the basics of royal smashing.",
                LevelStart            = 0,
                LevelEnd              = 19,
                BossLevelIndex        = 19,
                RequiredStarsToUnlock = 0,
                Environment           = WorldEnvironment.Forest,
                BackgroundKey         = "bg_forest",
                MusicKey              = "music_forest",
                CastleThemeKey        = "castle_wood_stone",
                AvailableMaterials    = new List<string> { "Wood", "Stone" },
                AvailableEnemies      = new List<string> { "Guard", "ShieldGuard", "Archer", "EnemyKing" },
                NewMechanics          = new List<string> { "Basic launch", "Structural collapse", "Explosive barrels", "Shield breaking" },
                DifficultyMin         = 1,
                DifficultyMax         = 5,
            },
            new WorldDefinition
            {
                WorldIndex            = 1,
                DisplayName           = "Desert Fortress",
                Description           = "Ancient sandstone fortresses rise from the desert. Learn to use the environment.",
                LevelStart            = 20,
                LevelEnd              = 39,
                BossLevelIndex        = 39,
                RequiredStarsToUnlock = 30,
                Environment           = WorldEnvironment.Desert,
                BackgroundKey         = "bg_desert",
                MusicKey              = "music_desert",
                CastleThemeKey        = "castle_sandstone",
                AvailableMaterials    = new List<string> { "Wood", "Stone", "Sandstone" },
                AvailableEnemies      = new List<string> { "Guard", "ShieldGuard", "Archer", "EnemyKing" },
                NewMechanics          = new List<string> { "Sandstone material", "Sand collapse", "Canyon ricochet", "Siege towers" },
                DifficultyMin         = 4,
                DifficultyMax         = 7,
            },
            new WorldDefinition
            {
                WorldIndex            = 2,
                DisplayName           = "Ice Kingdom",
                Description           = "Frozen castles with slippery physics. Timing and precision are everything.",
                LevelStart            = 40,
                LevelEnd              = 59,
                BossLevelIndex        = 59,
                RequiredStarsToUnlock = 70,
                Environment           = WorldEnvironment.Ice,
                BackgroundKey         = "bg_ice",
                MusicKey              = "music_ice",
                CastleThemeKey        = "castle_ice_stone",
                AvailableMaterials    = new List<string> { "Wood", "Stone", "Ice" },
                AvailableEnemies      = new List<string> { "Guard", "ShieldGuard", "Archer", "EnemyKing" },
                NewMechanics          = new List<string> { "Ice material", "Slippery surfaces", "Unstable frozen towers", "Chain slide collapse" },
                DifficultyMin         = 6,
                DifficultyMax         = 8,
            },
            new WorldDefinition
            {
                WorldIndex            = 3,
                DisplayName           = "Dark Realm",
                Description           = "Metal fortresses guarded by elite soldiers. Brute force alone won't work.",
                LevelStart            = 60,
                LevelEnd              = 79,
                BossLevelIndex        = 79,
                RequiredStarsToUnlock = 130,
                Environment           = WorldEnvironment.Dark,
                BackgroundKey         = "bg_dark",
                MusicKey              = "music_dark",
                CastleThemeKey        = "castle_metal_dark",
                AvailableMaterials    = new List<string> { "Stone", "Metal", "Wood" },
                AvailableEnemies      = new List<string> { "Guard", "ShieldGuard", "Archer", "EnemyKing" },
                NewMechanics          = new List<string> { "Metal structures", "Multi-section castles", "Advanced explosive chains", "Elite enemy placement" },
                DifficultyMin         = 7,
                DifficultyMax         = 9,
            },
            new WorldDefinition
            {
                WorldIndex            = 4,
                DisplayName           = "Final Castle",
                Description           = "The Enemy King's ultimate fortress. Everything you have learned leads to this.",
                LevelStart            = 80,
                LevelEnd              = 99,
                BossLevelIndex        = 99,
                RequiredStarsToUnlock = 200,
                Environment           = WorldEnvironment.Final,
                BackgroundKey         = "bg_final",
                MusicKey              = "music_final_boss",
                CastleThemeKey        = "castle_final",
                AvailableMaterials    = new List<string> { "Wood", "Stone", "Metal", "Sandstone", "Ice" },
                AvailableEnemies      = new List<string> { "Guard", "ShieldGuard", "Archer", "EnemyKing" },
                NewMechanics          = new List<string> { "All previous mechanics combined", "Multi-tower fortress", "Protected queen chamber", "Final battle" },
                DifficultyMin         = 9,
                DifficultyMax         = 10,
            },
        };
    }
}
