# King Smash M17 — Post-Launch Monitoring Plan

**Document:** Post-Launch Monitoring Plan
**Version:** 1.0
**Date:** 2026-10-05
**Scope:** Stage 1 (10%) launch through Stage 3 (100%) advancement
**Prerequisite:** M17-LAUNCH-CHECKLIST.md — all 10 gates PASSED, all 8 blockers resolved

---

## Overview

This document defines the monitoring schedule, success metrics, escalation procedures, and stage advancement criteria for the King Smash production launch. The plan covers the first 14 days of the Stage 1 rollout. Stage advancement decisions are made at Day 1, Day 7, and Day 14 based on the metrics defined here.

**On-call during initial rollout:** Engineering on-call assigned in Gate 10 of M17-LAUNCH-CHECKLIST.md. Must be available T+0 through T+24h with uninterrupted access to Firebase Console, GCP Console, and Google Play Console.

**Stop criteria — halt the rollout immediately if:**
- Crash-free sessions drops below 95% in the first 60 minutes.
- Any cloud save data loss incident is confirmed.
- Any duplicate purchase grant is confirmed.
- Cloud Run 5xx error rate exceeds 2% for more than 5 consecutive minutes.

---

## Monitoring Schedule

### Hour 1: T+0 to T+60 Minutes

**Frequency:** Check every 15 minutes.
**Owner:** Engineering on-call (required to be active and monitoring, not passively on standby).

#### Check Sequence (each 15-minute interval)

| # | Data Source | What to Check | Stop Criteria |
|---|---|---|---|
| 1 | Firebase Crashlytics → Overview | Crash-free sessions percentage | < 95% → HALT ROLLOUT immediately |
| 2 | Firebase Crashlytics → Issues | Any new issue with > 10 affected sessions | New P0 pattern → assess for immediate halt |
| 3 | GCP Console → Cloud Run → `king-smash-api` | 5xx error rate (last 15 min), request count | 5xx > 2% → escalate to Tier 3 response |
| 4 | Firebase Analytics → Realtime | `app_open` event count accumulating | No `app_open` events → investigate Firebase connection |
| 5 | Google Play Console → Statistics | Install count, update count | Unexpected install failure spike → investigate |

#### Hour 1 Checkpoint Log Template

```
T+15min
  Crashlytics crash-free sessions: ____%
  Crashlytics new issues (P0): ____
  Cloud Run 5xx rate: ____%
  Cloud Run request count: ____
  Analytics app_open events (realtime): ____
  Play Console installs: ____
  Status: OK / ESCALATED / HALTED

T+30min
  [repeat fields above]

T+45min
  [repeat fields above]

T+60min
  [repeat fields above]
  Hour 1 summary: PASS / ESCALATE / HALT
```

---

### Hours 2–6: T+1h to T+6h

**Frequency:** Check every 60 minutes.
**Owner:** Engineering on-call (can reduce to passive alerting between hourly checks after T+2h if Hour 1 is clean).

| # | Data Source | What to Check |
|---|---|---|
| 1 | Firebase Crashlytics → Issues | New issues filtered by device model — look for device-specific patterns |
| 2 | Firebase Crashlytics → Overview | Crash-free sessions percentage trend |
| 3 | GCP Console → Cloud Run → Metrics | p95 request latency, 5xx rate, instance count |
| 4 | Firebase Analytics → Events | `level_start` with `level_id=0` (Level 1 begins), `level_complete` with `level_id=0` (Level 1 completes) |
| 5 | Google Play Console → Statistics | Install count, early reviews tab (star rating, any reviews mentioning crashes or purchases) |

#### Hours 2–6 Escalation Triggers

| Condition | Action |
|---|---|
| Crashlytics crash-free < 98% | Escalate to engineering lead, begin investigation, prepare halt decision |
| New Crashlytics issue with > 50 affected sessions in any single hour | P1 response — investigate device/OS pattern, assess halt |
| Cloud Run p95 latency > 2000ms sustained over 30 minutes | Investigate Cloud Run scaling, RC mitigation options |
| `level_complete` (level_id=0) events are 0% of `level_start` (level_id=0) events | Level 1 is not completable — P1 response |
| Any Play Console review rating 1-star mentioning "crash", "payment", or "lost progress" | Immediate investigation |

---

### Day 1: T+0 to T+24 Hours

The Day 1 metrics report is the primary decision gate for Stage 1 continuation.

#### Day 1 Metrics Table

