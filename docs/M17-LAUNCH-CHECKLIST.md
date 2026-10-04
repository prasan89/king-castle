# King Smash M17 — Production Launch Checklist

**Document:** Production Launch Gate Checklist
**Version:** 1.0
**Date:** 2026-10-05
**Status:** PRODUCTION_LAUNCH_BLOCKED — 8 open blockers
**Build Target:** Android (ARM64), Unity 6 LTS (6000.0.47f1), IL2CPP Release
**Package:** com.kingcastle.kingsmash | Version Code: 3 | Version Name: 1.0.0

> **Legend:**
> - `[x]` PASSED — verified complete
> - `[ ]` OPEN — not yet verified
> - `[!]` BLOCKER — must be resolved before Production track submission

---

## Current Status

**PRODUCTION_LAUNCH_BLOCKED**

8 blockers must be resolved before the AAB is submitted to the Google Play Production track. All blockers are operational (provisioning, execution, device testing). All M0–M17 code is committed and code-complete.

| # | Blocker | Gate |
|---|---|---|
| 1 | M16 soft launch not executed — no real user data | Gate 1 |
| 2 | Physical device testing not performed (M15 carry-over) | Gate 2 |
| 3 | Production google-services.json not provisioned | Gate 3 |
| 4 | Production signing keystore not provisioned | Gate 6 |
| 5 | Google Play Console not configured (no app listing, no testing tracks) | Gate 7 / Gate 9 |
| 6 | Google Play Billing not tested with real sandbox (PurchaseServiceMock in path) | Gate 4 |
| 7 | AdMob production ad unit IDs not verified on physical device | Gate 5 |
| 8 | Firebase production connection not verified end-to-end | Gate 3 |

---

## Gate 1: M15 / M16 Status

### M15 Release Gate

| Item | Status | Notes |
|---|---|---|
| Automated unit test suite | [x] PASSED | All test suites green, M15 sign-off in history |
| Security review (M17-SECURITY-FINAL.md) | [x] CONDITIONAL PASS | 2 launch blockers (SEC-BLOCK-001, SEC-BLOCK-002) tracked in Gates 4 and 5 |
| M15 BUG-001 fix (M2TestLevelSetup debug guard) | [x] FIXED | `#if UNITY_EDITOR` wraps full `Start()` body |
| M15 P2-002 fix (service mock guard) | [x] FIXED | `FIREBASE_ENABLED && !KING_SMASH_DEV` gate verified in `GameBootstrap.cs` |

**M15 Gate: PASSED**

### M16 Soft Launch Gate

> **BLOCKER** — M16 soft launch has not been executed. No real-world user data exists. All items below are open.

| Item | Target | Status | Notes |
|---|---|---|---|
| Soft launch minimum duration | 14 days Closed Alpha | [!] BLOCKER | Not started |
| Minimum cohort size for retention signal | 200 activated users | [!] BLOCKER | No real users |
| D1 retention | Observed and recorded | [!] BLOCKER | No data |
| Level 1 completion rate | >= 85% | [!] BLOCKER | No data |
| P0 crash-free rate during soft launch | 100% P0 crashes (no fatal progression-loss crashes) | [!] BLOCKER | Not measured |
| Progression loss incidents | 0 | [!] BLOCKER | Not measured |
| Duplicate purchase grant incidents | 0 | [!] BLOCKER | Not measured |
| Soft launch analytics report reviewed | Crashlytics + Firebase Analytics reviewed | [!] BLOCKER | No data |
| M16 status advancement | NEEDS_MORE_SOFT_LAUNCH → READY_FOR_PRODUCTION | [!] BLOCKER | Blocked on soft launch execution |

**M16 Gate: BLOCKED — Soft launch must be executed and reviewed before production submission.**

---

## Gate 2: Physical Device Validation

> **BLOCKER** — Physical device testing has not been performed. This is an M15 carry-over item that was never resolved.

### Device Coverage Matrix

| Device Tier | RAM | Android Version | Status |
|---|---|---|---|
| Low-end (e.g., Samsung Galaxy A12 or equivalent) | 2–3 GB | Android 9 (API 28) | [!] BLOCKER — not tested |
| Low-end (e.g., Motorola Moto G9 or equivalent) | 2–3 GB | Android 10 (API 29) | [!] BLOCKER — not tested |
| Mid-range (e.g., Samsung Galaxy A54 or equivalent) | 6 GB | Android 12 (API 31) | [!] BLOCKER — not tested |
| High-end (e.g., Samsung Galaxy S23 or equivalent) | 8+ GB | Android 13 (API 33) | [!] BLOCKER — not tested |
| Notch / cutout device | Any | Android 9+ | [!] BLOCKER — not tested |

