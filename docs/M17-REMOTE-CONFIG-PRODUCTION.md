# King Smash M17 — Remote Config Production Baseline

**Config Version:** 3
**Applies to:** 1.0.0 (version code 3, global production launch)
**Firebase Project:** king-smash-prod
**Milestone:** M17 Production Launch
**Replaces:** config_version=2 (M16 soft-launch profile)
**Prepared:** M17 Milestone
**References:** RemoteConfigKeys.cs, RemoteConfigDefaults.cs, docs/M16-REMOTE-CONFIG-PROFILE.md

---

## 1. Philosophy

### 1.1 Production Defaults Are More Conservative Than Soft Launch

The soft-launch Remote Config profile (config_version=2) was deliberately more generous than the designed defaults. That generosity served a specific purpose: reduce early friction so soft-launch users generated analytics signal across all 8 learning objectives. The boost to coin rewards, the discounted upgrades, and the reduced ad frequency were research instruments, not the intended production experience.

**The M17 production profile reverts all soft-launch generosity back to designed defaults.**

This is the intended long-term product. Users should feel the designed economy. Coins should take effort to accumulate. Upgrades should feel earned, not given. Power-ups should be a strategic decision, not a disposable commodity. The soft-launch data either validates these design decisions or provides specific, quantified reasons to change individual values — which would then be set intentionally, not left at the soft-launch research values.

### 1.2 Decision Rules for Retaining Soft-Launch Values

A soft-launch value is carried into production ONLY if:

1. An A/B experiment ran comparing the soft-launch value to the default
2. The experiment reached statistical significance (p < 0.05) with minimum 200 users per group
3. The variant outperformed the default on the primary metric by a meaningful margin
4. A documented decision exists in `docs/M16-DECISION.md`

If no experiment covered a changed parameter, the production value is the designed default. Period.

### 1.3 What Remote Config Can and Cannot Do

Remote Config is a powerful operational lever. It can change economy balance, ad behavior, and feature availability without a new build. It cannot:

**RC CAN fix:**
- Economy imbalance (multipliers, costs, rewards)
- Ad frequency issues causing churn
- Feature flags for broken or overpowered features
- Engagement mechanics that are under- or over-tuned

**RC CANNOT fix:**
- Security vulnerabilities (a compromised authentication flow requires a new build)
- Billing issues (IAP configuration is not RC-controlled)
- Crashes caused by code bugs (symptom may worsen or improve with RC, but root cause requires a build)
- Physics balance (king_launch_power and gravity_multiplier are intentionally excluded from production RC changes — see Section 2)
- Google Play policy violations (a policy violation requires a new build with the offending code removed)

Understanding this distinction prevents false confidence in RC as a production safety net.

---

## 2. Full Production Baseline Table

All 30+ Remote Config keys are listed. Soft-launch value = config_version=2 value. Production baseline = config_version=3 value for global launch.

### 2.1 GAMEPLAY Parameters

| Key | Soft-Launch (v2) | Production (v3) | Change | Rationale |
|---|---|---|---|---|
| `starting_kings` | 3 | **3** | None | Three kings is the designed starting experience. No soft-launch experiment covered this. Unchanged. |
| `max_kings` | 3 | **3** | None | Max kings caps the late-game resource ceiling. Not changed during soft launch; not changed for production. |
| `king_launch_power` | 1.0 | **1.0** | None | Core physics. Even a 0.05 change alters all 100 levels. Never changed via RC without full level re-validation. |
| `gravity_multiplier` | 1.0 | **1.0** | None | Core physics. Same constraint as king_launch_power. |
| `destruction_multiplier` | 1.05 | **1.0** | Revert -0.05 | Soft launch used 1.05 to reduce early-level friction for analytics generation. Production reverts to designed value. If soft-launch L1–L3 completion data shows no issue at 1.0, this is the correct default. |

### 2.2 ECONOMY Parameters

