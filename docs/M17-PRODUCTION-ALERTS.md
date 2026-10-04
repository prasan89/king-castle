# King Smash M17 — Production Monitoring Alerts Reference

**Version:** 1.0.0 (version code 3)  
**Firebase Project:** king-smash-prod  
**Milestone:** M17 Production Launch  
**Status:** CONFIGURE BEFORE STAGE 0 — alerts must be live before the first internal build is distributed  
**Prepared:** M17 Milestone  
**References:** docs/M17-ROLLOUT-STRATEGY.md, docs/M17-ROLLBACK-PLAYBOOK.md

---

## Purpose

This document defines the production monitoring alerting configuration for King Smash. All alerts listed here must be configured and tested before Stage 0 of the staged rollout. An alert that fires after an incident is detected is useless — alerts must catch incidents before the team notices them manually.

Alerts are organized by severity:
- **P0 Critical:** Notify immediately via Slack + PagerDuty. Wake someone up. These are revenue-loss or data-integrity incidents.
- **P1 Warning:** Notify within 1 hour via Slack. Investigate during business hours unless escalating toward P0.

---

## GCP Cloud Monitoring Alerts

Configure these alerts in GCP Cloud Console → Monitoring → Alerting → Create Policy.

All Cloud Run alerts use the metric `run.googleapis.com/request_count` (for 5xx/4xx) and `run.googleapis.com/request_latencies` (for latency). Filter by `service_name=game-api` and `project_id=king-smash-prod`.

### P0 Critical Alerts — Notify Immediately (Slack + PagerDuty)

| Alert Name | Service | Condition | Threshold | Evaluation Window | Notification Channel | Action |
|---|---|---|---|---|---|---|
| `game_api_5xx_spike` | Cloud Run (game-api) | `(count of 5xx responses / total responses) * 100 > threshold` | >2% | 5 minutes | Slack #king-smash-production + PagerDuty | Investigate Cloud Run logs immediately; consider Tier 3 rollback if sustained |
| `game_api_high_latency` | Cloud Run (game-api) | p95 request latency > threshold | >3000ms | 5 minutes | Slack #king-smash-production + PagerDuty | Check Cloud Run instance count; check Firestore latency; consider Tier 3 if not recovering |
| `firestore_permission_denied_spike` | Firestore | Count of `PERMISSION_DENIED` errors > threshold per minute | >20/min | 5 minutes | Slack #king-smash-production + PagerDuty | Check Firestore security rules; check if a rules deployment went wrong; check service account permissions on Cloud Run |
| `auth_verification_failures` | Firebase Auth (via Cloud Run middleware logs) | Count of auth middleware rejections > threshold per minute | >10/min | 5 minutes | Slack #king-smash-production + PagerDuty | May indicate token expiry bug in client, or an attack; review Cloud Run auth middleware logs for pattern |

**Configuration example for `game_api_5xx_spike` (gcloud CLI):**
```bash
gcloud monitoring policies create \
  --notification-channels=[SLACK_CHANNEL_ID],[PAGERDUTY_CHANNEL_ID] \
  --display-name="game_api_5xx_spike" \
  --condition-display-name="5xx rate >2% over 5 min" \
  --condition-filter='resource.type="cloud_run_revision" AND resource.labels.service_name="game-api" AND metric.type="run.googleapis.com/request_count" AND metric.labels.response_code_class="5xx"' \
  --condition-threshold-value=0.02 \
  --condition-threshold-comparison=COMPARISON_GT \
  --condition-duration=300s \
  --project=king-smash-prod
```

Adjust `--condition-filter` for each alert per the table above.

### P1 Warning Alerts — Notify Within 1 Hour (Slack)

| Alert Name | Service | Condition | Threshold | Evaluation Window | Notification Channel | Action |
|---|---|---|---|---|---|---|
| `game_api_4xx_elevated` | Cloud Run (game-api) | `(count of 4xx responses / total responses) * 100 > threshold` | >10% | 15 minutes | Slack #king-smash-production | 4xx spike often indicates a client bug (malformed requests) or an auth issue; check request patterns in logs |
| `game_api_latency_elevated` | Cloud Run (game-api) | p95 request latency > threshold | >2000ms | 15 minutes | Slack #king-smash-production | Latency warning before it hits P0 threshold; investigate Firestore read patterns; check if Cold starts are contributing |
| `instance_count_spike` | Cloud Run (game-api) | Active instance count sustained above threshold | >8 instances sustained | 10 minutes | Slack #king-smash-production | High instance count may indicate a retry storm from clients (check if a client bug is causing request loops); also triggers naturally at high traffic — assess in context of rollout percentage |

