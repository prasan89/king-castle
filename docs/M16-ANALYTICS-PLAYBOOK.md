# King Smash M16 — Analytics & Monitoring Playbook

**Version:** 1.0.0-sl1
**Build Date:** 2026-10-05
**Milestone:** M16 Soft Launch
**Depends on:** M14 Analytics implementation, M16 Soft-Launch Strategy

---

## 1. Pre-Launch Verification Checklist

Before inviting any external testers, every Tier-1 analytics event must fire correctly in Firebase DebugView. Complete this checklist with the Internal Testing build on a physical Android device (API 26+).

### Firebase DebugView Activation

1. Enable DebugView in Firebase Console: navigate to Analytics → DebugView, click the device icon, add your test device serial via ADB.
2. Install the soft-launch build (1.0.0-sl1) on the test device via `adb install -r KingSmash_SL1.apk`.
3. Enable DebugView mode on device: `adb shell setprop debug.firebase.analytics.app com.yourcompany.kingsmash`.
4. Open Firebase Console → Analytics → DebugView — events should appear within 5–15 seconds of being fired.

### Event Verification Steps (Numbered — All 18 Must Pass)

1. **app_open** — Launch the app cold. Verify `app_open` fires within 30 seconds. No parameters required.
2. **first_launch / onboarding_started** — Fresh install, first boot only. Verify `first_launch` fires, then `onboarding_started`. Both must appear before any level events.
3. **onboarding_completed** — Complete the tutorial. Verify `onboarding_completed` fires with no parameters.
4. **level_start (L1)** — Start Level 1. Verify `level_start` fires with `level_id=0`, `world_id=0`, `attempt_number=1`.
5. **level_complete (L1)** — Complete Level 1 (≥1 star). Verify `level_complete` fires with `level_id=0`, `world_id=0`, `stars` in [1,2,3], `attempt_number≥1`, `completion_time>0`, `remaining_kings≥0`.
6. **level_failed** — Fail a level (allow timer/health to expire). Verify `level_failed` fires with `level_id`, `world_id`, `attempt_number>0`, `source` parameter present.
7. **level_retried** — Tap Retry after a failure. Verify `level_retried` fires with `attempt_number` incremented.
8. **level_abandoned** — Start a level, tap the back/quit button mid-play. Verify `level_abandoned` fires with `level_id`, `world_id`, `attempt_number`.
9. **first_upgrade / stat_upgraded** — Open the King upgrade screen, purchase an upgrade. Verify `first_upgrade` fires (first time only), then `stat_upgraded` with `stat_name`, `old_value`, `new_value`.
10. **shop_opened** — Navigate to the shop. Verify `shop_opened` fires.
11. **ad_requested / ad_loaded / ad_started / ad_completed / rewarded_ad_reward_granted** — Fail a level, accept the rewarded ad offer. Verify these five events fire IN ORDER: `ad_requested` → `ad_loaded` → `ad_started` → `ad_completed` → `rewarded_ad_reward_granted`. Verify `rewarded_ad_reward_granted` has `placement` and `reward_type` parameters.
12. **ad_started (without reward)** — On a different attempt, start a rewarded ad but dismiss/skip before completion. Verify `ad_started` fires but `rewarded_ad_reward_granted` does NOT fire for that session event sequence.
13. **purchase_started / purchase_completed** — Initiate a test IAP via the sandbox Google Play account. Verify `purchase_started` fires with `product_id` and `product_type`. Complete the sandbox purchase. Verify `purchase_completed` (or `iap_completed`) fires. Verify `first_purchase` fires once and only once.
14. **daily_reward_claimed** — Claim the daily reward. Verify `daily_reward_claimed` fires with `reward_type`, `reward_amount`, `streak` parameters.
15. **cloud_save_started / cloud_save_success** — Progress through several levels (triggers auto-save). Verify `cloud_save_started` fires, then `cloud_save_success`. If on a network device, verify `is_online=true`.
16. **currency_earned / currency_spent** — Complete a level (coin earn), then spend coins on an upgrade. Verify `currency_earned` fires with `currency=coins`, `amount>0`. Verify `currency_spent` fires with `currency=coins`, `amount>0`.
17. **session_end** — Background the app. Verify `session_end` fires within the DebugView stream.
18. **retention_day_open** — Change device date to next day, reopen app. Verify `retention_day_open` fires with bucketed day parameter.

