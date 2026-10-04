# King Smash M16 — Soft-Launch Strategy

**Version:** 1.0.0-sl1
**Build Date:** 2026-10-05
**Unique Identifier:** KSSL1-20261005
**Prepared:** M16 Milestone
**Follows:** M15 Final QA Report (NOT_READY_FOR_SOFT_LAUNCH — physical/integration blockers resolved as of this milestone)

---

## 1. Overview

### Purpose

M16 marks King Smash's transition from internal quality assurance to controlled market exposure. The soft launch is not a revenue event and is not a marketing event. It is a structured learning experiment with real users in a limited geography, operating under real network, real device, and real store conditions for the first time.

### Core Principle: Goal is Learning, Not Downloads

Every decision in this document is governed by one rule: **we are here to learn, not to acquire users**. The soft launch succeeds if the team exits with actionable data about:

- Whether users understand the core mechanic
- Where they abandon the progression funnel
- Whether the economy feels rewarding or grinding
- Whether ads cause churn
- Whether the retention mechanics work

A soft launch that generates 2,000 engaged users and surfaces 3 critical balance issues is more valuable than one that generates 20,000 installs and surfaces nothing useful.

### What Soft Launch Is Not

- It is not a monetization event. No ARPU targets. No revenue milestones.
- It is not a marketing event. No paid user acquisition (UA).
- It is not a scale event. We are not trying to reach 100K users.
- It is not a final fix window. P0s discovered must be fixed, but the goal is signal, not perfection.

### Success Metric Philosophy

Metrics are divided into two categories:

**Hard requirements (binary gate):** Issues that, if present, make the product unsafe to continue operating. These are not percentages to optimize — they are thresholds. If any hard requirement is violated, the soft launch pauses for a fix.

**Soft targets (learning benchmarks):** These inform decisions about balancing, progression, and UX. They are measured against industry benchmarks for casual mobile games. They do not block the path to M17 unless they are catastrophically low (e.g., D1 < 10%).

---

## 2. Build Version

| Field | Value |
|---|---|
| Version name | 1.0.0-sl1 |
| Version code | 2 |
| Source build | M15 RC1 (1.0.0-rc1), no breaking changes |
| Unique identifier | KSSL1-20261005 |
| Build date | 2026-10-05 |
| Unity version | 6000.0.47f1 (Unity 6 LTS) |
| Target SDK | Android API 35 |
| Minimum SDK | Android API 26 (Android 8.0) |
| Build type | Release (IL2CPP, ARM64) |
| Scripting defines | FIREBASE_ENABLED, production (no KING_SMASH_DEV, no KING_SMASH_STAGING) |
| Firebase environment | king-smash-prod (production project) |
| Signing | Release keystore (provisioned as part of M15 blocker resolution) |
| Code stripping | ManagedStripping.High |
| Compression | LZ4HC |

### Version Increment Rationale

Version code increments from 1 (rc1) to 2 (sl1). Version name uses `-sl1` suffix to distinguish soft-launch builds from RC builds and from production builds (which will use clean semver `1.0.0`). All analytics events include `app_version` as a dimension so data from this build can be filtered from future production data.

### Scripting Define Changes from M15

M15 used `KING_SMASH_STAGING` as the environment guard. The sl1 build removes all staging and dev defines. Only `FIREBASE_ENABLED` is set. All `#if KING_SMASH_STAGING` branches are inactive. The `ServiceLocator` will register real Firebase, real Analytics, real Crashlytics, real Remote Config, and real Billing.

### Build Checklist Before Upload

- [ ] `google-services.json` points to `king-smash-prod`
- [ ] No debug MonoBehaviours in production scenes (BUG-001 fix confirmed)
- [ ] IL2CPP release mode verified (not Development Build)
- [ ] Remote Config defaults in `RemoteConfigDefaults.cs` match soft-launch profile
- [ ] Firebase App Check enabled on production project
- [ ] Play Store listing: privacy policy URL live
- [ ] Play Store listing: support email configured

---

## 3. Google Play Testing Track

### Overview of Testing Tracks

Google Play Console offers four testing tracks that control who can install the build and what review process applies:

**Internal Testing**
- Up to 100 testers (must be added by email or Google Group)
- Instant publishing — no Play Store review, no review queue
- Not discoverable via Play Store search
- Suitable for: dev team, QA, first-pass device validation

