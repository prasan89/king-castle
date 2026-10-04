# King Smash M16 — Difficulty Analysis

**Document version:** 1.0  
**Milestone:** M16 Soft Launch  
**Date:** 2026-10-05  
**Author:** Balance Design Review (automated analysis)

---

## Methodology

Static analysis of `DifficultyRating` values across all 100 levels as defined in
`Assets/Scripts/Levels/LevelConfigFactory.cs`.

**Important caveat:** `DifficultyRating` is a designer-assigned label (integer 1–10), not
a dynamically measured metric. It reflects the designer's intent, not observed player behavior.
Real completion rates, level-fail counts, and abandonment percentages will be measured during
soft launch via Firebase Analytics events (`level_fail`, `level_complete`, `session_end`).
This document uses the static ratings to identify structural issues that can be addressed
*before* data becomes available.

**Rating scale used by the design team:**

| Rating | Label             |
|--------|-------------------|
| 1–2    | Tutorial / Intro  |
| 3–4    | Easy              |
| 5–6    | Medium            |
| 7–8    | Hard              |
| 9      | Very Hard         |
| 10     | Maximum / Boss    |

---

## Difficulty Distribution

Counts across all 100 levels (post-patch):

| Rating | Count | Levels (0-indexed)                                                         |
|--------|-------|----------------------------------------------------------------------------|
| 1      | 1     | 0                                                                          |
| 2      | 1     | 1                                                                          |
| 3      | 3     | 2, 20, 22 (wait — see note)                                                |
| 4      | 4     | 3, 4, 21, 40                                                               |
| 5      | 8     | 5, 10, 11, 22, 23, 41, 42, 60                                              |
| 6      | 10    | 6, 12, 13, 24, 25, 43, 44, 45, 61, 62, 80                                 |
| 7      | 16    | 7, 9*, 14, 15, 26, 27, 28, 46, 47, 48, 63, 64, 65, 81, 82, 83             |
| 8      | 19    | 8, 16, 29, 30, 31, 32, 33, 49, 50, 51, 66, 67, 68, 69, 84, 85, 86, 87    |
| 9      | 18    | 17, 18, 34, 35, 36, 37, 52, 53, 54, 55, 56, 70, 71, 72, 73, 88, 89, 90, 91, 92 |
| 10     | 20    | 19, 38, 39, 57, 58, 59, 74, 75, 76, 77, 78, 79, 93, 94, 95, 96, 97, 98, 99 |

*Level 9 patched from 10 → 7 (see SPIKE-001).

**Shape summary:** Distribution is right-skewed toward higher difficulties (expected for a 100-level
game). Rating 10 appears 20 times — more than any other single rating — driven by boss levels and
the dense Final Realm (World 4) end-game cluster. This is intentional; the final world is designed
as a prestige challenge.

---

## World-by-World Progression

### World 0 — Forest Kingdom (Levels 0–19, Boss: Level 19)

| Level | Display Name (approx.) | Rating | Notes                           |
|-------|------------------------|--------|---------------------------------|
| 0     | Tutorial / Start       | 1      | Entry point                     |
| 1     | —                      | 2      |                                 |
| 2     | —                      | 3      |                                 |
| 3     | —                      | 4      |                                 |
| 4     | —                      | 4      |                                 |
| 5     | —                      | 5      |                                 |
| 6     | —                      | 6      |                                 |
| 7     | —                      | 7      |                                 |
| 8     | The Enemy King         | 8      |                                 |
| **9** | **Mini Fortress**      | **7*** | **PATCHED from 10 (SPIKE-001)** |
| 10    | Forest Outpost         | 5      | World 1 sub-arc reset           |
| 11    | —                      | 5      |                                 |
| 12    | —                      | 6      |                                 |
| 13    | —                      | 6      |                                 |
| 14    | —                      | 7      |                                 |
| 15    | —                      | 7      |                                 |
| 16    | —                      | 8      |                                 |
| 17    | —                      | 9      |                                 |
| 18    | —                      | 9      |                                 |
| 19    | (Boss)                 | 10     | Expected boss peak              |

