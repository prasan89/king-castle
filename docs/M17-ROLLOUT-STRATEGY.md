# King Smash M17 — Staged Production Rollout Strategy

**Version:** 1.0.0 (version code 3)  
**Firebase Project:** king-smash-prod  
**Milestone:** M17 Production Launch  
**Status:** BLOCKED — M16 must reach READY_FOR_PRODUCTION before any rollout action  
**Prepared:** M17 Milestone  
**References:** docs/M17-ROLLBACK-PLAYBOOK.md, docs/M17-PRODUCTION-ALERTS.md, docs/M17-FIREBASE-PRODUCTION-CHECKLIST.md

---

## Gate Status

> **THIS DOCUMENT IS A PLAN ONLY.**  
> No action in this document may be taken until M16 soft-launch status is **READY_FOR_PRODUCTION**.  
> Current M16 status: **NEEDS_MORE_SOFT_LAUNCH**  
> Attempting to run a staged rollout while M16 is not complete risks launching an under-validated build to real users without sufficient baseline data.

When M16 is marked READY_FOR_PRODUCTION, return to Section 1 (Pre-Conditions) and begin gate verification.

---

## 1. Pre-Conditions — All 10 Required Before Any Rollout Action

Every item below is a hard gate. A single failure blocks launch. Do not proceed to Stage 0 until all 10 items are verified and signed off by a lead.

### PC-1: M16 = READY_FOR_PRODUCTION

**Owner:** Live-ops lead / Product  
**Verification:** M16 final report (`docs/M16-FINAL-REPORT.md`) must show status `READY_FOR_PRODUCTION`.  
This means:
- Soft-launch crash-free session rate held >=99% for at least 7 consecutive days
- D1, D3, D7 retention benchmarks met
- No unresolved P0 or P1 issues open in the M16 tracker
- Economy balance confirmed stable (no runaway coin accumulation, no spend dead zones)
- Firebase Remote Config production baseline finalized and committed to `docs/M17-REMOTE-CONFIG-PRODUCTION.md`

**Current status:** NOT MET — M16 is NEEDS_MORE_SOFT_LAUNCH

### PC-2: Physical Device Testing Complete

**Owner:** QA lead  
**Verification:** `docs/M17-FIREBASE-PRODUCTION-CHECKLIST.md` must be 100% complete with all sections showing `[PASS]` on at least 2 physical Android devices (different API levels, recommend API 29 and API 34).  
Emulator testing does not satisfy this gate. Crashlytics symbol upload, AdMob real ads, and Google Sign-In OAuth all behave differently on real devices.

**Current status:** NOT YET PERFORMED

### PC-3: Production Firebase Verified

**Owner:** Backend engineer  
**Verification:** All sections of `docs/M17-FIREBASE-PRODUCTION-CHECKLIST.md` show `[PASS]`:  
- Section 1: Android app registration and SHA fingerprints (both upload key and app signing key)
- Section 2: Authentication (anonymous + Google Sign-In)
- Section 3: Firestore security rules deployed and economy collections are server-write-only
- Section 6: Remote Config production baseline (config_version=3) uploaded and fetched on device
- Section 7: Cloud Run game-api deployed, auth middleware active, rate limiting active

**Current status:** NOT YET PERFORMED

### PC-4: Billing Smoke Test Passed

**Owner:** Engineer + Finance  
**Verification:** At least one real in-app purchase completed successfully on a physical device against the production Google Play billing environment (not test SKUs). Verify:  
- Purchase receipt acknowledged server-side within 5 seconds
- Coins/gems credited to account after acknowledgment (not before)
- Receipt stored in Firestore `users/{uid}/purchases/{orderId}` for audit trail
- Duplicate purchase attempt (replay of same receipt) returns 409 from Cloud Run
- Play Console → Order Management shows the test order

**Current status:** NOT YET PERFORMED

### PC-5: AdMob Verified

**Owner:** Monetization engineer  
**Verification:** On a physical device with the production AAB:  
- Rewarded ads load and display (not test ads — confirm Ad Unit IDs are production IDs from `RemoteConfigKeys` or `AdsConfig.cs`)
- `rewarded_ad_reward_granted` Analytics event fires only after `ad_completed` event (reward integrity)
- Interstitial ads fire at correct frequency (`interstitial_frequency` RC key = 3, meaning every 3rd level-end)
- No ad fires during a level (only between levels)
- GDPR consent banner appears on first launch in EU region (if applicable)

**Current status:** NOT YET PERFORMED