| Metric | Target | Alert Threshold | Source | Actual |
|---|---|---|---|---|
| Crash-free users | >= 99% | < 98.5% | Firebase Crashlytics | |
| Crash-free sessions | >= 99% | < 98.5% | Firebase Crashlytics | |
| ANR rate | < 0.47% | > 1% | Google Play Console (Android vitals) | |
| Level 1 completion rate | >= 80% | < 60% | Firebase Analytics: `level_complete` (level_id=0) / `level_start` (level_id=0) | |
| Cloud Run 5xx error rate | < 0.5% | > 1% | GCP Cloud Run metrics | |
| Cloud save error rate | < 1% | > 2% | Custom metric: Cloud Run `/save-progress` 4xx+5xx / total requests | |
| D0 retention (same-session return) | >= 50% | < 30% | Firebase Analytics: retained_users cohort | |
| Rewarded ad load success rate | >= 90% | < 75% | Firebase Analytics: `ad_impression` / `ad_request` events | |
| IAP purchase success rate (if applicable) | >= 95% of initiated purchases | < 85% | Firebase Analytics: `purchase` / `begin_checkout` | |
| Play Console install success rate | >= 99% | < 95% | Google Play Console: Statistics | |

#### Day 1 Advancement Decision

**Advance to Stage 1 continuation (hold at 10%):** All targets met, no alert thresholds breached, crash-free >= 99%.

**Place Stage 1 on hold:** One or more metrics below target but above alert threshold. Root cause investigation required before advancement decision.

**Halt rollout:** Any metric at or below alert threshold, or any P0 event (data loss, duplicate purchase, crash-free < 98.5%).

---

### Day 3: T+72 Hours

**Focus:** First D1 retention signal, session behavior, level difficulty flags.

| Metric | Target | Alert Threshold | Source |
|---|---|---|---|
| D1 retention (players active on Day 1 who return on Day 2) | >= 35% | < 20% | Firebase Analytics: retention cohort (D+1 from install date) |
| Average session length | >= 5 minutes | < 3 minutes | Firebase Analytics: `session_start` → `session_end` duration |
| Average levels per session | >= 2.5 | < 1.5 | Firebase Analytics: `level_complete` events per session |
| Crash-free sessions (cumulative Day 1–3) | >= 99% | < 98.5% | Firebase Crashlytics |

#### Day 3 Level Difficulty Audit

Pull `level_start` and `level_complete` event counts by `level_id` for levels 0–9 (Levels 1–10). Flag any level where:

- Completion rate is below 80% and declining (not just difficult — players are abandoning).
- `level_start` volume drops more than 40% between consecutive levels (players are quitting before reaching that level).

If any level shows 0% completion (impossible to complete), this is a **P1 incident** requiring immediate investigation and Remote Config or hotfix response.

#### Day 3 Action Items

- Document all level difficulty flags in the launch war room channel.
- If D1 retention is above target and no flags, confirm Stage 1 continuation.
- If D1 retention is below target but above alert, schedule a level design review for Day 7.

---

### Day 7: T+7 Days

**Focus:** D7 retention, economy health, Stage 1 → Stage 2 (50%) advancement decision.

| Metric | Target | Alert Threshold | Source |
|---|---|---|---|
| D7 retention | >= 15% | < 8% | Firebase Analytics: retention cohort |
| D1 retention (updated with larger cohort) | >= 35% | < 20% | Firebase Analytics |
| Average session length | >= 5 minutes | < 3 minutes | Firebase Analytics |
| Coin economy: avg earned per session | Define baseline | > 3× baseline = economy break | Firebase Analytics: `earn_virtual_currency` |
| Coin economy: avg spent per session | Define baseline | > 3× earned = negative economy | Firebase Analytics: `spend_virtual_currency` |
| Upgrade purchase rate | >= 5% of active users | < 2% | Firebase Analytics: `purchase` event, item_id=upgrade |
| Power-up use rate | >= 30% of active users | < 10% | Firebase Analytics: `use_item` events |
| Rewarded ad watch rate | >= 20% of active users | < 5% | Firebase Analytics: `ad_impression` (rewarded) |
| Level 10 completion rate | >= 70% of users who reach Level 10 | < 40% | Firebase Analytics: `level_complete` (level_id=9) / `level_start` (level_id=9) |

#### Day 7 Economy Health Assessment

Run the following queries in Firebase Analytics (BigQuery export or Analytics dashboard):