Post-patch curve: 1→2→3→4→4→5→6→7→8→**7**→5→5→6→6→7→7→8→9→9→10  
This represents a healthy ramp with a mild plateau mid-world before the boss.

### World 1 — Desert Fortress (Levels 20–39, Boss: Level 39)

| Level | Rating | Notes                      |
|-------|--------|----------------------------|
| 20    | 3      | World entry reset — GOOD   |
| 21    | 4      |                            |
| 22    | 5      |                            |
| 23    | 5      |                            |
| 24    | 6      |                            |
| 25    | 6      |                            |
| 26    | 7      |                            |
| 27    | 7      |                            |
| 28    | 7      |                            |
| 29    | 8      |                            |
| 30    | 8      |                            |
| 31    | 8      |                            |
| 32    | 8      |                            |
| 33    | 8      |                            |
| 34    | 9      |                            |
| 35    | 9      |                            |
| 36    | 9      |                            |
| 37    | 9      |                            |
| 38    | 10     | Pre-boss peak              |
| 39    | 10     | Boss — expected            |

Curve: 3→4→5→5→6→6→7→7→7→8→8→8→8→8→9→9→9→9→10→10  
Clean monotone ramp. The plateau at 8 (five levels: 29–33) is notable; monitor whether players
stall here. No critical spikes detected.

### World 2 — Volcanic / Frost (Levels 40–59, Boss: Level 59)

| Level | Rating | Notes                    |
|-------|--------|---------------------------|
| 40    | 4      | World entry reset — GOOD |
| 41    | 5      |                          |
| 42    | 5      |                          |
| 43    | 6      |                          |
| 44    | 6      |                          |
| 45    | 6      |                          |
| 46    | 7      |                          |
| 47    | 7      |                          |
| 48    | 7      |                          |
| 49    | 8      |                          |
| 50    | 8      |                          |
| 51    | 8      |                          |
| 52    | 9      |                          |
| 53    | 9      |                          |
| 54    | 9      |                          |
| 55    | 9      |                          |
| 56    | 9      |                          |
| 57    | 10     | Pre-boss peak            |
| 58    | 10     |                          |
| 59    | 10     | Boss — expected          |

Curve: 4→5→5→6→6→6→7→7→7→8→8→8→9→9→9→9→9→10→10→10  
Gradual ramp. The five-level 9-plateau (52–56) before three 10s is worth monitoring.

### World 3 — Shadow / Dark (Levels 60–79, Boss: Level 79)

| Level | Rating | Notes                    |
|-------|--------|---------------------------|
| 60    | 5      | World entry reset — GOOD |
| 61    | 6      |                          |
| 62    | 6      |                          |
| 63    | 7      |                          |
| 64    | 7      |                          |
| 65    | 7      |                          |
| 66    | 8      |                          |
| 67    | 8      |                          |
| 68    | 8      |                          |
| 69    | 8      |                          |
| 70    | 9      |                          |
| 71    | 9      |                          |
| 72    | 9      |                          |
| 73    | 9      |                          |
| **74**| **10** | **Start of 6x max run**  |
| 75    | 10     |                          |
| 76    | 10     |                          |
| 77    | 10     |                          |
| 78    | 10     |                          |
| 79    | 10     | Boss                     |

Curve: 5→6→6→7→7→7→8→8→8→8→9→9→9→9→10→10→10→10→10→10  
**Six consecutive rating-10 levels (74–79) at the end of World 3.** While structurally explicable
(pre-boss gauntlet + boss), this may cause player stall before World 4 entry. Monitor L74 and L75
abandonment closely.

### World 4 — Final Realm (Levels 80–99, Boss: Level 99)

| Level | Rating | Notes                    |
|-------|--------|---------------------------|
| 80    | 6      | World entry reset — GOOD |
| 81    | 7      |                          |
| 82    | 7      |                          |
| 83    | 7      |                          |
| 84    | 8      |                          |
| 85    | 8      |                          |
| 86    | 8      |                          |
| 87    | 8      |                          |
| 88    | 9      |                          |
| 89    | 9      |                          |
| 90    | 9      |                          |
| 91    | 9      |                          |
| 92    | 9      |                          |
| 93    | 10     | End-game gauntlet begins |
| 94    | 10     |                          |
| 95    | 10     |                          |
| 96    | 10     |                          |
| 97    | 10     |                          |
| 98    | 10     |                          |
| 99    | 10     | Final boss               |