### Functional Verification Checklist (per device)

| Test | Pass Criteria | Status |
|---|---|---|
| Level 1 launch and completion | Completes without crash or hang | [!] BLOCKER |
| Levels 2–10 sequential play | All levels launch, play, and complete | [!] BLOCKER |
| Camera / audio / touch input | All inputs register correctly, no dead zones | [!] BLOCKER |
| Safe area / notch rendering | UI not obscured by notch, punch-hole, or status bar | [!] BLOCKER |
| Background and resume (app switch) | Game state preserved on resume, no crash | [!] BLOCKER |
| 30-minute continuous play (thermal test) | No excessive heating, no performance cliff, no crash | [!] BLOCKER |
| Firebase services on-device | Auth, Firestore, Analytics, Crashlytics all active on physical device | [!] BLOCKER |
| AdMob ad loading on physical device | Rewarded and interstitial ads load and display correctly | [!] BLOCKER |

**Gate 2: BLOCKED — Must be completed on all three device tiers before production submission.**

---

## Gate 3: Production Firebase

> **BLOCKER** — Production google-services.json has not been provisioned. Firebase production connection has not been verified end-to-end.

### Project Provisioning

| Item | Status | Notes |
|---|---|---|
| Firebase project `king-smash-prod` exists in Firebase Console | [ ] OPEN | Verify project is created |
| `google-services.json` provisioned for `king-smash-prod` | [!] BLOCKER | File is gitignored; must be provisioned from GCP Secret Manager at build time |
| SHA-1 fingerprint added to Firebase Console (release keystore) | [!] BLOCKER | Requires production keystore (Gate 6) |
| SHA-256 fingerprint added to Firebase Console (release keystore) | [!] BLOCKER | Requires production keystore (Gate 6) |
| Package name `com.kingcastle.kingsmash` registered in Firebase Console | [ ] OPEN | Confirm exact package name matches build config |

### Anonymous Auth Verification

| Item | Status | Notes |
|---|---|---|
| Anonymous Sign-In enabled in Firebase Console → Authentication | [ ] OPEN | |
| Anonymous auth sign-in succeeds on physical device | [!] BLOCKER | Requires device testing (Gate 2) |
| Firebase UID persists across app close and reopen | [ ] OPEN | Confirm on physical device |

### Cloud Save End-to-End Verification

| Item | Status | Notes |
|---|---|---|
| Complete Level 1 → Firestore write occurs (verify in Firebase Console) | [!] BLOCKER | |
| Close app → reopen → Level 1 progress restored from Firestore | [!] BLOCKER | |
| Cloud save error rate < 1% over test session | [ ] OPEN | |

### Firebase Analytics Verification

| Item | Status | Notes |
|---|---|---|
| `app_open` event appears in DebugView within 60 seconds | [!] BLOCKER | Enable DebugView on test device via `adb shell setprop debug.firebase.analytics.app com.kingcastle.kingsmash` |
| `level_start` (level_id=0) event fires when Level 1 begins | [ ] OPEN | |
| `level_complete` (level_id=0) event fires when Level 1 completes | [ ] OPEN | |
| User ID is hashed (SHA-256, 16-character hex) in DebugView — not the raw Firebase UID | [ ] OPEN | Confirm `user_id` field is not the raw UID |

### Crashlytics Verification

| Item | Status | Notes |
|---|---|---|
| Crashlytics initialized and reporting in Firebase Console | [!] BLOCKER | |
| Test non-fatal error triggered via `AnalyticsCrashTest` (dev build) appears in Crashlytics within 5 minutes | [ ] OPEN | Use `KING_SMASH_DEV` build for this test only |
| Crashlytics crash-free sessions baseline established | [ ] OPEN | Record baseline from first 100 sessions |

### Remote Config Verification

| Item | Status | Notes |
|---|---|---|
| Remote Config fetch succeeds (no timeout, no fallback-only path) | [!] BLOCKER | |
| `config_version=3` returned from production Remote Config | [ ] OPEN | Confirm parameter matches expected schema |
| Fallback values load correctly when Remote Config is unavailable | [ ] OPEN | Test with network disabled |

### Firestore Security Rules

