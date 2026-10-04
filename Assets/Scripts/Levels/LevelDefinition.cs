namespace KingSmash.Levels
{
    public class LevelDefinition
    {
        public int    LevelIndex;
        public int    WorldIndex;
        public string DisplayName;
        public string Description;
        public int    KingLaunches;
        public int    DifficultyRating;
        public int    RequiredStarsToUnlock;
        public string SceneName;

        public int    RequiredScore;
        public int    TwoStarScore;
        public int    ThreeStarScore;
        public bool   MustRescueQueen;

        public float  OneStar_DestructionMin;
        public float  TwoStar_DestructionMin;
        public float  TwoStar_EnemyRatioMin;
        public float  ThreeStar_DestructionMin;
        public float  ThreeStar_EnemyRatioMin;
        public int    ThreeStar_AttemptsRemaining;

        public int    GuardCount;
        public int    ShieldGuardCount;
        public int    ArcherCount;
        public bool   HasEnemyKing;
        public bool   HasExplosiveBarrel;
        public bool   HasTutorialHints;

        public string PrimaryMaterial;
        public string SecondaryMaterial;
        public int    CastleFloors;
        public bool   HasQueen;
        public bool   IsBossLevel;

        public int    MaxPowerUpsAllowed = 2;
    }
}