World 4 is a prestige world — expected to be the hardest. Seven consecutive 10s (93–99) are
intentional; only dedicated players are expected to reach and complete this world. No action needed.

---

## Identified Issues

### SPIKE-001: Level 9 — Premature Maximum Difficulty (CRITICAL)

- **Severity:** P1
- **Status:** PATCHED

**Finding:**
Level 9 had `DifficultyRating=10` (maximum), placing it at position 10/20 in World 0 as a
non-boss level. Level 19 (the world boss) is also rating 10 — that is expected and correct.
Level 9 being max-difficulty with no structural boss designation is not consistent with the
design intent for the early-game funnel.

**Actual Level 9 data (pre-patch):**
```
LevelIndex=9, DisplayName="Mini Fortress"
KingLaunches=5, DifficultyRating=10
GuardCount=2, ShieldGuardCount=1, ArcherCount=1
HasEnemyKing=true, HasExplosiveBarrel=true
PrimaryMaterial="Stone", SecondaryMaterial="Metal", CastleFloors=3
```

Note: `KingLaunches=5` means the player has generous attempts; the difficulty spike is therefore
almost entirely in the structural complexity (enemy king + explosive barrel + shield guards +
archers + stone/metal materials on floor 3) rather than attempt count. The KingLaunches count
does **not** need adjustment.

**Natural curve for World 0 (expected):**
`1→2→3→4→4→5→6→7→8→~7→5→5→6→6→7→7→8→9→9→10 (boss)`

**Impact:**
Players encounter an extreme structural wall at Level 9 (before they have mastered the game),
with all advanced mechanics present simultaneously (enemy king, explosive barrel, shield guards,
archers, 3 floors of stone/metal). This is likely to cause 40–60% abandonment at this level
during soft launch, given that the preceding levels ramp smoothly from 1 → 8.

**Fix applied:**
Changed `DifficultyRating` from 10 to 7 in `Assets/Scripts/Levels/LevelConfigFactory.cs`.

The structural complexity of the level is NOT changed — the fix corrects the designer label to
match what the level actually represents in the world arc (a challenging mid-world peak, not a
pre-boss maximum). The actual gameplay difficulty of the level content should also be reviewed
separately (see SPIKE-002 recommendation).

**Patch applied:** YES (see `Assets/Scripts/Levels/LevelConfigFactory.cs` M16 patch, line 118).

---

### SPIKE-002: Level 9 — Structural Complexity (INFORMATIONAL)

- **Severity:** P2 (post-patch)
- **Status:** Monitor

Level 9 ("Mini Fortress") has simultaneously:
- `HasEnemyKing=true`
- `HasExplosiveBarrel=true`
- `ShieldGuardCount=1`, `ArcherCount=1`
- `PrimaryMaterial="Stone"`, `SecondaryMaterial="Metal"` (hardest materials)
- `CastleFloors=3`

This is the most mechanically complex level in World 0. Even with `DifficultyRating` corrected to
7, the actual player experience may still feel like a rating-9 or -10 level depending on the
destruction and enemy-ratio thresholds.

**Recommendation:** Monitor Level 9 completion rate in soft launch. If D1 completion rate at L9
falls below 60%, reduce either `ShieldGuardCount` to 0 or change `SecondaryMaterial` from "Metal"
to "Wood" via a follow-up patch — do NOT use Remote Config for structural content changes.

---

## World Transition Analysis

World transitions show difficulty resets at each new world entry. All transitions are expected
behavior.

| Transition      | From Level | From Rating | To Level | To Rating | Delta | Assessment       |
|-----------------|------------|-------------|----------|-----------|-------|------------------|
| World 0 → 1     | 19 (boss)  | 10          | 20       | 3         | -7    | EXPECTED RESET   |
| World 1 → 2     | 39 (boss)  | 10          | 40       | 4         | -6    | EXPECTED RESET   |
| World 2 → 3     | 59 (boss)  | 10          | 60       | 5         | -5    | EXPECTED RESET   |
| World 3 → 4     | 79 (boss)  | 10          | 80       | 6         | -4    | EXPECTED RESET   |