### Checklist Sign-Off

All 18 verification items must be checked before inviting external Closed Alpha testers. If any event fails:
- Check that `FIREBASE_ENABLED` scripting define is active in the build.
- Verify `google-services.json` references `king-smash-prod` (not staging).
- Check `ServiceLocator` registration for `IAnalyticsService` — ensure `FirebaseAnalyticsService` is registered, not `AnalyticsServiceMock`.
- Check Logcat for Firebase initialization errors (`FirebaseApp initialization unsuccessful`).

---

## 2. Funnel Definition

### Primary Acquisition-to-Engagement Funnel

This is the core conversion funnel from install to meaningful engagement. Monitor daily during Closed Alpha.

| Step | Event | Key Parameter | Drop-off Alert Threshold |
|---|---|---|---|
| 1 | Install | (Google Play metric — not an Analytics event) | — |
| 2 | First Launch | `app_open` where `first_launch=true` | < 50% of installs → ALERT (Firebase SDK may not be initializing) |
| 3 | Onboarding Completed | `onboarding_completed` | < 70% of first launches → ALERT (tutorial too long/confusing) |
| 4 | Level 1 Started | `level_start` (level_id=0) | < 95% of onboarding completions → ALERT (UI navigation issue) |
| 5 | Level 1 Completed | `level_complete` (level_id=0) | < 80% → ALERT (core mechanic not understood — see LO-1) |
| 6 | Level 2 Started | `level_start` (level_id=1) | < 85% of L1 completions → ALERT (early drop-off before loop forms) |
| 7 | Level 3 Completed | `level_complete` (level_id=2) | < 65% → ALERT (first real difficulty wall — see LO-2) |
| 8 | First Upgrade | `stat_upgraded` | < 40% of L3 completions → investigate economy accessibility |
| 9 | Level 10 Completed | `level_complete` (level_id=9) | < 35% → monitor (post-L9 patch expected to improve; see LO-3) |
| 10 | World 1 Completed | `world_completed` (world_id=0) | < 25% → expected for casual mobile; watch trend, not absolute |

**Minimum cohort for drawing conclusions:** 200 users who reached each step.

**Funnel query in Firebase Analytics (BigQuery):**

```sql
-- Funnel step completion rates — run daily
SELECT
  step,
  COUNT(DISTINCT user_pseudo_id) AS users,
  COUNT(DISTINCT user_pseudo_id) / FIRST_VALUE(COUNT(DISTINCT user_pseudo_id)) OVER (ORDER BY step_order) AS pct_of_install_base
FROM your_funnel_cte
GROUP BY step, step_order
ORDER BY step_order;
```

### Level 9 Funnel (Special Focus — LO-3)

Level 9 carried difficulty rating 10 (maximum) and represents the most important single balance hypothesis in M16. The level has been patched in the M16 RC build (difficulty rating lowered to 7 via level config rebalancing). Monitoring must confirm the patch is working.

**Target metrics post-patch:**

| Metric | Pre-Patch Estimate | Post-Patch Target | Alert Threshold |
|---|---|---|---|
| Average attempt count for L9 | ~6+ attempts | 2–4 attempts | > 5 → ALERT, patch not effective |
| L9 completion rate (players who attempt it) | ~30–40% | ≥ 55% | < 40% → ALERT |
| L9 abandonment rate (3+ fails, then no session) | ~20%+ | < 10% | > 15% → ALERT |
| L9 vs L8 completion rate gap | > 20 pp gap | < 10 pp gap | > 15 pp gap → ALERT |

**Events to watch:**
- `level_start` (level_id=9) — total attempts
- `level_complete` (level_id=9) — completions
- `level_failed` (level_id=9) — grouped by `attempt_number`
- `session_end` where last event was `level_failed` (level_id=9)

**Decision gate:** If L9 completion rate < 40% after 200+ users have attempted it, escalate to M17 pre-launch backlog and consider reducing `destruction_multiplier` via Remote Config in the next soft-launch RC.