1. **Coin earned vs. spent ratio** per player segment (new, casual, engaged). A ratio > 3:1 earned-to-spent suggests coins are too easy to earn; < 0.5:1 suggests players are being blocked by currency.
2. **Level 10 completion rate** — the Level 9 patch referenced in M16 must be assessed here. If Level 10 completion is below target, verify the patch was applied correctly via Remote Config.
3. **IAP conversion funnel:** `view_item` → `begin_checkout` → `purchase`. Drop-off at each stage indicates friction points.

#### Day 7 Advancement Decision

**Advance to Stage 2 (50%):** D7 retention >= 15%, crash-free >= 99%, no open P0 or P1 incidents, economy health within normal range, Level 10 completion >= 70%.

**Hold at Stage 1 (10%):** D7 retention below 15% but above 8%, or open P1 incident under investigation. Require engineering and product lead review.

**Halt / rollback:** D7 retention < 8%, or any P0 event, or crash-free < 98.5%.

---

### Day 14: T+14 Days

**Focus:** D14 retention, first purchase conversion, Stage 2 → Stage 3 (100%) advancement decision.

| Metric | Target | Alert Threshold | Source |
|---|---|---|---|
| D14 retention | >= 10% | < 5% | Firebase Analytics: retention cohort |
| First purchase conversion (any IAP) | >= 2% of activated users | < 0.5% | Firebase Analytics: `purchase` / total unique users |
| Remove Ads conversion | >= 1% of activated users | < 0.2% | Firebase Analytics: `purchase`, item_id=remove_ads |
| World 1 completion rate (Level 10 complete) | >= 50% of users who reached Level 5 | < 25% | Firebase Analytics: `level_complete` (level_id=9) |
| Economy trajectory | Coins earned/spent ratio stable within Day 7 baseline ±20% | > 50% deviation | Firebase Analytics |
| Crash-free sessions (cumulative 14 days) | >= 99% | < 98.5% | Firebase Crashlytics |

#### Day 14 Advancement Decision

**Advance to Stage 3 (100%):** D14 retention >= 10%, first purchase conversion >= 2%, crash-free >= 99%, no open P0 or P1 incidents, economy stable.

**Hold at Stage 2 (50%):** D14 retention above 5% but below 10%, or first purchase conversion below target but above 0.5%. Require product lead decision.

**Roll back / halt:** D14 retention < 5%, or any P0 event, or crash-free < 98.5%, or economy break detected.

---

## Weekly Review Template

Conduct a structured weekly review using this template. Populate each column during the Week 1, 2, and 3 reviews. The Trend column is calculated at each review.

| Metric | Week 1 | Week 2 | Week 3 | Trend | Notes |
|---|---|---|---|---|---|
| D1 retention | | | | | |
| D7 retention | | | | | |
| D14 retention | | | | | |
| Daily active users (DAU) | | | | | |
| Avg session length (minutes) | | | | | |
| Avg levels completed per session | | | | | |
| Level 1 completion rate | | | | | |
| Level 10 completion rate | | | | | |
| Crash-free sessions % | | | | | |
| ANR rate % | | | | | |
| Cloud Run 5xx rate % | | | | | |
| Cloud save error rate % | | | | | |
| Rewarded ad watch rate % | | | | | |
| IAP conversion rate % | | | | | |
| Remove Ads conversion rate % | | | | | |
| Avg coins earned per session | | | | | |
| Avg coins spent per session | | | | | |
| 1-star review count | | | | | |
| Play Console average rating | | | | | |
| New Crashlytics issues (week) | | | | | |

**Trend codes:** UP (↑ >= 10%), STABLE (±10%), DOWN (↓ >= 10%), SPIKE (> 50% change requiring investigation).

---

## Escalation Matrix

All escalation actions must be executed by the named owner within the response time window. If the owner is unavailable, escalate to the next level immediately.