| Item | Status | Notes |
|---|---|---|
| Production Firestore rules deployed to `king-smash-prod` | [ ] OPEN | Run `firebase firestore:rules:get --project king-smash-prod` to verify |
| `allow write: if false` confirmed on: economy, transactions, purchases, entitlements, dailyRewards | [ ] OPEN | Do not assume staging rules match production |
| Rules deployment verified with `firebase deploy --only firestore:rules --project king-smash-prod` | [ ] OPEN | |

**Gate 3: BLOCKED — google-services.json and end-to-end Firebase verification required.**

---

## Gate 4: Billing

> **BLOCKER** — `IPurchaseService` is registered as `PurchaseServiceMock` in `GameBootstrap.cs` line 118. Real Google Play Billing has never been active. This is SEC-BLOCK-001 from M17-SECURITY-FINAL.md.

### Real Billing Implementation

| Item | Status | Notes |
|---|---|---|
| Real `GooglePlayPurchaseService` implemented (replaces `PurchaseServiceMock`) | [!] BLOCKER | See GameBootstrap.cs line 118 |
| Registration gated on `#if FIREBASE_ENABLED && !KING_SMASH_DEV` | [!] BLOCKER | Must match other Firebase service registration guards |
| Google Play Billing Library 6.x dependency added and configured | [!] BLOCKER | |

### Sandbox Test Scenarios

| Scenario | Pass Criteria | Status |
|---|---|---|
| Sandbox purchase — successful purchase flow | Play dialog appears → player confirms → receipt sent to Cloud Run → entitlement written → content unlocked | [!] BLOCKER |
| Purchase cancellation | Player cancels Play dialog → no charge, no entitlement grant, UX recovers gracefully | [!] BLOCKER |
| Duplicate callback idempotency | Same purchase token sent twice to `/verify-purchase` → second request rejected, no double-grant | [!] BLOCKER |
| Restore purchases | Previously purchased entitlement restored after app reinstall | [!] BLOCKER |
| Remove Ads purchase | Remove Ads entitlement written to Firestore → interstitials suppressed from next session | [!] BLOCKER |
| Purchase acknowledgment within 3 days | Acknowledge call confirmed within Play policy window | [ ] OPEN |
| Network failure during purchase | Purchase token retained, retry on reconnect, no duplicate charge | [ ] OPEN |

### Cloud Run Billing Verification

| Item | Status | Notes |
|---|---|---|
| `/verify-purchase` endpoint receives purchase token from client | [ ] OPEN | |
| Cloud Run verifies token against Google Play Developer API | [ ] OPEN | |
| Entitlement written to Firestore by Cloud Run on success | [ ] OPEN | |
| Purchase token never logged or sent to Analytics | [ ] OPEN | Confirm via search of GameLogger call sites |

**Gate 4: BLOCKED — PurchaseServiceMock must be replaced with real Google Play Billing before production.**

---

## Gate 5: AdMob

> **BLOCKER** — Production ad unit IDs have not been verified. AdMob has not been tested on a physical device with production configuration. This is SEC-BLOCK-002 from M17-SECURITY-FINAL.md.

### Ad Unit ID Verification

| Item | Status | Notes |
|---|---|---|
| AdMob App ID in `AdConfiguration` ScriptableObject is production value (not test ID `ca-app-pub-3940256099942544`) | [!] BLOCKER | |
| AdMob App ID in `AndroidManifest.xml` is production value | [!] BLOCKER | |
| Rewarded ad unit ID is production value | [!] BLOCKER | |
| Interstitial ad unit ID is production value | [!] BLOCKER | |
| All production ad unit IDs verified against AdMob Console for `com.kingcastle.kingsmash` | [!] BLOCKER | |
| CI assertion added: no field in `AdConfiguration` asset contains `ca-app-pub-3940256099942544` | [ ] OPEN | Recommended — prevents accidental test ID in production build |

### Rewarded Ad Verification (physical device)

| Scenario | Pass Criteria | Status |
|---|---|---|
| Rewarded ad loads | Ad available, no load error | [!] BLOCKER |
| Rewarded ad plays to completion | Full ad plays, reward granted after completion | [!] BLOCKER |
| Early close / skip | Player closes ad early → reward NOT granted | [!] BLOCKER |
| Ad failure graceful handling | Load failure → UI recovers, no crash, player informed | [ ] OPEN |

### Interstitial Ad Verification (physical device)