---

## 3. Retention Dashboard

### D1 / D7 / D14 Retention

Retention is defined as: users who return and fire `app_open` on the Nth day (within a ±24h window of the Nth day since first open) divided by all users who fired `app_open` on Day 0 (install day).

**Firebase Analytics audience definitions:**

- **D1 Retained:** users who fire `app_open` (or any event) on calendar day 1 / total users who fired `app_open` on calendar day 0
- **D7 Retained:** users who fire `app_open` on day 6–8 since first open / total users who first opened 7+ days ago
- **D14 Retained:** users who fire `app_open` on day 13–15 / total first-open cohort ≥ 14 days old

**Minimum cohort for D-N retention conclusions:**
- D1: 200 users (can see D1 from Day 2 of Internal Testing)
- D7: 200 users from the same cohort (not available until Day 8 of Closed Alpha at earliest)
- D14: 200 users (not available until Day 15 of Closed Alpha)

**Soft targets (observation benchmarks):**

| Metric | Target | Industry Benchmark (Casual Mobile) | Alert If |
|---|---|---|---|
| D1 Retention | 25–35% | 30–40% | < 20% → critical |
| D7 Retention | 10–15% | 10–15% | < 8% → flag for LO-6 |
| D14 Retention | 5–10% | 5–8% | < 4% → flag |
| Session length (median) | ≥ 5 minutes | 5–8 minutes | < 3 minutes → ALERT |

**Segmentation:** Segment D1 retention by whether the user completed Level 9. Players who passed L9 are hypothesized to have significantly higher D1 than those who were blocked by it.

### Engagement Quality Signals

Monitor these alongside raw retention:

| Signal | Source Event | Parameter | Target | Alert |
|---|---|---|---|---|
| Session length average | `session_end` | `session_duration` | ≥ 5 minutes | < 3 min → ALERT |
| Levels per session | `level_start` count / `session_start` count | — | ≥ 2.5 | < 1.5 → ALERT |
| Level fail rate per world | `level_failed` / (`level_failed` + `level_complete`) | `world_id` | < 50% | > 70% for any world → ALERT |
| Daily reward claim rate | `daily_reward_claimed` / DAU | `streak` | ≥ 40% of DAU | < 20% → review placement |
| Return visit rate | Users with ≥ 2 `session_start` events | — | ≥ D1 rate | — |

---

## 4. Economy Monitoring

### Coin Flow Balance

The economy is one of the primary learning objectives (LO-4). The soft-launch RC profile sets a `coin_multiplier=1.2` (20% bonus over base earn rates) to ensure first upgrades feel accessible.

**Monitor: `currency_earned` vs `currency_spent` (currency=coins)**

| Metric | Calculation | Healthy Range | Alert Condition |
|---|---|---|---|
| Earned/Spent ratio (L1–L20) | SUM(currency_earned.amount WHERE currency=coins, level_id ≤ 19) / SUM(currency_spent.amount) | 2.0 – 4.0 | < 1.5 → economy too tight → ALERT |
| | | | > 10.0 → economy too loose → ALERT |
| Time-to-first-upgrade | Median session playtime when `first_upgrade` fires | ≤ 15 minutes | > 20 min → grinding signal |
| Upgrade purchase rate | Users who fire `stat_upgraded` / users who reach Level 5 | ≥ 40% | < 20% → investigate economy UX |

**Interpretation guidance:**
- Ratio < 1.5: Players cannot afford upgrades. Consider increasing `coin_multiplier` via Remote Config (max 1.5 without platest signoff).
- Ratio > 10.0: Upgrades are trivial; the economy has no meaningful tension. Reduce `coin_multiplier` or increase costs in the next balance patch.
- Neither case requires a hotfix unless it correlates with LO-8 issues (upgrade dominance) or D1 drop.

### Upgrade Distribution Monitoring

Track `stat_upgraded` events by `stat_name` to understand which upgrades players prioritize.

