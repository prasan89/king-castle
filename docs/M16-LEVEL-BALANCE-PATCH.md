# King Smash M16 — Level Balance Patch Log

**Version:** 1.0.0-sl1 → 1.0.0-sl1-patch1  
**Milestone:** M16 Soft Launch  
**Date:** 2026-10-05  
**Author:** Balance Design Review

> Data-only fixes do not increment the Remote Config version counter.
> This log records LevelConfigFactory.cs changes only. Firebase RC adjustments are tracked
> separately in the RC dashboard and documented in docs/M16-REMOTE-CONFIG-PROFILE.md.

---

## PATCH-001: Level 9 Difficulty Rating Correction

**File:** `Assets/Scripts/Levels/LevelConfigFactory.cs`  
**Line:** ~118  
**Change:** `LevelIndex=9` — `DifficultyRating`: `10` → `7`

**Pre-patch entry (relevant fields):**
```csharp
LevelIndex=9, WorldIndex=0, DisplayName="Mini Fortress",
KingLaunches=5, DifficultyRating=10, ...
GuardCount=2, ShieldGuardCount=1, ArcherCount=1,
HasEnemyKing=true, HasExplosiveBarrel=true,
PrimaryMaterial="Stone", SecondaryMaterial="Metal", CastleFloors=3
```

**Post-patch entry (relevant fields):**
```csharp
LevelIndex=9, WorldIndex=0, DisplayName="Mini Fortress",
KingLaunches=5, DifficultyRating=7, ...
GuardCount=2, ShieldGuardCount=1, ArcherCount=1,
HasEnemyKing=true, HasExplosiveBarrel=true,
PrimaryMaterial="Stone", SecondaryMaterial="Metal", CastleFloors=3
```

**Rationale:**  
Level 9 had a premature maximum-difficulty spike (rating 10) at position 10/20 in World 0
(Forest Kingdom), making it as hard as the world boss (Level 19) despite having no boss
designation. The natural World 0 difficulty curve ramps 1→2→3→4→4→5→6→7→8 through Levels 0–8,
then descends for the Level 10 sub-arc reset. A rating of 7 fits this arc correctly and reflects
the level's role as the most complex non-boss challenge in World 0.

Note: `KingLaunches` was NOT changed (actual value is 5, not 3 as originally estimated). The
player has generous attempts; the difficulty is structural (enemy king + explosive barrel + shield
guards + archers + 3-floor stone/metal castle). The DifficultyRating label correction is the
appropriate intervention at this stage.

**Expected impact:**  
- Reduced display of the level in "hardest levels" UI (if any such UI exists)
- Corrected difficulty curve display in any level-select screen that uses DifficultyRating
- Reduced early-game abandonment at Level 9 during soft launch
- No change to actual level gameplay — enemy configuration, materials, and launch count unchanged

**Regression status:**  
`LevelComprehensiveTests` and `WorldValidatorTests` do not validate `DifficultyRating` against
a specific numeric value (only range 1–10 is validated). All existing tests pass.  
**Status: APPLIED**

---

## Pending Remote Config Adjustments

The following parameters are adjusted via Firebase Remote Config and require NO code change.
They are listed here for cross-reference. See `docs/M16-REMOTE-CONFIG-PROFILE.md` for full
RC profile with rationale.

| RC Key                    | Soft-Launch Value | Default Value | Effect                              |
|---------------------------|-------------------|---------------|-------------------------------------|
| `coin_reward_multiplier`  | 1.2               | 1.0           | +20% coin rewards all levels        |
| `upgrade_cost_multiplier` | 0.9               | 1.0           | -10% upgrade costs                  |
| `interstitial_frequency`  | 4                 | 3             | Ad every 4 levels (not 3)           |
| `destruction_multiplier`  | 1.1               | 1.0           | Slightly easier block destruction   |
| `starting_kings`          | 1                 | 1             | No change for launch                |
| `king_launch_power`       | 1.0               | 1.0           | No change for launch                |

These are NOT code changes — applied via Firebase Remote Config dashboard.
RC adjustments can be reverted instantly without a new build submission.

---

## Deferred / Rejected Changes

### Considered: Level 9 KingLaunches increase (3 → 4)
**Decision: N/A — actual value is already 5.**
Pre-analysis estimated KingLaunches=3; actual code shows KingLaunches=5. No change needed.

### Considered: Level 9 structural simplification (remove HasExplosiveBarrel or reduce ShieldGuardCount)
**Decision: DEFERRED to post-launch.**
Structural changes during M16 freeze risk introducing untested regressions. The DifficultyRating
label correction is sufficient for launch. If L9 D7 completion rate falls below 55%, a structural
patch (PATCH-002) will be filed.

### Considered: World 3 L74–L79 difficulty reduction (six consecutive rating-10 levels)
**Decision: MONITOR ONLY.**
These levels are intentional (pre-boss gauntlet). No change until soft-launch data shows
an abnormal stall rate at L74.

---

*End of M16 Level Balance Patch Log.*