**Note on `instance_count_spike`:**  
At 100% rollout, 8 instances may be normal peak traffic. Adjust this threshold after observing normal peak instance counts at each rollout stage. At Stage 1 (5%), >8 sustained instances is anomalous and likely indicates a retry storm. Recalibrate after Stage 3 (50%) data.

---

## Crashlytics Alerts

Configure these in Firebase Console → Crashlytics → Alerts, or via Firebase CLI alert policies.

| Alert Name | Condition | Evaluation Period | Threshold | Notification Channel | Action |
|---|---|---|---|---|---|
| `crash_free_users_drop` | Daily crash-free users rate drops below threshold | Daily (computed once per 24h) | <99% | Email to team + Slack #king-smash-production | Review Crashlytics issues immediately; if multiple days below threshold, consider halt |
| `new_issue_spike` | New crash issue created with occurrences exceeding threshold within time window | 1 hour from issue creation | >50 occurrences/1h | Slack #king-smash-production | Triage the new issue immediately; assign severity; if P0 pattern — halt rollout |
| `high_velocity_issue` | Existing or new issue exceeds occurrence velocity threshold | 1 hour | >500 occurrences/1h | Slack #king-smash-production + PagerDuty | This indicates a widespread crash; halt rollout immediately; begin P0 procedure from docs/M17-ROLLBACK-PLAYBOOK.md |

**How to configure Crashlytics alerts in Firebase Console:**
1. Firebase Console → Crashlytics → Alerts (left nav)
2. Click **Add alert**
3. Select alert type (velocity alert, regression alert, or new issue alert)
4. Set threshold values from the table above
5. Add email and webhook (Slack incoming webhook URL) as notification channels

**Slack webhook configuration:**
Create an incoming webhook in the King Smash Slack workspace for `#king-smash-production` and use the webhook URL in Firebase alert notification channel settings.

### Crashlytics Regression Alerts

In addition to the velocity alerts above, configure regression alerts:

| Alert Name | Trigger | Action |
|---|---|---|
| `issue_regressed` | A previously-closed Crashlytics issue re-opens (new occurrences after being marked resolved) | Slack notification; investigate whether the fix in the hotfix build actually resolved the issue |

Regression alerts are critical during hotfix cycles — they catch cases where a fix was deployed but the underlying issue was not fully resolved.

---

## Analytics Integrity Monitors

These are not real-time alerts but daily checks that should be performed as part of the monitoring cadence during the staged rollout. Configure as scheduled queries in BigQuery (if BigQuery export is enabled) or as daily reports in Looker Studio.

### Monitor 1: App Open Volume

**Purpose:** Detect sudden drops in active user volume that may indicate the app is crashing on launch before Crashlytics catches it, or that there is a distribution issue.

**BigQuery query (run daily):**
```sql
SELECT
  event_date,
  COUNT(DISTINCT user_pseudo_id) AS daily_active_users,
  COUNT(*) AS app_open_count
FROM
  `king-smash-prod.analytics_XXXXXXXXXX.events_*`
WHERE
  _TABLE_SUFFIX BETWEEN
    FORMAT_DATE('%Y%m%d', DATE_SUB(CURRENT_DATE(), INTERVAL 7 DAY))
    AND FORMAT_DATE('%Y%m%d', CURRENT_DATE())
  AND event_name = 'app_open'
GROUP BY event_date
ORDER BY event_date DESC
```

**Threshold:** A day-over-day drop of >30% in `app_open` event count (during rollout, account for expected volume changes from rollout percentage changes — a 5%→20% jump means a ~4x increase is expected).

**Action if triggered:** Check Play Console for distribution issues; check Crashlytics for launch crashes; check if the build was mistakenly set to 0% rollout.

### Monitor 2: Reward Integrity

**Purpose:** Ensure that `rewarded_ad_reward_granted` events never exceed `ad_completed` events. A ratio >1.0 indicates a double-grant bug.

**BigQuery query (run daily):**
```sql
SELECT
  event_date,
  COUNTIF(event_name = 'ad_completed') AS ad_completed_count,
  COUNTIF(event_name = 'rewarded_ad_reward_granted') AS reward_granted_count,
  SAFE_DIVIDE(
    COUNTIF(event_name = 'rewarded_ad_reward_granted'),
    COUNTIF(event_name = 'ad_completed')
  ) AS reward_to_ad_ratio
FROM
  `king-smash-prod.analytics_XXXXXXXXXX.events_*`
WHERE
  _TABLE_SUFFIX BETWEEN
    FORMAT_DATE('%Y%m%d', DATE_SUB(CURRENT_DATE(), INTERVAL 7 DAY))
    AND FORMAT_DATE('%Y%m%d', CURRENT_DATE())
  AND event_name IN ('ad_completed', 'rewarded_ad_reward_granted')
GROUP BY event_date
ORDER BY event_date DESC
```