| Upgrade Stat | stat_name Value | Expected Purchase Share | Alert Condition |
|---|---|---|---|
| Launch Power | Power | ~35% | < 15% → possible discoverability issue |
| Move Speed | Speed | ~25% | < 10% → possible value perception issue |
| Smash Radius | SmashRadius | ~25% | < 10% → flag |
| Armor | Armor | ~15% | < 10% → ALERT: Armor value not understood |

**Alert: if Armor purchase share < 10% of all `stat_upgraded` events** → Investigate whether the Armor stat description clearly communicates its value to players. May require tooltip update or in-game tutorial beat.

**Alert: if any single stat_name accounts for > 60% of upgrades** → Upgrade dominance (LO-8 failure). Escalate to M17 balance review.

---

## 5. Ad Experience Monitoring

### Rewarded Ad Funnel Metrics

The rewarded ad funnel has four sequential steps. Each ratio reveals a different problem:

| Ratio | Calculation | Name | Target | Alert Threshold |
|---|---|---|---|---|
| Offer rate | `ad_requested` / `level_failed` | What % of failures get an offer | 70–90% | < 60% → ad fill rate issue (network problem) |
| Accept rate | `ad_started` / `ad_requested` | What % of offers are accepted | 25–40% | < 15% → placement poorly timed or reward too low |
| Completion rate | `ad_completed` / `ad_started` | What % of started ads finish | ≥ 85% | < 70% → ad quality issue (too long, poor UX) |
| Reward integrity | `rewarded_ad_reward_granted` / `ad_completed` | Reward granted vs. ad completed | 100% | < 99.9% → CRITICAL |

**CRITICAL ALERT: if `rewarded_ad_reward_granted` / `ad_completed` < 0.999:**
This means players are watching ads and not receiving their reward. This is both a player experience catastrophe and a hard requirement violation (H7). If this fires:
1. Stop all rewarded ad requests via Remote Config (`rewarded_ads_enabled=false`).
2. Review `AdAnalyticsBridge.TrackAdRewardGranted` log flow.
3. Check for race condition between `ad_completed` callback and reward grant logic.
4. Do not re-enable until root cause is identified and verified in staging.

### Interstitial Impact Monitoring

**Goal (LO-7):** Verify that the interstitial frequency (4 levels between shows) keeps post-ad abandonment below 10%.

| Metric | Calculation | Target | Alert |
|---|---|---|---|
| Post-interstitial session abandonment | % of sessions that end within 60 seconds of `interstitial_shown` | < 10% | > 30% → reduce frequency via RC immediately |
| Post-interstitial level start | % of users who start a new level within 5 minutes of `interstitial_shown` | > 80% | < 60% → flag |

**RC lever:** If post-interstitial abandonment > 30%, update `interstitial_frequency_cap` in Remote Config from 4 to 5 or 6. Monitor the impact over the following 48 hours.

---

## 6. Crashlytics Dashboard Setup

### Minimum Alerts to Configure in Firebase Console

Configure these alerts before Internal Testing begins (Firebase Console → Crashlytics → Alerts):

| Alert # | Name | Condition | Threshold | Notification |
|---|---|---|---|---|
| 1 | Crash-free users drop | Crash-free users rate | Drops below 99.0% | Slack + email (immediate) |
| 2 | New issue spike | New crash issue occurrences | > 100 in 1 hour | Slack + email |
| 3 | Device model cluster | Crash rate for single device model | > 10% for that model | Email (next business hour) |
| 4 | Velocity alert | Existing issue re-occurring | > 50% increase vs. prior day | Email |

### Custom Key Verification

Every Crashlytics session must have these context keys set (via `LevelAnalyticsBridge` and `GameBootstrap`):

| Key | Expected Value Format | Set By |
|---|---|---|
| `player_id` | 16 hex chars (hashed — no raw PII) | Auth flow / `SetPlayerContext` |
| `level` | Integer (current level_id) | `LevelAnalyticsBridge.HandleLevelStateChanged` |
| `world` | Integer (current world_id) | `LevelAnalyticsBridge.HandleLevelStateChanged` |
| `attempt` | Integer (current attempt) | `LevelAnalyticsBridge.HandleLevelStateChanged` |
| `environment` | `production` | `GameBootstrap.Initialize` |
| `config_version` | `2` (M16 RC profile) | `GameBootstrap.Initialize` |
| `app_version` | `1.0.0-sl1` | `FirebaseCrashlyticsService` constructor |
| `session_id` | GUID string | `FirebaseCrashlyticsService` constructor |