| Key | Soft-Launch (v2) | Production (v3) | Change | Rationale |
|---|---|---|---|---|
| `coin_reward_multiplier` | 1.2 | **1.0** | Revert -0.2 | Soft launch boosted coins by 20% to ensure players could afford upgrades quickly for economy analytics. Production reverts to designed earning rate. If EXP-01 data shows 1.2 meaningfully improved D1 retention (p<0.05), this decision must be revisited per M16-DECISION.md — only then does 1.2 survive into production. |
| `gem_reward_multiplier` | 1.0 | **1.0** | None | Gems unchanged throughout. Premium currency earn rate is not tuned via RC. |
| `upgrade_cost_multiplier` | 0.9 | **1.0** | Revert +0.1 | Soft launch made upgrades 10% cheaper to generate upgrade purchase data. Production reverts to designed prices. The designed upgrade economy is the one that was balanced over 100 levels — the 0.9 value was a research instrument. |

### 2.3 ADS Parameters

| Key | Soft-Launch (v2) | Production (v3) | Change | Rationale |
|---|---|---|---|---|
| `rewarded_ad_enabled` | true | **true** | None | Rewarded ads are core monetization. Always enabled in production. |
| `interstitial_enabled` | true | **true** | None | Interstitials are active in production. If interstitials cause measurable churn in production, use emergency lever (Section 4). |
| `interstitial_frequency` | 4 | **3** | Revert -1 | Soft launch tested frequency=4 (EXP-02) against the default of 3. Production reverts to 3 unless EXP-02 showed statistically significant session length improvement with 4. Frequency=3 is the designed ad cadence for maximum ad revenue consistent with acceptable churn. |
| `rewarded_continue_enabled` | true | **true** | None | Rewarded continue is unchanged. This is the primary player-controlled monetization mechanic. |
| `ad_interstitial_min_levels` | 3 | **3** | None | Level gate unchanged. First interstitial only after Level 3. |
| `ad_max_interstitials_per_session` | 3 | **5** | Revert +2 | Soft launch capped at 3 to protect session quality while building confidence. Production reverts to the designed cap of 5. This is the maximum the ad system can show — average sessions will not reach this cap. |
| `ad_interstitial_min_session_seconds` | 90 | **60** | Revert -30 | Soft launch extended the protection window to 90 seconds. Production reverts to 60. The 60-second gate was the designed value for protecting the first impression; 90 was a conservative soft-launch safety measure. |
| `rewarded_ad_cooldown` | 30 | **30** | None | 30-second cooldown between rewarded ad opportunities unchanged. |

### 2.4 PROGRESSION Parameters

| Key | Soft-Launch (v2) | Production (v3) | Change | Rationale |
|---|---|---|---|---|
| `xp_multiplier` | 1.1 | **1.0** | Revert -0.1 | Soft launch boosted XP by 10% to ensure players reached the first king-level threshold quickly enough to see the upgrade system. Production reverts to designed XP rates. If data showed fewer than 80% of players reached the first XP threshold in the first session, this revert should be revisited. |
| `level_unlock_requirements` | stars | **stars** | None | Level unlock via star gating is core design. Not changed via RC. |
| `powerup_cost_multiplier` | 0.85 | **1.0** | Revert +0.15 | Soft launch made power-ups 15% cheaper to ensure usage data was generated (LO-8). Production reverts to designed prices. Power-ups should feel like a meaningful investment decision. |
| `powerup_reward_multiplier` | 1.0 | **1.0** | None | Unchanged throughout. |

### 2.5 RETENTION Parameters

| Key | Soft-Launch (v2) | Production (v3) | Change | Rationale |
|---|---|---|---|---|
| `daily_reward_enabled` | true | **true** | None | Daily reward is a primary D1 retention mechanic. Always enabled. |
| `mission_enabled` | true | **true** | None | Missions provide medium-term engagement. Always enabled. |
| `achievement_enabled` | true | **true** | None | Achievements are a secondary retention signal. Always enabled. |
| `mission_refresh_interval_hours` | 24 | **24** | None | 24-hour mission refresh is the designed daily cadence. Unchanged. |

### 2.6 FEATURE FLAGS

All feature flags remain enabled in production. Disabling a feature flag at launch means users encounter a broken feature path — which generates support tickets and one-star reviews. Feature flags are intended as emergency kill switches, not launch configuration.