**Closed Testing (Alpha / Beta)**
- Alpha: typically 1,000–10,000 testers (Google does not hard-cap Alpha)
- Beta: can expand further
- Testers join via invite link or opt-in URL
- Requires a Play Store review (usually 1–3 days for first submission)
- Not discoverable via general Play Store search
- Suitable for: controlled soft launch with enough users for analytics signal

**Open Testing**
- Unlimited testers
- Full Play Store review required
- Discoverable in Play Store
- Suitable for: wide soft launch or pre-launch phase

**Production**
- Full public release
- Supports staged rollout by percentage

### Recommended Track for King Smash M16

**Start: Internal Testing** (first 5 days)
**Then promote: Closed Testing — Alpha** (14–21 days)

Rationale for Closed Alpha over Open or Production:

1. **Controlled audience.** We can gate the number of users and not be overwhelmed with support volume or crash volume before we've validated stability.
2. **Enough signal.** 500–2,000 users is sufficient for D1/D7 retention baselines, funnel analysis, and crash rate validation. We do not need tens of thousands of users to answer our M16 learning objectives.
3. **Quick iteration.** If a P0 is discovered on Day 3, we can push a fix build and the Closed Alpha track allows it with a shorter review cycle than production.
4. **Protects brand.** The game has not been publicly announced. Using a closed track means it does not appear in Play Store search and cannot be discovered by general users or press.
5. **No download pressure.** The team will not feel pressure to convert or retain users because they were never treated as paying customers.

### Track Promotion Timeline

```
Day 0   — Upload 1.0.0-sl1 to Internal Testing track
Day 1-5 — Internal Testing: dev team + friends/family (20–50 testers)
          Gate: zero P0 crashes, Firebase connection confirmed, billing smoke test passed
Day 6   — If gate passed: submit for Play Store review (Closed Alpha)
Day 7-8 — Play Store review window
Day 9   — Closed Alpha goes live
Day 9-23 — Closed Alpha soft launch (14 days minimum)
Day 23  — M16 decision checkpoint (go/no-go for M17)
```

---

## 4. Countries / Regions

### Selected Soft-Launch Markets

| Country | Region | Rationale |
|---|---|---|
| Philippines | Southeast Asia | High mobile penetration, strong casual gaming market, English-comfortable population, low CPI for organic discovery |
| Canada | North America | English-speaking, strong Android market, early-adopter demographic, excellent analytics signal quality |
| Australia | Oceania | English-speaking, iOS/Android parity, similar purchasing behavior to US without burning the US market |
| New Zealand | Oceania | Small, clean test market, culturally close to Australia, good for catching edge cases before Australia sees them |

### Protected Markets — NOT in M16

| Country | Reason for Exclusion |
|---|---|
| United States | Largest market by revenue — protect for full launch |
| United Kingdom | Tier-1 English market — protect for full launch |
| Germany | Large European market with strong mobile presence |
| France | Large European market |
| Japan | High-ARPU mobile market — first impression matters enormously |
| South Korea | High-ARPU mobile market |

### Rationale

The selected markets provide:

- **Language coverage:** English-primary markets (CA, AU, NZ) plus a partial-English SEA market (PH). No localization work required for soft launch.
- **Market diversity:** Mix of high-income (CA, AU) and price-sensitive (PH) markets provides a useful signal range on economy tuning.
- **Low brand risk:** None of these markets are the primary targets for the full launch campaign. A rough experience or negative press in these markets has limited bleed-over to Tier-1 markets.
- **Competitive intelligence:** Philippines is an active casual game market. If the core loop fails to engage Filipino players, it will likely also fail in Tier-1 markets.

---

## 5. Tester Audience

### Target User Volume

- **Minimum for statistical validity:** 500 active users (users who reach Level 1 and complete it)
- **Target:** 1,000–2,000 active users over the 14-day Closed Alpha window
- **Maximum:** Do not attempt to recruit beyond 2,000 users in M16

"Active" is defined as: installed + completed Level 1 within 24 hours of install. Raw installs are not the target metric.

### Acquisition Sources

**Phase 1 — Internal Testing (5 days):**
- Dev team, QA (if contracted), producer, stakeholders
- Friends and family of team members
- Maximum 50 testers
- No external posting, no social media

**Phase 2 — Closed Alpha (14–21 days):**
- Play Store Closed Alpha invite link (shared in controlled channels)
- Existing communities where the team has organic presence (gaming Discord servers, mobile gaming subreddits if appropriate)
- Absolutely no paid advertising

### No Paid User Acquisition in M16