If any crash report is missing `level` and `world` keys, it indicates `LevelAnalyticsBridge` was not initialized at the time of the crash — a separate P1 bug.

### Crash Triage Workflow

1. New crash appears in Crashlytics (check daily minimum, or via Slack alert).
2. Open the crash issue. Check: does it have `level`, `world`, `attempt` context keys? If not, note as instrumentation gap.
3. Read the stack trace. Identify the component: gameplay (LevelManager, LaunchController), UI (UIManager), economy (CurrencyService), ads (AdMob), cloud (FirestoreService, CloudSaveService).
4. Identify the top 3 affected device models and Android versions.
5. Reproduce if possible using the device matrix from M15.
6. Assign severity:

| Severity | Definition | Response SLA |
|---|---|---|
| P0 | > 2% of sessions, or crashes all users of a device model, or blocks progression entirely | Hotfix within 24h — emergency RC2 build if needed |
| P1 | > 0.5% of sessions, or affects a critical flow (IAP, cloud save, ads) | Fix within 48h, include in next patch build |
| P2 | < 0.5% of sessions, non-critical flow | Fix before M17 |
| P3 | Cosmetic / rare device-specific | Backlog |

7. P0 handling: Notify all team members immediately. Evaluate whether the game should be taken off Internal Testing or Closed Alpha while fix is deployed. Prepare RC2 (patch build) — increment version code to 3.

---

## 7. Backend Monitoring

### Cloud Run Health

Monitor via GCP Cloud Monitoring — set up dashboards and alerts before Day 0 of Internal Testing.

**Key metrics:**

| Metric | GCP Metric Name | Alert Threshold | Window | Channel |
|---|---|---|---|---|
| Request latency p95 | `run.googleapis.com/request_latencies` p95 | > 2000ms | 5 min | Slack + email |
| Request latency p99 | `run.googleapis.com/request_latencies` p99 | > 5000ms | 5 min | Slack + email |
| 5xx error rate | `run.googleapis.com/request_count` (500-599) / total | > 0.5% | 5 min | Slack + email (P0 level) |
| 4xx error rate | `run.googleapis.com/request_count` (400-499) / total | > 5% | 5 min | Slack (may indicate auth issues) |
| Instance count | `run.googleapis.com/container/instance_count` | > 5 sustained | 2 min | Email (possible retry storm) |
| Request volume spike | `run.googleapis.com/request_count` | > 1000/min sustained | 2 min | Email |

**Expected load for M16 (500–2,000 users):** Well within default Cloud Run auto-scaling (0–10 instances). If instance count exceeds 5 and the user base is under 2,000, investigate for retry storms or exponential backoff failure in the Unity client.

### Firestore Monitoring

| Metric | Alert Condition | Interpretation |
|---|---|---|
| Failed reads (PERMISSION_DENIED) | > 10/min | May indicate auth rule mismatch or expired token — investigate immediately |
| Failed writes (PERMISSION_DENIED) on economy/main | Any | Expected (server-write-only collections). If client is attempting writes, it is a security bug. |
| Failed writes (PERMISSION_DENIED) on progression/main | Any (unexpected) | Should only fail if `isOwner()` check fails — possible auth regression |
| p95 read latency | > 1000ms | Escalate to GCP support if persistent |

**Security rules compliance check (run before Day 0):**

Verify the following Firestore security rules are deployed to `king-smash-prod`:

```
economy/main         → allow write: if false  ✓ (server-only)
transactions/{txId}  → allow write: if false  ✓ (server-only idempotency)
dailyRewards/main    → allow write: if false  ✓ (server-only)
purchases/{id}       → allow write: if false  ✓ (receipt verification server writes only)
entitlements/{id}    → allow write: if false  ✓ (server grants only)
progression/main     → allow write: if isOwner() ✓ (client writes player progression)
```

If any economy collection allows client writes, that is a P0 security violation. Do not launch until corrected.