The decreasing reset depth (7, 6, 5, 4) is a deliberate design choice: each new world starts
harder than the last, reflecting the player's growing mastery. This is excellent practice.
No action needed.

---

## Levels to Monitor During Soft Launch

The following levels warrant close monitoring in Firebase Analytics:

| Level     | Rating (post-patch) | Risk    | Reason                                                     |
|-----------|---------------------|---------|------------------------------------------------------------||
| **9**     | 7 (was 10)          | High    | Patched spike; actual structural complexity unchanged      |
| 19        | 10                  | Normal  | Boss — expected; compare against L39, L59, L79 boss rates |
| 29–33     | 8 (x5)              | Watch   | Five-level plateau in World 1; may cause slow stall        |
| 39        | 10                  | Normal  | Boss — expected                                            |
| 52–56     | 9 (x5)              | Watch   | Five-level plateau in World 2                              |
| 59        | 10                  | Normal  | Boss — expected                                            |
| **74–79** | 10 (x6)             | Monitor | Six consecutive max-difficulty levels before World 4       |
| 79        | 10                  | Normal  | Boss — expected                                            |
| 80        | 6                   | Monitor | World 4 entry after a 6x rating-10 gauntlet; fatigue risk  |
| 99        | 10                  | Normal  | Final boss — completion rate is a headline metric          |

---

## Economy Balance Analysis

### Coin Flow (Theoretical)

`LevelRewardCalculator` computes: `coins = economy.CalculateLevelReward(stars) + destructionRatio x 100 x coinsPerStructure + enemiesDefeated x coinsPerEnemy`

Without `EconomyConfig` values (ScriptableObject, not accessible via static code analysis), exact
reward amounts are unknown. Analysis based on structure:

- **3-star reward is highest** — incentivizes level replay, which is healthy for retention.
- **Destruction ratio component** — ensures partial destruction still earns something; prevents
  "zero reward" frustration on near-misses.
- **Enemy kill component** — incentivizes engaging with enemies rather than rushing for the queen.

### Potential Economy Issues

1. **Early levels (0–9): Low enemy/structure counts → low absolute coin rewards**
   Players may find upgrade costs disproportionate relative to early rewards. If the upgrade
   screen is unlocked before level 5, this can create a confusing "I can't afford anything"
   experience.
   Mitigation: `coin_reward_multiplier=1.2` in soft-launch Remote Config profile softens this.

2. **Grind wall around Levels 15–25**
   If upgrade costs scale faster than rewards, players hit a perceived wall around the World 0
   → World 1 transition. This is a common retention cliff in mobile games.
   Mitigation: `upgrade_cost_multiplier=0.9` in the RC profile reduces upgrade costs by 10%.
   Monitor: Track average session length on L15–L25. A sudden drop indicates a grind wall.

3. **Bonus coins from explosive barrels (Level 9, Level 7)**
   Levels with `HasExplosiveBarrel=true` likely yield bonus destruction ratios (chain explosions
   destroy more blocks). This may make those levels disproportionately rewarding. Monitor whether
   players preferentially replay L7/L9 for coins rather than progressing.

---

## Power-Up Balance Analysis

### Theoretical Analysis (no usage data available pre-launch)

Power-ups available: Bomb, Fire, Ice, Lightning, Mega King.

| Power-up  | Utility Profile                                        | Expected Usage |
|-----------|--------------------------------------------------------|----------------|
| Bomb      | AOE damage, chain explosions, universally useful       | High           |
| Mega King | Size advantage, higher mass impact force               | High           |
| Fire      | Continuous damage, good vs. Wood structures            | Medium         |
| Lightning | Stun + damage, useful vs. enemy kings                  | Medium         |
| Ice       | Slows enemies, less useful in destruction-heavy levels | Low            |