This cannot be overstated: **no paid UA in M16**. Reasons:

1. We do not yet know if the game has sufficient D1/D7 retention to justify UA spend. Spending money acquiring users into a product with 15% D1 retention destroys LTV.
2. Paid UA users are not representative of organic users' behavior, motivation, or tolerance.
3. M16 users' purpose is to generate learning signal, not revenue. Paid UA users expect a polished product.
4. The budget for UA will be unlocked after M17 when the retention and monetization baselines are established.

---

## 6. Test Duration

### Phase Structure

**Phase 1 — Internal Testing: 5 days**

Purpose: Device validation, Firebase connectivity confirmation, billing smoke test, crash baseline.

Success criteria to exit Phase 1:
- Zero P0 crashes in Internal Testing
- Real Firebase Analytics events confirmed in Firebase Console (not just defaults)
- Real Crashlytics session confirmed
- Remote Config fetch confirmed (config_version=2 received)
- Test purchase (sandbox) confirmed: no duplicate grant, no receipt loss
- At least 10 testers have reached Level 5

**Phase 2 — Closed Alpha: 14–21 days**

Purpose: Funnel data, retention data, economy signal, balance signal.

Minimum duration: 14 days (required for D7 retention data to be meaningful).
Maximum duration: 21 days.

### Decision Checkpoint: Day 14 of Closed Alpha (approximately Day 23 of M16)

On Day 14 of Closed Alpha, the team reviews:
1. All hard requirements (pass/fail)
2. Soft target benchmarks with whatever data is available
3. Level 9 abandonment spike hypothesis (tested or not yet)
4. Economy flow data (coin accumulation rate vs. upgrade cost)
5. Open P1/P2 bug count

The output of this checkpoint is one of:
- **GO:** Proceed to M17 (global launch preparation) — all hard requirements met, soft targets within acceptable range
- **EXTEND:** Extend by 7 days maximum to collect more data on a specific learning objective
- **PATCH + RECHECK:** A significant balance issue has been identified that requires a Remote Config change or a patch build

**Hard time-box: Do NOT run M16 beyond 4 weeks total.** If after 4 weeks the data is insufficient to make a decision, the issue is not the duration — it is either the user volume (acquire more) or the metrics instrumentation (fix analytics gaps). An indefinitely-running soft launch is a failure mode.

---

## 7. Success Criteria

### Hard Requirements (Binary Gate — Must Be Met to Proceed to M17)

These are not targets to optimize. They are minimum acceptable states. Any violation triggers an M16 pause.

| # | Requirement | Threshold | Measurement |
|---|---|---|---|
| H1 | No critical crash affecting >2% of sessions | Crash-free session rate ≥ 98% | Firebase Crashlytics |
| H2 | Zero progression loss from cloud save | 0 verified reports of save data loss | Player reports + Cloud Firestore audit |
| H3 | Zero duplicate purchase grants | 0 confirmed cases | Transaction log audit (server-side idempotency key check) |
| H4 | Zero duplicate reward grants | 0 confirmed cases | Economy transaction log + idempotency audit |
| H5 | No critical backend instability | Cloud Run error rate < 1% | GCP Cloud Monitoring |
| H6 | No critical billing issue | 0 confirmed cases of billing failure with successful charge | Google Play Order audit |
| H7 | No critical rewarded-ad issue | 0 confirmed cases of reward not granted after ad fully watched | Ad reward idempotency log |
| H8 | No critical level blocker | Every level L1–L10 completable (verified with ≥20 attempts per level) | Analytics: level_complete events |
| H9 | No critical security issue | No confirmed exploits of economy, save, or billing | Security log review |

### Soft Targets (Learning Benchmarks — Inform Balancing Decisions)

These are benchmarks, not blockers. They are compared against industry benchmarks for casual mobile games.

| Metric | Soft Target | Industry Benchmark (Casual) | Notes |
|---|---|---|---|
| D1 Retention | Observe | 30–40% | Benchmark comparison after ≥500 users |
| D7 Retention | Observe | 10–15% | Benchmark comparison after ≥1,000 users |
| Level 1 completion rate | ≥ 85% | 80–90% | Measured from: tutorial_complete event |
| First 3 levels completion rate | ≥ 60% | 55–70% | Measured from: level_complete for L1, L2, L3 |
| Session length (median) | ≥ 5 minutes | 5–8 minutes casual | Firebase Analytics: session_duration |
| Crash-free session rate | ≥ 99.0% | 99.5%+ (production target) | Crashlytics |
| Level 9 abandonment rate | Observe | N/A (hypothesis) | Flagged as anomaly — see Learning Objective #3 |

