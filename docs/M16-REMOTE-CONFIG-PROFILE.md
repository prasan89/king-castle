# King Smash M16 — Remote Config Soft-Launch Profile

**Version:** config_version = 2
**Applies to:** 1.0.0-sl1 (Soft Launch)
**Firebase Project:** king-smash-prod
**Prepared:** M16 Milestone
**References:** RemoteConfigKeys.cs, RemoteConfigDefaults.cs, M16-SOFT-LAUNCH-STRATEGY.md

---

## Philosophy

The soft-launch Remote Config profile is **slightly more generous than production defaults**.

The purpose is narrow and explicit: reduce early friction so that users get far enough into King Smash to generate meaningful analytics signal across all 8 learning objectives defined in M16-SOFT-LAUNCH-STRATEGY.md. If users churn at Level 1 because coins feel insufficient or Level 9 feels brutally hard, we learn nothing about D7 retention, economy balance, or upgrade usage. A small generosity buffer during soft launch produces more useful data — and that data shapes the production configuration.

**This profile is NOT the production configuration.** Every parameter that changes from its default has a documented revert condition. When soft-launch data is reviewed, the team will make an explicit, data-driven decision about which values to carry forward into production (M17).

### Constraints on This Profile

1. No parameter changes so large as to make the soft-launch experience unrecognizable as the production product.
2. No feature flags disabled (all features must be active to generate signal on all systems).
3. No changes to save logic, billing logic, authentication, or core physics parameters via Remote Config.
4. All changes must be reversible via Remote Config push without a new build.

---

## Full Parameters Table

All parameters are listed. Columns: Key | Default | Soft-Launch Value | Min | Max | Rationale | Revert If

### GAMEPLAY Parameters

| Key | Default | SL Value | Min | Max | Rationale | Revert If |
|---|---|---|---|---|---|---|
| `starting_kings` | 3 | 3 | 1 | 10 | No change. Three kings is the designed starting experience; changing this would invalidate early-level design. | N/A |
| `max_kings` | 3 | 3 | 1 | 10 | No change. Max kings determines late-game resource ceiling; altering it invalidates progression data. | N/A |
| `king_launch_power` | 1.0 | 1.0 | 0.1 | 5.0 | No change. Launch power is core physics; even small changes drastically alter all 100 levels. Reserved for post-data tuning. | N/A |
| `gravity_multiplier` | 1.0 | 1.0 | 0.1 | 5.0 | No change. Gravity is core physics. Altering it makes all level completion data unrepresentative of production. | N/A |
| `destruction_multiplier` | 1.0 | 1.05 | 0.1 | 5.0 | +5% structural destruction effectiveness. Marginally reduces frustration on early levels (L1–L3) without meaningfully changing level strategy. Addresses potential early churn without redesigning levels. | Revert to 1.0 if L1–L3 completion rates exceed 95% (too easy) or if D1 retention shows no correlation with this group. |

### ECONOMY Parameters

| Key | Default | SL Value | Min | Max | Rationale | Revert If |
|---|---|---|---|---|---|---|
| `coin_reward_multiplier` | 1.0 | 1.2 | 0.1 | 10.0 | +20% coin rewards across all sources. Purpose: ensure players feel they can progress toward upgrades within their first session. Without this, players may hit the upgrade store, find it unaffordable, and disengage. The 1.2 value is also the Variant A in EXPERIMENT-01. | Revert to 1.0 if upgrade purchase rate exceeds 60% (economy too generous) or if EXPERIMENT-01 control vs. variant shows no D1 difference. |
| `gem_reward_multiplier` | 1.0 | 1.0 | 0.1 | 10.0 | No change. Gems are premium currency; adjusting their earn rate would confound IAP data and make soft-launch monetization signals unrepresentative. | N/A |
| `upgrade_cost_multiplier` | 1.0 | 0.9 | 0.1 | 10.0 | 10% cheaper upgrades. Combined with the 1.2 coin multiplier, this ensures the first upgrade is reachable within 4–6 levels. Generates actual upgrade purchase data so LO-8 can be answered. | Revert to 1.0 if time-to-first-upgrade median drops below 3 minutes (too cheap, no perceived value). |