### PC-6: Cloud Save Verified

**Owner:** Backend engineer  
**Verification:**  
- Google Sign-In account linking flow completes without error
- After linking, anonymous data migrated to Google account UID
- Progress and economy correctly restored after signing in on a second physical device
- Re-install on same device + sign in restores all progress (Firestore cloud save)
- Conflict resolution (server-wins) works correctly when local progress is newer than cloud

**Current status:** NOT YET PERFORMED

### PC-7: Pre-Launch Report Reviewed

**Owner:** Product lead  
**Verification:** Google Play Console → Pre-launch report has been reviewed. Specifically:  
- No crashes in the Pre-Launch Report robo test (Firebase Test Lab)
- No ANRs flagged
- Accessibility issues reviewed and any critical ones triaged (not all accessibility issues are blockers, but crashes are)
- Screenshots from various form factors reviewed (tablet, foldable if applicable)

**Current status:** Pre-Launch Report not yet available — requires AAB upload to Play Console

### PC-8: Crash-Free Rate >=99% on Internal Build

**Owner:** QA lead  
**Verification:** Firebase Crashlytics dashboard for the internal testing track shows:  
- Crash-free session rate >=99% over the most recent 5-day window
- Zero P0 issues (crash on launch, crash during IAP, data loss)
- No new issue with >50 occurrences in the last 24 hours that has not been triaged
- ANR rate <0.47% (Google Play baseline threshold)

**Current status:** NOT YET PERFORMED — requires internal build on physical devices

### PC-9: Staged Rollout Approved by Leads

**Owner:** Engineering lead + Product lead  
**Verification:** Written approval recorded in project tracker (Jira/Linear/equivalent) with:
- Engineering lead sign-off
- Product lead sign-off
- Date and build version code noted (must match version code 3)
- Acknowledgment that M17-ROLLBACK-PLAYBOOK.md has been read

**Current status:** NOT OBTAINED

### PC-10: Rollback Procedures Tested

**Owner:** Engineering lead  
**Verification:** At least one person on the team has walked through each tier of `docs/M17-ROLLBACK-PLAYBOOK.md` in a staging environment:  
- Tier 1: Toggled `interstitial_enabled = false` via RC on staging and confirmed ads stopped
- Tier 2: Confirmed the Play Console path to halt a staged rollout (tested on a staging app listing)
- Tier 3: Executed `gcloud run services update-traffic` to redirect traffic to a prior revision in staging
- Tier 4: Confirmed a hotfix branch can be cut from the release tag, incremented to version code 4, and uploaded to internal track

**Current status:** NOT YET TESTED

---

## 2. Staged Rollout Plan

Total estimated duration from Stage 0 to Stage 4 (100%): **10–15 days**.

Do not compress the timeline. Each stage provides a distinct safety net. Compressing removes the ability to catch stage-specific issues before they affect a larger audience.

| Stage | Track | Percentage | Duration | Success Criteria | Halt Trigger |
|---|---|---|---|---|---|
| Stage 0 | Internal Testing | ~0.01% (~100 testers) | 5 days | Crash-free >=99% over full 5-day window; zero P0 issues; billing smoke test confirmed; all PC gates signed off | Any P0 issue; crash-free <99% on any single day; billing failure |
| Stage 1 | Production | 5% | 48 hours | Crash-free >=99%; Cloud Run 5xx rate <0.5%; no billing failures; `level_start` event flowing in Analytics | Crash-free <99%; any P0; 5xx >0.5%; any billing failure or duplicate reward |
| Stage 2 | Production | 20% | 48–72 hours | All Stage 1 gates met; no new P0 or P1 issue with >50 occurrences in prior 24h; ANR rate <0.47% | Any new P0; crash-free drops below 99%; 5xx >0.5%; ANR >0.47% |
| Stage 3 | Production | 50% | 48–72 hours | All Stage 2 gates met; D1 retention not critically below benchmark (not >20% below M16 soft-launch D1); p95 API latency <2s; no progression loss reports | D1 retention drops >20% below benchmark; any P0; crash-free <99%; API p95 >3s |
| Stage 4 | Production | 100% | Permanent | Sustained health across all metrics for >=48h at 50%; all prior gates met | Any P0 post-100%; regression in crash-free rate by >1pp within 24h of reaching 100% |

### Stage 0: Internal Testing

**Track:** Play Console → Internal Testing  
**Testers:** ~100 (QA team, leads, select beta users enrolled in internal track)  
**Version code:** 3  
**Duration:** 5 days minimum (do not promote to Stage 1 before 5 days regardless of metrics)