### Explicitly Out of Scope for M16

- No IAP revenue targets
- No ARPU targets
- No LTV targets
- No Day 30 retention targets
- No viral coefficient targets
- No store rating targets (the game should not yet be requesting ratings during soft launch)

---

## 8. Learning Objectives (Priority Order)

The following eight questions, in priority order, represent the knowledge gaps M16 is designed to close.

### LO-1 (Highest Priority): Do users understand the core mechanic?

**Proxy metrics:** Tutorial completion rate, Level 1 completion rate
**Events to watch:** `tutorial_complete`, `level_complete` (level_id=0), `level_fail` (level_id=0)
**Decision threshold:** If L1 completion < 70%, the tutorial requires revision before global launch.
**Hypothesis:** The slingshot/king-launch mechanic is intuitive within 3–5 attempts.

### LO-2: Where do first-session drop-offs occur?

**Proxy metrics:** Level-by-level completion and abandonment funnel (L1 through L10)
**Events to watch:** `level_start`, `level_complete`, `level_fail`, `app_background` (mid-level), `app_remove`
**Decision threshold:** Any level with >30% drop-off relative to the previous level is flagged for analysis.
**Hypothesis:** Most early-session drop-offs occur at the level select screen, not mid-level.

### LO-3: Level 9 difficulty spike — does it cause abandonment?

**This is the most important balance hypothesis to test in M16.**

Level 9 has difficulty rating 10 (maximum) at only the 10th level of World 1 (Forest Kingdom). The surrounding levels (L7: 7, L8: 8, L9: 10, L10: 5) show a discontinuous spike. This is abnormal difficulty progression for a casual mobile game. Industry data consistently shows that early-world difficulty spikes above the player's current skill curve cause disproportionate session abandonment, especially on mobile where frustration tolerance is lower than PC or console.

**Proxy metrics:** Level 9 attempt count, L9 completion rate vs. L8 and L10, D1 retention segmented by players who passed L9 vs. players who did not
**Events to watch:** `level_start` (level_id=9), `level_complete` (level_id=9), `level_fail` (level_id=9, attempts bucket), `session_end` (last_level=9)
**Decision threshold:** If L9 completion rate is >15 percentage points lower than L8, or if D1 retention for players who reached but did not pass L9 is >10 percentage points lower than those who passed L8, the level requires rebalancing before global launch.
**Proposed intervention (if confirmed):** Use Remote Config `destruction_multiplier` or `king_launch_power` to temporarily reduce L9 effective difficulty, or flag for level redesign in M17 pre-launch prep.

### LO-4: Is the economy rewarding or grinding?

**Proxy metrics:** Coin balance at Level 5, Level 10, Level 20. Upgrade purchase rate. Time-to-first-upgrade.
**Events to watch:** `upgrade_purchased`, `shop_open`, `coin_earned`, `coin_spent`
**Decision threshold:** If median time-to-first-upgrade exceeds 15 minutes of play time, the economy is grinding. If upgrade purchase rate is <10% of users who reach Level 5, the economy signal is insufficient.
**Hypothesis:** 1.2x coin multiplier (soft-launch profile) will make the first upgrade feel accessible within the first 3–4 levels.

### LO-5: Are rewarded ads used voluntarily?

**Proxy metrics:** Rewarded ad accept rate (impressions / available placements), rewarded ad completion rate
**Events to watch:** `rewarded_ad_shown`, `rewarded_ad_completed`, `rewarded_ad_declined`, `rewarded_ad_reward_granted`
**Decision threshold:** If accept rate < 20%, the rewarded ad placements are poorly timed or the reward value is insufficient.
**Hypothesis:** Players will voluntarily accept rewarded ads to continue after a failed level at a rate of 25–40%.

### LO-6: Do users return next day?

**Proxy metric:** D1 retention (users who open the app again within 24 hours of first open)
**Events to watch:** `session_start` segmented by install cohort day
**Decision threshold:** D1 < 20% is critically low and will require UX review before global launch.
**Hypothesis:** D1 retention will be 25–35% for users who complete at least Level 5 in their first session.

### LO-7: Do interstitials cause uninstalls?

