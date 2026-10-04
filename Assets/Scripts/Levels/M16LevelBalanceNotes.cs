// M16 Balance Notes — documents all M16 level balance decisions.
// Actual data changes applied to LevelConfigFactory.cs directly.
// This file exists for audit trail purposes only.
namespace KingSmash.Levels
{
    // M16 Soft-Launch Balance Patch
    // PATCH-001: LevelIndex=9, DifficultyRating changed from 10 to 7.
    //   Rationale: premature max-difficulty spike in early game.
    //   Level 9 ("Mini Fortress") had DifficultyRating=10 — same as the World 0 boss (Level 19).
    //   At position 10/20 in World 0, this creates an early-game abandonment wall.
    //   Natural curve expects ~7 at this position. Structural content unchanged.
    //   Applied: 2026-10-05
    //   Verified: LevelComprehensiveTests pass (DifficultyRating range 1-10 still satisfied)
    //   Author: Balance Design Review (M16)
    internal static class M16LevelBalanceNotes { }
}