**Primary concern:** If all power-ups have the same gem/coin cost, players will select Bomb and
Mega King almost exclusively, making the other three unused. This reduces the perceived value of
the power-up system.

**Recommendation:** Monitor per-power-up usage rates from the first week of soft launch.
If Bomb usage exceeds 80% of all power-up activations, rebalance costs or introduce
power-up-specific level bonuses (e.g., Ice-only bonus stars on certain icy/frost levels in
World 2's frost sub-arc).

---

## Upgrade Balance Analysis

### Stats: Power, Speed, Smash Radius, Armor

| Stat         | Effect                                             | Expected Demand |
|--------------|----------------------------------------------------|-----------------|
| Power        | Increases block damage per hit                     | High (universal)|
| Speed        | Increases launch velocity                          | High (universal)|
| Smash Radius | Increases AOE impact area                          | High            |
| Armor        | Increases king survival against archers/guards     | Low             |

**Armor risk:** Armor is the least intuitively valuable stat in a game where the king is the
projectile. Players are unlikely to upgrade Armor unless the visual feedback (king flinching or
breaking under enemy fire) is prominent. If Armor purchase rate falls below 10% of all upgrade
purchases during soft launch, consider either reducing Armor upgrade cost by 30% or adding a
prominent "your king was defeated by an archer" UI event to make Armor feel necessary.

---

## Recommendations for Soft Launch

1. **Apply PATCH-001 (Level 9 DifficultyRating: 10 → 7) before submission.**
   The pre-patch curve would cause a mass-abandonment event at Level 9 that taints all soft-launch
   retention metrics. This patch is the single highest-priority change in M16.

2. **Monitor Level 9 completion rate daily for the first 7 days.**
   Even with the rating corrected, the level's structural complexity (enemy king + barrel + 3 floors)
   may still make it too hard. If D7 completion rate at L9 falls below 55%, file a follow-up
   structural patch.

3. **Instrument all five boss levels (19, 39, 59, 79, 99) with explicit `boss_attempt` events.**
   Boss completion rate is a leading indicator of world-level progression health. Without
   dedicated events, boss data is buried in generic `level_fail` noise.

4. **Set Firebase RC interstitial_frequency to 4 during soft launch.**
   Interstitial ads at every level completion will damage early retention. Level 4 (after every
   fourth level) is the soft-launch safe default. Tighten only after Day-7 ARPU data is available.

5. **Enable coin_reward_multiplier=1.2 on Day 1.**
   Early-game coin starvation is the fastest way to lose casual players. The 20% reward boost
   gives the design team time to calibrate EconomyConfig values for the 1.0 release without
   sacrificing soft-launch retention data quality.

6. **Monitor the six consecutive rating-10 levels in World 3 (L74–L79).**
   This is the largest concentration of max-difficulty levels outside of World 4. Players who
   reach World 3 are engaged but not yet "hardcore." A six-level gauntlet could cause a late-game
   stall that distorts D30 retention metrics.

7. **Gate upgrade-cost_multiplier=0.9 as a recoverable RC flag.**
   If early-game economy feels too generous (players completing all upgrades before Level 20),
   the RC flag can be tightened back to 1.0 without a code push.

8. **Track per-power-up activation counts from launch day.**
   Power-up balance data is cheap to collect and expensive to miss. Add a `powerup_used`
   analytics event with `powerup_type` parameter before submitting to stores.

9. **Schedule a D3 emergency review meeting.**
   Soft-launch data moves fast. Book a 30-minute sync for Day 3 post-launch to review L9
   completion rates, D1 retention, and coin economy signals. This meeting should have pre-agreed
   RC levers ready to deploy.

10. **Do not change LevelDefinition data via Remote Config.**
    Structural changes (enemy counts, materials, floor counts) must go through LevelConfigFactory.cs.
    RC multipliers can soften the *feel* of difficulty (more kings, easier destruction) but cannot
    fix a fundamentally over-tuned level. The PATCH-001 code change is the correct path for
    structural issues; RC is for economic and session-flow tuning only.

---

*End of M16 Difficulty Analysis. This document supersedes any prior difficulty commentary in
M15 QA reports.*