| Scenario | Pass Criteria | Status |
|---|---|---|
| Interstitial appears after 3 levels | Interstitial fires between level 3 and level 4 completion screens, not during gameplay | [!] BLOCKER |
| Interstitial does NOT appear during gameplay | No interstitial mid-level | [!] BLOCKER |
| Remove Ads suppresses interstitials | After Remove Ads purchase, no interstitials appear | [!] BLOCKER |
| Interstitial cooldown enforced | Interstitial does not fire more frequently than Remote Config `interstitial_cooldown_seconds` setting | [ ] OPEN |

**Gate 5: BLOCKED — Production ad unit IDs must be verified and ad behavior confirmed on physical device.**

---

## Gate 6: Build

> **BLOCKER** — Production signing keystore has not been provisioned. The production AAB cannot be signed or submitted.

### Keystore Provisioning

| Item | Status | Notes |
|---|---|---|
| Production signing keystore provisioned in GCP Secret Manager | [!] BLOCKER | Not in repository (correct — should never be committed) |
| Keystore alias, password confirmed and stored securely | [!] BLOCKER | |
| CI retrieves keystore via Workload Identity Federation at build time | [!] BLOCKER | |
| SHA-1 and SHA-256 fingerprints extracted from keystore and registered in Firebase Console | [!] BLOCKER | Required for Gate 3 |

### Release AAB Validation

| Item | Pass Criteria | Status |
|---|---|---|
| Package name | `com.kingcastle.kingsmash` | [!] BLOCKER — AAB not built yet |
| Version code | 3 | [!] BLOCKER |
| Version name | 1.0.0 | [!] BLOCKER |
| Scripting backend | IL2CPP confirmed (not Mono) | [ ] OPEN |
| Architecture | ARM64 (aab with splits or universal) | [ ] OPEN |
| No `DEVELOPMENT_BUILD` define | Absent from production build defines | [ ] OPEN |
| No `KING_SMASH_DEV` define | Absent from production build defines | [ ] OPEN |
| No `KING_SMASH_STAGING` define | Absent from production build defines | [ ] OPEN |
| No `-xxx` placeholder Cloud Run URL | Absent from extracted assets (verify post-build) | [ ] OPEN |
| APK signature valid | `apksigner verify --print-certs release.apk` passes | [ ] OPEN |
| File size | < 200 MB (AAB before Play store optimization) | [ ] OPEN |

### Production Defines Verification

Confirm that the production build configuration in Unity (`Edit → Project Settings → Player → Scripting Define Symbols`) for the Android release configuration contains:

```
FIREBASE_ENABLED;GOOGLE_MOBILE_ADS
```

And does NOT contain:

```
DEVELOPMENT_BUILD
KING_SMASH_DEV
KING_SMASH_STAGING
```

**Gate 6: BLOCKED — Production signing keystore must be provisioned before the release AAB can be generated or verified.**

---

## Gate 7: Store Listing

> **Note:** Google Play Console has not been configured. No app listing exists, no testing tracks are set up.

### App Listing Assets

| Asset | Specification | Status |
|---|---|---|
| App name | "King Smash" (max 50 characters) | [ ] OPEN |
| Short description | Up to 80 characters | [ ] OPEN |
| Full description | Up to 4,000 characters | [ ] OPEN |
| App icon | 512 × 512 px, PNG, no transparency (Play Console requirement) | [ ] OPEN |
| Feature graphic | 1024 × 500 px, JPG or PNG | [ ] OPEN |
| Screenshots — phone | Minimum 2, maximum 8, portrait 9:16 recommended | [ ] OPEN |
| Screenshots — tablet (optional, recommended) | 10-inch tablet, landscape | [ ] OPEN |
| Promo video (optional) | YouTube URL, 30–120 seconds | [ ] OPEN |

### Store Copy Review (reference: M17-STORE-COPY.md)

| Item | Status |
|---|---|
| App title does not use prohibited terms (e.g., "best", "#1", "free" in title) | [ ] OPEN |
| Description does not contain keyword-stuffing or misleading claims | [ ] OPEN |
| Store copy reviewed and approved by product lead | [ ] OPEN |

### Legal and Compliance