### Cloud Save Error Tracking

**Analytics events to monitor:** `cloud_save_failed`, `cloud_load_failed`

| Metric | Calculation | Alert Condition |
|---|---|---|
| Save failure rate | `cloud_save_failed` / `cloud_save_started` | > 1% → investigate |
| Load failure rate | `cloud_load_failed` / `cloud_load_started` | > 1% → investigate |
| Save error category distribution | `error_category` param on `cloud_save_failed` | Any `PERMISSION_DENIED` category → P0 |

**CRITICAL:** Any verified player report of progression loss (saved data not restored on reinstall or device switch) is an immediate P0 escalation. Cross-reference:
1. Firestore read logs for the affected `player_id` (hashed)
2. Cloud Run `/cloud/load` endpoint response logs
3. Client `cloud_load_failed` event for that session

---

## 8. Daily Soft-Launch Review Checklist

Run this checklist every morning during both Internal Testing and Closed Alpha phases. Time required: ~20 minutes.

### Operations Review (Morning)

- [ ] **Crashlytics new issues (last 24h):** Navigate to Firebase → Crashlytics → Issues, filter by "New". Any new P0 or P1? If yes, initiate triage immediately.
- [ ] **Crash-free sessions rate:** Check current value. Must be ≥ 99%. Below 99%: investigate before any new tester invitations.
- [ ] **Cloud Run 5xx error rate:** Check GCP Cloud Monitoring dashboard. Must be < 0.5%. If > 0.5%: check logs immediately.
- [ ] **Analytics event volume:** Navigate to Firebase → Analytics → Events. Are all top events (app_open, level_start, level_complete) showing expected volume vs. yesterday? A sudden drop-off in event volume indicates an analytics SDK regression.
- [ ] **Cloud save error rate:** Check `cloud_save_failed` / `cloud_save_started` in Analytics. Must be < 1%.
- [ ] **Rewarded ad reward integrity:** Check `rewarded_ad_reward_granted` / `ad_completed`. Must be 100%. Any deviation: escalate immediately.

### Analytics Review (Daily)

- [ ] **Funnel step drop-offs:** Compare each funnel step rate vs. previous day. Any step that drops > 5 percentage points in one day is flagged.
- [ ] **Level 9 completion rate:** Pull from `level_complete` (level_id=9) / (`level_complete` + `level_failed`) for level_id=9. Target ≥ 55%. Below 40%: escalate to team.
- [ ] **Session length median:** Pull from `session_end` `session_duration` parameter. Target ≥ 5 minutes.
- [ ] **Coin earn/spend ratio:** Check `currency_earned` vs `currency_spent` for currency=coins. Healthy: 2.0–4.0.

### Communication Review (Daily)

- [ ] **Support emails:** Check the support email address configured in Play Store listing.
- [ ] **Play Console reviews:** Check for any reviews in the Internal Testing / Closed Alpha track.
- [ ] **Team Slack:** Check #king-smash-monitoring channel for automated alerts that fired overnight.

---

## 9. Analytics Event Reference (Soft-Launch Priority Events)

All 18 Tier-1 events tracked for M16, plus essential supporting events.