**Objective:** Final validation that the production AAB, production Firebase environment, and production Remote Config baseline all work together on real devices before any public traffic.

**Actions during Stage 0:**
1. Upload AAB (version code 3, version name 1.0.0) to Play Console → Internal Testing track
2. Confirm google-services.json embedded in build corresponds to `king-smash-prod` (not staging)
3. Share internal track link with all testers
4. Monitor Crashlytics daily — any new issue >10 occurrences requires triage same day
5. Verify billing smoke test (PC-4) if not already completed
6. Confirm Analytics events are flowing: check Firebase Console → Analytics → DebugView and Events tab

**Stage 0 → Stage 1 gate:**  
- [ ] 5 days elapsed with crash-free >=99% each day
- [ ] Zero P0 issues
- [ ] All 10 PC gates confirmed
- [ ] Leads signed off on promotion

### Stage 1: Production 5%

**Track:** Play Console → Production  
**Action:** Play Console → Production → Manage release → Set rollout percentage to 5%  
**Duration:** 48 hours minimum

**Objective:** First real-user exposure. Catch issues that only appear at scale or with real payment instruments, real carrier networks, or real device diversity that internal testing did not cover.

**Actions during Stage 1:**
1. Set rollout to 5% in Play Console
2. Post in team Slack: "Stage 1 rollout started — 5% production. On-call: [name]"
3. Monitor Crashlytics every 2 hours for first 12 hours, then every 4 hours
4. Check Cloud Run 5xx rate in Cloud Monitoring every 4 hours
5. Verify `level_start` event volume is proportional to user count in Analytics
6. Check for any billing failure reports in Play Console → Order Management

**Stage 1 → Stage 2 gate:**  
- [ ] 48h elapsed
- [ ] Crash-free >=99% sustained across full 48h
- [ ] Cloud Run 5xx <0.5% (check in Cloud Monitoring: filter by service=game-api)
- [ ] No billing failures or duplicate reward grants
- [ ] `level_start` event flowing correctly (no 100% drop or suspicious spike)
- [ ] ANR rate <0.47% in Play Console → Android vitals

### Stage 2: Production 20%

**Track:** Play Console → Production  
**Action:** Play Console → Production → Manage release → Update rollout to 20%  
**Duration:** 48–72 hours

**Objective:** Wider device and network diversity. Surface issues in lower-end devices or less common Android versions.

**Actions during Stage 2:**
1. Update rollout to 20% in Play Console
2. Post in team Slack: "Stage 2 rollout started — 20% production"
3. Check Crashlytics → Issues → Sort by occurrences. Any new issue with >50 occurrences in 24h needs same-day triage.
4. Check Android vitals for new ANR patterns
5. Review Cloud Run metrics: request volume should be approximately 4x Stage 1 volume. If not proportional, investigate.
6. Review BigQuery Analytics (if BigQuery export configured) for anomalous event patterns

**Stage 2 → Stage 3 gate:**  
- [ ] 48h elapsed (72h preferred)
- [ ] All Stage 1 gates still met
- [ ] No new P0 or P1 issue with >50 occurrences in last 24h
- [ ] ANR rate <0.47%
- [ ] Cloud Run p95 latency <2s at 20% traffic load

### Stage 3: Production 50%

**Track:** Play Console → Production  
**Action:** Play Console → Production → Manage release → Update rollout to 50%  
**Duration:** 48–72 hours

**Objective:** Majority traffic test. Retention signals become statistically meaningful at this scale. D1 data from Stage 3 cohorts is the primary leading indicator of long-term retention health.

**Actions during Stage 3:**
1. Update rollout to 50% in Play Console
2. Post in team Slack: "Stage 3 rollout started — 50% production"
3. Begin monitoring D1 retention: Firebase Analytics → Retention → Filter to last 7 days. Compare against M16 soft-launch D1 benchmark.
4. Check for progression loss reports in user reviews or support inbox
5. Verify Cloud Run auto-scaling is handling the increased load (instance count should not be pegged at maximum)
6. Review ad revenue per session — if significantly below M16 benchmark, check AdMob configuration

**Stage 3 → Stage 4 gate:**  
- [ ] 48h elapsed (72h preferred)
- [ ] All Stage 2 gates still met
- [ ] D1 retention not >20% below M16 soft-launch D1 benchmark
- [ ] No progression loss reports in user reviews
- [ ] Cloud Run p95 latency <2s sustained at 50% load
- [ ] Ad reward integrity holds: `rewarded_ad_reward_granted` count <= `ad_completed` count in Analytics

