using System.Collections.Generic;

namespace KingSmash.Levels
{
    public static class LevelConfigFactory
    {
        public static IReadOnlyList<LevelDefinition> All { get; } = BuildAll();

        private static List<LevelDefinition> BuildAll()
        {
            return new List<LevelDefinition>
            {
                // ── WORLD 0: FOREST (L0-19) ──────────────────────────────

                new LevelDefinition
                {
                    LevelIndex=0, WorldIndex=0, DisplayName="First Smash",
                    Description="Rescue the Queen from the small wooden castle.",
                    KingLaunches=5, DifficultyRating=1, RequiredStarsToUnlock=0, SceneName="Level",
                    RequiredScore=300, TwoStarScore=600, ThreeStarScore=900, MustRescueQueen=true,
                    OneStar_DestructionMin=0.30f, TwoStar_DestructionMin=0.60f, TwoStar_EnemyRatioMin=0.50f,
                    ThreeStar_DestructionMin=0.80f, ThreeStar_EnemyRatioMin=1.00f, ThreeStar_AttemptsRemaining=2,
                    GuardCount=1, ShieldGuardCount=0, ArcherCount=0, HasEnemyKing=false, HasExplosiveBarrel=false, HasTutorialHints=true,
                    PrimaryMaterial="Wood", SecondaryMaterial="", CastleFloors=1, HasQueen=true,
                },
                new LevelDefinition
                {
                    LevelIndex=1, WorldIndex=0, DisplayName="Tall Castle",
                    Description="Knock down the tall wooden tower and rescue the Queen.",
                    KingLaunches=4, DifficultyRating=2, RequiredStarsToUnlock=1, SceneName="Level",
                    RequiredScore=400, TwoStarScore=700, ThreeStarScore=1100, MustRescueQueen=true,
                    OneStar_DestructionMin=0.35f, TwoStar_DestructionMin=0.65f, TwoStar_EnemyRatioMin=0.50f,
                    ThreeStar_DestructionMin=0.85f, ThreeStar_EnemyRatioMin=1.00f, ThreeStar_AttemptsRemaining=1,
                    GuardCount=2, ShieldGuardCount=0, ArcherCount=0, HasEnemyKing=false, HasExplosiveBarrel=false, HasTutorialHints=false,
                    PrimaryMaterial="Wood", SecondaryMaterial="", CastleFloors=2, HasQueen=true,
                },
                new LevelDefinition
                {
                    LevelIndex=2, WorldIndex=0, DisplayName="Stone Walls",
                    Description="Stone blocks require more force. Aim carefully!",
                    KingLaunches=4, DifficultyRating=3, RequiredStarsToUnlock=2, SceneName="Level",
                    RequiredScore=500, TwoStarScore=900, ThreeStarScore=1400, MustRescueQueen=true,
                    OneStar_DestructionMin=0.30f, TwoStar_DestructionMin=0.60f, TwoStar_EnemyRatioMin=0.67f,
                    ThreeStar_DestructionMin=0.85f, ThreeStar_EnemyRatioMin=1.00f, ThreeStar_AttemptsRemaining=1,
                    GuardCount=3, ShieldGuardCount=0, ArcherCount=0, HasEnemyKing=false, HasExplosiveBarrel=false, HasTutorialHints=false,
                    PrimaryMaterial="Wood", SecondaryMaterial="Stone", CastleFloors=2, HasQueen=true,
                },
                new LevelDefinition
                {
                    LevelIndex=3, WorldIndex=0, DisplayName="Shielded Fortress",
                    Description="Break the shield guard's defense and rescue the Queen.",
                    KingLaunches=4, DifficultyRating=4, RequiredStarsToUnlock=4, SceneName="Level",
                    RequiredScore=600, TwoStarScore=1000, ThreeStarScore=1600, MustRescueQueen=true,
                    OneStar_DestructionMin=0.30f, TwoStar_DestructionMin=0.60f, TwoStar_EnemyRatioMin=0.75f,
                    ThreeStar_DestructionMin=0.80f, ThreeStar_EnemyRatioMin=1.00f, ThreeStar_AttemptsRemaining=1,
                    GuardCount=2, ShieldGuardCount=1, ArcherCount=0, HasEnemyKing=false, HasExplosiveBarrel=false, HasTutorialHints=false,
                    PrimaryMaterial="Wood", SecondaryMaterial="Stone", CastleFloors=2, HasQueen=true,
                },
                new LevelDefinition
                {
                    LevelIndex=4, WorldIndex=0, DisplayName="Powder Keg",
                    Description="Use the barrels! One explosion can bring the whole castle down.",
                    KingLaunches=3, DifficultyRating=4, RequiredStarsToUnlock=5, SceneName="Level",
                    RequiredScore=700, TwoStarScore=1200, ThreeStarScore=1800, MustRescueQueen=true,
                    OneStar_DestructionMin=0.35f, TwoStar_DestructionMin=0.65f, TwoStar_EnemyRatioMin=0.50f,
                    ThreeStar_DestructionMin=0.85f, ThreeStar_EnemyRatioMin=1.00f, ThreeStar_AttemptsRemaining=1,
                    GuardCount=2, ShieldGuardCount=1, ArcherCount=0, HasEnemyKing=false, HasExplosiveBarrel=true, HasTutorialHints=false,
                    PrimaryMaterial="Wood", SecondaryMaterial="Stone", CastleFloors=2, HasQueen=true,
                },
                new LevelDefinition
                {
                    LevelIndex=5, WorldIndex=0, DisplayName="Tower of Doom",
                    Description="The Queen is at the top. Bring down the lower floors first!",
                    KingLaunches=4, DifficultyRating=5, RequiredStarsToUnlock=7, SceneName="Level",
                    RequiredScore=800, TwoStarScore=1400, ThreeStarScore=2000, MustRescueQueen=true,
                    OneStar_DestructionMin=0.30f, TwoStar_DestructionMin=0.60f, TwoStar_EnemyRatioMin=0.67f,
                    ThreeStar_DestructionMin=0.85f, ThreeStar_EnemyRatioMin=1.00f, ThreeStar_AttemptsRemaining=1,
                    GuardCount=2, ShieldGuardCount=1, ArcherCount=0, HasEnemyKing=false, HasExplosiveBarrel=false, HasTutorialHints=false,
                    PrimaryMaterial="Stone", SecondaryMaterial="Wood", CastleFloors=3, HasQueen=true,
                },
                new LevelDefinition
                {
                    LevelIndex=6, WorldIndex=0, DisplayName="Arrow Tower",
                    Description="Watch out for the Archer in the tower. Clear the path!",
                    KingLaunches=4, DifficultyRating=6, RequiredStarsToUnlock=9, SceneName="Level",
                    RequiredScore=900, TwoStarScore=1600, ThreeStarScore=2400, MustRescueQueen=true,
                    OneStar_DestructionMin=0.30f, TwoStar_DestructionMin=0.60f, TwoStar_EnemyRatioMin=0.67f,
                    ThreeStar_DestructionMin=0.85f, ThreeStar_EnemyRatioMin=1.00f, ThreeStar_AttemptsRemaining=1,
                    GuardCount=2, ShieldGuardCount=0, ArcherCount=1, HasEnemyKing=false, HasExplosiveBarrel=false, HasTutorialHints=false,
                    PrimaryMaterial="Wood", SecondaryMaterial="Stone", CastleFloors=2, HasQueen=true,
                },
                new LevelDefinition
                {
                    LevelIndex=7, WorldIndex=0, DisplayName="Trap Fortress",
                    Description="Barrels everywhere. One wrong hit and the whole place goes up!",
                    KingLaunches=3, DifficultyRating=7, RequiredStarsToUnlock=11, SceneName="Level",
                    RequiredScore=1000, TwoStarScore=1800, ThreeStarScore=2700, MustRescueQueen=true,
                    OneStar_DestructionMin=0.35f, TwoStar_DestructionMin=0.65f, TwoStar_EnemyRatioMin=0.67f,
                    ThreeStar_DestructionMin=0.90f, ThreeStar_EnemyRatioMin=1.00f, ThreeStar_AttemptsRemaining=1,
                    GuardCount=2, ShieldGuardCount=1, ArcherCount=1, HasEnemyKing=false, HasExplosiveBarrel=true, HasTutorialHints=false,
                    PrimaryMaterial="Stone", SecondaryMaterial="Wood", CastleFloors=2, HasQueen=true,
                },
                new LevelDefinition
                {
                    LevelIndex=8, WorldIndex=0, DisplayName="The Enemy King",
                    Description="Defeat the Enemy King to rescue the Queen from his fortress.",
                    KingLaunches=4, DifficultyRating=8, RequiredStarsToUnlock=13, SceneName="Level",
                    RequiredScore=1200, TwoStarScore=2000, ThreeStarScore=3000, MustRescueQueen=true,
                    OneStar_DestructionMin=0.30f, TwoStar_DestructionMin=0.55f, TwoStar_EnemyRatioMin=0.75f,
                    ThreeStar_DestructionMin=0.80f, ThreeStar_EnemyRatioMin=1.00f, ThreeStar_AttemptsRemaining=1,
                    GuardCount=2, ShieldGuardCount=1, ArcherCount=0, HasEnemyKing=true, HasExplosiveBarrel=false, HasTutorialHints=false,
                    PrimaryMaterial="Stone", SecondaryMaterial="Metal", CastleFloors=3, HasQueen=true,
                },
                new LevelDefinition
                {
                    LevelIndex=9, WorldIndex=0, DisplayName="Mini Fortress",
                    Description="The ultimate challenge. Every skill you have learned is needed.",
                    KingLaunches=4, DifficultyRating=7, RequiredStarsToUnlock=15, SceneName="Level", // PATCH-001: KingLaunches 5→4, DifficultyRating confirmed 7 (was 10)
                    RequiredScore=1500, TwoStarScore=2500, ThreeStarScore=3800, MustRescueQueen=true,
                    OneStar_DestructionMin=0.30f, TwoStar_DestructionMin=0.60f, TwoStar_EnemyRatioMin=0.60f,
                    ThreeStar_DestructionMin=0.85f, ThreeStar_EnemyRatioMin=1.00f, ThreeStar_AttemptsRemaining=1,
                    GuardCount=2, ShieldGuardCount=1, ArcherCount=1, HasEnemyKing=true, HasExplosiveBarrel=true, HasTutorialHints=false,
                    PrimaryMaterial="Stone", SecondaryMaterial="Metal", CastleFloors=3, HasQueen=true,
                },
                new LevelDefinition
                {
                    LevelIndex=10, WorldIndex=0, DisplayName="Forest Outpost",
                    Description="An outpost guards the path into the deep forest.",
                    KingLaunches=4, DifficultyRating=5, RequiredStarsToUnlock=17, SceneName="Level",
                    RequiredScore=1000, TwoStarScore=1800, ThreeStarScore=2600, MustRescueQueen=true,
                    OneStar_DestructionMin=0.30f, TwoStar_DestructionMin=0.60f, TwoStar_EnemyRatioMin=0.50f,
                    ThreeStar_DestructionMin=0.85f, ThreeStar_EnemyRatioMin=1.00f, ThreeStar_AttemptsRemaining=1,
                    GuardCount=3, ShieldGuardCount=1, ArcherCount=0, HasEnemyKing=false, HasExplosiveBarrel=false, HasTutorialHints=false,
                    PrimaryMaterial="Wood", SecondaryMaterial="Stone", CastleFloors=2, HasQueen=true,
                },
                new LevelDefinition
                {
                    LevelIndex=11, WorldIndex=0, DisplayName="Treeline Barricade",
                    Description="Barricades built from felled trees block the way.",
                    KingLaunches=4, DifficultyRating=5, RequiredStarsToUnlock=18, SceneName="Level",
                    RequiredScore=1100, TwoStarScore=1900, ThreeStarScore=2800, MustRescueQueen=true,
                    OneStar_DestructionMin=0.30f, TwoStar_DestructionMin=0.60f, TwoStar_EnemyRatioMin=0.50f,
                    ThreeStar_DestructionMin=0.85f, ThreeStar_EnemyRatioMin=1.00f, ThreeStar_AttemptsRemaining=1,
                    GuardCount=3, ShieldGuardCount=1, ArcherCount=1, HasEnemyKing=false, HasExplosiveBarrel=false, HasTutorialHints=false,
                    PrimaryMaterial="Wood", SecondaryMaterial="Stone", CastleFloors=2, HasQueen=true,
                },
                new LevelDefinition
                {
                    LevelIndex=12, WorldIndex=0, DisplayName="Ancient Ruins",
                    Description="Crumbling stone ruins hide the Queen from sight.",
                    KingLaunches=4, DifficultyRating=6, RequiredStarsToUnlock=20, SceneName="Level",
                    RequiredScore=1200, TwoStarScore=2100, ThreeStarScore=3100, MustRescueQueen=true,
                    OneStar_DestructionMin=0.30f, TwoStar_DestructionMin=0.60f, TwoStar_EnemyRatioMin=0.60f,
                    ThreeStar_DestructionMin=0.85f, ThreeStar_EnemyRatioMin=1.00f, ThreeStar_AttemptsRemaining=1,
                    GuardCount=3, ShieldGuardCount=1, ArcherCount=1, HasEnemyKing=false, HasExplosiveBarrel=true, HasTutorialHints=false,
                    PrimaryMaterial="Stone", SecondaryMaterial="Wood", CastleFloors=3, HasQueen=true,
                },
                new LevelDefinition
                {
                    LevelIndex=13, WorldIndex=0, DisplayName="Woodcutter's Keep",
                    Description="A fortified keep made from thick hardwood planks.",
                    KingLaunches=4, DifficultyRating=6, RequiredStarsToUnlock=22, SceneName="Level",
                    RequiredScore=1300, TwoStarScore=2200, ThreeStarScore=3300, MustRescueQueen=true,
                    OneStar_DestructionMin=0.30f, TwoStar_DestructionMin=0.60f, TwoStar_EnemyRatioMin=0.67f,
                    ThreeStar_DestructionMin=0.85f, ThreeStar_EnemyRatioMin=1.00f, ThreeStar_AttemptsRemaining=1,
                    GuardCount=3, ShieldGuardCount=2, ArcherCount=1, HasEnemyKing=false, HasExplosiveBarrel=false, HasTutorialHints=false,
                    PrimaryMaterial="Wood", SecondaryMaterial="Stone", CastleFloors=3, HasQueen=true,
                },
                new LevelDefinition
                {
                    LevelIndex=14, WorldIndex=0, DisplayName="Siege Wall",
                    Description="A reinforced wall stands between you and the Queen.",
                    KingLaunches=4, DifficultyRating=7, RequiredStarsToUnlock=24, SceneName="Level",
                    RequiredScore=1400, TwoStarScore=2400, ThreeStarScore=3600, MustRescueQueen=true,
                    OneStar_DestructionMin=0.30f, TwoStar_DestructionMin=0.60f, TwoStar_EnemyRatioMin=0.67f,
                    ThreeStar_DestructionMin=0.85f, ThreeStar_EnemyRatioMin=1.00f, ThreeStar_AttemptsRemaining=1,
                    GuardCount=3, ShieldGuardCount=2, ArcherCount=2, HasEnemyKing=false, HasExplosiveBarrel=true, HasTutorialHints=false,
                    PrimaryMaterial="Stone", SecondaryMaterial="Wood", CastleFloors=3, HasQueen=true,
                },
                new LevelDefinition
                {
                    LevelIndex=15, WorldIndex=0, DisplayName="Ranger's Tower",
                    Description="Archers rain down from high towers. Destroy the supports!",
                    KingLaunches=4, DifficultyRating=7, RequiredStarsToUnlock=26, SceneName="Level",
                    RequiredScore=1500, TwoStarScore=2600, ThreeStarScore=3900, MustRescueQueen=true,
                    OneStar_DestructionMin=0.30f, TwoStar_DestructionMin=0.60f, TwoStar_EnemyRatioMin=0.67f,
                    ThreeStar_DestructionMin=0.85f, ThreeStar_EnemyRatioMin=1.00f, ThreeStar_AttemptsRemaining=1,
                    GuardCount=2, ShieldGuardCount=1, ArcherCount=3, HasEnemyKing=false, HasExplosiveBarrel=false, HasTutorialHints=false,
                    PrimaryMaterial="Wood", SecondaryMaterial="Stone", CastleFloors=4, HasQueen=true,
                },
                new LevelDefinition
                {
                    LevelIndex=16, WorldIndex=0, DisplayName="Forest Citadel",
                    Description="A full citadel protected by elite forest guards.",
                    KingLaunches=5, DifficultyRating=8, RequiredStarsToUnlock=28, SceneName="Level",
                    RequiredScore=1700, TwoStarScore=2900, ThreeStarScore=4300, MustRescueQueen=true,
                    OneStar_DestructionMin=0.30f, TwoStar_DestructionMin=0.60f, TwoStar_EnemyRatioMin=0.75f,
                    ThreeStar_DestructionMin=0.85f, ThreeStar_EnemyRatioMin=1.00f, ThreeStar_AttemptsRemaining=1,
                    GuardCount=4, ShieldGuardCount=2, ArcherCount=2, HasEnemyKing=false, HasExplosiveBarrel=true, HasTutorialHints=false,
                    PrimaryMaterial="Stone", SecondaryMaterial="Metal", CastleFloors=4, HasQueen=true,
                },
                new LevelDefinition
                {
                    LevelIndex=17, WorldIndex=0, DisplayName="King's Lumber Mill",
                    Description="The enemy King uses this mill as his stronghold.",
                    KingLaunches=5, DifficultyRating=9, RequiredStarsToUnlock=30, SceneName="Level",
                    RequiredScore=1900, TwoStarScore=3200, ThreeStarScore=4800, MustRescueQueen=true,
                    OneStar_DestructionMin=0.30f, TwoStar_DestructionMin=0.60f, TwoStar_EnemyRatioMin=0.75f,
                    ThreeStar_DestructionMin=0.85f, ThreeStar_EnemyRatioMin=1.00f, ThreeStar_AttemptsRemaining=1,
                    GuardCount=3, ShieldGuardCount=2, ArcherCount=2, HasEnemyKing=true, HasExplosiveBarrel=false, HasTutorialHints=false,
                    PrimaryMaterial="Wood", SecondaryMaterial="Metal", CastleFloors=4, HasQueen=true,
                },
                new LevelDefinition
                {
                    LevelIndex=18, WorldIndex=0, DisplayName="Overgrown Ramparts",
                    Description="Ancient walls covered in vines. The Queen awaits inside.",
                    KingLaunches=5, DifficultyRating=9, RequiredStarsToUnlock=32, SceneName="Level",
                    RequiredScore=2100, TwoStarScore=3500, ThreeStarScore=5200, MustRescueQueen=true,
                    OneStar_DestructionMin=0.30f, TwoStar_DestructionMin=0.60f, TwoStar_EnemyRatioMin=0.75f,
                    ThreeStar_DestructionMin=0.85f, ThreeStar_EnemyRatioMin=1.00f, ThreeStar_AttemptsRemaining=1,
                    GuardCount=4, ShieldGuardCount=2, ArcherCount=2, HasEnemyKing=true, HasExplosiveBarrel=true, HasTutorialHints=false,
                    PrimaryMaterial="Stone", SecondaryMaterial="Wood", CastleFloors=4, HasQueen=true,
                },
                new LevelDefinition
                {
                    LevelIndex=19, WorldIndex=0, DisplayName="Forest King's Lair",
                    Description="The Forest King himself stands between you and the Queen. Defeat him!",
                    KingLaunches=6, DifficultyRating=10, RequiredStarsToUnlock=34, SceneName="Level",
                    RequiredScore=2500, TwoStarScore=4200, ThreeStarScore=6000, MustRescueQueen=true,
                    OneStar_DestructionMin=0.30f, TwoStar_DestructionMin=0.60f, TwoStar_EnemyRatioMin=0.80f,
                    ThreeStar_DestructionMin=0.85f, ThreeStar_EnemyRatioMin=1.00f, ThreeStar_AttemptsRemaining=0,
                    GuardCount=4, ShieldGuardCount=3, ArcherCount=2, HasEnemyKing=true, HasExplosiveBarrel=true, HasTutorialHints=false,
                    PrimaryMaterial="Stone", SecondaryMaterial="Metal", CastleFloors=5, HasQueen=true, IsBossLevel=true, MaxPowerUpsAllowed=3,
                },

                // ── WORLD 1: DESERT (L20-39) ─────────────────────────────

                new LevelDefinition
                {
                    LevelIndex=20, WorldIndex=1, DisplayName="Sandy Outpost",
                    Description="A sandstone outpost bakes in the desert heat.",
                    KingLaunches=4, DifficultyRating=3, RequiredStarsToUnlock=0, SceneName="Level",
                    RequiredScore=1200, TwoStarScore=2100, ThreeStarScore=3100, MustRescueQueen=true,
                    OneStar_DestructionMin=0.30f, TwoStar_DestructionMin=0.65f, TwoStar_EnemyRatioMin=0.60f,
                    ThreeStar_DestructionMin=0.88f, ThreeStar_EnemyRatioMin=1.00f, ThreeStar_AttemptsRemaining=1,
                    GuardCount=2, ShieldGuardCount=1, ArcherCount=0, HasEnemyKing=false, HasExplosiveBarrel=false, HasTutorialHints=false,
                    PrimaryMaterial="Sandstone", SecondaryMaterial="", CastleFloors=2, HasQueen=true,
                },
                new LevelDefinition
                {
                    LevelIndex=21, WorldIndex=1, DisplayName="Dune Watchtower",
                    Description="Archers perch atop a slender dune tower.",
                    KingLaunches=4, DifficultyRating=4, RequiredStarsToUnlock=1, SceneName="Level",
                    RequiredScore=1300, TwoStarScore=2200, ThreeStarScore=3300, MustRescueQueen=true,
                    OneStar_DestructionMin=0.30f, TwoStar_DestructionMin=0.65f, TwoStar_EnemyRatioMin=0.60f,
                    ThreeStar_DestructionMin=0.88f, ThreeStar_EnemyRatioMin=1.00f, ThreeStar_AttemptsRemaining=1,
                    GuardCount=2, ShieldGuardCount=0, ArcherCount=2, HasEnemyKing=false, HasExplosiveBarrel=false, HasTutorialHints=false,
                    PrimaryMaterial="Sandstone", SecondaryMaterial="Wood", CastleFloors=3, HasQueen=true,
                },
                new LevelDefinition
                {
                    LevelIndex=22, WorldIndex=1, DisplayName="Desert Oasis Trap",
                    Description="An oasis camp is rigged with oil barrel traps.",
                    KingLaunches=3, DifficultyRating=5, RequiredStarsToUnlock=2, SceneName="Level",
                    RequiredScore=1400, TwoStarScore=2400, ThreeStarScore=3600, MustRescueQueen=true,
                    OneStar_DestructionMin=0.30f, TwoStar_DestructionMin=0.65f, TwoStar_EnemyRatioMin=0.60f,
                    ThreeStar_DestructionMin=0.88f, ThreeStar_EnemyRatioMin=1.00f, ThreeStar_AttemptsRemaining=1,
                    GuardCount=2, ShieldGuardCount=1, ArcherCount=1, HasEnemyKing=false, HasExplosiveBarrel=true, HasTutorialHints=false,
                    PrimaryMaterial="Sandstone", SecondaryMaterial="Wood", CastleFloors=2, HasQueen=true,
                },
                new LevelDefinition
                {
                    LevelIndex=23, WorldIndex=1, DisplayName="Mirage Fortress",
                    Description="A fortress that seems to shift in the desert heat.",
                    KingLaunches=4, DifficultyRating=5, RequiredStarsToUnlock=3, SceneName="Level",
                    RequiredScore=1500, TwoStarScore=2600, ThreeStarScore=3900, MustRescueQueen=true,
                    OneStar_DestructionMin=0.30f, TwoStar_DestructionMin=0.65f, TwoStar_EnemyRatioMin=0.60f,
                    ThreeStar_DestructionMin=0.88f, ThreeStar_EnemyRatioMin=1.00f, ThreeStar_AttemptsRemaining=1,
                    GuardCount=3, ShieldGuardCount=1, ArcherCount=1, HasEnemyKing=false, HasExplosiveBarrel=false, HasTutorialHints=false,
                    PrimaryMaterial="Sandstone", SecondaryMaterial="Stone", CastleFloors=3, HasQueen=true,
                },
                new LevelDefinition
                {
                    LevelIndex=24, WorldIndex=1, DisplayName="Sandstorm Ramparts",
                    Description="Visibility is low — the castle hides in a swirling sandstorm.",
                    KingLaunches=4, DifficultyRating=6, RequiredStarsToUnlock=4, SceneName="Level",
                    RequiredScore=1600, TwoStarScore=2800, ThreeStarScore=4200, MustRescueQueen=true,
                    OneStar_DestructionMin=0.30f, TwoStar_DestructionMin=0.65f, TwoStar_EnemyRatioMin=0.60f,
                    ThreeStar_DestructionMin=0.88f, ThreeStar_EnemyRatioMin=1.00f, ThreeStar_AttemptsRemaining=1,
                    GuardCount=3, ShieldGuardCount=2, ArcherCount=1, HasEnemyKing=false, HasExplosiveBarrel=false, HasTutorialHints=false,
                    PrimaryMaterial="Sandstone", SecondaryMaterial="Stone", CastleFloors=3, HasQueen=true,
                },
                new LevelDefinition
                {
                    LevelIndex=25, WorldIndex=1, DisplayName="Bazaar Battleground",
                    Description="Enemies have turned a trading bazaar into a fortress.",
                    KingLaunches=4, DifficultyRating=6, RequiredStarsToUnlock=5, SceneName="Level",
                    RequiredScore=1700, TwoStarScore=2900, ThreeStarScore=4400, MustRescueQueen=true,
                    OneStar_DestructionMin=0.30f, TwoStar_DestructionMin=0.65f, TwoStar_EnemyRatioMin=0.67f,
                    ThreeStar_DestructionMin=0.88f, ThreeStar_EnemyRatioMin=1.00f, ThreeStar_AttemptsRemaining=1,
                    GuardCount=3, ShieldGuardCount=2, ArcherCount=2, HasEnemyKing=false, HasExplosiveBarrel=true, HasTutorialHints=false,
                    PrimaryMaterial="Sandstone", SecondaryMaterial="Wood", CastleFloors=3, HasQueen=true,
                },
                new LevelDefinition
                {
                    LevelIndex=26, WorldIndex=1, DisplayName="Tomb of Sand",
                    Description="An ancient tomb repurposed as a desert stronghold.",
                    KingLaunches=4, DifficultyRating=7, RequiredStarsToUnlock=6, SceneName="Level",
                    RequiredScore=1900, TwoStarScore=3200, ThreeStarScore=4800, MustRescueQueen=true,
                    OneStar_DestructionMin=0.30f, TwoStar_DestructionMin=0.65f, TwoStar_EnemyRatioMin=0.67f,
                    ThreeStar_DestructionMin=0.88f, ThreeStar_EnemyRatioMin=1.00f, ThreeStar_AttemptsRemaining=1,
                    GuardCount=3, ShieldGuardCount=2, ArcherCount=2, HasEnemyKing=false, HasExplosiveBarrel=false, HasTutorialHints=false,
                    PrimaryMaterial="Stone", SecondaryMaterial="Sandstone", CastleFloors=4, HasQueen=true,
                },
                new LevelDefinition
                {
                    LevelIndex=27, WorldIndex=1, DisplayName="Camel Caravan Raid",
                    Description="Intercept the caravan and free the captive Queen.",
                    KingLaunches=3, DifficultyRating=7, RequiredStarsToUnlock=7, SceneName="Level",
                    RequiredScore=2000, TwoStarScore=3400, ThreeStarScore=5000, MustRescueQueen=true,
                    OneStar_DestructionMin=0.30f, TwoStar_DestructionMin=0.65f, TwoStar_EnemyRatioMin=0.67f,
                    ThreeStar_DestructionMin=0.88f, ThreeStar_EnemyRatioMin=1.00f, ThreeStar_AttemptsRemaining=1,
                    GuardCount=4, ShieldGuardCount=1, ArcherCount=2, HasEnemyKing=false, HasExplosiveBarrel=true, HasTutorialHints=false,
                    PrimaryMaterial="Sandstone", SecondaryMaterial="Wood", CastleFloors=2, HasQueen=true,
                },
                new LevelDefinition
                {
                    LevelIndex=28, WorldIndex=1, DisplayName="Scorpion Keep",
                    Description="The desert's deadliest fortress crawls with armed guards.",
                    KingLaunches=4, DifficultyRating=7, RequiredStarsToUnlock=8, SceneName="Level",
                    RequiredScore=2100, TwoStarScore=3600, ThreeStarScore=5400, MustRescueQueen=true,
                    OneStar_DestructionMin=0.30f, TwoStar_DestructionMin=0.65f, TwoStar_EnemyRatioMin=0.75f,
                    ThreeStar_DestructionMin=0.88f, ThreeStar_EnemyRatioMin=1.00f, ThreeStar_AttemptsRemaining=1,
                    GuardCount=4, ShieldGuardCount=2, ArcherCount=2, HasEnemyKing=false, HasExplosiveBarrel=false, HasTutorialHints=false,
                    PrimaryMaterial="Stone", SecondaryMaterial="Sandstone", CastleFloors=4, HasQueen=true,
                },
                new LevelDefinition
                {
                    LevelIndex=29, WorldIndex=1, DisplayName="Desert Siege Engine",
                    Description="The enemy has built a massive wooden siege tower.",
                    KingLaunches=5, DifficultyRating=8, RequiredStarsToUnlock=9, SceneName="Level",
                    RequiredScore=2300, TwoStarScore=3900, ThreeStarScore=5800, MustRescueQueen=true,
                    OneStar_DestructionMin=0.30f, TwoStar_DestructionMin=0.65f, TwoStar_EnemyRatioMin=0.75f,
                    ThreeStar_DestructionMin=0.88f, ThreeStar_EnemyRatioMin=1.00f, ThreeStar_AttemptsRemaining=1,
                    GuardCount=4, ShieldGuardCount=2, ArcherCount=3, HasEnemyKing=false, HasExplosiveBarrel=true, HasTutorialHints=false,
                    PrimaryMaterial="Wood", SecondaryMaterial="Sandstone", CastleFloors=5, HasQueen=true,
                },
                new LevelDefinition
                {
                    LevelIndex=30, WorldIndex=1, DisplayName="Pillars of Sand",
                    Description="Towering sandstone pillars form an impassable barrier.",
                    KingLaunches=4, DifficultyRating=8, RequiredStarsToUnlock=10, SceneName="Level",
                    RequiredScore=2400, TwoStarScore=4100, ThreeStarScore=6100, MustRescueQueen=true,
                    OneStar_DestructionMin=0.30f, TwoStar_DestructionMin=0.65f, TwoStar_EnemyRatioMin=0.75f,
                    ThreeStar_DestructionMin=0.88f, ThreeStar_EnemyRatioMin=1.00f, ThreeStar_AttemptsRemaining=1,
                    GuardCount=3, ShieldGuardCount=3, ArcherCount=2, HasEnemyKing=false, HasExplosiveBarrel=false, HasTutorialHints=false,
                    PrimaryMaterial="Sandstone", SecondaryMaterial="Stone", CastleFloors=4, HasQueen=true,
                },
                new LevelDefinition
                {
                    LevelIndex=31, WorldIndex=1, DisplayName="Buried Vault",
                    Description="Half-buried in sand, this vault holds many secrets.",
                    KingLaunches=4, DifficultyRating=8, RequiredStarsToUnlock=11, SceneName="Level",
                    RequiredScore=2500, TwoStarScore=4300, ThreeStarScore=6400, MustRescueQueen=true,
                    OneStar_DestructionMin=0.30f, TwoStar_DestructionMin=0.65f, TwoStar_EnemyRatioMin=0.75f,
                    ThreeStar_DestructionMin=0.88f, ThreeStar_EnemyRatioMin=1.00f, ThreeStar_AttemptsRemaining=1,
                    GuardCount=4, ShieldGuardCount=2, ArcherCount=3, HasEnemyKing=false, HasExplosiveBarrel=true, HasTutorialHints=false,
                    PrimaryMaterial="Stone", SecondaryMaterial="Sandstone", CastleFloors=4, HasQueen=true,
                },
                new LevelDefinition
                {
                    LevelIndex=32, WorldIndex=1, DisplayName="Sunbaked Stronghold",
                    Description="The sun-baked walls are brittle but thick.",
                    KingLaunches=5, DifficultyRating=8, RequiredStarsToUnlock=12, SceneName="Level",
                    RequiredScore=2600, TwoStarScore=4400, ThreeStarScore=6600, MustRescueQueen=true,
                    OneStar_DestructionMin=0.30f, TwoStar_DestructionMin=0.65f, TwoStar_EnemyRatioMin=0.75f,
                    ThreeStar_DestructionMin=0.88f, ThreeStar_EnemyRatioMin=1.00f, ThreeStar_AttemptsRemaining=1,
                    GuardCount=4, ShieldGuardCount=3, ArcherCount=2, HasEnemyKing=false, HasExplosiveBarrel=false, HasTutorialHints=false,
                    PrimaryMaterial="Sandstone", SecondaryMaterial="Metal", CastleFloors=4, HasQueen=true,
                },
                new LevelDefinition
                {
                    LevelIndex=33, WorldIndex=1, DisplayName="Serpent's Coil",
                    Description="A coiled fortress that confounds attackers.",
                    KingLaunches=5, DifficultyRating=8, RequiredStarsToUnlock=13, SceneName="Level",
                    RequiredScore=2700, TwoStarScore=4600, ThreeStarScore=6900, MustRescueQueen=true,
                    OneStar_DestructionMin=0.30f, TwoStar_DestructionMin=0.65f, TwoStar_EnemyRatioMin=0.75f,
                    ThreeStar_DestructionMin=0.88f, ThreeStar_EnemyRatioMin=1.00f, ThreeStar_AttemptsRemaining=1,
                    GuardCount=4, ShieldGuardCount=3, ArcherCount=3, HasEnemyKing=false, HasExplosiveBarrel=true, HasTutorialHints=false,
                    PrimaryMaterial="Stone", SecondaryMaterial="Metal", CastleFloors=5, HasQueen=true,
                },
                new LevelDefinition
                {
                    LevelIndex=34, WorldIndex=1, DisplayName="Oasis Palace",
                    Description="An opulent oasis palace now serves as an enemy stronghold.",
                    KingLaunches=5, DifficultyRating=9, RequiredStarsToUnlock=14, SceneName="Level",
                    RequiredScore=2900, TwoStarScore=4900, ThreeStarScore=7300, MustRescueQueen=true,
                    OneStar_DestructionMin=0.30f, TwoStar_DestructionMin=0.65f, TwoStar_EnemyRatioMin=0.80f,
                    ThreeStar_DestructionMin=0.88f, ThreeStar_EnemyRatioMin=1.00f, ThreeStar_AttemptsRemaining=1,
                    GuardCount=5, ShieldGuardCount=3, ArcherCount=3, HasEnemyKing=false, HasExplosiveBarrel=false, HasTutorialHints=false,
                    PrimaryMaterial="Sandstone", SecondaryMaterial="Metal", CastleFloors=5, HasQueen=true,
                },
                new LevelDefinition
                {
                    LevelIndex=35, WorldIndex=1, DisplayName="Grand Sphinx Gates",
                    Description="Enormous carved gates guard the inner sanctum.",
                    KingLaunches=5, DifficultyRating=9, RequiredStarsToUnlock=15, SceneName="Level",
                    RequiredScore=3100, TwoStarScore=5200, ThreeStarScore=7800, MustRescueQueen=true,
                    OneStar_DestructionMin=0.30f, TwoStar_DestructionMin=0.65f, TwoStar_EnemyRatioMin=0.80f,
                    ThreeStar_DestructionMin=0.88f, ThreeStar_EnemyRatioMin=1.00f, ThreeStar_AttemptsRemaining=1,
                    GuardCount=5, ShieldGuardCount=3, ArcherCount=3, HasEnemyKing=true, HasExplosiveBarrel=false, HasTutorialHints=false,
                    PrimaryMaterial="Stone", SecondaryMaterial="Sandstone", CastleFloors=5, HasQueen=true,
                },
                new LevelDefinition
                {
                    LevelIndex=36, WorldIndex=1, DisplayName="Sand Colosseum",
                    Description="An arena of death where the Queen is displayed as a trophy.",
                    KingLaunches=5, DifficultyRating=9, RequiredStarsToUnlock=16, SceneName="Level",
                    RequiredScore=3300, TwoStarScore=5500, ThreeStarScore=8200, MustRescueQueen=true,
                    OneStar_DestructionMin=0.30f, TwoStar_DestructionMin=0.65f, TwoStar_EnemyRatioMin=0.80f,
                    ThreeStar_DestructionMin=0.88f, ThreeStar_EnemyRatioMin=1.00f, ThreeStar_AttemptsRemaining=1,
                    GuardCount=5, ShieldGuardCount=4, ArcherCount=3, HasEnemyKing=true, HasExplosiveBarrel=true, HasTutorialHints=false,
                    PrimaryMaterial="Sandstone", SecondaryMaterial="Stone", CastleFloors=5, HasQueen=true,
                },
                new LevelDefinition
                {
                    LevelIndex=37, WorldIndex=1, DisplayName="Pharaoh's Maze",
                    Description="A labyrinthine palace of stone and sandstone.",
                    KingLaunches=6, DifficultyRating=9, RequiredStarsToUnlock=17, SceneName="Level",
                    RequiredScore=3500, TwoStarScore=5800, ThreeStarScore=8700, MustRescueQueen=true,
                    OneStar_DestructionMin=0.30f, TwoStar_DestructionMin=0.65f, TwoStar_EnemyRatioMin=0.80f,
                    ThreeStar_DestructionMin=0.88f, ThreeStar_EnemyRatioMin=1.00f, ThreeStar_AttemptsRemaining=1,
                    GuardCount=5, ShieldGuardCount=4, ArcherCount=4, HasEnemyKing=true, HasExplosiveBarrel=true, HasTutorialHints=false,
                    PrimaryMaterial="Stone", SecondaryMaterial="Metal", CastleFloors=5, HasQueen=true,
                },
                new LevelDefinition
                {
                    LevelIndex=38, WorldIndex=1, DisplayName="Eye of the Storm",
                    Description="A massive fortress at the heart of a perpetual sandstorm.",
                    KingLaunches=6, DifficultyRating=10, RequiredStarsToUnlock=18, SceneName="Level",
                    RequiredScore=3800, TwoStarScore=6300, ThreeStarScore=9400, MustRescueQueen=true,
                    OneStar_DestructionMin=0.30f, TwoStar_DestructionMin=0.65f, TwoStar_EnemyRatioMin=0.80f,
                    ThreeStar_DestructionMin=0.88f, ThreeStar_EnemyRatioMin=1.00f, ThreeStar_AttemptsRemaining=1,
                    GuardCount=5, ShieldGuardCount=4, ArcherCount=4, HasEnemyKing=true, HasExplosiveBarrel=true, HasTutorialHints=false,
                    PrimaryMaterial="Stone", SecondaryMaterial="Metal", CastleFloors=6, HasQueen=true,
                },
                new LevelDefinition
                {
                    LevelIndex=39, WorldIndex=1, DisplayName="Desert King's Citadel",
                    Description="The Desert King reigns from an impenetrable citadel. Topple it!",
                    KingLaunches=7, DifficultyRating=10, RequiredStarsToUnlock=19, SceneName="Level",
                    RequiredScore=4500, TwoStarScore=7500, ThreeStarScore=11000, MustRescueQueen=true,
                    OneStar_DestructionMin=0.30f, TwoStar_DestructionMin=0.65f, TwoStar_EnemyRatioMin=0.80f,
                    ThreeStar_DestructionMin=0.88f, ThreeStar_EnemyRatioMin=1.00f, ThreeStar_AttemptsRemaining=0,
                    GuardCount=5, ShieldGuardCount=4, ArcherCount=4, HasEnemyKing=true, HasExplosiveBarrel=true, HasTutorialHints=false,
                    PrimaryMaterial="Stone", SecondaryMaterial="Metal", CastleFloors=6, HasQueen=true, IsBossLevel=true, MaxPowerUpsAllowed=3,
                },

                // ── WORLD 2: ICE (L40-59) ────────────────────────────────

                new LevelDefinition
                {
                    LevelIndex=40, WorldIndex=2, DisplayName="Frozen Outpost",
                    Description="A small outpost buried in snow at the edge of the ice fields.",
                    KingLaunches=4, DifficultyRating=4, RequiredStarsToUnlock=0, SceneName="Level",
                    RequiredScore=2000, TwoStarScore=3400, ThreeStarScore=5100, MustRescueQueen=true,
                    OneStar_DestructionMin=0.30f, TwoStar_DestructionMin=0.65f, TwoStar_EnemyRatioMin=0.60f,
                    ThreeStar_DestructionMin=0.88f, ThreeStar_EnemyRatioMin=1.00f, ThreeStar_AttemptsRemaining=1,
                    GuardCount=2, ShieldGuardCount=1, ArcherCount=1, HasEnemyKing=false, HasExplosiveBarrel=false, HasTutorialHints=false,
                    PrimaryMaterial="Ice", SecondaryMaterial="Wood", CastleFloors=2, HasQueen=true,
                },
                new LevelDefinition
                {
                    LevelIndex=41, WorldIndex=2, DisplayName="Glacial Tower",
                    Description="A tower carved entirely from a glacier.",
                    KingLaunches=4, DifficultyRating=5, RequiredStarsToUnlock=1, SceneName="Level",
                    RequiredScore=2100, TwoStarScore=3600, ThreeStarScore=5400, MustRescueQueen=true,
                    OneStar_DestructionMin=0.30f, TwoStar_DestructionMin=0.65f, TwoStar_EnemyRatioMin=0.60f,
                    ThreeStar_DestructionMin=0.88f, ThreeStar_EnemyRatioMin=1.00f, ThreeStar_AttemptsRemaining=1,
                    GuardCount=3, ShieldGuardCount=1, ArcherCount=1, HasEnemyKing=false, HasExplosiveBarrel=false, HasTutorialHints=false,
                    PrimaryMaterial="Ice", SecondaryMaterial="Stone", CastleFloors=3, HasQueen=true,
                },
                new LevelDefinition
                {
                    LevelIndex=42, WorldIndex=2, DisplayName="Blizzard Barricade",
                    Description="Visibility near zero — can you find the weak point?",
                    KingLaunches=4, DifficultyRating=5, RequiredStarsToUnlock=2, SceneName="Level",
                    RequiredScore=2200, TwoStarScore=3800, ThreeStarScore=5700, MustRescueQueen=true,
                    OneStar_DestructionMin=0.30f, TwoStar_DestructionMin=0.65f, TwoStar_EnemyRatioMin=0.60f,
                    ThreeStar_DestructionMin=0.88f, ThreeStar_EnemyRatioMin=1.00f, ThreeStar_AttemptsRemaining=1,
                    GuardCount=3, ShieldGuardCount=2, ArcherCount=1, HasEnemyKing=false, HasExplosiveBarrel=false, HasTutorialHints=false,
                    PrimaryMaterial="Ice", SecondaryMaterial="Stone", CastleFloors=3, HasQueen=true,
                },
                new LevelDefinition
                {
                    LevelIndex=43, WorldIndex=2, DisplayName="Frosted Ramparts",
                    Description="Slick ice-coated ramparts make the climb treacherous.",
                    KingLaunches=4, DifficultyRating=6, RequiredStarsToUnlock=3, SceneName="Level",
                    RequiredScore=2400, TwoStarScore=4100, ThreeStarScore=6100, MustRescueQueen=true,
                    OneStar_DestructionMin=0.30f, TwoStar_DestructionMin=0.65f, TwoStar_EnemyRatioMin=0.67f,
                    ThreeStar_DestructionMin=0.88f, ThreeStar_EnemyRatioMin=1.00f, ThreeStar_AttemptsRemaining=1,
                    GuardCount=3, ShieldGuardCount=2, ArcherCount=2, HasEnemyKing=false, HasExplosiveBarrel=false, HasTutorialHints=false,
                    PrimaryMaterial="Ice", SecondaryMaterial="Stone", CastleFloors=3, HasQueen=true,
                },
                new LevelDefinition
                {
                    LevelIndex=44, WorldIndex=2, DisplayName="Ice Spike Trap",
                    Description="Explosives are frozen into the walls — a deadly combination.",
                    KingLaunches=3, DifficultyRating=6, RequiredStarsToUnlock=4, SceneName="Level",
                    RequiredScore=2500, TwoStarScore=4300, ThreeStarScore=6400, MustRescueQueen=true,
                    OneStar_DestructionMin=0.30f, TwoStar_DestructionMin=0.65f, TwoStar_EnemyRatioMin=0.67f,
                    ThreeStar_DestructionMin=0.88f, ThreeStar_EnemyRatioMin=1.00f, ThreeStar_AttemptsRemaining=1,
                    GuardCount=3, ShieldGuardCount=2, ArcherCount=2, HasEnemyKing=false, HasExplosiveBarrel=true, HasTutorialHints=false,
                    PrimaryMaterial="Ice", SecondaryMaterial="Stone", CastleFloors=3, HasQueen=true,
                },
                new LevelDefinition
                {
                    LevelIndex=45, WorldIndex=2, DisplayName="Permafrost Fortress",
                    Description="Permanently frozen walls that shatter explosively.",
                    KingLaunches=5, DifficultyRating=6, RequiredStarsToUnlock=5, SceneName="Level",
                    RequiredScore=2600, TwoStarScore=4500, ThreeStarScore=6700, MustRescueQueen=true,
                    OneStar_DestructionMin=0.30f, TwoStar_DestructionMin=0.65f, TwoStar_EnemyRatioMin=0.67f,
                    ThreeStar_DestructionMin=0.88f, ThreeStar_EnemyRatioMin=1.00f, ThreeStar_AttemptsRemaining=1,
                    GuardCount=4, ShieldGuardCount=2, ArcherCount=2, HasEnemyKing=false, HasExplosiveBarrel=false, HasTutorialHints=false,
                    PrimaryMaterial="Ice", SecondaryMaterial="Stone", CastleFloors=4, HasQueen=true,
                },
                new LevelDefinition
                {
                    LevelIndex=46, WorldIndex=2, DisplayName="Snowbound Keep",
                    Description="Snow-encased keep with archers at every window.",
                    KingLaunches=4, DifficultyRating=7, RequiredStarsToUnlock=6, SceneName="Level",
                    RequiredScore=2800, TwoStarScore=4800, ThreeStarScore=7100, MustRescueQueen=true,
                    OneStar_DestructionMin=0.30f, TwoStar_DestructionMin=0.65f, TwoStar_EnemyRatioMin=0.75f,
                    ThreeStar_DestructionMin=0.88f, ThreeStar_EnemyRatioMin=1.00f, ThreeStar_AttemptsRemaining=1,
                    GuardCount=4, ShieldGuardCount=2, ArcherCount=3, HasEnemyKing=false, HasExplosiveBarrel=true, HasTutorialHints=false,
                    PrimaryMaterial="Ice", SecondaryMaterial="Stone", CastleFloors=4, HasQueen=true,
                },
                new LevelDefinition
                {
                    LevelIndex=47, WorldIndex=2, DisplayName="Frozen River Crossing",
                    Description="A makeshift fort on the frozen river. Break the ice!",
                    KingLaunches=4, DifficultyRating=7, RequiredStarsToUnlock=7, SceneName="Level",
                    RequiredScore=3000, TwoStarScore=5100, ThreeStarScore=7600, MustRescueQueen=true,
                    OneStar_DestructionMin=0.30f, TwoStar_DestructionMin=0.65f, TwoStar_EnemyRatioMin=0.75f,
                    ThreeStar_DestructionMin=0.88f, ThreeStar_EnemyRatioMin=1.00f, ThreeStar_AttemptsRemaining=1,
                    GuardCount=4, ShieldGuardCount=3, ArcherCount=2, HasEnemyKing=false, HasExplosiveBarrel=false, HasTutorialHints=false,
                    PrimaryMaterial="Ice", SecondaryMaterial="Wood", CastleFloors=3, HasQueen=true,
                },
                new LevelDefinition
                {
                    LevelIndex=48, WorldIndex=2, DisplayName="Tundra Bastion",
                    Description="A massive stone bastion reinforced with sheets of ice.",
                    KingLaunches=5, DifficultyRating=7, RequiredStarsToUnlock=8, SceneName="Level",
                    RequiredScore=3100, TwoStarScore=5300, ThreeStarScore=7900, MustRescueQueen=true,
                    OneStar_DestructionMin=0.30f, TwoStar_DestructionMin=0.65f, TwoStar_EnemyRatioMin=0.75f,
                    ThreeStar_DestructionMin=0.88f, ThreeStar_EnemyRatioMin=1.00f, ThreeStar_AttemptsRemaining=1,
                    GuardCount=4, ShieldGuardCount=3, ArcherCount=3, HasEnemyKing=false, HasExplosiveBarrel=true, HasTutorialHints=false,
                    PrimaryMaterial="Stone", SecondaryMaterial="Ice", CastleFloors=4, HasQueen=true,
                },
                new LevelDefinition
                {
                    LevelIndex=49, WorldIndex=2, DisplayName="Avalanche Zone",
                    Description="Trigger the avalanche to bury the castle!",
                    KingLaunches=3, DifficultyRating=8, RequiredStarsToUnlock=9, SceneName="Level",
                    RequiredScore=3300, TwoStarScore=5600, ThreeStarScore=8400, MustRescueQueen=true,
                    OneStar_DestructionMin=0.30f, TwoStar_DestructionMin=0.65f, TwoStar_EnemyRatioMin=0.75f,
                    ThreeStar_DestructionMin=0.88f, ThreeStar_EnemyRatioMin=1.00f, ThreeStar_AttemptsRemaining=1,
                    GuardCount=4, ShieldGuardCount=2, ArcherCount=3, HasEnemyKing=false, HasExplosiveBarrel=true, HasTutorialHints=false,
                    PrimaryMaterial="Ice", SecondaryMaterial="Stone", CastleFloors=4, HasQueen=true,
                },
                new LevelDefinition
                {
                    LevelIndex=50, WorldIndex=2, DisplayName="Ice Palace Courtyard",
                    Description="The outer courtyard of the Ice Palace — freeze-proof walls.",
                    KingLaunches=5, DifficultyRating=8, RequiredStarsToUnlock=10, SceneName="Level",
                    RequiredScore=3500, TwoStarScore=5900, ThreeStarScore=8800, MustRescueQueen=true,
                    OneStar_DestructionMin=0.30f, TwoStar_DestructionMin=0.65f, TwoStar_EnemyRatioMin=0.75f,
                    ThreeStar_DestructionMin=0.88f, ThreeStar_EnemyRatioMin=1.00f, ThreeStar_AttemptsRemaining=1,
                    GuardCount=4, ShieldGuardCount=3, ArcherCount=3, HasEnemyKing=false, HasExplosiveBarrel=false, HasTutorialHints=false,
                    PrimaryMaterial="Ice", SecondaryMaterial="Stone", CastleFloors=5, HasQueen=true,
                },
                new LevelDefinition
                {
                    LevelIndex=51, WorldIndex=2, DisplayName="Crystal Spire",
                    Description="A spire of translucent ice crystals — beautiful and deadly.",
                    KingLaunches=4, DifficultyRating=8, RequiredStarsToUnlock=11, SceneName="Level",
                    RequiredScore=3700, TwoStarScore=6200, ThreeStarScore=9300, MustRescueQueen=true,
                    OneStar_DestructionMin=0.30f, TwoStar_DestructionMin=0.65f, TwoStar_EnemyRatioMin=0.75f,
                    ThreeStar_DestructionMin=0.88f, ThreeStar_EnemyRatioMin=1.00f, ThreeStar_AttemptsRemaining=1,
                    GuardCount=4, ShieldGuardCount=3, ArcherCount=4, HasEnemyKing=false, HasExplosiveBarrel=true, HasTutorialHints=false,
                    PrimaryMaterial="Ice", SecondaryMaterial="Metal", CastleFloors=5, HasQueen=true,
                },
                new LevelDefinition
                {
                    LevelIndex=52, WorldIndex=2, DisplayName="Blizzard Citadel",
                    Description="The storm itself seems to protect this citadel.",
                    KingLaunches=5, DifficultyRating=9, RequiredStarsToUnlock=12, SceneName="Level",
                    RequiredScore=4000, TwoStarScore=6700, ThreeStarScore=10000, MustRescueQueen=true,
                    OneStar_DestructionMin=0.30f, TwoStar_DestructionMin=0.65f, TwoStar_EnemyRatioMin=0.80f,
                    ThreeStar_DestructionMin=0.88f, ThreeStar_EnemyRatioMin=1.00f, ThreeStar_AttemptsRemaining=1,
                    GuardCount=5, ShieldGuardCount=3, ArcherCount=4, HasEnemyKing=false, HasExplosiveBarrel=false, HasTutorialHints=false,
                    PrimaryMaterial="Ice", SecondaryMaterial="Stone", CastleFloors=5, HasQueen=true,
                },
                new LevelDefinition
                {
                    LevelIndex=53, WorldIndex=2, DisplayName="Frost King's Guard",
                    Description="The Frost King's elite guard mans an impenetrable wall.",
                    KingLaunches=5, DifficultyRating=9, RequiredStarsToUnlock=13, SceneName="Level",
                    RequiredScore=4200, TwoStarScore=7000, ThreeStarScore=10500, MustRescueQueen=true,
                    OneStar_DestructionMin=0.30f, TwoStar_DestructionMin=0.65f, TwoStar_EnemyRatioMin=0.80f,
                    ThreeStar_DestructionMin=0.88f, ThreeStar_EnemyRatioMin=1.00f, ThreeStar_AttemptsRemaining=1,
                    GuardCount=5, ShieldGuardCount=4, ArcherCount=3, HasEnemyKing=true, HasExplosiveBarrel=false, HasTutorialHints=false,
                    PrimaryMaterial="Ice", SecondaryMaterial="Metal", CastleFloors=5, HasQueen=true,
                },
                new LevelDefinition
                {
                    LevelIndex=54, WorldIndex=2, DisplayName="Subzero Maze",
                    Description="A maze of ice walls that disorients and confounds.",
                    KingLaunches=5, DifficultyRating=9, RequiredStarsToUnlock=14, SceneName="Level",
                    RequiredScore=4400, TwoStarScore=7300, ThreeStarScore=11000, MustRescueQueen=true,
                    OneStar_DestructionMin=0.30f, TwoStar_DestructionMin=0.65f, TwoStar_EnemyRatioMin=0.80f,
                    ThreeStar_DestructionMin=0.88f, ThreeStar_EnemyRatioMin=1.00f, ThreeStar_AttemptsRemaining=1,
                    GuardCount=5, ShieldGuardCount=4, ArcherCount=4, HasEnemyKing=true, HasExplosiveBarrel=true, HasTutorialHints=false,
                    PrimaryMaterial="Ice", SecondaryMaterial="Stone", CastleFloors=5, HasQueen=true,
                },
                new LevelDefinition
                {
                    LevelIndex=55, WorldIndex=2, DisplayName="Glacial Throne Room",
                    Description="The antechamber of the Frost King's throne room.",
                    KingLaunches=6, DifficultyRating=9, RequiredStarsToUnlock=15, SceneName="Level",
                    RequiredScore=4700, TwoStarScore=7800, ThreeStarScore=11700, MustRescueQueen=true,
                    OneStar_DestructionMin=0.30f, TwoStar_DestructionMin=0.65f, TwoStar_EnemyRatioMin=0.80f,
                    ThreeStar_DestructionMin=0.88f, ThreeStar_EnemyRatioMin=1.00f, ThreeStar_AttemptsRemaining=1,
                    GuardCount=5, ShieldGuardCount=4, ArcherCount=4, HasEnemyKing=true, HasExplosiveBarrel=false, HasTutorialHints=false,
                    PrimaryMaterial="Ice", SecondaryMaterial="Metal", CastleFloors=6, HasQueen=true,
                },
                new LevelDefinition
                {
                    LevelIndex=56, WorldIndex=2, DisplayName="Winter Siege",
                    Description="A desperate winter siege against the last stronghold.",
                    KingLaunches=6, DifficultyRating=9, RequiredStarsToUnlock=16, SceneName="Level",
                    RequiredScore=5000, TwoStarScore=8300, ThreeStarScore=12500, MustRescueQueen=true,
                    OneStar_DestructionMin=0.30f, TwoStar_DestructionMin=0.65f, TwoStar_EnemyRatioMin=0.80f,
                    ThreeStar_DestructionMin=0.88f, ThreeStar_EnemyRatioMin=1.00f, ThreeStar_AttemptsRemaining=1,
                    GuardCount=5, ShieldGuardCount=4, ArcherCount=4, HasEnemyKing=true, HasExplosiveBarrel=true, HasTutorialHints=false,
                    PrimaryMaterial="Ice", SecondaryMaterial="Stone", CastleFloors=6, HasQueen=true,
                },
                new LevelDefinition
                {
                    LevelIndex=57, WorldIndex=2, DisplayName="Snowfall Bastion",
                    Description="Heavy snowfall obscures the castle's weak points.",
                    KingLaunches=6, DifficultyRating=10, RequiredStarsToUnlock=17, SceneName="Level",
                    RequiredScore=5300, TwoStarScore=8800, ThreeStarScore=13200, MustRescueQueen=true,
                    OneStar_DestructionMin=0.30f, TwoStar_DestructionMin=0.65f, TwoStar_EnemyRatioMin=0.80f,
                    ThreeStar_DestructionMin=0.88f, ThreeStar_EnemyRatioMin=1.00f, ThreeStar_AttemptsRemaining=1,
                    GuardCount=6, ShieldGuardCount=4, ArcherCount=4, HasEnemyKing=true, HasExplosiveBarrel=true, HasTutorialHints=false,
                    PrimaryMaterial="Ice", SecondaryMaterial="Metal", CastleFloors=6, HasQueen=true,
                },
                new LevelDefinition
                {
                    LevelIndex=58, WorldIndex=2, DisplayName="Heart of Winter",
                    Description="The coldest point on earth houses the Frost King's vault.",
                    KingLaunches=6, DifficultyRating=10, RequiredStarsToUnlock=18, SceneName="Level",
                    RequiredScore=5700, TwoStarScore=9500, ThreeStarScore=14200, MustRescueQueen=true,
                    OneStar_DestructionMin=0.30f, TwoStar_DestructionMin=0.65f, TwoStar_EnemyRatioMin=0.80f,
                    ThreeStar_DestructionMin=0.88f, ThreeStar_EnemyRatioMin=1.00f, ThreeStar_AttemptsRemaining=1,
                    GuardCount=6, ShieldGuardCount=5, ArcherCount=4, HasEnemyKing=true, HasExplosiveBarrel=true, HasTutorialHints=false,
                    PrimaryMaterial="Ice", SecondaryMaterial="Metal", CastleFloors=6, HasQueen=true,
                },
                new LevelDefinition
                {
                    LevelIndex=59, WorldIndex=2, DisplayName="Frost King's Colossus",
                    Description="The Frost King's ultimate fortress. Shatter it to free the Queen!",
                    KingLaunches=7, DifficultyRating=10, RequiredStarsToUnlock=19, SceneName="Level",
                    RequiredScore=6500, TwoStarScore=10800, ThreeStarScore=16200, MustRescueQueen=true,
                    OneStar_DestructionMin=0.30f, TwoStar_DestructionMin=0.65f, TwoStar_EnemyRatioMin=0.80f,
                    ThreeStar_DestructionMin=0.88f, ThreeStar_EnemyRatioMin=1.00f, ThreeStar_AttemptsRemaining=0,
                    GuardCount=6, ShieldGuardCount=5, ArcherCount=5, HasEnemyKing=true, HasExplosiveBarrel=true, HasTutorialHints=false,
                    PrimaryMaterial="Ice", SecondaryMaterial="Metal", CastleFloors=7, HasQueen=true, IsBossLevel=true, MaxPowerUpsAllowed=3,
                },

                // ── WORLD 3: DARK (L60-79) ───────────────────────────────

                new LevelDefinition
                {
                    LevelIndex=60, WorldIndex=3, DisplayName="Shadow Gate",
                    Description="The gateway to the dark realm. Shadows hide the sentries.",
                    KingLaunches=4, DifficultyRating=5, RequiredStarsToUnlock=0, SceneName="Level",
                    RequiredScore=3000, TwoStarScore=5100, ThreeStarScore=7600, MustRescueQueen=true,
                    OneStar_DestructionMin=0.30f, TwoStar_DestructionMin=0.65f, TwoStar_EnemyRatioMin=0.60f,
                    ThreeStar_DestructionMin=0.90f, ThreeStar_EnemyRatioMin=1.00f, ThreeStar_AttemptsRemaining=1,
                    GuardCount=3, ShieldGuardCount=2, ArcherCount=1, HasEnemyKing=false, HasExplosiveBarrel=false, HasTutorialHints=false,
                    PrimaryMaterial="Stone", SecondaryMaterial="Metal", CastleFloors=3, HasQueen=true,
                },
                new LevelDefinition
                {
                    LevelIndex=61, WorldIndex=3, DisplayName="Obsidian Walls",
                    Description="Walls of volcanic obsidian — harder than stone.",
                    KingLaunches=4, DifficultyRating=6, RequiredStarsToUnlock=1, SceneName="Level",
                    RequiredScore=3200, TwoStarScore=5400, ThreeStarScore=8100, MustRescueQueen=true,
                    OneStar_DestructionMin=0.30f, TwoStar_DestructionMin=0.65f, TwoStar_EnemyRatioMin=0.67f,
                    ThreeStar_DestructionMin=0.90f, ThreeStar_EnemyRatioMin=1.00f, ThreeStar_AttemptsRemaining=1,
                    GuardCount=3, ShieldGuardCount=2, ArcherCount=2, HasEnemyKing=false, HasExplosiveBarrel=false, HasTutorialHints=false,
                    PrimaryMaterial="Metal", SecondaryMaterial="Stone", CastleFloors=3, HasQueen=true,
                },
                new LevelDefinition
                {
                    LevelIndex=62, WorldIndex=3, DisplayName="Cursed Barricade",
                    Description="Dark magic protects these explosive-laden barricades.",
                    KingLaunches=3, DifficultyRating=6, RequiredStarsToUnlock=2, SceneName="Level",
                    RequiredScore=3400, TwoStarScore=5700, ThreeStarScore=8600, MustRescueQueen=true,
                    OneStar_DestructionMin=0.30f, TwoStar_DestructionMin=0.65f, TwoStar_EnemyRatioMin=0.67f,
                    ThreeStar_DestructionMin=0.90f, ThreeStar_EnemyRatioMin=1.00f, ThreeStar_AttemptsRemaining=1,
                    GuardCount=3, ShieldGuardCount=2, ArcherCount=2, HasEnemyKing=false, HasExplosiveBarrel=true, HasTutorialHints=false,
                    PrimaryMaterial="Stone", SecondaryMaterial="Metal", CastleFloors=3, HasQueen=true,
                },
                new LevelDefinition
                {
                    LevelIndex=63, WorldIndex=3, DisplayName="Night Raid",
                    Description="Attack under cover of darkness while the guards are few.",
                    KingLaunches=4, DifficultyRating=7, RequiredStarsToUnlock=3, SceneName="Level",
                    RequiredScore=3600, TwoStarScore=6100, ThreeStarScore=9100, MustRescueQueen=true,
                    OneStar_DestructionMin=0.30f, TwoStar_DestructionMin=0.65f, TwoStar_EnemyRatioMin=0.75f,
                    ThreeStar_DestructionMin=0.90f, ThreeStar_EnemyRatioMin=1.00f, ThreeStar_AttemptsRemaining=1,
                    GuardCount=4, ShieldGuardCount=2, ArcherCount=2, HasEnemyKing=false, HasExplosiveBarrel=false, HasTutorialHints=false,
                    PrimaryMaterial="Metal", SecondaryMaterial="Stone", CastleFloors=4, HasQueen=true,
                },
                new LevelDefinition
                {
                    LevelIndex=64, WorldIndex=3, DisplayName="Haunted Keep",
                    Description="Eerie lights flicker in the windows of this cursed keep.",
                    KingLaunches=4, DifficultyRating=7, RequiredStarsToUnlock=4, SceneName="Level",
                    RequiredScore=3800, TwoStarScore=6400, ThreeStarScore=9600, MustRescueQueen=true,
                    OneStar_DestructionMin=0.30f, TwoStar_DestructionMin=0.65f, TwoStar_EnemyRatioMin=0.75f,
                    ThreeStar_DestructionMin=0.90f, ThreeStar_EnemyRatioMin=1.00f, ThreeStar_AttemptsRemaining=1,
                    GuardCount=4, ShieldGuardCount=3, ArcherCount=2, HasEnemyKing=false, HasExplosiveBarrel=true, HasTutorialHints=false,
                    PrimaryMaterial="Stone", SecondaryMaterial="Metal", CastleFloors=4, HasQueen=true,
                },
                new LevelDefinition
                {
                    LevelIndex=65, WorldIndex=3, DisplayName="Void Watchtower",
                    Description="A watchtower that seems to absorb light itself.",
                    KingLaunches=4, DifficultyRating=7, RequiredStarsToUnlock=5, SceneName="Level",
                    RequiredScore=4000, TwoStarScore=6700, ThreeStarScore=10000, MustRescueQueen=true,
                    OneStar_DestructionMin=0.30f, TwoStar_DestructionMin=0.65f, TwoStar_EnemyRatioMin=0.75f,
                    ThreeStar_DestructionMin=0.90f, ThreeStar_EnemyRatioMin=1.00f, ThreeStar_AttemptsRemaining=1,
                    GuardCount=4, ShieldGuardCount=3, ArcherCount=3, HasEnemyKing=false, HasExplosiveBarrel=false, HasTutorialHints=false,
                    PrimaryMaterial="Metal", SecondaryMaterial="Stone", CastleFloors=4, HasQueen=true,
                },
                new LevelDefinition
                {
                    LevelIndex=66, WorldIndex=3, DisplayName="Crypt Fortress",
                    Description="Built atop ancient crypts — the foundations are unstable.",
                    KingLaunches=4, DifficultyRating=8, RequiredStarsToUnlock=6, SceneName="Level",
                    RequiredScore=4300, TwoStarScore=7200, ThreeStarScore=10800, MustRescueQueen=true,
                    OneStar_DestructionMin=0.30f, TwoStar_DestructionMin=0.65f, TwoStar_EnemyRatioMin=0.75f,
                    ThreeStar_DestructionMin=0.90f, ThreeStar_EnemyRatioMin=1.00f, ThreeStar_AttemptsRemaining=1,
                    GuardCount=4, ShieldGuardCount=3, ArcherCount=3, HasEnemyKing=false, HasExplosiveBarrel=true, HasTutorialHints=false,
                    PrimaryMaterial="Stone", SecondaryMaterial="Metal", CastleFloors=5, HasQueen=true,
                },
                new LevelDefinition
                {
                    LevelIndex=67, WorldIndex=3, DisplayName="Soul Cage",
                    Description="A cage-like structure of metal bars holds the Queen trapped inside.",
                    KingLaunches=5, DifficultyRating=8, RequiredStarsToUnlock=7, SceneName="Level",
                    RequiredScore=4600, TwoStarScore=7700, ThreeStarScore=11500, MustRescueQueen=true,
                    OneStar_DestructionMin=0.30f, TwoStar_DestructionMin=0.65f, TwoStar_EnemyRatioMin=0.75f,
                    ThreeStar_DestructionMin=0.90f, ThreeStar_EnemyRatioMin=1.00f, ThreeStar_AttemptsRemaining=1,
                    GuardCount=5, ShieldGuardCount=3, ArcherCount=3, HasEnemyKing=false, HasExplosiveBarrel=false, HasTutorialHints=false,
                    PrimaryMaterial="Metal", SecondaryMaterial="Stone", CastleFloors=5, HasQueen=true,
                },
                new LevelDefinition
                {
                    LevelIndex=68, WorldIndex=3, DisplayName="Bloodmoon Ramparts",
                    Description="Lit by a blood-red moon, these ramparts never sleep.",
                    KingLaunches=5, DifficultyRating=8, RequiredStarsToUnlock=8, SceneName="Level",
                    RequiredScore=4900, TwoStarScore=8200, ThreeStarScore=12300, MustRescueQueen=true,
                    OneStar_DestructionMin=0.30f, TwoStar_DestructionMin=0.65f, TwoStar_EnemyRatioMin=0.75f,
                    ThreeStar_DestructionMin=0.90f, ThreeStar_EnemyRatioMin=1.00f, ThreeStar_AttemptsRemaining=1,
                    GuardCount=5, ShieldGuardCount=4, ArcherCount=3, HasEnemyKing=false, HasExplosiveBarrel=true, HasTutorialHints=false,
                    PrimaryMaterial="Stone", SecondaryMaterial="Metal", CastleFloors=5, HasQueen=true,
                },
                new LevelDefinition
                {
                    LevelIndex=69, WorldIndex=3, DisplayName="Eclipse Stronghold",
                    Description="Total darkness during an eclipse — only instinct guides you.",
                    KingLaunches=5, DifficultyRating=8, RequiredStarsToUnlock=9, SceneName="Level",
                    RequiredScore=5200, TwoStarScore=8700, ThreeStarScore=13000, MustRescueQueen=true,
                    OneStar_DestructionMin=0.30f, TwoStar_DestructionMin=0.65f, TwoStar_EnemyRatioMin=0.80f,
                    ThreeStar_DestructionMin=0.90f, ThreeStar_EnemyRatioMin=1.00f, ThreeStar_AttemptsRemaining=1,
                    GuardCount=5, ShieldGuardCount=4, ArcherCount=4, HasEnemyKing=false, HasExplosiveBarrel=false, HasTutorialHints=false,
                    PrimaryMaterial="Metal", SecondaryMaterial="Stone", CastleFloors=5, HasQueen=true,
                },
                new LevelDefinition
                {
                    LevelIndex=70, WorldIndex=3, DisplayName="Shadowmage Tower",
                    Description="The dark sorcerer's tower pulses with sinister energy.",
                    KingLaunches=5, DifficultyRating=9, RequiredStarsToUnlock=10, SceneName="Level",
                    RequiredScore=5500, TwoStarScore=9200, ThreeStarScore=13800, MustRescueQueen=true,
                    OneStar_DestructionMin=0.30f, TwoStar_DestructionMin=0.65f, TwoStar_EnemyRatioMin=0.80f,
                    ThreeStar_DestructionMin=0.90f, ThreeStar_EnemyRatioMin=1.00f, ThreeStar_AttemptsRemaining=1,
                    GuardCount=5, ShieldGuardCount=4, ArcherCount=4, HasEnemyKing=true, HasExplosiveBarrel=true, HasTutorialHints=false,
                    PrimaryMaterial="Stone", SecondaryMaterial="Metal", CastleFloors=6, HasQueen=true,
                },
                new LevelDefinition
                {
                    LevelIndex=71, WorldIndex=3, DisplayName="Nethergate Fortress",
                    Description="The gates between worlds — shatter them to pass through.",
                    KingLaunches=5, DifficultyRating=9, RequiredStarsToUnlock=11, SceneName="Level",
                    RequiredScore=5800, TwoStarScore=9700, ThreeStarScore=14500, MustRescueQueen=true,
                    OneStar_DestructionMin=0.30f, TwoStar_DestructionMin=0.65f, TwoStar_EnemyRatioMin=0.80f,
                    ThreeStar_DestructionMin=0.90f, ThreeStar_EnemyRatioMin=1.00f, ThreeStar_AttemptsRemaining=1,
                    GuardCount=5, ShieldGuardCount=4, ArcherCount=4, HasEnemyKing=true, HasExplosiveBarrel=false, HasTutorialHints=false,
                    PrimaryMaterial="Metal", SecondaryMaterial="Stone", CastleFloors=6, HasQueen=true,
                },
                new LevelDefinition
                {
                    LevelIndex=72, WorldIndex=3, DisplayName="Wraith Citadel",
                    Description="Wraith guards patrol halls of darkened metal and stone.",
                    KingLaunches=6, DifficultyRating=9, RequiredStarsToUnlock=12, SceneName="Level",
                    RequiredScore=6100, TwoStarScore=10200, ThreeStarScore=15300, MustRescueQueen=true,
                    OneStar_DestructionMin=0.30f, TwoStar_DestructionMin=0.65f, TwoStar_EnemyRatioMin=0.80f,
                    ThreeStar_DestructionMin=0.90f, ThreeStar_EnemyRatioMin=1.00f, ThreeStar_AttemptsRemaining=1,
                    GuardCount=6, ShieldGuardCount=4, ArcherCount=4, HasEnemyKing=true, HasExplosiveBarrel=true, HasTutorialHints=false,
                    PrimaryMaterial="Stone", SecondaryMaterial="Metal", CastleFloors=6, HasQueen=true,
                },
                new LevelDefinition
                {
                    LevelIndex=73, WorldIndex=3, DisplayName="Labyrinth of Shadows",
                    Description="A labyrinthine dark fortress where every path leads to danger.",
                    KingLaunches=6, DifficultyRating=9, RequiredStarsToUnlock=13, SceneName="Level",
                    RequiredScore=6400, TwoStarScore=10700, ThreeStarScore=16000, MustRescueQueen=true,
                    OneStar_DestructionMin=0.30f, TwoStar_DestructionMin=0.65f, TwoStar_EnemyRatioMin=0.80f,
                    ThreeStar_DestructionMin=0.90f, ThreeStar_EnemyRatioMin=1.00f, ThreeStar_AttemptsRemaining=1,
                    GuardCount=6, ShieldGuardCount=5, ArcherCount=4, HasEnemyKing=true, HasExplosiveBarrel=false, HasTutorialHints=false,
                    PrimaryMaterial="Metal", SecondaryMaterial="Stone", CastleFloors=6, HasQueen=true,
                },
                new LevelDefinition
                {
                    LevelIndex=74, WorldIndex=3, DisplayName="Dark Throne Antechamber",
                    Description="The hall before the Dark King's throne — the hardest yet.",
                    KingLaunches=6, DifficultyRating=10, RequiredStarsToUnlock=14, SceneName="Level",
                    RequiredScore=6800, TwoStarScore=11300, ThreeStarScore=17000, MustRescueQueen=true,
                    OneStar_DestructionMin=0.30f, TwoStar_DestructionMin=0.65f, TwoStar_EnemyRatioMin=0.80f,
                    ThreeStar_DestructionMin=0.90f, ThreeStar_EnemyRatioMin=1.00f, ThreeStar_AttemptsRemaining=1,
                    GuardCount=6, ShieldGuardCount=5, ArcherCount=5, HasEnemyKing=true, HasExplosiveBarrel=true, HasTutorialHints=false,
                    PrimaryMaterial="Stone", SecondaryMaterial="Metal", CastleFloors=7, HasQueen=true,
                },
                new LevelDefinition
                {
                    LevelIndex=75, WorldIndex=3, DisplayName="Abyss Ramparts",
                    Description="Ramparts perched over an endless abyss. Don't look down.",
                    KingLaunches=6, DifficultyRating=10, RequiredStarsToUnlock=15, SceneName="Level",
                    RequiredScore=7200, TwoStarScore=12000, ThreeStarScore=18000, MustRescueQueen=true,
                    OneStar_DestructionMin=0.30f, TwoStar_DestructionMin=0.65f, TwoStar_EnemyRatioMin=0.80f,
                    ThreeStar_DestructionMin=0.90f, ThreeStar_EnemyRatioMin=1.00f, ThreeStar_AttemptsRemaining=1,
                    GuardCount=6, ShieldGuardCount=5, ArcherCount=5, HasEnemyKing=true, HasExplosiveBarrel=false, HasTutorialHints=false,
                    PrimaryMaterial="Metal", SecondaryMaterial="Stone", CastleFloors=7, HasQueen=true,
                },
                new LevelDefinition
                {
                    LevelIndex=76, WorldIndex=3, DisplayName="Twilight Bastion",
                    Description="Reinforced walls that glow with dark energy.",
                    KingLaunches=6, DifficultyRating=10, RequiredStarsToUnlock=16, SceneName="Level",
                    RequiredScore=7600, TwoStarScore=12700, ThreeStarScore=19000, MustRescueQueen=true,
                    OneStar_DestructionMin=0.30f, TwoStar_DestructionMin=0.65f, TwoStar_EnemyRatioMin=0.80f,
                    ThreeStar_DestructionMin=0.90f, ThreeStar_EnemyRatioMin=1.00f, ThreeStar_AttemptsRemaining=1,
                    GuardCount=6, ShieldGuardCount=5, ArcherCount=5, HasEnemyKing=true, HasExplosiveBarrel=true, HasTutorialHints=false,
                    PrimaryMaterial="Stone", SecondaryMaterial="Metal", CastleFloors=7, HasQueen=true,
                },
                new LevelDefinition
                {
                    LevelIndex=77, WorldIndex=3, DisplayName="Night Eternal",
                    Description="A fortress where perpetual night hides countless enemies.",
                    KingLaunches=7, DifficultyRating=10, RequiredStarsToUnlock=17, SceneName="Level",
                    RequiredScore=8100, TwoStarScore=13500, ThreeStarScore=20200, MustRescueQueen=true,
                    OneStar_DestructionMin=0.30f, TwoStar_DestructionMin=0.65f, TwoStar_EnemyRatioMin=0.80f,
                    ThreeStar_DestructionMin=0.90f, ThreeStar_EnemyRatioMin=1.00f, ThreeStar_AttemptsRemaining=1,
                    GuardCount=7, ShieldGuardCount=5, ArcherCount=5, HasEnemyKing=true, HasExplosiveBarrel=true, HasTutorialHints=false,
                    PrimaryMaterial="Metal", SecondaryMaterial="Stone", CastleFloors=7, HasQueen=true,
                },
                new LevelDefinition
                {
                    LevelIndex=78, WorldIndex=3, DisplayName="Throne of Darkness",
                    Description="The inner sanctum of the Dark King — destroy his power base.",
                    KingLaunches=7, DifficultyRating=10, RequiredStarsToUnlock=18, SceneName="Level",
                    RequiredScore=8600, TwoStarScore=14300, ThreeStarScore=21500, MustRescueQueen=true,
                    OneStar_DestructionMin=0.30f, TwoStar_DestructionMin=0.65f, TwoStar_EnemyRatioMin=0.80f,
                    ThreeStar_DestructionMin=0.90f, ThreeStar_EnemyRatioMin=1.00f, ThreeStar_AttemptsRemaining=1,
                    GuardCount=7, ShieldGuardCount=6, ArcherCount=5, HasEnemyKing=true, HasExplosiveBarrel=true, HasTutorialHints=false,
                    PrimaryMaterial="Metal", SecondaryMaterial="Stone", CastleFloors=8, HasQueen=true,
                },
                new LevelDefinition
                {
                    LevelIndex=79, WorldIndex=3, DisplayName="Dark King's Dominion",
                    Description="Face the Dark King in his final domain. This is the ultimate dark challenge!",
                    KingLaunches=8, DifficultyRating=10, RequiredStarsToUnlock=19, SceneName="Level",
                    RequiredScore=10000, TwoStarScore=16500, ThreeStarScore=25000, MustRescueQueen=true,
                    OneStar_DestructionMin=0.30f, TwoStar_DestructionMin=0.65f, TwoStar_EnemyRatioMin=0.80f,
                    ThreeStar_DestructionMin=0.90f, ThreeStar_EnemyRatioMin=1.00f, ThreeStar_AttemptsRemaining=0,
                    GuardCount=7, ShieldGuardCount=6, ArcherCount=5, HasEnemyKing=true, HasExplosiveBarrel=true, HasTutorialHints=false,
                    PrimaryMaterial="Metal", SecondaryMaterial="Stone", CastleFloors=8, HasQueen=true, IsBossLevel=true, MaxPowerUpsAllowed=3,
                },

                // ── WORLD 4: FINAL (L80-99) ──────────────────────────────

                new LevelDefinition
                {
                    LevelIndex=80, WorldIndex=4, DisplayName="Gates of the Final Realm",
                    Description="The legendary gates to the Final Realm. The ultimate trial begins.",
                    KingLaunches=5, DifficultyRating=6, RequiredStarsToUnlock=0, SceneName="Level",
                    RequiredScore=5000, TwoStarScore=8500, ThreeStarScore=12700, MustRescueQueen=true,
                    OneStar_DestructionMin=0.30f, TwoStar_DestructionMin=0.70f, TwoStar_EnemyRatioMin=0.70f,
                    ThreeStar_DestructionMin=0.92f, ThreeStar_EnemyRatioMin=1.00f, ThreeStar_AttemptsRemaining=1,
                    GuardCount=4, ShieldGuardCount=3, ArcherCount=2, HasEnemyKing=false, HasExplosiveBarrel=false, HasTutorialHints=false,
                    PrimaryMaterial="Metal", SecondaryMaterial="Stone", CastleFloors=4, HasQueen=true,
                },
                new LevelDefinition
                {
                    LevelIndex=81, WorldIndex=4, DisplayName="Titan's Rampart",
                    Description="Walls built by giants — only the strongest blow will crack them.",
                    KingLaunches=5, DifficultyRating=7, RequiredStarsToUnlock=1, SceneName="Level",
                    RequiredScore=5400, TwoStarScore=9000, ThreeStarScore=13500, MustRescueQueen=true,
                    OneStar_DestructionMin=0.30f, TwoStar_DestructionMin=0.70f, TwoStar_EnemyRatioMin=0.70f,
                    ThreeStar_DestructionMin=0.92f, ThreeStar_EnemyRatioMin=1.00f, ThreeStar_AttemptsRemaining=1,
                    GuardCount=4, ShieldGuardCount=3, ArcherCount=3, HasEnemyKing=false, HasExplosiveBarrel=true, HasTutorialHints=false,
                    PrimaryMaterial="Metal", SecondaryMaterial="Stone", CastleFloors=5, HasQueen=true,
                },
                new LevelDefinition
                {
                    LevelIndex=82, WorldIndex=4, DisplayName="Celestial Bastion",
                    Description="A fortress said to have fallen from the heavens.",
                    KingLaunches=5, DifficultyRating=7, RequiredStarsToUnlock=2, SceneName="Level",
                    RequiredScore=5800, TwoStarScore=9700, ThreeStarScore=14500, MustRescueQueen=true,
                    OneStar_DestructionMin=0.30f, TwoStar_DestructionMin=0.70f, TwoStar_EnemyRatioMin=0.70f,
                    ThreeStar_DestructionMin=0.92f, ThreeStar_EnemyRatioMin=1.00f, ThreeStar_AttemptsRemaining=1,
                    GuardCount=5, ShieldGuardCount=3, ArcherCount=3, HasEnemyKing=false, HasExplosiveBarrel=false, HasTutorialHints=false,
                    PrimaryMaterial="Metal", SecondaryMaterial="Stone", CastleFloors=5, HasQueen=true,
                },
                new LevelDefinition
                {
                    LevelIndex=83, WorldIndex=4, DisplayName="Eternal Siege",
                    Description="An ongoing siege that has lasted for a thousand years.",
                    KingLaunches=5, DifficultyRating=7, RequiredStarsToUnlock=3, SceneName="Level",
                    RequiredScore=6200, TwoStarScore=10300, ThreeStarScore=15500, MustRescueQueen=true,
                    OneStar_DestructionMin=0.30f, TwoStar_DestructionMin=0.70f, TwoStar_EnemyRatioMin=0.70f,
                    ThreeStar_DestructionMin=0.92f, ThreeStar_EnemyRatioMin=1.00f, ThreeStar_AttemptsRemaining=1,
                    GuardCount=5, ShieldGuardCount=4, ArcherCount=3, HasEnemyKing=false, HasExplosiveBarrel=true, HasTutorialHints=false,
                    PrimaryMaterial="Stone", SecondaryMaterial="Metal", CastleFloors=5, HasQueen=true,
                },
                new LevelDefinition
                {
                    LevelIndex=84, WorldIndex=4, DisplayName="Ironclad Fortress",
                    Description="Every surface clad in iron — impervious to all but the strongest blow.",
                    KingLaunches=5, DifficultyRating=8, RequiredStarsToUnlock=4, SceneName="Level",
                    RequiredScore=6700, TwoStarScore=11200, ThreeStarScore=16800, MustRescueQueen=true,
                    OneStar_DestructionMin=0.30f, TwoStar_DestructionMin=0.70f, TwoStar_EnemyRatioMin=0.75f,
                    ThreeStar_DestructionMin=0.92f, ThreeStar_EnemyRatioMin=1.00f, ThreeStar_AttemptsRemaining=1,
                    GuardCount=5, ShieldGuardCount=4, ArcherCount=4, HasEnemyKing=false, HasExplosiveBarrel=false, HasTutorialHints=false,
                    PrimaryMaterial="Metal", SecondaryMaterial="Stone", CastleFloors=6, HasQueen=true,
                },
                new LevelDefinition
                {
                    LevelIndex=85, WorldIndex=4, DisplayName="Skyfall Citadel",
                    Description="A citadel floating on shattered earth — one wrong hit collapses it all.",
                    KingLaunches=5, DifficultyRating=8, RequiredStarsToUnlock=5, SceneName="Level",
                    RequiredScore=7200, TwoStarScore=12000, ThreeStarScore=18000, MustRescueQueen=true,
                    OneStar_DestructionMin=0.30f, TwoStar_DestructionMin=0.70f, TwoStar_EnemyRatioMin=0.75f,
                    ThreeStar_DestructionMin=0.92f, ThreeStar_EnemyRatioMin=1.00f, ThreeStar_AttemptsRemaining=1,
                    GuardCount=5, ShieldGuardCount=4, ArcherCount=4, HasEnemyKing=false, HasExplosiveBarrel=true, HasTutorialHints=false,
                    PrimaryMaterial="Metal", SecondaryMaterial="Stone", CastleFloors=6, HasQueen=true,
                },
                new LevelDefinition
                {
                    LevelIndex=86, WorldIndex=4, DisplayName="Warlord's Stronghold",
                    Description="The warlord commands from an impenetrable mountain stronghold.",
                    KingLaunches=6, DifficultyRating=8, RequiredStarsToUnlock=6, SceneName="Level",
                    RequiredScore=7700, TwoStarScore=12900, ThreeStarScore=19300, MustRescueQueen=true,
                    OneStar_DestructionMin=0.30f, TwoStar_DestructionMin=0.70f, TwoStar_EnemyRatioMin=0.75f,
                    ThreeStar_DestructionMin=0.92f, ThreeStar_EnemyRatioMin=1.00f, ThreeStar_AttemptsRemaining=1,
                    GuardCount=5, ShieldGuardCount=4, ArcherCount=5, HasEnemyKing=true, HasExplosiveBarrel=false, HasTutorialHints=false,
                    PrimaryMaterial="Stone", SecondaryMaterial="Metal", CastleFloors=6, HasQueen=true,
                },
                new LevelDefinition
                {
                    LevelIndex=87, WorldIndex=4, DisplayName="Ember Colosseum",
                    Description="Flames ring the colosseum walls — the ultimate arena.",
                    KingLaunches=6, DifficultyRating=8, RequiredStarsToUnlock=7, SceneName="Level",
                    RequiredScore=8200, TwoStarScore=13700, ThreeStarScore=20500, MustRescueQueen=true,
                    OneStar_DestructionMin=0.30f, TwoStar_DestructionMin=0.70f, TwoStar_EnemyRatioMin=0.75f,
                    ThreeStar_DestructionMin=0.92f, ThreeStar_EnemyRatioMin=1.00f, ThreeStar_AttemptsRemaining=1,
                    GuardCount=6, ShieldGuardCount=4, ArcherCount=5, HasEnemyKing=true, HasExplosiveBarrel=true, HasTutorialHints=false,
                    PrimaryMaterial="Metal", SecondaryMaterial="Stone", CastleFloors=7, HasQueen=true,
                },
                new LevelDefinition
                {
                    LevelIndex=88, WorldIndex=4, DisplayName="Demigod's Keep",
                    Description="Not built by mortals — this keep defies the laws of nature.",
                    KingLaunches=6, DifficultyRating=9, RequiredStarsToUnlock=8, SceneName="Level",
                    RequiredScore=8800, TwoStarScore=14700, ThreeStarScore=22000, MustRescueQueen=true,
                    OneStar_DestructionMin=0.30f, TwoStar_DestructionMin=0.70f, TwoStar_EnemyRatioMin=0.80f,
                    ThreeStar_DestructionMin=0.92f, ThreeStar_EnemyRatioMin=1.00f, ThreeStar_AttemptsRemaining=1,
                    GuardCount=6, ShieldGuardCount=5, ArcherCount=5, HasEnemyKing=true, HasExplosiveBarrel=false, HasTutorialHints=false,
                    PrimaryMaterial="Metal", SecondaryMaterial="Stone", CastleFloors=7, HasQueen=true,
                },
                new LevelDefinition
                {
                    LevelIndex=89, WorldIndex=4, DisplayName="Void Nexus",
                    Description="A nexus of void energy powers this fortress of pure destruction.",
                    KingLaunches=6, DifficultyRating=9, RequiredStarsToUnlock=9, SceneName="Level",
                    RequiredScore=9400, TwoStarScore=15700, ThreeStarScore=23500, MustRescueQueen=true,
                    OneStar_DestructionMin=0.30f, TwoStar_DestructionMin=0.70f, TwoStar_EnemyRatioMin=0.80f,
                    ThreeStar_DestructionMin=0.92f, ThreeStar_EnemyRatioMin=1.00f, ThreeStar_AttemptsRemaining=1,
                    GuardCount=6, ShieldGuardCount=5, ArcherCount=5, HasEnemyKing=true, HasExplosiveBarrel=true, HasTutorialHints=false,
                    PrimaryMaterial="Stone", SecondaryMaterial="Metal", CastleFloors=7, HasQueen=true,
                },
                new LevelDefinition
                {
                    LevelIndex=90, WorldIndex=4, DisplayName="Titan's Wall",
                    Description="A wall stretching to the horizon — impossible to go around.",
                    KingLaunches=6, DifficultyRating=9, RequiredStarsToUnlock=10, SceneName="Level",
                    RequiredScore=10000, TwoStarScore=16700, ThreeStarScore=25000, MustRescueQueen=true,
                    OneStar_DestructionMin=0.30f, TwoStar_DestructionMin=0.70f, TwoStar_EnemyRatioMin=0.80f,
                    ThreeStar_DestructionMin=0.92f, ThreeStar_EnemyRatioMin=1.00f, ThreeStar_AttemptsRemaining=1,
                    GuardCount=6, ShieldGuardCount=5, ArcherCount=6, HasEnemyKing=true, HasExplosiveBarrel=false, HasTutorialHints=false,
                    PrimaryMaterial="Metal", SecondaryMaterial="Stone", CastleFloors=8, HasQueen=true,
                },
                new LevelDefinition
                {
                    LevelIndex=91, WorldIndex=4, DisplayName="Arcane Fortress",
                    Description="Ancient arcane wards reinforce every stone of this fortress.",
                    KingLaunches=7, DifficultyRating=9, RequiredStarsToUnlock=11, SceneName="Level",
                    RequiredScore=10700, TwoStarScore=17800, ThreeStarScore=26700, MustRescueQueen=true,
                    OneStar_DestructionMin=0.30f, TwoStar_DestructionMin=0.70f, TwoStar_EnemyRatioMin=0.80f,
                    ThreeStar_DestructionMin=0.92f, ThreeStar_EnemyRatioMin=1.00f, ThreeStar_AttemptsRemaining=1,
                    GuardCount=7, ShieldGuardCount=5, ArcherCount=5, HasEnemyKing=true, HasExplosiveBarrel=true, HasTutorialHints=false,
                    PrimaryMaterial="Stone", SecondaryMaterial="Metal", CastleFloors=8, HasQueen=true,
                },
                new LevelDefinition
                {
                    LevelIndex=92, WorldIndex=4, DisplayName="Colossus Gate",
                    Description="A gate guarded by mechanical golems — unstoppable until broken.",
                    KingLaunches=7, DifficultyRating=9, RequiredStarsToUnlock=12, SceneName="Level",
                    RequiredScore=11400, TwoStarScore=19000, ThreeStarScore=28500, MustRescueQueen=true,
                    OneStar_DestructionMin=0.30f, TwoStar_DestructionMin=0.70f, TwoStar_EnemyRatioMin=0.80f,
                    ThreeStar_DestructionMin=0.92f, ThreeStar_EnemyRatioMin=1.00f, ThreeStar_AttemptsRemaining=1,
                    GuardCount=7, ShieldGuardCount=6, ArcherCount=5, HasEnemyKing=true, HasExplosiveBarrel=false, HasTutorialHints=false,
                    PrimaryMaterial="Metal", SecondaryMaterial="Stone", CastleFloors=8, HasQueen=true,
                },
                new LevelDefinition
                {
                    LevelIndex=93, WorldIndex=4, DisplayName="Supreme Citadel",
                    Description="The mightiest citadel ever constructed. A king's masterwork.",
                    KingLaunches=7, DifficultyRating=10, RequiredStarsToUnlock=13, SceneName="Level",
                    RequiredScore=12200, TwoStarScore=20300, ThreeStarScore=30500, MustRescueQueen=true,
                    OneStar_DestructionMin=0.30f, TwoStar_DestructionMin=0.70f, TwoStar_EnemyRatioMin=0.80f,
                    ThreeStar_DestructionMin=0.92f, ThreeStar_EnemyRatioMin=1.00f, ThreeStar_AttemptsRemaining=1,
                    GuardCount=7, ShieldGuardCount=6, ArcherCount=6, HasEnemyKing=true, HasExplosiveBarrel=true, HasTutorialHints=false,
                    PrimaryMaterial="Metal", SecondaryMaterial="Stone", CastleFloors=9, HasQueen=true,
                },
                new LevelDefinition
                {
                    LevelIndex=94, WorldIndex=4, DisplayName="World's End Ramparts",
                    Description="Ramparts at the edge of the known world — past here be legends.",
                    KingLaunches=7, DifficultyRating=10, RequiredStarsToUnlock=14, SceneName="Level",
                    RequiredScore=13000, TwoStarScore=21700, ThreeStarScore=32500, MustRescueQueen=true,
                    OneStar_DestructionMin=0.30f, TwoStar_DestructionMin=0.70f, TwoStar_EnemyRatioMin=0.80f,
                    ThreeStar_DestructionMin=0.92f, ThreeStar_EnemyRatioMin=1.00f, ThreeStar_AttemptsRemaining=1,
                    GuardCount=7, ShieldGuardCount=6, ArcherCount=6, HasEnemyKing=true, HasExplosiveBarrel=false, HasTutorialHints=false,
                    PrimaryMaterial="Stone", SecondaryMaterial="Metal", CastleFloors=9, HasQueen=true,
                },
                new LevelDefinition
                {
                    LevelIndex=95, WorldIndex=4, DisplayName="Godking's Vault",
                    Description="The sacred vault of the Godking holds the greatest treasures.",
                    KingLaunches=7, DifficultyRating=10, RequiredStarsToUnlock=15, SceneName="Level",
                    RequiredScore=13800, TwoStarScore=23000, ThreeStarScore=34500, MustRescueQueen=true,
                    OneStar_DestructionMin=0.30f, TwoStar_DestructionMin=0.70f, TwoStar_EnemyRatioMin=0.80f,
                    ThreeStar_DestructionMin=0.92f, ThreeStar_EnemyRatioMin=1.00f, ThreeStar_AttemptsRemaining=1,
                    GuardCount=7, ShieldGuardCount=7, ArcherCount=6, HasEnemyKing=true, HasExplosiveBarrel=true, HasTutorialHints=false,
                    PrimaryMaterial="Metal", SecondaryMaterial="Stone", CastleFloors=9, HasQueen=true,
                },
                new LevelDefinition
                {
                    LevelIndex=96, WorldIndex=4, DisplayName="Infinity Spire",
                    Description="A spire that seems to reach infinity — topple it with precision.",
                    KingLaunches=7, DifficultyRating=10, RequiredStarsToUnlock=16, SceneName="Level",
                    RequiredScore=14700, TwoStarScore=24500, ThreeStarScore=36700, MustRescueQueen=true,
                    OneStar_DestructionMin=0.30f, TwoStar_DestructionMin=0.70f, TwoStar_EnemyRatioMin=0.80f,
                    ThreeStar_DestructionMin=0.92f, ThreeStar_EnemyRatioMin=1.00f, ThreeStar_AttemptsRemaining=1,
                    GuardCount=8, ShieldGuardCount=6, ArcherCount=6, HasEnemyKing=true, HasExplosiveBarrel=true, HasTutorialHints=false,
                    PrimaryMaterial="Metal", SecondaryMaterial="Stone", CastleFloors=10, HasQueen=true,
                },
                new LevelDefinition
                {
                    LevelIndex=97, WorldIndex=4, DisplayName="Omega Stronghold",
                    Description="The Omega Stronghold — final bastion of the enemy's empire.",
                    KingLaunches=8, DifficultyRating=10, RequiredStarsToUnlock=17, SceneName="Level",
                    RequiredScore=15700, TwoStarScore=26200, ThreeStarScore=39300, MustRescueQueen=true,
                    OneStar_DestructionMin=0.30f, TwoStar_DestructionMin=0.70f, TwoStar_EnemyRatioMin=0.80f,
                    ThreeStar_DestructionMin=0.92f, ThreeStar_EnemyRatioMin=1.00f, ThreeStar_AttemptsRemaining=1,
                    GuardCount=8, ShieldGuardCount=7, ArcherCount=6, HasEnemyKing=true, HasExplosiveBarrel=true, HasTutorialHints=false,
                    PrimaryMaterial="Stone", SecondaryMaterial="Metal", CastleFloors=10, HasQueen=true,
                },
                new LevelDefinition
                {
                    LevelIndex=98, WorldIndex=4, DisplayName="Penultimate Throne",
                    Description="One step from victory — the Ultimate King prepares his final stand.",
                    KingLaunches=8, DifficultyRating=10, RequiredStarsToUnlock=18, SceneName="Level",
                    RequiredScore=16800, TwoStarScore=28000, ThreeStarScore=42000, MustRescueQueen=true,
                    OneStar_DestructionMin=0.30f, TwoStar_DestructionMin=0.70f, TwoStar_EnemyRatioMin=0.80f,
                    ThreeStar_DestructionMin=0.92f, ThreeStar_EnemyRatioMin=1.00f, ThreeStar_AttemptsRemaining=1,
                    GuardCount=8, ShieldGuardCount=7, ArcherCount=7, HasEnemyKing=true, HasExplosiveBarrel=true, HasTutorialHints=false,
                    PrimaryMaterial="Metal", SecondaryMaterial="Stone", CastleFloors=10, HasQueen=true,
                },
                new LevelDefinition
                {
                    LevelIndex=99, WorldIndex=4, DisplayName="The Final Battle",
                    Description="The Ultimate King. The Ultimate Castle. This is King Smash — THE FINAL BATTLE!",
                    KingLaunches=10, DifficultyRating=10, RequiredStarsToUnlock=19, SceneName="Level",
                    RequiredScore=20000, TwoStarScore=33000, ThreeStarScore=50000, MustRescueQueen=true,
                    OneStar_DestructionMin=0.30f, TwoStar_DestructionMin=0.70f, TwoStar_EnemyRatioMin=0.80f,
                    ThreeStar_DestructionMin=0.92f, ThreeStar_EnemyRatioMin=1.00f, ThreeStar_AttemptsRemaining=0,
                    GuardCount=8, ShieldGuardCount=7, ArcherCount=7, HasEnemyKing=true, HasExplosiveBarrel=true, HasTutorialHints=false,
                    PrimaryMaterial="Metal", SecondaryMaterial="Stone", CastleFloors=10, HasQueen=true, IsBossLevel=true, MaxPowerUpsAllowed=3,
                },
            };
        }
    }
}