| Item | Status | Notes |
|---|---|---|
| Privacy policy hosted at a permanent HTTPS URL | [ ] OPEN | Required for apps collecting any user data |
| Privacy policy URL entered in Play Console | [ ] OPEN | |
| Data Safety section completed in Play Console | [ ] OPEN | Declare Firebase Analytics, Crashlytics, Firebase Auth usage |
| IARC content rating questionnaire completed | [ ] OPEN | Required before submission |
| Target audience confirmed — NOT directed at users under 13 | [ ] OPEN | Confirm in audience and content settings |
| Support email address configured in Play Console | [ ] OPEN | |
| Developer address (if required by Play policies) | [ ] OPEN | |

### Google Play Console Configuration

| Item | Status | Notes |
|---|---|---|
| Play Console account active and app listing created | [!] BLOCKER | Not configured |
| App created with package name `com.kingcastle.kingsmash` | [!] BLOCKER | |
| Internal Testing track configured | [ ] OPEN | Required for Gate 9 |
| Closed Alpha testing track configured | [ ] OPEN | Required for soft launch / Gate 1 |
| Play Billing configured for in-app products | [ ] OPEN | Required for Gate 4 |

**Gate 7: OPEN — Requires Play Console setup and asset creation. No items are committed to a blocked state except Play Console itself not existing.**

---

## Gate 8: Security

| Item | Source | Status | Notes |
|---|---|---|---|
| SEC-BLOCK-001: PurchaseServiceMock replaced | M17-SECURITY-FINAL.md | [!] BLOCKER | Tracked in Gate 4 |
| SEC-BLOCK-002: AdMob production IDs verified | M17-SECURITY-FINAL.md | [!] BLOCKER | Tracked in Gate 5 |
| Client-side secrets audit | M17-SECURITY-FINAL.md §1 | [x] CLEAR | No production secrets in tracked codebase |
| Debug surface audit | M17-SECURITY-FINAL.md §2 | [x] CLEAR | All debug tooling gated from production builds |
| Economy integrity | M17-SECURITY-FINAL.md §3 | [x] CLEAR | Server-authoritative, allow write: if false |
| Authentication security | M17-SECURITY-FINAL.md §4 | [x] CLEAR | Firebase ID token verified on every Cloud Run call |
| Firestore security rules deployed to production | M17-SECURITY-FINAL.md §8 | [ ] OPEN | Tracked in Gate 3 |
| Cloud Run auth middleware active | M17-SECURITY-FINAL.md §4 | [ ] OPEN | Verify on production Cloud Run service revision |
| No `-xxx` placeholder Cloud Run URL in production AAB | M17-SECURITY-FINAL.md §8 | [ ] OPEN | Tracked in Gate 6 |
| SEC-P2-001: Certificate pinning | M17-SECURITY-FINAL.md §7 | [x] ACKNOWLEDGED | Non-blocking, post-launch recommendation |
| SEC-P2-002: Root/emulator detection | M17-SECURITY-FINAL.md §7 | [x] ACKNOWLEDGED | Non-blocking, post-launch recommendation |
| SEC-P2-003: CI URL validation step | M17-SECURITY-FINAL.md §7 | [ ] OPEN | Recommended CI guard, non-blocking |

**Gate 8: BLOCKED — SEC-BLOCK-001 and SEC-BLOCK-002 must be resolved. Non-security items are tracked in earlier gates.**

---

## Gate 9: Pre-Launch Report

> **Note:** Requires Gate 6 (AAB signed) and Gate 7 (Play Console configured and Internal Testing track active) to be complete first.

| Item | Pass Criteria | Status |
|---|---|---|
| Release AAB uploaded to Internal Testing track | Upload completes, no policy violations | [ ] OPEN — blocked on Gates 6, 7 |
| Play Console Pre-Launch Report generated | Report available in Play Console | [ ] OPEN |
| Critical ANRs in Pre-Launch Report | 0 critical ANRs | [ ] OPEN |
| ANR rate in Pre-Launch Report | < 0.47% (Play Console "bad behavior" threshold) | [ ] OPEN |
| Crash rate in Pre-Launch Report | 0 crashes in test matrix | [ ] OPEN |
| Device compatibility matrix reviewed | No unexpected incompatibilities flagged | [ ] OPEN |
| Accessibility warnings reviewed | All warnings assessed and either resolved or documented with justification | [ ] OPEN |
| Form factor issues reviewed | Tablet, foldable, and large-screen issues noted | [ ] OPEN |

**Gate 9: OPEN — Blocked on Gates 6 and 7.**

---

## Gate 10: Operations

### Rollout Strategy