### Stage 4: Production 100%

**Track:** Play Console → Production  
**Action:** Play Console → Production → Manage release → Update rollout to 100%  
**Duration:** Permanent

**Actions at Stage 4:**
1. Update rollout to 100% in Play Console
2. Post in team Slack: "King Smash is now at 100% production rollout"
3. Continue monitoring for 48h post-100% before reducing monitoring cadence
4. Archive M17 staging Firebase Remote Config experiments
5. Begin M18 planning

---

## 3. Between-Stage Health Checklist

Before promoting from any stage to the next, verify all items in this checklist. This is in addition to the stage-specific success criteria above.

### 3.1 Crashlytics Crash-Free Rate

- [ ] Navigate to Firebase Console → Crashlytics → Dashboard
- [ ] Filter to the current version (1.0.0 / version code 3)
- [ ] Crash-free users rate is >=99% for the entire stage duration (not just the most recent 24h)
- [ ] Crash-free sessions rate is >=99% (sessions metric is more sensitive than users metric for games)
- [ ] No single issue accounts for >0.5% of all sessions

### 3.2 New Issue Volume

- [ ] Navigate to Crashlytics → Issues tab → Sort by occurrences → Filter to last 24h
- [ ] No new issue has >50 occurrences in the last 24h without a triage note
- [ ] All issues with >50 occurrences in 24h have been assigned a severity (P0/P1/P2/P3) and an owner
- [ ] No issue is in "open" state with >200 occurrences in 24h (automatic halt trigger regardless of crash-free rate — a concentrated crash pattern may be under-counted in the overall crash-free rate if it only affects a specific flow)

### 3.3 ANR Rate