| # | Event | When Fired | Key Parameters | Alert Condition |
|---|---|---|---|---|
| 1 | `app_open` | Cold/warm launch | _(none)_ | Sudden drop in volume vs. prior day |
| 2 | `first_launch` | Very first install open | _(none)_ | If missing: analytics SDK not initialized |
| 3 | `onboarding_started` | Onboarding flow begins | _(none)_ | < 90% of first_launch → ALERT |
| 4 | `onboarding_completed` | Tutorial finished | _(none)_ | < 70% of onboarding_started → ALERT |
| 5 | `level_start` | Level gameplay begins | `level_id`, `world_id`, `attempt_number` | Mismatch with level_complete + level_failed counts |
| 6 | `level_complete` | Level finished ≥ 1 star | `level_id`, `world_id`, `stars`, `attempt_number`, `completion_time`, `remaining_kings` | L1 completion < 80% → ALERT (LO-1) |
| 7 | `level_failed` | Level ended 0 stars | `level_id`, `world_id`, `attempt_number`, `source` | L9 fail rate > 60% → ALERT (LO-3) |
| 8 | `level_abandoned` | Quit button mid-level | `level_id`, `world_id`, `attempt_number` | > 20% abandon rate for any level → flag |
| 9 | `level_retried` | Retry after failure | `level_id`, `world_id`, `attempt_number` | Healthy signal; high rate = engaged players |
| 10 | `stat_upgraded` | Any king stat upgraded | `stat_name`, `old_value`, `new_value` | Armor share < 10% → review (LO-8) |
| 11 | `first_upgrade` | First upgrade ever | _(none)_ | < 40% of L3 completions → ALERT (LO-4) |
| 12 | `currency_earned` | Coins/gems credited | `currency`, `amount`, `source` | Ratio vs currency_spent out of range |
| 13 | `currency_spent` | Coins/gems deducted | `currency`, `amount`, `source` | Ratio vs currency_earned out of range |
| 14 | `ad_requested` | Ad request sent | `ad_type`, `placement` | Low fill rate (< 60% of level_failed) |
| 15 | `ad_completed` | Ad watched to end | `ad_type`, `placement` | Completion < 85% of ad_started → flag |
| 16 | `rewarded_ad_reward_granted` | Reward granted after ad | `placement`, `reward_type` | < 100% of ad_completed → CRITICAL (H7) |
| 17 | `cloud_save_failed` | Save operation failed | `operation`, `is_online`, `error_category` | > 1% of cloud_save_started → ALERT |
| 18 | `retention_day_open` | App opened on bucket day | `amount` (day bucket) | Low D1 (< 20%) → ALERT (LO-6) |
| — | `session_end` | App backgrounded | `session_duration` | Median < 3 min → ALERT |
| — | `world_completed` | All levels in world done | `world_id` | < 25% World 1 completion → expected, monitor |
| — | `purchase_completed` | IAP confirmed | `product_id`, `product_type` | Any followed by duplicate → H3 violation |
| — | `interstitial_shown` | Interstitial ad shown | `ad_type`, `placement` | > 30% sessions end within 60s → ALERT (LO-7) |
| — | `daily_reward_claimed` | Daily login reward | `reward_type`, `reward_amount`, `streak` | < 40% DAU → review placement UX |

---

## 10. BigQuery Export Setup

Firebase Analytics raw data exports to BigQuery. Set this up on Day 0 in the Firebase Console (Project Settings → Integrations → BigQuery → Link).

### Recommended Tables

BigQuery export creates `events_YYYYMMDD` tables. Key queries for M16:

**Daily active users:**
```sql
SELECT
  DATE(TIMESTAMP_MICROS(event_timestamp)) AS date,
  COUNT(DISTINCT user_pseudo_id) AS DAU
FROM `king-smash-prod.analytics_XXXXXXXXX.events_*`
WHERE _TABLE_SUFFIX BETWEEN '20261005' AND FORMAT_DATE('%Y%m%d', CURRENT_DATE())
  AND event_name = 'app_open'
GROUP BY date
ORDER BY date;
```

**Level completion funnel:**
```sql
SELECT
  params.value.int_value AS level_id,
  SUM(IF(event_name = 'level_start', 1, 0)) AS starts,
  SUM(IF(event_name = 'level_complete', 1, 0)) AS completes,
  SUM(IF(event_name = 'level_failed', 1, 0)) AS failures,
  SAFE_DIVIDE(SUM(IF(event_name = 'level_complete', 1, 0)),
              SUM(IF(event_name = 'level_start', 1, 0))) AS completion_rate
FROM `king-smash-prod.analytics_XXXXXXXXX.events_*`,
  UNNEST(event_params) AS params
WHERE params.key = 'level_id'
  AND event_name IN ('level_start', 'level_complete', 'level_failed')
  AND _TABLE_SUFFIX BETWEEN '20261005' AND FORMAT_DATE('%Y%m%d', CURRENT_DATE())
GROUP BY level_id
ORDER BY level_id;
```

---

*Playbook version: 1.0.0-sl1 | Last updated: 2026-10-05 | Milestone: M16*