**Threshold:** `reward_to_ad_ratio` > 1.0 on any day is an immediate P0 incident trigger.  
**Acceptable range:** 0.80–1.00 (some users close the reward dialog without collecting; some complete ads without a reward being granted due to fill rate).

**Action if >1.0:** Halt rollout; disable rewarded ads via RC (`rewarded_ads_enabled=false`); investigate Cloud Run reward endpoint idempotency.

### Monitor 3: Level Start Funnel Integrity

**Purpose:** Confirm that all new users begin at level 1 (not a higher level due to a data initialization bug) and that the level progression funnel is healthy.

**BigQuery query (run daily):**
```sql
-- New user first level check: all new users should have first level_start with level_number = 1
SELECT
  event_date,
  COUNT(DISTINCT user_pseudo_id) AS new_users_first_level_start,
  COUNTIF(first_level_number = 1) AS started_at_level_1,
  COUNTIF(first_level_number != 1) AS did_not_start_at_level_1
FROM (
  SELECT
    event_date,
    user_pseudo_id,
    FIRST_VALUE(
      (SELECT value.int_value FROM UNNEST(event_params) WHERE key = 'level_number')
    ) OVER (PARTITION BY user_pseudo_id ORDER BY event_timestamp) AS first_level_number
  FROM
    `king-smash-prod.analytics_XXXXXXXXXX.events_*`
  WHERE
    _TABLE_SUFFIX BETWEEN
      FORMAT_DATE('%Y%m%d', DATE_SUB(CURRENT_DATE(), INTERVAL 7 DAY))
      AND FORMAT_DATE('%Y%m%d', CURRENT_DATE())
    AND event_name = 'level_start'
    AND user_first_touch_timestamp >= UNIX_MICROS(TIMESTAMP_SUB(CURRENT_TIMESTAMP(), INTERVAL 7 DAY))
)
GROUP BY event_date
ORDER BY event_date DESC
```

**Threshold:** `did_not_start_at_level_1` should be 0. Any non-zero value indicates a data initialization bug.

**Action if triggered:** Check `GameBootstrap.cs` new-user initialization logic; check if Firestore progress document is being created with incorrect initial values; check RC `starting_kings` value.

### Monitor 4: Level Completion Funnel

**Purpose:** Check that `level_complete` events follow `level_start` events in a healthy ratio. A ratio significantly below 1.0 may indicate difficulty tuning issues or a crash in level flow.

**Looker Studio implementation:** Create a calculated metric: `level_complete count / level_start count` per day. A healthy game should have a ratio of 0.4–0.7 for level 1 (40–70% of players who start level 1 complete it). If this drops below 0.3, investigate whether there is a crash during gameplay or a difficulty spike.

---

## Dashboard Setup

Configure a Looker Studio (Google Data Studio) dashboard connected to Firebase Analytics and BigQuery for daily operational visibility.

**Recommended dashboard: King Smash Production Health**

### Data Sources Required

| Source | Connection | Purpose |
|---|---|---|
| Firebase Analytics | Looker Studio Firebase connector or BigQuery export | Event volume, user counts, retention |
| BigQuery (`analytics_XXXXXXXXXX`) | BigQuery connector | Custom queries (reward integrity, level funnel) |
| Google Play Console | Play Console data export to BigQuery (optional) | Install volume, crash rate from Play perspective |

### Recommended Dashboard Layout

**Row 1: Acquisition + Engagement KPIs (stat tiles)**

| Tile | Metric | Source | Alert Threshold |
|---|---|---|---|
| DAU | Distinct `user_pseudo_id` with `app_open` today | Firebase Analytics | <previous day DAU * 0.7 |
| New Users | Distinct users with `first_open` today | Firebase Analytics | Context-dependent on rollout % |
| Sessions | Total `session_start` events today | Firebase Analytics | <DAU (should be >= DAU) |
| Level 1 Completion Rate | `level_complete (level=1)` / `level_start (level=1)` | BigQuery | <30% |

**Row 2: Stability KPIs (stat tiles)**

