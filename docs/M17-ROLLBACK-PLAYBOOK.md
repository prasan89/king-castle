# King Smash M17 — Rollback and Emergency Response Playbook

**Version:** 1.0.0 (version code 3)  
**Firebase Project:** king-smash-prod  
**Milestone:** M17 Production Launch  
**Status:** READY — review and test procedures against staging before any rollout begins  
**Prepared:** M17 Milestone  
**References:** docs/M17-ROLLOUT-STRATEGY.md, docs/M17-PRODUCTION-ALERTS.md

---

## Purpose

This playbook is the single authoritative reference for responding to production incidents during and after the King Smash staged rollout. Every team member with on-call responsibility must read and understand this document before the Stage 0 internal testing build is distributed.

The playbook is organized into four tiers based on response speed and mechanism. Start at Tier 1 and escalate only if lower tiers are insufficient.

**Decision rule:** Always attempt the fastest, lowest-blast-radius fix first. Do not skip to a hotfix build when a Remote Config toggle can mitigate the issue.

---

## Rollback Tiers Overview

| Tier | Mechanism | Requires New Build? | Response Time | Use When |
|---|---|---|---|---|
| Tier 1 | Remote Config emergency lever | No | <5 minutes | Feature behavior can be toggled or adjusted via RC |
| Tier 2 | Halt staged rollout | No | <15 minutes | Issue affects new installs; existing users can keep current build |
| Tier 3 | Cloud Run traffic rollback | No | <10 minutes | Backend regression in current revision; prior revision is healthy |
| Tier 4 | Hotfix build | Yes | 24–72 hours | Code bug that RC cannot mitigate; requires binary fix |

---

## Tier 1: Remote Config Emergency Lever

**Response time target: <5 minutes from incident detection to mitigation active**

Firebase Remote Config changes take effect for users on next fetch (minimum fetch interval is 12 hours for production builds, but an active fetch at session start means most new sessions will pick up the change within minutes). For immediate effect, the game can be configured to fetch on every foreground resume — confirm this behavior in `FirebaseRemoteConfigService.cs` minimum fetch interval setting.