**Proxy metrics:** Post-interstitial session abandonment rate, uninstall rate segmented by interstitial exposure
**Events to watch:** `interstitial_shown`, `session_end` (immediately following interstitial), `app_remove` (within 24h of first interstitial)
**Decision threshold:** If >15% of sessions that show an interstitial end immediately after the ad (no further level start within 5 minutes), interstitial timing requires adjustment.
**Hypothesis:** Raising interstitial_frequency to 4 levels (from default 3) will keep post-interstitial abandonment below 10%.

### LO-8: Are upgrades meaningful?

**Proxy metrics:** Upgrade purchase rate per upgrade type, level completion rate improvement for players who upgraded vs. those who have not
**Events to watch:** `upgrade_purchased` (with upgrade_type dimension), `level_complete` segmented by upgrade ownership
**Decision threshold:** If any single upgrade type accounts for >60% of all purchases, the upgrade balance requires review (players are finding one upgrade dominant).
**Hypothesis:** Players will spread purchases across 2–3 upgrade types, indicating perceived value of multiple upgrade paths.

---

## 9. Remote Config Soft-Launch Profile

*Full parameter table and experiment framework in `docs/M16-REMOTE-CONFIG-PROFILE.md`.*

The soft-launch Remote Config profile is **slightly more generous than production defaults**. The purpose is to reduce early friction so users reach far enough into the game to generate meaningful analytics signal across all 8 learning objectives. Aggressive monetization and difficulty have been slightly relaxed. No parameter has been changed so dramatically as to make the data unrepresentative of the production experience.

### Key Changes from Defaults

| Key | Default | Soft-Launch Value | Rationale |
|---|---|---|---|
| `coin_reward_multiplier` | 1.0 | 1.2 | +20% coins to test economy feel without grinding |
| `upgrade_cost_multiplier` | 1.0 | 0.9 | 10% cheaper upgrades to ensure upgrade feels accessible |
| `destruction_multiplier` | 1.0 | 1.05 | Slightly easier destruction to reduce L1–L3 frustration |
| `xp_multiplier` | 1.0 | 1.1 | +10% XP to ensure upgrade availability early |
| `powerup_cost_multiplier` | 1.0 | 0.85 | Power-ups slightly cheaper so users can discover them |
| `interstitial_frequency` | 3 | 4 | Fewer interstitials to protect early session experience |
| `ad_max_interstitials_per_session` | 5 | 3 | Cap lower for soft launch to reduce churn risk |
| `ad_interstitial_min_session_seconds` | 60 | 90 | Protect first 90 seconds from interstitial interruption |
| `config_version` | "1" | "2" | Track users on soft-launch profile vs. default |

All other parameters remain at production defaults. Feature flags are all enabled: `shop_enabled`, `powerups_enabled`, `new_user_tutorial_enabled`, `ads_enabled`, `cloud_save_enabled`, `google_signin_enabled`.

### Active Experiments

Two A/B experiments run in parallel during M16 (see M16-REMOTE-CONFIG-PROFILE.md for full specs):
- **EXPERIMENT-01:** Coin reward multiplier (1.0 control vs. 1.2 variant)
- **EXPERIMENT-02:** Interstitial frequency (3 control vs. 4 variant)

---

## 10. Escalation / Stop Conditions

### Immediate Stop — Do Not Wait for Next Review Cycle

The following conditions require immediate M16 suspension and a fix build:

| Condition | Threshold | Action |
|---|---|---|
| P0 crash rate | >5% of sessions on any single device model or OS version | Suspend Closed Alpha, hotfix required |
| Cloud save data loss | Any confirmed case (>0 verified reports) | Suspend immediately, investigate Firestore rules and save logic |
| Duplicate billing grant | Any confirmed case | Suspend billing, audit server-side idempotency keys |
| Level completion rate: 0% | Any level L1–L10 at 0% completion with ≥20 attempts | Emergency Remote Config adjustment or patch |

### Review Cycle Escalation (Not Immediate Stop)

| Condition | Threshold | Action |
|---|---|---|
| Crash rate rising trend | Crash rate increases >2x in 24h without explanation | Investigate within 4 hours |
| D1 retention < 15% | After first 200-user cohort | Emergency UX review, consider extending soft launch |
| L9 abandonment confirmed | L9 vs. L8 completion delta >20 pp | Adjust `destruction_multiplier` via Remote Config |
| Support email volume spike | >10 reports of same issue within 24h | Treat as potential P1, investigate same day |

### P0 Severity Definition

A P0 is any issue that meets one or more of:
- It causes data loss (save, economy, billing)
- It blocks completion of any level that is intended to be completable
- It affects >5% of sessions
- It involves a security vulnerability
- It involves a financial transaction anomaly