### ADS Parameters

| Key | Default | SL Value | Min | Max | Rationale | Revert If |
|---|---|---|---|---|---|---|
| `rewarded_ad_enabled` | true | true | — | — | Feature stays enabled. Rewarded ads are a core monetization mechanic; disabling them prevents LO-5 data collection. | N/A |
| `interstitial_enabled` | true | true | — | — | Feature stays enabled. Interstitials must be on to measure LO-7 (churn impact). | N/A |
| `interstitial_frequency` | 3 | 4 | 1 | 20 | Raise minimum levels between interstitials from 3 to 4. Reduces interstitial exposure per session while still generating exposure data. Variant A in EXPERIMENT-02. | Revert to 3 if EXPERIMENT-02 shows no session-length or D1 difference between groups. |
| `rewarded_continue_enabled` | true | true | — | — | No change. Rewarded continue is a key user-controlled mechanic; must be enabled to measure acceptance rate. | N/A |
| `ad_interstitial_min_levels` | 3 | 3 | 1 | 50 | No change. Minimum level gate (first interstitial only shown after Level 3) is already generous; raising it would prevent sufficient data collection. | N/A |
| `ad_max_interstitials_per_session` | 5 | 3 | 1 | 20 | Cap at 3 interstitials per session (down from 5). Protects session quality during soft launch when we are still building confidence in the ad placement experience. | Revert to 5 if session data shows users are not encountering the cap (i.e., median session length is below 3 levels, so cap is irrelevant). |
| `ad_interstitial_min_session_seconds` | 60 | 90 | 0 | 3600 | First interstitial cannot show until 90 seconds into session (up from 60). Protects the critical first 90 seconds. New users evaluating the game should not see an ad before they've formed an opinion of the core loop. | Revert to 60 if average level 1 session time is below 90 seconds (i.e., the gate is preventing all interstitials from showing). |
| `rewarded_ad_cooldown` | 30 | 30 | 0 | 3600 | No change. 30-second cooldown between rewarded ad opportunities is already user-friendly. | N/A |

### PROGRESSION Parameters

| Key | Default | SL Value | Min | Max | Rationale | Revert If |
|---|---|---|---|---|---|---|
| `xp_multiplier` | 1.0 | 1.1 | 0.1 | 10.0 | +10% XP gain. Ensures players accumulate XP quickly enough to level up their king and unlock upgrade categories. Without this, the upgrade system may be invisible to players who don't reach the required XP threshold in their first session. | Revert to 1.0 if >80% of players reach the first XP threshold in their first session (boost is unnecessary at that point). |
| `level_unlock_requirements` | stars | stars | — | — | No change. Level unlock gating via stars is core to the progression design. Changing this would alter all level progression data. | N/A |
| `powerup_cost_multiplier` | 1.0 | 0.85 | 0.1 | 10.0 | Power-ups 15% cheaper. Power-ups are a key differentiator in King Smash's late game. If soft-launch users never use them (because they can't afford them), LO-8 (upgrade meaningfulness) cannot be answered for power-ups. | Revert to 1.0 if power-up purchase rate exceeds 40% of sessions (too cheap, trivializes strategy). |
| `powerup_reward_multiplier` | 1.0 | 1.0 | 0.1 | 10.0 | No change. Power-up reward scaling remains at default. | N/A |

### RETENTION Parameters