| Condition | Severity | Response Time | Action | Owner |
|---|---|---|---|---|
| Crash-free sessions < 98% | P0 | Immediate (< 5 min) | HALT ROLLOUT via Play Console. Engineering lead personally investigates Crashlytics. Hotfix decision within 2 hours. | Engineering Lead |
| Cloud save data loss confirmed (player reports saved progress lost, confirmed in Firestore) | P0 | Immediate (< 5 min) | HALT ROLLOUT. Preserve Firestore state — do NOT purge or overwrite any affected user documents. Begin forensic investigation. Notify product lead. | Engineering Lead |
| Billing duplicate purchase grant confirmed (player charged twice, entitlement granted twice) | P0 | Immediate (< 5 min) | HALT ROLLOUT. Audit all `entitlements` collection writes for the affected time window. Identify scope. Begin reconciliation. Notify product and legal. | Engineering Lead + Product Lead |
| Cloud Run 5xx error rate > 2% sustained for 5+ minutes | P0 | 15 minutes | Attempt Tier 1 (Remote Config kill-switch for affected endpoints). If no improvement in 15 min, Tier 3: `gcloud run services update-traffic --to-revisions=PREV=100` to roll back Cloud Run to previous revision. | Engineering On-Call |
| ANR rate > 1% (Play Console Android vitals) | P1 | 1 hour | Investigate ANR traces in Play Console. Identify affected device/OS combination. Assess halt: if ANR is blocking core loop, halt. If isolated to edge device, document and continue. | Engineering Lead |
| Level impossible to complete (level_complete rate = 0% for any level, confirmed over 50+ attempts) | P1 | 4 hours | Attempt Tier 1 RC mitigation: adjust level parameters or temporarily skip the broken level in the level flow. If RC cannot fix, prepare and deploy hotfix build. | Engineering Lead |
| Rewarded ad reward not granted after ad completion | P1 | 4 hours | Investigate AdMob callback vs. Cloud Run `/grant-reward` endpoint. Check for SDK version incompatibility. RC mitigation: disable rewarded ads if grant failure rate > 10%. | Engineering On-Call |
| Cloud Run p95 latency > 2000ms sustained for 30+ minutes | P2 | 2 hours | Investigate Cloud Run scaling configuration. Check for cold start issues. Assess if latency is affecting user experience (cloud save failures, IAP failures). | Engineering On-Call |
| D1 retention < 20% (first cohort of 200+ users) | P2 | Next business day | Schedule level design and onboarding review. Identify drop-off points in Analytics. Prepare Level 1–3 difficulty adjustments via Remote Config. | Product Lead + Engineering Lead |
| Play Console review rating drops to < 3.5 stars | P2 | Next business day | Review all new 1-star and 2-star reviews. Categorize by topic (crash, performance, billing, gameplay). Address top category with a hotfix or RC change within 5 business days. | Product Lead |
| GCP billing spike > 2× daily baseline | P2 | 4 hours | Review Cloud Run invocation count, Firestore read/write count. Check for runaway client retry loop or abuse pattern. | Engineering On-Call |
| New Crashlytics issue affecting > 1% of sessions | P2 | 4 hours | Investigate crash type. If same root cause as a known P0 condition, escalate to P0 immediately. Otherwise document and prioritize in next patch. | Engineering On-Call |
| IAP purchase success rate < 85% of initiated purchases | P2 | 4 hours | Check Cloud Run `/verify-purchase` error logs. Verify Google Play Developer API availability. Check for Play Billing Library version issues. | Engineering Lead |

### Escalation Chain

```
On-Call Engineer
      |
      | (P0 or no response in 15 min)
      ↓
Engineering Lead
      |
      | (P0 data loss or billing, or no resolution in 1 hour)
      ↓
Product Lead
      |
      | (halt decision required or legal exposure)
      ↓
Executive Sponsor
```

---

## Rollback Procedures

### Tier 1: Remote Config Mitigation (< 5 minutes)

Use Remote Config to disable or modify specific features without a new build or Play Store submission. This is the fastest mitigation path.

```
Disable rewarded ads:       Set rewarded_ads_enabled = false
Disable interstitial ads:   Set interstitial_ads_enabled = false
Disable IAP:                Set iap_enabled = false
Skip broken level:          Set skip_level_id = <broken_level_id>
Adjust level difficulty:    Set <level_param> to easier value
Disable cloud save:         Set cloud_save_enabled = false (local only fallback)
Enable maintenance mode:    Set maintenance_mode = true (shows maintenance screen)
```

RC changes propagate to clients within the configured fetch interval (default: 60 minutes, minimum: 5 minutes with `fetchAndActivate()`). For emergency changes, the client fetch interval may need to be reduced in a subsequent build.

### Tier 2: Play Console Rollout Halt (< 2 minutes)

In Google Play Console → Production → Current Release → Manage Rollout → "Halt rollout". This stops new installs and updates. Existing installed versions continue to function.

When to use: crash-free drops below 98.5%, P0 data loss or billing incident confirmed.

After halting: fix the root cause, build a new AAB with an incremented version code, submit to Internal Testing → Closed Alpha review, then resume or replace the Production release.

### Tier 3: Cloud Run Rollback (< 10 minutes)

Roll back to the previous Cloud Run revision if a backend deployment introduced a regression.