**How to change a Remote Config value:**
1. Navigate to [Firebase Console](https://console.firebase.google.com) → Project `king-smash-prod`
2. Remote Config → find the key in the parameter list
3. Click the key → Edit value
4. Set the emergency value from the table below
5. Click **Publish changes** (top right)
6. Confirm the publish in the dialog
7. Monitor Crashlytics and Analytics to confirm the issue abates within 15 minutes

### Tier 1 Emergency Levers

| Issue | RC Key | Emergency Value | Effect |
|---|---|---|---|
| Interstitial ads causing crashes or extreme user friction | `interstitial_enabled` | `false` | Disables all interstitial ads for all users immediately on next RC fetch; no revenue impact short-term; use while investigating |
| Rewarded ads breaking reward grant flow | `rewarded_ads_enabled` | `false` | Disables rewarded ad placement; players cannot earn bonus coins via ads; use if ad callback is double-granting rewards |
| Shop / IAP causing crashes or displaying incorrect prices | `shop_enabled` | `false` | Hides shop UI entirely; blocks new IAP transactions; does not affect coins already earned |
| Power-ups causing exploit or crash | `powerups_enabled` | `false` | Disables power-up purchase and use; players with existing power-ups see them grayed out |
| Daily reward causing double-grant or Firestore write errors | `daily_reward_enabled` | `false` | Disables daily reward UI and claim flow; use if idempotency check is failing |
| Economy inflation: coins accumulating too fast | `coin_reward_multiplier` | `0.5` | Halves coin rewards for all players; use only for severe economy emergencies; announce to players |
| Economy deflation: players cannot progress due to high costs | `upgrade_cost_multiplier` | `0.75` | Reduces upgrade costs to 75% of base; use to unblock progression if a cost calibration error is found |
| Starting resource grant causing exploit | `starting_kings` | `1` | Reduces starting king count to minimum safe value |
| Destruction physics causing performance crashes on low-end devices | `destruction_multiplier` | `0.5` | Reduces destruction particle/physics multiplier; less satisfying visually but reduces memory pressure |
| Power-up pricing exploit discovered | `powerup_cost_multiplier` | `2.0` | Doubles power-up cost to reduce exploit viability while investigating |

**After applying a Tier 1 lever:**
- Post in team Slack with: key changed, old value, new value, reason, time applied, and who applied it
- File an incident report (see Post-Incident Documentation Template at the end of this document)
- Do not revert the lever until root cause is understood and fixed
- All RC changes during an incident must be logged with timestamp and author for the post-mortem

---

## Tier 2: Halt Staged Rollout

**Response time target: <15 minutes from incident detection to rollout halted**

Halting the rollout prevents new users from downloading version code 3. It does not affect users who have already installed it. Use this tier when:
- The crash or issue is severe enough that new users should not be onboarded to the affected build
- The RC levers in Tier 1 are insufficient to mitigate the issue
- The issue only affects new installs (e.g., first-launch crash)

### Tier 2 Steps

1. **Open Play Console:**  
   Navigate to [play.google.com/console](https://play.google.com/console) → King Smash → Production

2. **Open the active release:**  
   Click **Manage release** under "Production" → find the row for version code 3 (1.0.0)

3. **Edit the rollout:**  
   Click **Edit rollout** → the percentage slider appears

4. **Set percentage to 0%:**  
   Drag slider to 0% or type 0 in the percentage field → Click **Save**

5. **Review and confirm:**  
   Play Console shows a summary of the change. Click **Review release** → **Confirm rollout change**.  
   Status should change to "Rollout halted (0%)"

**After halting:**
- Post in team Slack: "Rollout halted. Version code 3 set to 0%. Reason: [reason]. Time: [UTC timestamp]"
- Assess whether the issue also affects existing installs:
  - **If yes:** Proceed to Tier 3 (Cloud Run) or Tier 4 (hotfix) as appropriate
  - **If only new installs:** Halt is sufficient; begin root cause analysis; prepare a fixed build (Tier 4)
- Do not re-enable the rollout until the issue is resolved and a new version code (4+) is ready, OR the issue is confirmed as a non-reproducible transient

**Note on resuming rollout after a halt:**  
To resume rollout of version code 3 after a halt (if the issue was resolved without a new build), go back to Play Console → Production → Manage release → Edit rollout and set a non-zero percentage. However, if a hotfix (version code 4) is being submitted, do not resume version code 3 — let version code 4 be the new rollout vehicle.

---

## Tier 3: Cloud Run Rollback

**Response time target: <10 minutes from detection to traffic routed away from bad revision**

Cloud Run maintains a history of deployed revisions. Each deployment creates a new revision. If the current revision is causing backend errors (5xx, high latency, auth failures), traffic can be routed to a prior healthy revision without a new deployment.

Use this tier when:
- Cloud Run 5xx rate is elevated (>2%)
- Cloud Run p95 latency has spiked significantly (>3s)
- A recent backend deployment (not a client build) is correlated with the incident
- Firestore permission denied errors are spiking (may indicate a bad Admin SDK change in the latest revision)

### Step 1: Discover Available Revisions

```bash
gcloud run revisions list \
  --service=game-api \
  --project=king-smash-prod \
  --region=us-central1 \
  --sort-by="~metadata.creationTimestamp" \
  --limit=5
```

This lists the 5 most recent revisions, newest first. The output includes revision name (e.g., `game-api-00042-abc`), creation time, and current traffic allocation.

Identify the last known-good revision: the one deployed before the incident began. Check deployment logs or CI/CD pipeline history to correlate revision timestamps with the incident start time.

### Step 2: Route Traffic to the Prior Revision

Replace `GOOD_REVISION` with the revision name identified in Step 1:

```bash
gcloud run services update-traffic game-api \
  --project=king-smash-prod \
  --region=us-central1 \
  --to-revisions=GOOD_REVISION=100
```

This routes 100% of traffic to the specified revision immediately. The command takes effect within seconds.

### Step 3: Verify the Rollback

```bash
gcloud run services describe game-api \
  --project=king-smash-prod \
  --region=us-central1 \
  --format="value(status.traffic)"
```

Confirm that `GOOD_REVISION` shows 100% traffic allocation.

Then check Cloud Run metrics:
```bash
# Check recent 5xx rate (requires Cloud Monitoring API or Console)
# Navigate to: Cloud Console -> Cloud Run -> game-api -> Metrics -> 5xx errors
# The error rate should drop within 2-3 minutes of the traffic switch
```

Alternatively, send a test request:
```bash
curl -s -o /dev/null -w "%{http_code}" https://[CLOUD_RUN_URL]/health
# Expected: 200
```

### Step 4: Post-Rollback Actions

- Post in Slack: "Cloud Run rolled back to [REVISION]. Incident time: [start] to [end]. 5xx rate should normalize within 5 minutes."
- Do NOT redeploy the bad revision. Investigate the failure in the revision's logs before any new deployment:
  ```bash
  gcloud logging read \
    "resource.type=cloud_run_revision AND resource.labels.revision_name=[BAD_REVISION] AND severity>=ERROR" \
    --project=king-smash-prod \
    --limit=50 \
    --format=json
  ```
- File an incident report (see template at end of document)
- A new Cloud Run deployment to fix the issue should be treated as Tier 4 equivalent for backend: internal soak on a staging revision before routing production traffic

---

## Tier 4: Hotfix Build

**Response time target: 24–72 hours from incident detection to hotfix live for affected users**

Use this tier when:
- The issue is in client-side code that cannot be mitigated by RC or Cloud Run rollback
- The crash or bug requires a binary fix
- Tier 1–3 mitigations are in place (halt rollout, disable feature via RC) but are not sufficient long-term

### Step 1: Cut Hotfix Branch

Cut the hotfix branch from the release tag (not from main, which may contain unreleased features):

```bash
git fetch --tags
git checkout -b hotfix/1.0.1 refs/tags/v1.0.0
```

The release tag `v1.0.0` marks the exact commit used for version code 3. Working from main risks including unintended changes.

### Step 2: Increment Version

Update both the version code and version name:

| Field | Current (broken) | Hotfix |
|---|---|---|
| `versionCode` | 3 | **4** |
| `versionName` | 1.0.0 | **1.0.1** |

In `Assets/Plugins/Android/mainTemplate.gradle` (or the Unity Player Settings — confirm actual location in the project):

```groovy
defaultConfig {
    versionCode 4          // was 3
    versionName "1.0.1"    // was "1.0.0"
}
```

Commit the version bump:
```bash
git add Assets/Plugins/Android/mainTemplate.gradle
git commit -m "Bump version to 1.0.1 (version code 4) for hotfix"
```

### Step 3: Apply the Fix

Make the minimum necessary change to fix the P0/P1 issue. Hotfix commits should be surgical — do not refactor, add features, or address non-critical issues in a hotfix branch. Each additional change is a risk.

Code review is required for the fix even in a hotfix context. At minimum, a second engineer must review the change before the build is submitted.

### Step 4: Internal 24-Hour Soak

Build the hotfix AAB and upload to Play Console → Internal Testing track (version code 4).

- Share with the internal testing team
- Monitor Crashlytics for 24 hours
- Confirm the P0/P1 issue is resolved
- Confirm crash-free rate >=99% over the soak period
- Confirm no new issues introduced by the fix

Do not promote to production without the 24-hour internal soak. The pressure to release quickly is real, but a broken hotfix compounds the incident.

### Step 5: Staged Rollout of Hotfix

Do not release the hotfix at 100% immediately. Even for a P0, a brief staged rollout (5% for a shortened window of 12–24 hours instead of the standard 48h) gives early warning if the fix itself has issues.

- Upload version code 4 to Play Console → Production at 5%
- Monitor for 12–24 hours
- If crash-free >=99% and P0 is resolved: promote to 100%
- If a new issue appears: halt version code 4 and prepare version code 5

### Step 6: Merge Back

After the hotfix is stable at 100%, merge the hotfix branch back to main:

```bash
git checkout main
git merge hotfix/1.0.1 --no-ff -m "Merge hotfix/1.0.1 into main"
git tag v1.0.1
git push origin main --tags
```

Delete the hotfix branch after merge.

---

## Critical Incident Procedures

### Incident P0-1: Crash on Launch (>5% of Sessions)

**Severity:** P0 — all hands  
**Definition:** More than 5% of sessions are crashing during app startup (before the main menu is displayed). Crashlytics crash-free rate drops below 95%.

**Response timeline:**

| Time | Action | Owner |
|---|---|---|
| T+0 | Incident detected via Crashlytics alert or manual monitoring | On-call |
| T+5 min | Halt staged rollout (Tier 2) | On-call |
| T+10 min | Identify crash issue in Crashlytics; check stack trace | On-call |
| T+15 min | Assess RC mitigation: can the crashing feature be disabled via Tier 1? | On-call |
| T+20 min | If RC can mitigate: apply lever and monitor; post to Slack | On-call |
| T+30 min | All-hands notification: engineering lead, product lead, QA lead | On-call |
| T+1h | Root cause identified or escalated to Tier 4 | Engineering lead |
| T+24–72h | Hotfix build (Tier 4) if RC mitigation is insufficient | Engineering team |

**Investigation checklist:**
- [ ] Open Crashlytics → Issues → Sort by occurrences → Find the top issue
- [ ] Read the stack trace — which C# class and method is the top frame?
- [ ] Check if the crash correlates with a specific Android API level (Crashlytics → filter by OS version)
- [ ] Check if the crash correlates with a specific device model
- [ ] Check if the crash started with the current version code (3) or was present in prior builds
- [ ] Check Cloud Run logs: did any backend request fail during boot sequence?
- [ ] Check Remote Config: did a RC publish happen within 1 hour of the crash spike?

**RC mitigation options for launch crashes:**
- If crash is in ad initialization: set `interstitial_enabled=false`, `rewarded_ads_enabled=false`
- If crash is in Remote Config fetch: the RC fetch failure handling should prevent this, but if not, a default-only mode may be needed via a backend flag
- If crash is in shop/IAP initialization: set `shop_enabled=false`
- If crash is in daily reward: set `daily_reward_enabled=false`

**If RC mitigation brings crash-free back above 99%:** keep lever active, file Tier 4 hotfix for the underlying crash, re-enable feature after fix is deployed.

**If RC mitigation is insufficient:** Tier 4 hotfix is the only remaining option. The build will remain halted at 0% until version code 4 is ready.

---

### Incident P0-2: Cloud Save Data Loss

**Severity:** P0 — all hands  
**Definition:** Users are reporting loss of game progress, coin balances, or upgrade states after reinstalling the app or signing in from a new device. Firestore documents are missing or contain stale data.

**Response timeline:**

| Time | Action | Owner |
|---|---|---|
| T+0 | Incident detected via support reports or monitoring | On-call |
| T+5 min | Halt staged rollout (Tier 2) | On-call |
| T+10 min | Disable cloud save writes via RC: set a flag to make cloud save read-only while investigating | On-call |
| T+15 min | All-hands notification | On-call |
| T+30 min | Cloud Run logs review: check save endpoint for errors | Backend engineer |
| T+1h | Firestore audit: compare affected user documents with Analytics events for those UIDs | Backend engineer |

**Critical constraints for data loss incidents:**

- **DO NOT directly edit Firestore documents** to "restore" data. Manual edits bypass all server-side validation and idempotency checks. A manual edit can create inconsistencies that are worse than the original loss.
- **DO NOT run batch Firestore scripts** without a full dry-run and lead sign-off.
- The only safe restore path is through the Cloud Run game-api endpoints with proper authentication.

**Investigation checklist:**
- [ ] Check Cloud Run logs for save endpoint errors (500, 403, timeout) in the incident window:
  ```bash
  gcloud logging read \
    "resource.type=cloud_run_revision AND httpRequest.requestUrl=~\"/api/save\" AND severity>=ERROR" \
    --project=king-smash-prod --limit=100 --format=json
  ```
- [ ] Check Firestore rules: were the rules updated recently? Could a rules change have blocked server writes?
- [ ] Check Cloud Run service account permissions: does it still have Firestore write access?
- [ ] Identify affected UIDs from support reports
- [ ] For affected UIDs, check Analytics for `level_complete` events that occurred without corresponding Firestore `progress` document updates
- [ ] Check if the issue is specific to Google Sign-In linked accounts (vs anonymous accounts)

**If data loss is confirmed:**
- Keep cloud save disabled via RC while investigating scope
- Do not announce a specific restore ETA until root cause is confirmed
- Firestore Backup may be available if Point-in-Time Recovery (PITR) is enabled in the Firestore settings — check before any restore attempt

---

### Incident P0-3: Duplicate Purchase / Reward Grant

**Severity:** P0 — all hands (economy integrity)  
**Definition:** Users are receiving double or multiple grants for a single purchase or rewarded ad view. Coin or gem balances are increasing beyond expected values for the transactions performed.

**Response timeline:**

| Time | Action | Owner |
|---|---|---|
| T+0 | Incident detected via analytics (reward_granted > ad_completed) or user reports | On-call |
| T+5 min | Halt staged rollout (Tier 2) | On-call |
| T+10 min | Disable affected features via RC: if IAP — `shop_enabled=false`; if ad reward — `rewarded_ads_enabled=false`; if daily reward — `daily_reward_enabled=false` | On-call |
| T+15 min | All-hands notification | On-call |
| T+30 min | Audit Cloud Run transaction idempotency | Backend engineer |
| T+1h | Scope determination: how many users affected, total economy impact | Backend engineer + Product |

**Critical constraints for economy incidents:**

- **DO NOT make manual economy adjustments** (adding or removing coins from user accounts directly in Firestore). Manual adjustments are not auditable through the normal transaction log and create legal/compliance risk if users dispute them.
- Economy corrections, if necessary, must go through the Cloud Run game-api with a purpose-built correction endpoint that logs the correction with reason and authorizing engineer.
- **DO NOT announce** specific compensation amounts publicly until the scope and root cause are confirmed.

**Investigation checklist:**
- [ ] Check Cloud Run purchase endpoint for idempotency: each transaction must have a unique `orderId` or `requestId` stored in Firestore; a second request with the same ID must return 409 (not 200)
- [ ] Check Cloud Run logs for the reward grant endpoint:
  ```bash
  gcloud logging read \
    "resource.type=cloud_run_revision AND httpRequest.requestUrl=~\"/api/rewards\"" \
    --project=king-smash-prod --limit=100 --format=json
  ```
- [ ] In Analytics: `rewarded_ad_reward_granted` count vs `ad_completed` count — if `reward_granted > ad_completed`, the idempotency check is failing at the API level
- [ ] Check Firestore transaction document (`/users/{uid}/purchases/{orderId}`) for duplicate entries
- [ ] Check if the duplication is correlated with specific network conditions (retry storm from client on slow connection)
- [ ] Review `FirebaseAdService.cs` client-side: is the reward callback being called multiple times for a single ad view?

---

### Incident P1-1: High Crash Rate on Specific Device / API Level

**Severity:** P1 — response within 1 hour  
**Definition:** A specific device model or Android API level is showing a significantly elevated crash rate (>5% for that segment) while the overall crash-free rate remains above 99%. The issue is isolated and does not affect the majority of users.

**Response timeline:**

| Time | Action | Owner |
|---|---|---|
| T+0 | Issue detected in Crashlytics → Issues → filter by device or OS version | On-call |
| T+30 min | Assess RC mitigation: can the crashing feature be disabled? | On-call |
| T+1h | Assign to engineer for investigation; do not halt rollout unless crash-free drops below 99% overall | Engineering lead |
| T+24–48h | Hotfix targeting specific API level, or RC conditional feature flag by API level | Engineering team |

**Investigation checklist:**
- [ ] Crashlytics → Issues → click the issue → filter by Device and OS Version to confirm scope
- [ ] Check the stack trace for API-level-specific calls (e.g., a feature that uses an API not available on API 26)
- [ ] Check if the device model has a known Unity issue (Unity bug tracker, known issues for Unity 6 LTS)
- [ ] Assess RC mitigation: Remote Config can be configured with conditions based on device properties (Firebase RC conditions support `device.os` targeting)
  - If the feature causing the crash can be disabled for the affected API level via a RC condition, apply it
- [ ] If no RC mitigation is possible, prepare a Tier 4 hotfix targeting the specific API level issue
- [ ] Do not halt the overall rollout unless the affected segment is large enough to pull overall crash-free below 99%

---

## Post-Incident Documentation Template

Every P0 and P1 incident must produce a completed post-incident report within 48 hours of resolution. P2 and P3 incidents should produce a brief (1-page) summary.

Store post-incident reports in `docs/incidents/` with filename format `INC-YYYY-MM-DD-SHORT-TITLE.md`.

---

```
## Incident Report

### Incident ID
[Format: INC-YYYY-MM-DD-NNN, e.g. INC-2026-10-15-001]

### Date and Time
- Detected: [UTC datetime]
- Mitigated: [UTC datetime]
- Resolved: [UTC datetime]
- Total duration: [hours and minutes]

### Severity
[P0 / P1 / P2 / P3]

### Impact
- Users affected: [count or estimated percentage]
- User experience: [e.g., "App crash on launch", "Duplicate coin grant"]
- Revenue impact: [e.g., "IAP disabled for 3 hours", "Estimated N duplicate grants"]
- Rollout impact: [e.g., "Rollout halted at 5% for 18 hours"]

### Timeline

| Time (UTC) | Event |
|---|---|
| HH:MM | Incident detected |
| HH:MM | First response action |
| HH:MM | Mitigation applied |
| HH:MM | Root cause identified |
| HH:MM | Fix deployed |
| HH:MM | Resolved |

### Root Cause
[Technical description of what caused the incident. Be specific: include code paths, services, and data flows involved.]

### Fix Applied
[What was changed to resolve the incident. Include:
- Tier 1 RC changes (key, old value, new value)
- Tier 2 halt details
- Tier 3 Cloud Run revision change
- Tier 4 code change (commit hash, files changed)]

### Prevention
[What will be done to prevent recurrence. Assign each item to an owner with a due date.]

| Action | Owner | Due Date | Status |
|---|---|---|---|
| [action] | [owner] | [date] | Open |

### Lessons Learned
[What went well? What would you do differently? What monitoring or alerting gaps were revealed?]
```

---

## Quick Reference: Emergency Contact and Access

Before the rollout begins, ensure the following access is verified and documented (in the internal team wiki, not in this public document):

- [ ] Firebase Console access confirmed for all on-call engineers
- [ ] Google Play Console access confirmed for on-call engineer (Editor role minimum)
- [ ] GCP Cloud Console access confirmed for backend on-call (Cloud Run revision list and traffic update)
- [ ] Slack channel for incidents created: `#king-smash-production`
- [ ] PagerDuty or equivalent on-call rotation set up with escalation policy
- [ ] War room process documented: video link or conference bridge for P0 incidents

---

*Document version: 1.0 — M17 Milestone*  
*See also: docs/M17-ROLLOUT-STRATEGY.md, docs/M17-PRODUCTION-ALERTS.md*