- [ ] Navigate to Play Console → Android vitals → ANRs & crashes
- [ ] Filter to the current APK version
- [ ] ANR rate is below 0.47% (Google's threshold for "bad behavior" designation)
- [ ] No single ANR issue accounts for >0.2% of daily active devices

### 3.4 Cloud Run 5xx Error Rate

- [ ] Navigate to Cloud Console → Cloud Run → game-api → Metrics
- [ ] Filter to the last 24h
- [ ] 5xx error rate is <0.5% of total requests
- [ ] No sustained 5xx spike (>2% over any 5-minute window) in the last 24h
- [ ] Check error logs: Cloud Console → Logging → Filter `resource.type="cloud_run_revision" AND severity>=ERROR`

### 3.5 p95 API Latency

- [ ] Cloud Run → game-api → Metrics → Request latency → p95 percentile
- [ ] p95 latency is <2s over the last 24h
- [ ] No sustained period (>10 minutes) where p95 exceeded 3s
- [ ] If latency has increased since Stage 1, investigate before promoting — latency increases at higher traffic are expected but must remain within bounds

### 3.6 Analytics Event Integrity

- [ ] Firebase Console → Analytics → Events → `level_start` event count is not zero and not anomalously high
- [ ] `level_start` volume is proportional to the rollout percentage relative to prior stage (20% stage should show ~4x Stage 1 volume)
- [ ] `ad_rewarded_shown` count is <=`ad_completed` count (a reward should never be granted without a completed ad view)
  - Query: Firebase Analytics → Explore → create funnel `ad_completed` → `rewarded_ad_reward_granted`; funnel completion rate should be <=100% not >100%
- [ ] `level_start` with `level_number=0` or `level_number=1` fires for all new users (level 1 funnel entry)
- [ ] No `level_complete` event fires without a prior `level_start` in the same session (Analytics Explorer event sequence check)

### 3.7 Progression Loss Reports

- [ ] Check Google Play Console → Reviews → Filter last 24h for keywords: "lost", "progress", "reset", "coins", "gone", "disappeared"
- [ ] Check support inbox (if applicable) for cloud save or progress restoration complaints
- [ ] Zero reports of progression reset after app close/update
- [ ] If any progression loss reports found: halt rollout and investigate Cloud Run save endpoint and Firestore transaction logs before promoting

---

## 4. Halt Rollout Procedure

If any halt trigger fires (see Stage table in Section 2) or any between-stage check fails beyond acceptable thresholds, halt the rollout immediately.

### 4.1 Halt Steps

1. **Navigate to Play Console:**  
   Google Play Console → Select King Smash → Production → Manage release

2. **Locate the active release:**  
   Under "Current release", find version code 3 (1.0.0)

3. **Set rollout percentage to 0%:**  
   Click "Edit rollout" → Set percentage to 0% → Save → Review and confirm

4. **Confirm halt:**  
   Play Console will show "Rollout halted". New users will not receive the build.

5. **Post to team Slack:**  
   "ROLLOUT HALTED — King Smash v1.0.0 rollout set to 0%. Reason: [reason]. On-call lead: [name]. War room: [link]"

### 4.2 Important Notes on Halt Behavior

**Halting does NOT remove the build from devices that already have it installed.**

Once a user has installed the build, halting the rollout prevents new users from receiving it but does not force-update or uninstall from existing users. Existing installs continue to run version code 3.

**To force existing users off a version:**  
The only mechanism to force users off a version is to release a higher version code (4 or above) at 100% or at a sufficient rollout percentage to reach affected users. A higher version code will be offered as an update to all existing users.

This means:
- A halt alone does not mitigate a crash that is already happening on installed builds
- If the crash is not mitigatable via Remote Config (Tier 1 in the Rollback Playbook), a hotfix build (Tier 4) must be submitted to provide a fix
- See `docs/M17-ROLLBACK-PLAYBOOK.md` for the full decision tree

### 4.3 When to Halt vs. When to Monitor

| Situation | Action |
|---|---|
| Crash-free drops from 99.5% to 99.1% with single known non-critical issue | Monitor for 2h; do not halt unless trend continues |
| Crash-free drops below 99% | Halt immediately |
| New P0 issue with >50 occurrences in 1h | Halt immediately |
| Cloud Run 5xx spikes to 1% for 10 minutes then recovers | Investigate root cause; do not promote to next stage until explained |
| Cloud Run 5xx sustained >2% for 5 minutes | Halt immediately |
| Single device model showing elevated crashes (not affecting overall crash-free) | Do not halt; file P1; use RC mitigation if possible |
| Billing failure reported by 1 user | Investigate; do not halt unless confirmed systematic |
| Billing failure confirmed systematic (>3 independent reports) | Halt immediately |

---

## 5. Remote Config During Rollout

Firebase Remote Config applies to all users regardless of rollout percentage. A change to a Remote Config value will affect 100% of users on any installed version, not just the rollout cohort.

**Implications for staged rollout:**

- Do not use RC to run experiments on the rollout cohort without understanding that RC changes are global
- If a RC change is needed to mitigate an issue (e.g., disable a feature via `shop_enabled=false`), it will affect all users including those on previous soft-launch builds
- The RC baseline for launch (config_version=3) is set before Stage 0 and should not be changed mid-rollout unless specifically as a mitigation action
- Refer to the RC emergency levers table in `docs/M17-ROLLBACK-PLAYBOOK.md` (Tier 1) for approved mid-rollout RC changes

---

## 6. Team Responsibilities During Rollout

| Role | Stage 0 | Stage 1 | Stage 2 | Stage 3 | Stage 4 |
|---|---|---|---|---|---|
| Engineering lead | Gate sign-off | On-call primary | On-call primary | On-call primary | Monitor 48h |
| Backend engineer | Cloud Run health | Cloud Run + billing | Cloud Run + latency | Scale verification | Monitor |
| QA lead | Physical device testing | Crashlytics monitor | Crashlytics + ANR | Retention check | Sign-off |
| Product lead | Gate sign-off | Review analytics | Funnel review | D1 retention | 100% announcement |
| Monetization engineer | AdMob verification | Ad integrity check | Revenue per session | Revenue trend | Monitor |

---

## 7. Post-Launch Monitoring Cadence

After reaching 100% (Stage 4), reduce monitoring intensity on the following schedule:

| Period | Crashlytics Check | Cloud Run Check | Analytics Review |
|---|---|---|---|
| Days 1–2 post-100% | Every 2 hours | Every 4 hours | Daily |
| Days 3–7 post-100% | Twice daily | Once daily | Daily |
| Days 8–14 post-100% | Once daily | Once daily | Weekly |
| Day 15+ | Alert-driven only | Alert-driven only | Weekly |

Alerts are configured in `docs/M17-PRODUCTION-ALERTS.md`. Alert-driven means the team acts when an alert fires, not on a fixed schedule.

---

*Document version: 1.0 — M17 Milestone*  
*See also: docs/M17-ROLLBACK-PLAYBOOK.md, docs/M17-PRODUCTION-ALERTS.md, docs/M17-FIREBASE-PRODUCTION-CHECKLIST.md*