| Key | Default | SL Value | Min | Max | Rationale | Revert If |
|---|---|---|---|---|---|---|
| `daily_reward_enabled` | true | true | — | — | Enabled. Daily reward is a primary retention mechanic and must be on to measure LO-6 (D1 return). | N/A |
| `mission_enabled` | true | true | — | — | Enabled. Missions provide medium-term engagement goals. Must be on to observe mission interaction in the soft-launch cohort. | N/A |
| `achievement_enabled` | true | true | — | — | Enabled. Achievements are a secondary retention signal. | N/A |
| `mission_refresh_interval_hours` | 24 | 24 | 1 | 168 | No change. 24-hour refresh is the standard daily cadence. Changing it would alter retention behavior and confound D1/D7 data. | N/A |

### FEATURE FLAGS

| Key | Default | SL Value | Rationale |
|---|---|---|---|
| `shop_enabled` | true | true | Shop must be enabled. Without it, no IAP or upgrade data is generated. |
| `powerups_enabled` | true | true | Power-ups must be enabled. Required for LO-8. |
| `new_user_tutorial_enabled` | true | true | Tutorial must be enabled. Required for LO-1. |
| `ads_enabled` | true | true | Ads must be enabled. Required for LO-5 and LO-7. |
| `cloud_save_enabled` | true | true | Cloud save must be enabled. Hard requirement H2 (zero save data loss) cannot be verified if cloud save is off. |
| `google_signin_enabled` | true | true | Sign-in must be enabled. Cloud save and account-linked purchases require it. |

### VERSIONING

| Key | Default | SL Value | Rationale |
|---|---|---|---|
| `config_version` | "1" | "2" | Increment to 2 to identify users who received the soft-launch profile. All analytics events include `config_version` as a user property, enabling segmentation of sl1 users from any subsequent builds that may use config_version=3+. |

---

## Safety Limits Table

These limits are enforced in `RemoteConfigValidator.cs`. Any value outside these bounds will be clamped to the nearest valid value and a warning logged. A Firebase Remote Config console push that violates these limits is blocked by the validator at fetch-and-apply time (values are applied after validation, not directly).

| Key | Min Enforced | Max Enforced | Validator Method |
|---|---|---|---|
| `king_launch_power` | 0.1 | 5.0 | `ValidateMultiplier` |
| `gravity_multiplier` | 0.1 | 5.0 | `ValidateMultiplier` |
| `destruction_multiplier` | 0.1 | 5.0 | `ValidateMultiplier` |
| `coin_reward_multiplier` | 0.1 | 10.0 | `ValidateMultiplier` |
| `gem_reward_multiplier` | 0.1 | 10.0 | `ValidateMultiplier` |
| `upgrade_cost_multiplier` | 0.1 | 10.0 | `ValidateMultiplier` |
| `xp_multiplier` | 0.1 | 10.0 | `ValidateMultiplier` |
| `powerup_cost_multiplier` | 0.1 | 10.0 | `ValidateMultiplier` |
| `powerup_reward_multiplier` | 0.1 | 10.0 | `ValidateMultiplier` |
| `interstitial_frequency` | 1 | 20 | `ValidateInt` |
| `ad_max_interstitials_per_session` | 1 | 20 | `ValidateInt` |
| `ad_interstitial_min_levels` | 1 | 50 | `ValidateInt` |
| `ad_interstitial_min_session_seconds` | 0 | 3600 | `ValidateInt` |
| `rewarded_ad_cooldown` | 0 | 3600 | `ValidateInt` |
| `starting_kings` | 1 | 10 | `ValidateInt` |
| `max_kings` | 1 | 10 | `ValidateInt` |
| `mission_refresh_interval_hours` | 1 | 168 | `ValidateInt` |
| `config_version` | (string, any) | (string, any) | `ValidateString` |

All boolean flags (`rewarded_ad_enabled`, `interstitial_enabled`, `rewarded_continue_enabled`, `daily_reward_enabled`, `mission_enabled`, `achievement_enabled`, `shop_enabled`, `powerups_enabled`, `new_user_tutorial_enabled`, `ads_enabled`, `cloud_save_enabled`, `google_signin_enabled`) are validated as `bool` via `ValidateBool`. Invalid values default to `false` and log a warning.