| Key | Soft-Launch (v2) | Production (v3) | Rationale |
|---|---|---|---|
| `shop_enabled` | true | **true** | Economy system must be accessible. |
| `powerups_enabled` | true | **true** | Power-up system must be accessible. |
| `new_user_tutorial_enabled` | true | **true** | Tutorial must run for every new user. |
| `ads_enabled` | true | **true** | Ad system must be active. Emergency lever: `interstitial_enabled=false` (see Section 4). |
| `cloud_save_enabled` | true | **true** | Cloud save must be active. Disabling loses user data — not an acceptable emergency action. |
| `google_signin_enabled` | true | **true** | Google Sign-In required for account linking and cloud save identity. |

### 2.7 VERSIONING

| Key | Soft-Launch (v2) | Production (v3) | Rationale |
|---|---|---|---|
| `config_version` | "2" | **"3"** | Increment to 3 identifies users on the production profile. All Analytics events include `config_version` as a user property, enabling segmentation: users on v3 = global production; users on v2 = soft-launch. This segmentation remains useful for cohort analysis. |

---

## 3. Changes Summary Table

| Key | Soft-Launch Value | Production Value | Direction | Type |
|---|---|---|---|---|
| `destruction_multiplier` | 1.05 | **1.0** | Revert | Gameplay tuning |
| `coin_reward_multiplier` | 1.2 | **1.0** | Revert | Economy |
| `upgrade_cost_multiplier` | 0.9 | **1.0** | Revert | Economy |
| `interstitial_frequency` | 4 | **3** | Revert | Ads |
| `ad_max_interstitials_per_session` | 3 | **5** | Revert | Ads |
| `ad_interstitial_min_session_seconds` | 90 | **60** | Revert | Ads |
| `xp_multiplier` | 1.1 | **1.0** | Revert | Progression |
| `powerup_cost_multiplier` | 0.85 | **1.0** | Revert | Progression |
| `config_version` | "2" | **"3"** | Increment | Versioning |

All other keys: unchanged between soft-launch and production.

---

## 4. Emergency Levers

These are pre-authorized Remote Config changes that can be pushed without a new build to address specific production issues. They do not require a product review meeting to execute — the on-call engineer makes the call based on the trigger conditions.

**CRITICAL REMINDER:** Emergency levers can only address the issues listed below. They cannot fix crashes, security issues, billing failures, or Play Store policy violations. If an issue is not in this table, it requires a code fix and a new build.

| Issue | RC Key to Change | Emergency Value | Effect | Trigger Condition |
|---|---|---|---|---|
| Interstitial ads are causing measurable session abandonment or visible churn spike | `interstitial_enabled` | `false` | Disables all interstitials. Rewarded ads remain active. Significant ad revenue impact — use only if churn signal is strong and immediate. | Post-launch D1 drops >5 percentage points in 48 hours AND Crashlytics shows no crash-related cause |
| Economy too tight — users not affording upgrades, low upgrade purchase rate | `coin_reward_multiplier` | `1.3` | Increases coin rewards by 30%. More aggressive than the soft-launch 1.2 value — use only for acute issues. | Upgrade purchase rate (% of users reaching Level 5 who buy any upgrade) drops below 15% in 7-day cohort |
| Levels too hard — low early completion rates causing D1 churn | `destruction_multiplier` | `1.1` | Makes structures slightly easier to destroy. +10% from the designed default. | L1–L3 combined completion rate drops below 50% in 7-day cohort |
| Power-ups over-used — economy destabilized by power-up spending | `powerup_cost_multiplier` | `1.5` | Makes power-ups 50% more expensive. Significant — reduces power-up usage substantially. | Power-up purchase rate exceeds 40% of sessions, AND economy projections show unsustainable spend pace |
| Shop feature broken (IAP processing errors, upgrade purchase failures) | `shop_enabled` | `false` | Hides the shop entirely. Users cannot access upgrades or IAP. Removes the broken surface while a fix is prepared. | Shop-related crash rate exceeds 5% of sessions OR Crashlytics shows systematic purchase flow failures |
| Power-up system broken (power-up effects not applying, quantity bugs) | `powerups_enabled` | `false` | Disables power-up display and usage. | Crashlytics shows power-up related crashes in >3% of sessions |
| Interstitial frequency too aggressive (complaints, reviews) | `interstitial_frequency` | `5` | Reduces interstitial frequency to one per 5 levels (less than default 3). Moderate revenue impact. | Post-launch review score drops below 3.5 stars AND negative reviews cite "too many ads" as primary complaint |
| Session ad cap too high — users hitting the cap and churning | `ad_max_interstitials_per_session` | `3` | Reverts session cap to soft-launch value. | More than 20% of sessions are reaching the 5-ad session cap (meaning highly engaged users are being over-exposed) |