```bash
# List revisions to identify the previous stable revision
gcloud run revisions list --service=king-smash-api --region=<region> --project=king-smash-prod

# Route 100% of traffic to the previous revision
gcloud run services update-traffic king-smash-api \
  --to-revisions=<PREVIOUS_REVISION>=100 \
  --region=<region> \
  --project=king-smash-prod
```

After rollback, verify 5xx rate returns to baseline within 5 minutes. Document the regression in the incident log.

### Hotfix Process

For issues that cannot be resolved with RC or Cloud Run rollback and require a new client build:

1. Create a hotfix branch from the production tag: `git checkout -b hotfix/v1.0.1 v1.0.0`.
2. Apply the minimum fix. Do not include unrelated changes.
3. Increment version code (4 for first hotfix) and version name (1.0.1).
4. Run full automated test suite. Pass required before proceeding.
5. Build production AAB with the same signing keystore used for the original release.
6. Upload to Internal Testing track. Wait for Play Console Pre-Launch Report.
7. Submit to Production track as a new release, set to current rollout percentage.
8. Monitor for 60 minutes at the same intensity as the initial launch Hour 1 protocol.

---

## Monitoring Tools Reference

| Tool | Access | Primary Use |
|---|---|---|
| Firebase Console → Crashlytics | console.firebase.google.com → king-smash-prod | Crash-free sessions, issue list, device/OS breakdown |
| Firebase Console → Analytics → DebugView | console.firebase.google.com → Analytics | Real-time event verification (DebugView) |
| Firebase Console → Analytics → Events | console.firebase.google.com → Analytics | Level funnel, retention cohorts, event counts |
| Firebase Console → Firestore | console.firebase.google.com → Firestore | Manual data inspection for suspected corruption |
| Firebase Console → Remote Config | console.firebase.google.com → Remote Config | Emergency RC changes |
| GCP Console → Cloud Run | console.cloud.google.com → Cloud Run → king-smash-api | Request count, latency, 5xx rate, instance count |
| GCP Console → Cloud Monitoring | console.cloud.google.com → Monitoring | Custom alerts, dashboards, GCP billing |
| Google Play Console → Android vitals | play.google.com/console → Android vitals | ANR rate, crash rate, battery/wake lock stats |
| Google Play Console → Statistics | play.google.com/console → Statistics | Installs, active installs, ratings |
| Google Play Console → Reviews | play.google.com/console → Reviews | Player feedback, crash reports from reviews |

---

## Launch Day Communication Template

Use this template for launch day status updates in the designated war room channel. Post at each hourly checkpoint.

```
KING SMASH LAUNCH STATUS — T+[X]h [DATE TIME TZ]
Rollout: [X]% | Total installs: [N]

HEALTH
  Crash-free sessions: [X]% (target >=99%)
  ANR rate: [X]% (target <0.47%)
  Cloud Run 5xx: [X]% (target <0.5%)
  Cloud save errors: [X]% (target <1%)

ENGAGEMENT
  app_open events (last hour): [N]
  level_complete (Level 1) rate: [X]%
  Rewarded ad loads: [X]%

STORE
  Play Console rating: [X] ([N] reviews)
  Install count: [N]

STATUS: GREEN / YELLOW / RED
NEXT CHECK: [TIME]
ON-CALL: [NAME]
```

**GREEN:** All metrics at or above target. Continue rollout per plan.
**YELLOW:** One or more metrics below target but above alert threshold. Increased monitoring frequency, no halt.
**RED:** Any metric at or below alert threshold, or any P0 event. Halt decision required.

---

## Stage Advancement Summary

| Stage | Rollout % | Decision Point | Decision Owner | Advancement Criteria |
|---|---|---|---|---|
| Stage 1 | 10% | Day 1 (T+24h) | Engineering Lead | Crash-free >= 99%, D0 >= 50%, no P0 |
| Stage 1 hold | 10% | Day 7 (T+7d) | Engineering Lead + Product Lead | D7 retention >= 15%, all Day 1 criteria still met |
| Stage 2 | 50% | Day 7 (T+7d) | Engineering Lead + Product Lead | All Stage 1 criteria met, D7 retention >= 15%, economy healthy, no P0/P1 open |
| Stage 3 | 100% | Day 14 (T+14d) | Product Lead + Engineering Lead | D14 retention >= 10%, purchase conversion >= 2%, crash-free >= 99%, no open incidents |

---

*Last updated: 2026-10-05 — M17 Post-Launch Monitoring Plan v1.0*
