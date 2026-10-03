using System.Collections.Generic;
using UnityEngine;  // needed for Mathf
namespace KingSmash.Levels
{
    // Pure data record for a level definition — does NOT depend on ScriptableObject.
    public class LevelDefinition
    {
        public int    LevelIndex;
        public int    WorldIndex;
        public string DisplayName;
        public string Description;
        public int    KingLaunches;
        public int    DifficultyRating;       // 1-10
        public int    RequiredStarsToUnlock;
        public string SceneName;

        // Scoring
        public int    RequiredScore;
        public int    TwoStarScore;
        public int    ThreeStarScore;
        public bool   MustRescueQueen;

        // Star thresholds (dimensional)
        public float  OneStar_DestructionMin;
        public float  TwoStar_DestructionMin;
        public float  TwoStar_EnemyRatioMin;
        public float  ThreeStar_DestructionMin;
        public float  ThreeStar_EnemyRatioMin;
        public int    ThreeStar_AttemptsRemaining;

        // Enemies
        public int    GuardCount;
        public int    ShieldGuardCount;
        public int    ArcherCount;
        public bool   HasEnemyKing;
        public bool   HasExplosiveBarrel;
        public bool   HasTutorialHints;

        // Castle
        public string PrimaryMaterial;   // "Wood", "Stone", "Metal"
        public string SecondaryMaterial;
        public int    CastleFloors;
        public bool   HasQueen;
    }

    public static class LevelConfigFactory
    {
        public static IReadOnlyList<LevelDefinition> All { get; } = BuildAll();