| Item | Status | Notes |
|---|---|---|
| Rollout percentage confirmed: 10% (Stage 1) | [ ] OPEN | Reference M17-REMOTE-CONFIG-PRODUCTION.md and M16 rollout plan |
| Stage 1 → Stage 2 advancement criteria documented | [ ] OPEN | Day 1 report reviewed, crash-free >= 99%, D0 retention >= 50% |
| Stage 2: 50% rollout | [ ] OPEN | After Day 7 review |
| Stage 3: 100% rollout | [ ] OPEN | After Day 14 review |

### Rollback Procedures

| Item | Status | Notes |
|---|---|---|
| Tier 1: Remote Config rollback procedure tested | [ ] OPEN | Feature flags can disable economy features, ad features, or specific levels without a release |
| Tier 2: Play Console rollout halt procedure tested | [ ] OPEN | Confirm "Halt rollout" button in Play Console stops new installs/updates |
| Tier 3: Cloud Run rollback procedure tested | [ ] OPEN | `gcloud run services update-traffic` to previous revision documented and tested |
| Rollback decision tree documented | [ ] OPEN | Who decides to roll back and on what criteria (see M17-POST-LAUNCH-MONITORING.md escalation matrix) |

### GCP Monitoring

| Alert | Threshold | Status |
|---|---|---|
| Cloud Run 5xx error rate alert | > 1% over 5 minutes | [ ] OPEN |
| Cloud Run p95 latency alert | > 2000ms | [ ] OPEN |
| Cloud Run CPU / memory utilization alert | > 80% | [ ] OPEN |
| GCP billing anomaly alert | > 2× baseline daily spend | [ ] OPEN |

### Crashlytics Alerts

| Alert | Threshold | Status |
|---|---|---|
| Crash-free sessions alert | < 98.5% | [ ] OPEN |
| New issue velocity alert | > 5 new issues per hour | [ ] OPEN |
| Single issue affecting > 1% of sessions | Immediate notification | [ ] OPEN |

### Launch Day Readiness

| Item | Status | Notes |
|---|---|---|
| On-call engineer assigned for launch day (T-0 to T+24h) | [ ] OPEN | Name and contact required |
| Escalation chain documented (on-call → engineering lead → product lead) | [ ] OPEN | |
| Hotfix process documented (branch naming, build pipeline, emergency Play Console deploy process) | [ ] OPEN | |
| Post-launch monitoring schedule confirmed (M17-POST-LAUNCH-MONITORING.md) | [ ] OPEN | |
| War room / communication channel designated for launch day | [ ] OPEN | Slack channel or equivalent |

**Gate 10: OPEN — No hard blockers, but all items must be completed before the production track is set to "Live".**

---

## Final Sign-Off

All 10 gates must reach PASSED status before the production rollout is initiated. Sign-off confirms that all items in the relevant gate have been personally reviewed and verified.

| Role | Name | Signature | Date |
|---|---|---|---|
| Engineering Lead | | | |
| Product Lead | | | |
| QA Lead | | | |

> **Note:** Sign-off authorizes the initiation of the Stage 1 (10%) rollout only. Stage 2 and Stage 3 rollout authorizations are governed by the Day 7 and Day 14 review criteria in M17-POST-LAUNCH-MONITORING.md.

---

## Appendix: Blocker Resolution Order

The 8 blockers have dependencies. The recommended resolution order minimizes rework:

1. **Provision production signing keystore** (Gate 6) — required before Firebase SHA fingerprints can be registered (Gate 3) and before the release AAB can be built (Gates 5, 6, 9).
2. **Provision google-services.json from GCP Secret Manager** (Gate 3) — blocked on keystore for SHA fingerprints.
3. **Replace PurchaseServiceMock with real Google Play Billing** (Gate 4) — code work, can proceed in parallel with keystore provisioning.
4. **Verify AdMob production IDs in AdConfiguration ScriptableObject** (Gate 5) — can proceed in parallel.
5. **Set up Google Play Console app listing and testing tracks** (Gate 7) — can proceed in parallel.
6. **Perform physical device testing** (Gate 2) — requires Gates 3, 4, 5, 6 to be complete so the correct production build is tested.
7. **Execute M16 Closed Alpha soft launch** (Gate 1) — requires Gate 2 complete; requires minimum 14-day run.
8. **Upload AAB to Internal Testing, review Pre-Launch Report** (Gate 9) — requires Gates 6 and 7 complete.

---

*Last updated: 2026-10-05 — M17 Production Launch Checklist v1.0*