P0 issues require a response within 4 hours and a fix build within 48 hours.

---

## 11. Communication Plan

### Daily (First 5 Days — Internal Testing Phase)

- **Who:** Lead developer
- **What:** Check Crashlytics dashboard for new issues; check Cloud Run logs for backend errors; verify Remote Config fetch is occurring (config_version dimension in Analytics)
- **Format:** Async Slack message to #king-smash-ops with: crash count, new issues (Y/N), backend error rate
- **Time:** Each morning (09:00 local time)

### Weekly (Closed Alpha Phase)

- **Who:** Full team
- **What:** Analytics review meeting
- **Agenda:**
  1. Crash-free session rate (Firebase Crashlytics)
  2. D1 retention (Firebase Analytics cohort report)
  3. Level completion funnel (L1–L10 focus)
  4. Level 9 abandonment hypothesis status
  5. Economy metrics (coin flow, upgrade purchase rate)
  6. Ad metrics (interstitial abandonment, rewarded accept rate)
  7. Open P1/P2 bug count
  8. Decision: continue / adjust Remote Config / escalate
- **Duration:** 45 minutes maximum
- **Output:** Written summary posted to #king-smash-ops

### Immediate Alert (Any Time)

- **Trigger:** Any Crashlytics P0 alert (configured in Firebase console — alert on crash rate >2%)
- **Channel:** Slack #king-smash-ops + email to lead developer
- **Response SLA:** Acknowledge within 1 hour, triage within 4 hours

### Player-Facing Support

- **Email:** support@[studio].com (visible in Play Store listing)
- **Response SLA:** 48 hours for non-urgent reports, 24 hours for billing reports
- **Log all reports:** All reports go into a shared tracking sheet (bug/feedback type, date, device, OS version, account ID if provided)

### M16 Decision Checkpoint Communication

At Day 14 of Closed Alpha:
- Team reviews all metrics against success criteria
- GO / EXTEND / PATCH decision is made by team lead
- Decision is written up and committed to `docs/M16-DECISION.md`
- If GO: M17 planning begins
- If EXTEND or PATCH: new 7-day window begins with explicit re-check date

---

## Appendix A: Analytics Events Reference

The following events are critical to M16 learning objectives and must be confirmed firing before Phase 2 begins (during Internal Testing validation):

| Event Name | Parameters | LO Coverage |
|---|---|---|
| `level_start` | level_id, world_id, attempt_number | LO-1, LO-2, LO-3 |
| `level_complete` | level_id, world_id, score, coins_earned, time_seconds | LO-1, LO-2, LO-3, LO-4 |
| `level_fail` | level_id, world_id, attempt_number, fail_reason | LO-2, LO-3 |
| `tutorial_complete` | — | LO-1 |
| `upgrade_purchased` | upgrade_type, cost, currency | LO-4, LO-8 |
| `shop_open` | source | LO-4 |
| `rewarded_ad_shown` | placement, level_id | LO-5 |
| `rewarded_ad_completed` | placement, level_id | LO-5 |
| `rewarded_ad_declined` | placement, level_id | LO-5 |
| `rewarded_ad_reward_granted` | placement, reward_type, reward_amount | LO-5 |
| `interstitial_shown` | level_id, session_time_seconds | LO-7 |
| `session_start` | session_number | LO-6, D1/D7 |
| `session_end` | session_duration_seconds, levels_completed | LO-6, LO-7 |
| `iap_purchase_initiated` | product_id, price | All economy |
| `iap_purchase_complete` | product_id, order_id | All economy |

All events must have `user_id` (Firebase UID), `app_version` (1.0.0-sl1), `config_version` (2), and `session_id` attached as user properties or event parameters via FirebaseAnalyticsService.

---

## Appendix B: Milestone Gate Summary

| Gate | Condition | Owner |
|---|---|---|
| M16 Upload Gate | 1.0.0-sl1 build passes pre-upload checklist | Lead Developer |
| Phase 1 → Phase 2 Gate | 5-day internal test: zero P0s, Firebase confirmed | Lead Developer |
| Day 14 Decision Gate | Hard requirements met, data reviewed | Team Lead |
| M16 → M17 Gate | GO decision at Day 14 checkpoint | Team Lead |

---

*Document version: 1.0 — M16 Milestone*
*Next document: docs/M16-REMOTE-CONFIG-PROFILE.md*