        private static List<LevelDefinition> BuildAll()
        {
            return new List<LevelDefinition>
            {
                // ─── Level 1: First Smash ───────────────────────────────
                new LevelDefinition
                {
                    LevelIndex            = 0,
                    WorldIndex            = 0,
                    DisplayName           = "First Smash",
                    Description           = "Rescue the Queen from the small wooden castle.",
                    KingLaunches          = 5,
                    DifficultyRating      = 1,
                    RequiredStarsToUnlock = 0,
                    SceneName             = "Level",

                    RequiredScore  = 300,
                    TwoStarScore   = 600,
                    ThreeStarScore = 900,
                    MustRescueQueen= true,

                    OneStar_DestructionMin     = 0.30f,
                    TwoStar_DestructionMin     = 0.60f,
                    TwoStar_EnemyRatioMin      = 0.50f,
                    ThreeStar_DestructionMin   = 0.80f,
                    ThreeStar_EnemyRatioMin    = 1.00f,
                    ThreeStar_AttemptsRemaining= 2,

                    GuardCount         = 1,
                    ShieldGuardCount   = 0,
                    ArcherCount        = 0,
                    HasEnemyKing       = false,
                    HasExplosiveBarrel = false,
                    HasTutorialHints   = true,
                    PrimaryMaterial    = "Wood",
                    SecondaryMaterial  = "",
                    CastleFloors       = 1,
                    HasQueen           = true,
                },

                // ─── Level 2: Tall Castle ──────────────────────────────
                new LevelDefinition
                {
                    LevelIndex            = 1,
                    WorldIndex            = 0,
                    DisplayName           = "Tall Castle",
                    Description           = "Knock down the tall wooden tower and rescue the Queen.",
                    KingLaunches          = 4,
                    DifficultyRating      = 2,
                    RequiredStarsToUnlock = 1,
                    SceneName             = "Level",

                    RequiredScore  = 400,
                    TwoStarScore   = 700,
                    ThreeStarScore = 1100,
                    MustRescueQueen= true,

                    OneStar_DestructionMin     = 0.35f,
                    TwoStar_DestructionMin     = 0.65f,
                    TwoStar_EnemyRatioMin      = 0.50f,
                    ThreeStar_DestructionMin   = 0.85f,
                    ThreeStar_EnemyRatioMin    = 1.00f,
                    ThreeStar_AttemptsRemaining= 1,

                    GuardCount         = 2,
                    ShieldGuardCount   = 0,
                    ArcherCount        = 0,
                    HasEnemyKing       = false,
                    HasExplosiveBarrel = false,
                    HasTutorialHints   = false,
                    PrimaryMaterial    = "Wood",
                    SecondaryMaterial  = "",
                    CastleFloors       = 2,
                    HasQueen           = true,
                },

                // ─── Level 3: Stone ────────────────────────────────────
                new LevelDefinition
                {
                    LevelIndex            = 2,
                    WorldIndex            = 0,
                    DisplayName           = "Stone Walls",
                    Description           = "Stone blocks require more force. Aim carefully!",
                    KingLaunches          = 4,
                    DifficultyRating      = 3,
                    RequiredStarsToUnlock = 2,
                    SceneName             = "Level",

                    RequiredScore  = 500,
                    TwoStarScore   = 900,
                    ThreeStarScore = 1400,
                    MustRescueQueen= true,

                    OneStar_DestructionMin     = 0.30f,
                    TwoStar_DestructionMin     = 0.60f,
                    TwoStar_EnemyRatioMin      = 0.67f,
                    ThreeStar_DestructionMin   = 0.85f,
                    ThreeStar_EnemyRatioMin    = 1.00f,
                    ThreeStar_AttemptsRemaining= 1,

                    GuardCount         = 3,
                    ShieldGuardCount   = 0,
                    ArcherCount        = 0,
                    HasEnemyKing       = false,
                    HasExplosiveBarrel = false,
                    HasTutorialHints   = false,
                    PrimaryMaterial    = "Wood",
                    SecondaryMaterial  = "Stone",
                    CastleFloors       = 2,
                    HasQueen           = true,
                },

                // ─── Level 4: Shield Guard ─────────────────────────────
                new LevelDefinition
                {
                    LevelIndex            = 3,
                    WorldIndex            = 0,
                    DisplayName           = "Shielded Fortress",
                    Description           = "Break the shield guard's defense and rescue the Queen.",
                    KingLaunches          = 4,
                    DifficultyRating      = 4,
                    RequiredStarsToUnlock = 4,
                    SceneName             = "Level",

                    RequiredScore  = 600,
                    TwoStarScore   = 1000,
                    ThreeStarScore = 1600,
                    MustRescueQueen= true,

                    OneStar_DestructionMin     = 0.30f,
                    TwoStar_DestructionMin     = 0.60f,
                    TwoStar_EnemyRatioMin      = 0.75f,
                    ThreeStar_DestructionMin   = 0.80f,
                    ThreeStar_EnemyRatioMin    = 1.00f,
                    ThreeStar_AttemptsRemaining= 1,

                    GuardCount         = 2,
                    ShieldGuardCount   = 1,
                    ArcherCount        = 0,
                    HasEnemyKing       = false,
                    HasExplosiveBarrel = false,
                    HasTutorialHints   = false,
                    PrimaryMaterial    = "Wood",
                    SecondaryMaterial  = "Stone",
                    CastleFloors       = 2,
                    HasQueen           = true,
                },

                // ─── Level 5: Explosive Barrel ─────────────────────────
                new LevelDefinition
                {
                    LevelIndex            = 4,
                    WorldIndex            = 0,
                    DisplayName           = "Powder Keg",
                    Description           = "Use the barrels! One explosion can bring the whole castle down.",
                    KingLaunches          = 3,
                    DifficultyRating      = 4,
                    RequiredStarsToUnlock = 5,
                    SceneName             = "Level",

                    RequiredScore  = 700,
                    TwoStarScore   = 1200,
                    ThreeStarScore = 1800,
                    MustRescueQueen= true,

                    OneStar_DestructionMin     = 0.35f,
                    TwoStar_DestructionMin     = 0.65f,
                    TwoStar_EnemyRatioMin      = 0.50f,
                    ThreeStar_DestructionMin   = 0.85f,
                    ThreeStar_EnemyRatioMin    = 1.00f,
                    ThreeStar_AttemptsRemaining= 1,

                    GuardCount         = 2,
                    ShieldGuardCount   = 1,
                    ArcherCount        = 0,
                    HasEnemyKing       = false,
                    HasExplosiveBarrel = true,
                    HasTutorialHints   = false,
                    PrimaryMaterial    = "Wood",
                    SecondaryMaterial  = "Stone",
                    CastleFloors       = 2,
                    HasQueen           = true,
                },

                // ─── Level 6: Multi-Floor Castle ───────────────────────
                new LevelDefinition
                {
                    LevelIndex            = 5,
                    WorldIndex            = 0,
                    DisplayName           = "Tower of Doom",
                    Description           = "The Queen is at the top. Bring down the lower floors first!",
                    KingLaunches          = 4,
                    DifficultyRating      = 5,
                    RequiredStarsToUnlock = 7,
                    SceneName             = "Level",

                    RequiredScore  = 800,
                    TwoStarScore   = 1400,
                    ThreeStarScore = 2000,
                    MustRescueQueen= true,

                    OneStar_DestructionMin     = 0.30f,
                    TwoStar_DestructionMin     = 0.60f,
                    TwoStar_EnemyRatioMin      = 0.67f,
                    ThreeStar_DestructionMin   = 0.85f,
                    ThreeStar_EnemyRatioMin    = 1.00f,
                    ThreeStar_AttemptsRemaining= 1,

                    GuardCount         = 2,
                    ShieldGuardCount   = 1,
                    ArcherCount        = 0,
                    HasEnemyKing       = false,
                    HasExplosiveBarrel = false,
                    HasTutorialHints   = false,
                    PrimaryMaterial    = "Stone",
                    SecondaryMaterial  = "Wood",
                    CastleFloors       = 3,
                    HasQueen           = true,
                },

                // ─── Level 7: Archer Castle ────────────────────────────
                new LevelDefinition
                {
                    LevelIndex            = 6,
                    WorldIndex            = 0,
                    DisplayName           = "Arrow Tower",
                    Description           = "Watch out for the Archer in the tower. Clear the path!",
                    KingLaunches          = 4,
                    DifficultyRating      = 6,
                    RequiredStarsToUnlock = 9,
                    SceneName             = "Level",

                    RequiredScore  = 900,
                    TwoStarScore   = 1600,
                    ThreeStarScore = 2400,
                    MustRescueQueen= true,

                    OneStar_DestructionMin     = 0.30f,
                    TwoStar_DestructionMin     = 0.60f,
                    TwoStar_EnemyRatioMin      = 0.67f,
                    ThreeStar_DestructionMin   = 0.85f,
                    ThreeStar_EnemyRatioMin    = 1.00f,
                    ThreeStar_AttemptsRemaining= 1,

                    GuardCount         = 2,
                    ShieldGuardCount   = 0,
                    ArcherCount        = 1,
                    HasEnemyKing       = false,
                    HasExplosiveBarrel = false,
                    HasTutorialHints   = false,
                    PrimaryMaterial    = "Wood",
                    SecondaryMaterial  = "Stone",
                    CastleFloors       = 2,
                    HasQueen           = true,
                },

                // ─── Level 8: Trap Castle ──────────────────────────────
                new LevelDefinition
                {
                    LevelIndex            = 7,
                    WorldIndex            = 0,
                    DisplayName           = "Trap Fortress",
                    Description           = "Barrels everywhere. One wrong hit and the whole place goes up!",
                    KingLaunches          = 3,
                    DifficultyRating      = 7,
                    RequiredStarsToUnlock = 11,
                    SceneName             = "Level",

                    RequiredScore  = 1000,
                    TwoStarScore   = 1800,
                    ThreeStarScore = 2700,
                    MustRescueQueen= true,

                    OneStar_DestructionMin     = 0.35f,
                    TwoStar_DestructionMin     = 0.65f,
                    TwoStar_EnemyRatioMin      = 0.67f,
                    ThreeStar_DestructionMin   = 0.90f,
                    ThreeStar_EnemyRatioMin    = 1.00f,
                    ThreeStar_AttemptsRemaining= 1,

                    GuardCount         = 2,
                    ShieldGuardCount   = 1,
                    ArcherCount        = 1,
                    HasEnemyKing       = false,
                    HasExplosiveBarrel = true,
                    HasTutorialHints   = false,
                    PrimaryMaterial    = "Stone",
                    SecondaryMaterial  = "Wood",
                    CastleFloors       = 2,
                    HasQueen           = true,
                },

                // ─── Level 9: Enemy King ───────────────────────────────
                new LevelDefinition
                {
                    LevelIndex            = 8,
                    WorldIndex            = 0,
                    DisplayName           = "The Enemy King",
                    Description           = "Defeat the Enemy King to rescue the Queen from his fortress.",
                    KingLaunches          = 4,
                    DifficultyRating      = 8,
                    RequiredStarsToUnlock = 13,
                    SceneName             = "Level",

                    RequiredScore  = 1200,
                    TwoStarScore   = 2000,
                    ThreeStarScore = 3000,
                    MustRescueQueen= true,

                    OneStar_DestructionMin     = 0.30f,
                    TwoStar_DestructionMin     = 0.55f,
                    TwoStar_EnemyRatioMin      = 0.75f,
                    ThreeStar_DestructionMin   = 0.80f,
                    ThreeStar_EnemyRatioMin    = 1.00f,
                    ThreeStar_AttemptsRemaining= 1,

                    GuardCount         = 2,
                    ShieldGuardCount   = 1,
                    ArcherCount        = 0,
                    HasEnemyKing       = true,
                    HasExplosiveBarrel = false,
                    HasTutorialHints   = false,
                    PrimaryMaterial    = "Stone",
                    SecondaryMaterial  = "Metal",
                    CastleFloors       = 3,
                    HasQueen           = true,
                },

                // ─── Level 10: Mini Fortress ────────────────────────────
                new LevelDefinition
                {
                    LevelIndex            = 9,
                    WorldIndex            = 0,
                    DisplayName           = "Mini Fortress",
                    Description           = "The ultimate challenge. Every skill you have learned is needed.",
                    KingLaunches          = 5,
                    DifficultyRating      = 10,
                    RequiredStarsToUnlock = 15,
                    SceneName             = "Level",

                    RequiredScore  = 1500,
                    TwoStarScore   = 2500,
                    ThreeStarScore = 3800,
                    MustRescueQueen= true,

                    OneStar_DestructionMin     = 0.30f,
                    TwoStar_DestructionMin     = 0.60f,
                    TwoStar_EnemyRatioMin      = 0.60f,
                    ThreeStar_DestructionMin   = 0.85f,
                    ThreeStar_EnemyRatioMin    = 1.00f,
                    ThreeStar_AttemptsRemaining= 1,

                    GuardCount         = 2,
                    ShieldGuardCount   = 1,
                    ArcherCount        = 1,
                    HasEnemyKing       = true,
                    HasExplosiveBarrel = true,
                    HasTutorialHints   = false,
                    PrimaryMaterial    = "Stone",
                    SecondaryMaterial  = "Metal",
                    CastleFloors       = 3,
                    HasQueen           = true,
                },
            };
        }
    }
}