| Tile | Metric | Source | Alert Threshold |
|---|---|---|---|
| Crash-Free Sessions | From Crashlytics summary (manual daily update or Crashlytics API) | Crashlytics | <99% |
| Cloud Run 5xx Rate | From Cloud Monitoring export (if configured) | Cloud Monitoring | >0.5% |
| p95 API Latency | From Cloud Monitoring export | Cloud Monitoring | >2000ms |
| ANR Rate | From Play Console BigQuery export | Play Console | >0.47% |

**Row 3: Monetization KPIs (stat tiles)**

| Tile | Metric | Source | Alert Threshold |
|---|---|---|---|
| Daily Revenue | IAP revenue from Play Console or Firebase in-app purchases | Play Console | <previous day * 0.5 (investigate, may be normal volatility) |
| Reward Ad Impressions | `rewarded_ad_reward_granted` event count | Firebase Analytics | Context-dependent |
| Reward Integrity Ratio | `rewarded_ad_reward_granted` / `ad_completed` | BigQuery | >1.0 is P0 |
| ARPU | Daily Revenue / DAU | Calculated | Track trend |

**Row 4: Trends (time series charts, last 14 days)**

- DAU trend line (day-over-day)
- Crash-free sessions trend (target: stable at >=99%)
- Cloud Run request volume (expected to grow proportionally with rollout percentage increases)
- Cloud Run error rate (expected: flat near 0%)

**Row 5: Level Funnel (bar chart)**

- Level start count by level number (levels 1–10) for the last 7 days
- Level complete count by level number (levels 1–10) for the last 7 days
- Completion rate per level (overlaid line)

### Looker Studio Setup Steps

1. Navigate to [lookerstudio.google.com](https://lookerstudio.google.com)
2. Create new report → Add data source → Firebase Analytics (select `king-smash-prod` project)
3. Add a second data source → BigQuery → project `king-smash-prod` → dataset `analytics_XXXXXXXXXX`
4. Build stat tiles using the metric definitions above
5. Share the dashboard with the full team (view access) and production leads (edit access)
6. Bookmark the dashboard URL in the team Slack channel topic for `#king-smash-production`

**Replace `analytics_XXXXXXXXXX`** with the actual BigQuery dataset name for the Firebase Analytics export. Find this in Firebase Console → Analytics → BigQuery Linking → Dataset name.

---

## Alert Testing Checklist

Before Stage 0, verify each alert fires correctly:

- [ ] `game_api_5xx_spike`: Trigger a synthetic 5xx from Cloud Run by temporarily deploying a bad revision to staging and confirm PagerDuty fires
- [ ] `game_api_high_latency`: Inject artificial latency in staging Cloud Run and confirm Slack notification fires
- [ ] Crashlytics `new_issue_spike`: Use `FirebaseCrashlytics.Instance.TestIt()` in an internal build to generate a crash and confirm the Crashlytics alert fires within 1 hour
- [ ] Crashlytics `crash_free_users_drop`: Cannot easily test in production — verify the alert threshold is set correctly in the Firebase Console UI and simulate by checking the alert configuration
- [ ] Slack channel `#king-smash-production` has all relevant team members added before Stage 0
- [ ] PagerDuty escalation policy points to the correct on-call engineer rotation
- [ ] Looker Studio dashboard is accessible to all team members

---

## Monitoring Cadence Reference

This table summarizes when to check what during the staged rollout. See docs/M17-ROLLOUT-STRATEGY.md Section 7 for post-100% cadence.

| Check | Stage 0 | Stage 1 (0–24h) | Stage 1 (24–48h) | Stage 2 | Stage 3 |
|---|---|---|---|---|---|
| Crashlytics crash-free rate | Daily | Every 2h | Every 4h | Every 4h | Every 4h |
| New Crashlytics issues | Daily | Every 2h | Every 4h | Every 4h | Every 4h |
| Cloud Run 5xx rate | Daily | Every 4h | Every 4h | Every 4h | Daily |
| Cloud Run p95 latency | Daily | Every 4h | Every 4h | Daily | Daily |
| Analytics event volume | Daily | Daily | Daily | Daily | Daily |
| Reward integrity ratio | Daily | Daily | Daily | Daily | Daily |
| D1 retention | N/A | N/A | N/A | N/A | Daily (from day 2 of Stage 3) |
| Play Console reviews | Daily | Daily | Daily | Daily | Daily |

---

*Document version: 1.0 — M17 Milestone*  
*See also: docs/M17-ROLLOUT-STRATEGY.md, docs/M17-ROLLBACK-PLAYBOOK.md*