---

## Experiment Framework

During M16, exactly **two A/B experiments** run in parallel. Running more than two experiments simultaneously with user volumes of 500–2,000 fragments the sample to the point where no individual experiment reaches statistical significance.

### EXPERIMENT-01: Coin Reward Multiplier

**ID:** EXP-01-COIN-MULT
**Status:** Active (Day 1 of Closed Alpha through Day 14 minimum)

**Hypothesis:**
Higher coin rewards in the first 10 levels improve D1 retention by reducing the feeling of grinding toward a first upgrade.

**Groups:**
| Group | Size | `coin_reward_multiplier` |
|---|---|---|
| Control | 50% of users | 1.0 |
| Variant A | 50% of users | 1.2 |

**Assignment:** Firebase Remote Config A/B testing (random assignment at first fetch, sticky for the user's lifetime via Firebase Installations ID).

**Primary Metric:** D1 retention (day 1 return rate)

**Secondary Metrics:**
- Level 10 completion rate
- Time-to-first-upgrade (median, in minutes of session time)
- Upgrade purchase rate (% of users who reach Level 5 and buy at least one upgrade)

**Duration:** 14 days minimum, 21 days maximum
**Minimum sample per group:** 200 users who reach Level 1 completion

**Decision Rule:**
- If Variant A D1 retention is >5 percentage points higher than Control AND the difference is statistically significant (p < 0.05): adopt `coin_reward_multiplier=1.2` for production.
- If difference is <5 percentage points or not significant: revert to `coin_reward_multiplier=1.0` for production (default).
- If Control D1 > Variant A (reverse): investigate whether higher coin supply reduces session urgency and defers return.

**Do Not Modify During Experiment:** Once users are assigned, do not change their `coin_reward_multiplier` value mid-experiment. Remote Config condition must use the A/B experiment assignment, not a simple override.

---

### EXPERIMENT-02: Interstitial Frequency

**ID:** EXP-02-INTERSTITIAL-FREQ
**Status:** Active (Day 1 of Closed Alpha through Day 14 minimum)

**Hypothesis:**
Showing interstitials every 4 levels (vs. the default 3) reduces early-session churn, improving session length and D1 retention, with no significant loss of interstitial impressions over a full session.

**Groups:**
| Group | Size | `interstitial_frequency` |
|---|---|---|
| Control | 50% of users | 3 |
| Variant A | 50% of users | 4 |

**Assignment:** Firebase Remote Config A/B testing (same assignment mechanism as EXP-01, independent randomization).

**Primary Metrics:**
- Session length (median, minutes)
- D1 retention

**Secondary Metrics:**
- Post-interstitial level abandonment rate (% of sessions where a level start does not occur within 5 minutes of an interstitial_shown event)
- Total interstitial impressions per session (ensure Variant A does not catastrophically reduce ad revenue signal)

**Duration:** 14 days minimum, 21 days maximum
**Minimum sample per group:** 200 users who have been shown at least one interstitial

**Decision Rule:**
- If Variant A (frequency=4) session length is >10% longer than Control AND post-interstitial abandonment is >3 percentage points lower: adopt `interstitial_frequency=4` for production.
- If difference in session length is <10% or not significant: revert to `interstitial_frequency=3` for production.
- If Control outperforms on D1 (unexpected): analyze whether higher interstitial frequency correlates with higher engagement (some users respond positively to content interruption as a pacing mechanism).

**Note on Revenue Impact:**
At 500–2,000 users, the absolute difference in interstitial impressions between groups 3 and 4 is small and not economically significant for soft launch. Revenue is explicitly not a soft-launch metric. The experiment is valid as designed.

---

## What NOT to A/B Test in M16

The following systems must NOT be placed under A/B test conditions in M16. Testing them risks data corruption, financial liability, user trust damage, or invalidates other measurements:

### Save Logic
Cloud save is a correctness requirement, not a UX preference. Any variant that alters save behavior (write frequency, conflict resolution, schema version) would put user progress at risk in a way that cannot be undone. Variants that cause data loss violate Hard Requirement H2 and cannot be run in any experiment.

### Billing
IAP billing is a financial transaction. A/B testing billing UX (e.g., price point, paywall timing) requires careful legal review and is out of scope for soft launch. No variant should alter `upgrade_cost_multiplier` to the point where it represents a different price tier in the user's perception (the 0.9 default applied to all users is different from a 0.5 vs. 1.5 A/B test).

### Authentication
Authentication flows determine account linking and cloud save identity. A variant that causes auth to fail silently, skip Google Sign-In, or create orphaned anonymous accounts will corrupt the user base in ways that cannot be fully remediated.

### Core Physics
`king_launch_power` and `gravity_multiplier` define the physics simulation. Even a small change to these values changes the completion feasibility of all 100 levels. An A/B test on physics values would produce two incomparable datasets since the level completion rates would differ structurally, not just due to user behavior.

### Level Completion Criteria
The definition of what constitutes a successful level completion must not vary between experiment groups. Any variant that lowers the star threshold for level 3 in one group and not another would make all level completion rate comparisons invalid.

---

## Remote Config Push Procedure

When pushing the M16 soft-launch profile to `king-smash-prod`:

1. **Do not push all values at once.** Push the default condition first (all values matching `RemoteConfigDefaults.cs`), confirm the app fetches and applies, then add the experiment conditions.

2. **Experiment conditions are set in Firebase Remote Config as conditions**, not as the default value. The default value in the console remains the production default. Conditions override per user group.

3. **Confirm `config_version=2` is received by the app** by checking Firebase Analytics for `user_property` updates on the `config_version` dimension within 24 hours of the push. If any users remain on `config_version=1`, they failed to fetch — investigate Remote Config fetch error logs.

4. **Do not modify experiment conditions after Day 1 of Closed Alpha.** Changing a condition mid-experiment invalidates assignment consistency and may cause the same user to flip between groups.

5. **Rollback procedure:** If a Remote Config push causes unexpected behavior, the full rollback is to set all condition overrides to their default values in the console. The app will fetch the reset values on its next Remote Config fetch (within the configured fetch interval, typically 12 hours; can be forced by reinstall or clearing app cache).

---

## Soft-Launch → Production Config Transition

At the M16 Day 14 decision checkpoint, the team makes explicit decisions on each changed parameter:

| Parameter | Changed To | Decision Options |
|---|---|---|
| `coin_reward_multiplier` | 1.2 | Adopt 1.2 / Revert to 1.0 / Set to data-driven value |
| `upgrade_cost_multiplier` | 0.9 | Adopt 0.9 / Revert to 1.0 |
| `destruction_multiplier` | 1.05 | Adopt 1.05 / Revert to 1.0 |
| `xp_multiplier` | 1.1 | Adopt 1.1 / Revert to 1.0 |
| `powerup_cost_multiplier` | 0.85 | Adopt 0.85 / Revert to 1.0 |
| `interstitial_frequency` | 4 | Adopt 4 / Revert to 3 |
| `ad_max_interstitials_per_session` | 3 | Adopt 3 / Revert to 5 |
| `ad_interstitial_min_session_seconds` | 90 | Adopt 90 / Revert to 60 |
| `config_version` | "2" | Increment to "3" for production release |

Each decision must be documented in `docs/M16-DECISION.md` with the supporting data (specific metric value, group size, significance level).

---

*Document version: 1.0 — M16 Milestone*
*See also: docs/M16-SOFT-LAUNCH-STRATEGY.md, Assets/Scripts/Config/RemoteConfigKeys.cs, Assets/Scripts/Config/RemoteConfigDefaults.cs*