**Emergency lever procedure:**
1. Identify trigger condition (must be data-driven, not intuition)
2. Document the trigger: metric value, timestamp, data source
3. Push the RC change to the default condition in Firebase Console
4. Observe impact over 4–6 hours (next Analytics data refresh cycle)
5. Document outcome in `docs/M17-INCIDENT-LOG.md`

**RC push propagation time:** Updated values reach all active users within the fetch interval (typically up to 12 hours for background fetches, immediate on next app cold start with cache invalidated). For urgent changes, the fastest propagation is achieved by setting `minimumFetchIntervalInSeconds` to a low value in a hotfix build — but this requires a new build. RC changes are not instantaneous for all users.

---

## 5. Config Version History

| config_version | Milestone | Build | Description |
|---|---|---|---|
| `"1"` | M14 | Initial | Initial Remote Config schema established during analytics and observability milestone. All values at designed defaults. Corresponds to internal testing builds. |
| `"2"` | M16 | 1.0.0-sl1 (code 2) | Soft-launch profile. Economy boosted, ads softened, multipliers slightly inflated to generate analytics signal. EXP-01 (coin multiplier A/B) and EXP-02 (interstitial frequency A/B) active during this period. Applies to all users on the soft-launch build. |
| `"3"` | M17 | 1.0.0 (code 3) | Production baseline. All soft-launch boosts reverted to designed defaults. Global launch configuration. Represents the intended long-term product experience unless modified by data-driven decisions. |

---

## 6. Remote Config Push Procedure for Production Launch

Follow this procedure exactly when pushing the production baseline to `king-smash-prod`.

### Step 1: Conclude Soft-Launch Experiments

Before pushing production defaults, the soft-launch A/B experiments must be formally concluded:

- [ ] EXP-01-COIN-MULT: Record final results in `docs/M16-DECISION.md`, then end the experiment in Firebase Console → Remote Config → A/B Testing
- [ ] EXP-02-INTERSTITIAL-FREQ: Record final results in `docs/M16-DECISION.md`, then end the experiment in Firebase Console → Remote Config → A/B Testing

Ending the experiments removes the per-user condition overrides. After experiment end, all users fall back to the default condition.

### Step 2: Update Default Condition

In Firebase Console → Remote Config → Edit (default condition):

1. Set all 9 changed keys to their production values (Section 3 table)
2. Increment `config_version` to `"3"`
3. Do NOT change feature flags or retention parameters (they are already at the correct values)

### Step 3: Publish

Click "Publish changes" in Firebase Console. This pushes the new default condition to all users.

### Step 4: Verify Propagation

- [ ] Within 30 minutes of publish, at least one device in Firebase Analytics shows `config_version=3` as a user property
- [ ] Within 24 hours, >90% of active users (as of last Analytics refresh) show `config_version=3`
- [ ] If any users remain on `config_version=2` after 48 hours, they may be using the app in airplane mode or have network issues — this is acceptable and expected

### Step 5: Monitor Post-Push Metrics

Monitor for 48 hours after the production baseline push:

- [ ] No unusual crash spike in Crashlytics (RC value changes can surface latent bugs if a value goes outside expected ranges)
- [ ] Economy events (`upgrade_purchased`, `daily_reward_claimed`) continue to fire at normal rates
- [ ] Session length does not drop more than 10% relative to soft-launch average (the more conservative economy may cause some additional early-session churn — this is expected within limits)

---

*Document version: 1.0 — M17 Milestone*
*See also: docs/M17-PRODUCTION-BUILD-CONFIG.md, docs/M17-FIREBASE-PRODUCTION-CHECKLIST.md, docs/M16-REMOTE-CONFIG-PROFILE.md*
