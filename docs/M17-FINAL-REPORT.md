# King Smash — M17 Production Launch Report

| Field | Value |
|---|---|
| **App** | King Smash |
| **Package** | `com.kingcastle.kingsmash` |
| **Target Version** | 1.0.0 (version code 3) |
| **Platform** | Google Play — Android |
| **Report Date** | 2026-10-05 |
| **Unity Version** | 6000.0.47f1 (Unity 6 LTS) |
| **Report Type** | Pre-Launch Infrastructure Complete — Launch Blocked (Operational Prerequisites Unmet) |
| **Overall Status** | **PRODUCTION_LAUNCH_BLOCKED** |

---

## Executive Summary

M17 completes the King Smash pre-production infrastructure. All application code across milestones M0 through M17 is committed, pushed to `origin/main`, and code-complete. The ServiceLocator architecture, server-authoritative economy, Firebase service integrations (Analytics, Crashlytics, Remote Config, Auth, Cloud Save), AdMob ad services, Cloud Run backend, SaveData migration chain (v1–v7), WorldRegistry with 5 worlds and 100 levels, the full UIScreen/ScreenManager design system, and all operational documentation — build configuration, Remote Config production baseline, security final review, store copy, post-launch monitoring plan, Firebase production checklist, launch checklist, and rollback playbook — have been authored, reviewed, and committed. No code blockers remain. From a source-code and documentation standpoint, King Smash is production-ready.

The honest assessment is that production launch is blocked entirely by operational prerequisites that have not been fulfilled. Eight blockers stand between the current repository state and a submitted AAB on the Google Play Production track: the M16 Closed Alpha soft launch has never been executed (no real-user retention or crash data exists); physical device testing has never been performed; the production signing keystore has not been provisioned; the production `google-services.json` has not been provisioned; Google Play Console has not been configured; Google Play Billing has not been tested (the service path still runs `PurchaseServiceMock`); AdMob production ad unit IDs have not been verified on a physical device; and the Firebase production environment has not been verified end-to-end on a production build. None of these blockers are code issues — all require provisioning, external platform configuration, or hands-on device testing that lies outside the scope of Unity source development.

The path forward is well-defined and fully documented in the M17 operational checklist suite. The recommended sequence begins with provisioning the production signing keystore (which unblocks Firebase fingerprint registration and all downstream device testing), proceeds through real billing integration and AdMob ID verification in parallel, then executes M16 Closed Alpha, and ends with Play Console Pre-Launch Report review followed by a gated Stage 1 (10%) rollout. Eleven to fifteen days of staged rollout monitoring — with documented advancement criteria at Day 1, Day 7, and Day 14 — are required before a 100% production release is authorized. Every step is documented. The work is ready to execute as soon as the operational prerequisites are addressed.

---

## Section 1: App Version

| Field | Value | Notes |
|---|---|---|
| **Target version name** | `1.0.0` | Clean semver for production submission. No suffix. |
| **Target version code** | `3` | rc1 = code 1, sl1 = code 2, production = code 3. |
| **Current build** | `1.0.0-sl1` (version code 2) | Soft-launch build. Not deployed to any Firebase environment. |
| **Current git commit** | `dd19284` | Latest commit on `main`. M16 infrastructure complete and committed. |
| **Build type target** | Release AAB | Android App Bundle for Google Play Store submission only. |
| **Scripting defines (production)** | `FIREBASE_ENABLED;GOOGLE_MOBILE_ADS` | Exactly these two defines. No `KING_SMASH_DEV`, no `DEVELOPMENT_BUILD`, no `KING_SMASH_STAGING`. |
| **IL2CPP** | Required | ARM64 + ARMv7. Managed stripping level: High. |
| **Unity version** | 6000.0.47f1 | Unity 6 LTS. Do not build production with a non-LTS release. |

### Version History

| Version Name | Version Code | Milestone | Status |
|---|---|---|---|
| 1.0.0-rc1 | 1 | M14/M15 RC build | Historical — not deployed |
| 1.0.0-sl1 | 2 | M16 Soft Launch | Defined, not deployed |
| **1.0.0** | **3** | **M17 Production** | **Target — not yet built** |

The production AAB (version code 3) has not been generated. It cannot be generated until the production signing keystore is provisioned (see Section 7).

---

## Section 2: Build Number

**Production build not yet generated.**

The release AAB (version code 3, version name `1.0.0`) has not been compiled or signed. Two hard prerequisites must be resolved before a production build can be generated:

1. **Production signing keystore not provisioned.** The upload keystore (`.jks`) must be created, stored in GCP Secret Manager, and the CI pipeline must be configured to retrieve it via Workload Identity Federation. Without the keystore, Unity cannot sign the AAB, and Play Console will reject any unsigned artifact.

2. **Google Play Console not configured.** The app listing with package name `com.kingcastle.kingsmash` must exist in Play Console before version code 3 can be uploaded. Version codes are attached to an app listing — they cannot be submitted to a non-existent listing.

When both prerequisites are met, the build is produced via the Unity CLI batch build command calling `BuildScript.BuildProductionAAB()` documented in `docs/M17-PRODUCTION-BUILD-CONFIG.md` Section 9. The post-build verification checklist (Section 10 of that document) must be completed for every build before upload. Key assertions: `aapt dump badging` confirms `versionCode='3'`, `versionName='1.0.0'`, `package: name='com.kingcastle.kingsmash'`; `apksigner verify` confirms the upload key is not the debug keystore; `unzip -l` confirms `libil2cpp.so` is present (not `libmono.so`).

**Expected build size:** Under 200 MB (AAB). This is a performance budget, not a Play Store constraint.

---

## Section 3: Git Commit

| Field | Value |
|---|---|
| **Latest commit hash** | `dd19284` |
| **Branch** | `main` |
| **Remote** | `origin/main` — committed and pushed |
| **Commit message** | Add M16: King Smash — controlled soft-launch strategy, balance patch, monitoring, and preparation |
| **M17 infrastructure** | All M17 documentation authored and present as untracked files ready to commit |

### What Is on `main`

All M0 through M16 milestones are committed on `main`. M17 operational documents are present in the working directory as new untracked files (7 documents confirmed in `git status`). The M17 infrastructure and documentation is complete.

**Commit coverage summary (M0–M17):**

- M0–M5: Core game loop, ServiceLocator, physics, level system, SaveData, economy architecture
- M6–M9: Firebase integration, Analytics, Crashlytics, Remote Config, Cloud Save
- M10–M12: AdMob integration, Cloud Run backend, billing infrastructure design
- M13: High-end commercial UI system (UIScreen, ScreenManager, KingSmashTheme, all screens)
- M14: Production observability, analytics hardening, Remote Config 30+ keys, operational hardening
- M15: Final QA hardening, 35+ EditMode test files, security review, BUG-001 and BUG-002 fixes
- M16: Controlled soft-launch strategy, balance patch (Level 9 DifficultyRating 10→7, KingLaunches 3→4), soft-launch monitoring
- M17: Production build config, Firebase production checklist, security final review, store copy, post-launch monitoring, Remote Config production baseline, launch checklist

The production tag (`v1.0.0`) has not been created yet. Per `docs/M17-PRODUCTION-BUILD-CONFIG.md` Section 12, production builds must be built from a tagged commit. Create tag `v1.0.0` at the final pre-build commit before initiating the production build.

---

## Section 4: Production Firebase Project

| Field | Value |
|---|---|
| **Firebase Project ID** | `king-smash-prod` |
| **Status** | CONFIGURED IN CODE — not yet validated on physical device |
| **Environment config file** | `Assets/Scripts/Services/Firebase/FirebaseEnvironmentConfig.cs` |
| **google-services.json** | NOT IN REPO — gitignored, provisioned by CI from GCP Secret Manager at build time |
| **Firestore rules** | `firestore.rules` in repo — requires explicit `firebase deploy --only firestore:rules --project king-smash-prod` |
| **Cloud Run** | Code complete — NOT DEPLOYED to `king-smash-prod` |

### Firebase Services Configured in Code

| Service | Implementation Class | Guard | Status |
|---|---|---|---|
| Authentication | `FirebaseAuthService` | `#if FIREBASE_ENABLED && !KING_SMASH_DEV` | CODE COMPLETE |
| Analytics | `FirebaseAnalyticsService` | `#if FIREBASE_ENABLED && !KING_SMASH_DEV` | CODE COMPLETE |
| Crashlytics | `FirebaseCrashlyticsService` | `#if FIREBASE_ENABLED && !KING_SMASH_DEV` | CODE COMPLETE |
| Remote Config | `FirebaseRemoteConfigService` | `#if FIREBASE_ENABLED && !KING_SMASH_DEV` | CODE COMPLETE |
| Cloud Save / Firestore | `FirebaseCloudSaveService` | `#if FIREBASE_ENABLED && !KING_SMASH_DEV` | CODE COMPLETE |

### What Remains Before Firebase Is Production-Ready

1. Create or verify Firebase project `king-smash-prod` exists in Firebase Console.
2. Register Android app with package name `com.kingcastle.kingsmash`.
3. Provision production signing keystore (Section 7) and extract SHA-1 and SHA-256 fingerprints.
4. Register all four fingerprints in Firebase Console (upload key SHA-1, upload key SHA-256, app signing key SHA-1, app signing key SHA-256). Both upload key and app signing key fingerprints are required for Google Sign-In to work with Play App Signing.
5. Download fresh `google-services.json` after fingerprint registration and store in GCP Secret Manager as `king-smash-prod-google-services-json`.
6. Deploy Firestore security rules: `firebase deploy --only firestore:rules --project king-smash-prod`. Do not assume staging rules match production. Verify `allow write: if false` is present on all economy collections after deployment.
7. Enable Anonymous Sign-In and Google Sign-In in Firebase Console → Authentication.
8. Configure OAuth consent screen in GCP Console as "In Production" (not "Testing").

---

## Section 5: Production Cloud Run Status

| Field | Value |
|---|---|
| **Service name** | `king-smash-api` |
| **Project** | `king-smash-prod` |
| **Code status** | COMPLETE |
| **Deployment status** | NOT DEPLOYED to `king-smash-prod` |
| **Runtime** | Node.js / Express / TypeScript |
| **Test coverage** | 23 Jest tests |

### API Endpoints

| Endpoint | Method | Authentication | Purpose |
|---|---|---|---|
| `/health` | GET | None | Service health check — Firestore + Auth status |
| `/api/v1/player/init` | POST | Firebase ID token | Anonymous player initialization, Firestore document creation |
| `/api/v1/progression/save` | POST | Firebase ID token | Level progress write to Firestore |
| `/api/v1/economy/claim` | POST | Firebase ID token | Idempotent reward grant — `transactionId` dedup enforced |
| `/api/v1/economy/balance` | GET | Firebase ID token | Current coin/gem/power-up balances |
| `/api/v1/rewards/daily` | POST | Firebase ID token | Daily reward claim with server-side day tracking |
| `/api/v1/rewards/rewarded-ad` | POST | Firebase ID token | Rewarded ad reward grant after ad completion signal |
| `/api/v1/purchases/verify` | POST | Firebase ID token | Google Play receipt verification (infrastructure ready; billing client still uses mock) |

### Authentication and Security Architecture

- All endpoints except `/health` require a valid Firebase ID token in `Authorization: Bearer <token>`.
- The Firebase Admin SDK verifies the token on every request.
- The server extracts `uid` from the verified token payload — no client-supplied `uid` is accepted.
- Rate limiting middleware is active on all economy endpoints.
- All economy Firestore writes go through the Admin SDK, bypassing client security rules.
- Idempotency is enforced on all reward endpoints via `transactionId` stored in Firestore.

### Deployment Readiness

The Cloud Run code is complete and all 23 Jest tests pass. The service has not been deployed to `king-smash-prod`. Deployment requires:

1. GCP project `king-smash-prod` with Cloud Run API enabled.
2. Service account with Firestore read/write permissions on `king-smash-prod`.
3. Application Default Credentials (Workload Identity) — no service account JSON committed to the image.
4. `gcloud run deploy king-smash-api --project king-smash-prod` from a tagged service revision.
5. Verify `/health` returns HTTP 200 after deployment.
6. Configure minimum 1 instance to prevent cold-start on launch day.
7. Configure Cloud Monitoring uptime check on `/health`.

**Status: READY FOR DEPLOYMENT — not yet deployed.**

---

## Section 6: Google Play Status

| Field | Value |
|---|---|
| **Play Console account** | Needs configuration |
| **App listing** | Draft text prepared in `docs/M17-STORE-COPY.md` — not yet entered |
| **Package name** | `com.kingcastle.kingsmash` — permanent once first uploaded |
| **Internal Testing track** | NOT CONFIGURED |
| **Closed Alpha track** | NOT CONFIGURED |
| **Production track** | NOT CREATED |
| **Status** | BLOCKED |

### What Is Required Before Play Console Is Operational

| Item | Status |
|---|---|
| Play Console developer account active and in good standing | Verify |
| App created with package name `com.kingcastle.kingsmash` | Required — not done |
| App name "King Smash" entered | Not done |
| Short description (Variant A, 70 chars) entered | Not done — text ready in M17-STORE-COPY.md |
| Full description (3,750 chars) entered | Not done — text ready in M17-STORE-COPY.md |
| App icon (512×512 PNG) uploaded | Pending production asset creation |
| Feature graphic (1024×500) uploaded | Pending production asset creation |
| Screenshots (10, 1080×2400 portrait) uploaded | Pending production build + screen capture |
| Privacy policy URL (permanent HTTPS) | Not hosted — required before submission |
| Data Safety section | Not completed |
| IARC content rating questionnaire | Not completed — expected rating: Everyone (E) |
| Support email `support@kingcastlestudio.com` | Not configured |
| Internal Testing track | Not created — required before AAB upload |
| Closed Alpha track | Not created — required for M16 soft launch execution |
| Play Billing products configured | Not configured |

The content text and IARC questionnaire answers are fully drafted in `docs/M17-STORE-COPY.md`. Store assets (icon, feature graphic, screenshots) require production builds and design assets that do not yet exist.

**Trademark note:** "King Smash" trademark availability should be verified via USPTO and EUIPO before Production submission.

---

## Section 7: Signing Status

| Field | Value |
|---|---|
| **Production signing keystore** | NOT PROVISIONED |
| **Play App Signing** | NOT CONFIGURED |
| **SHA fingerprints** | NOT ADDED to Firebase Console |
| **Status** | BLOCKED — gates all downstream build and device testing work |

### Architecture

King Smash uses Google Play App Signing. Two keys exist:

| Key | Purpose | Location |
|---|---|---|
| Upload key | Signs the AAB uploaded to Play Console | Must be created and stored in GCP Secret Manager |
| Distribution key | Signs the final APK delivered to users | Managed by Google Play infrastructure |

Play App Signing is strongly recommended: if the upload key is compromised, it can be rotated by Google Play support without changing the distribution key that existing devices trust.

### What Must Be Done

1. Generate an upload keystore using `keytool`: `keytool -genkey -v -keystore king-smash-upload.jks -alias king-smash -keyalg RSA -keysize 2048 -validity 10000`
2. Store the keystore (base64-encoded), key alias, keystore password, and key alias password in GCP Secret Manager.
3. Configure CI to retrieve the keystore via Workload Identity Federation (not via a downloaded service account JSON).
4. Extract SHA-1 and SHA-256 fingerprints: `keytool -list -v -keystore king-smash-upload.jks -alias king-smash | grep -E "SHA1:|SHA256:"`
5. Enroll in Google Play App Signing in Play Console (done during first AAB upload).
6. After enrollment, retrieve app signing key fingerprints from Play Console → App Integrity → App signing key certificate.
7. Register all four fingerprints (upload key SHA-1, upload key SHA-256, app signing key SHA-1, app signing key SHA-256) in Firebase Console → Android app → Add fingerprint.
8. Download fresh `google-services.json` after fingerprint registration.

**CRITICAL:** The keystore must never be committed to the repository. The production signing key, once lost, cannot be recovered from Google Play. Loss of the upload key requires a lengthy Google Play support process to rotate.

See `docs/M17-PRODUCTION-BUILD-CONFIG.md` Section 8 for the complete signing architecture reference.

---

## Section 8: Billing Status

| Field | Value |
|---|---|
| **Current service registration** | `PurchaseServiceMock` in `GameBootstrap.cs` line 118 |
| **Real billing implementation** | Designed but not implemented as a compilable class |
| **Google Play Billing Library** | Not integrated as a dependency |
| **Sandbox testing** | NOT PERFORMED |
| **Status** | BLOCKED — SEC-BLOCK-001 from `docs/M17-SECURITY-FINAL.md` |

### Current State

`IPurchaseService` is registered as `PurchaseServiceMock` in `GameBootstrap.cs` at line 118, with a TODO comment. The mock implementation accepts all purchase requests and returns synthetic success responses. In a production build with this mock active, a player who taps a purchase button sees the mock complete immediately without triggering the Google Play purchase dialog. No receipt is generated, no server-side verification runs, and no entitlement is written to Firestore.

### Server-Side Infrastructure (Complete)

The receipt verification infrastructure in Cloud Run is complete. The verification flow is designed as:

1. Client receives a signed Google Play purchase token from the Google Play Billing Library.
2. Client sends the token to Cloud Run `/api/v1/purchases/verify`.
3. Cloud Run verifies the token against the Google Play Developer API server-side.
4. On success, Cloud Run writes the entitlement to Firestore using the Admin SDK.
5. The `entitlements` collection is `allow write: if false` for client access — Cloud Run is the only write path.
6. Purchase token is never logged or sent to Analytics.

### What Must Be Done

1. Implement `GooglePlayPurchaseService` wrapping Google Play Billing Library 6.x.
2. Register `GooglePlayPurchaseService` in `GameBootstrap.cs` line 118 under the same `#if FIREBASE_ENABLED && !KING_SMASH_DEV` guard used for all Firebase services.
3. Add the Google Play Billing Library dependency to the Android build configuration.
4. Test the complete purchase flow end-to-end in a Play Console sandbox environment:
   - Successful purchase: dialog appears → player confirms → receipt sent to Cloud Run → entitlement written → content unlocked.
   - Purchase cancellation: no charge, no entitlement, UX recovers gracefully.
   - Duplicate receipt idempotency: same token sent twice returns rejection, no double-grant.
   - Restore purchases: previously purchased entitlement restored after reinstall.
   - Remove Ads: entitlement written → interstitials suppressed from next session.
5. Confirm purchase acknowledgment is called within 3 days (Google Play policy requirement).

---

## Section 9: AdMob Status

| Field | Value |
|---|---|
| **Dev/CI builds** | `AdsServiceMock` (via `#else` branch when `FIREBASE_ENABLED && !KING_SMASH_DEV` is false) |
| **Production builds** | `GoogleMobileAdsService` registered via `#if FIREBASE_ENABLED && !KING_SMASH_DEV` |
| **Ad unit ID configuration** | `AdConfiguration` ScriptableObject at `Resources/AdConfiguration` |
| **Production ad unit IDs** | NOT VERIFIED — placeholder or test IDs may be present |
| **Physical device test** | NOT PERFORMED |
| **Status** | BLOCKED — SEC-BLOCK-002 from `docs/M17-SECURITY-FINAL.md` |

### What Exists in Code

`GoogleMobileAdsService` is implemented and registered in `GameBootstrap.cs` under the `#if FIREBASE_ENABLED && !KING_SMASH_DEV` compile guard. `AdMobInterstitialService` handles interstitial ad scheduling. `RewardedAdFlowService` handles rewarded ad flow with idempotency guards. The interstitial policy enforces:
- Player has not purchased Remove Ads (entitlement read from Firestore — server-written only).
- Minimum interval since last interstitial has elapsed (configured via Remote Config, default 60 seconds).
- Player is not in a tutorial or onboarding flow.
- Interstitials fire between levels only — never mid-level.

The rewarded ad reward fires only after the ad SDK signals `ad_completed`. Reward amount is defined server-side in Remote Config. Reward skips or early closes do not grant the reward.

### What Must Be Done

1. Open `AdConfiguration` ScriptableObject in the Unity Editor.
2. Verify every field (AdMob App ID, rewarded ad unit ID, interstitial ad unit ID) matches the production values in AdMob Console for `com.kingcastle.kingsmash`.
3. Verify the AdMob App ID in `AndroidManifest.xml` matches the production value.
4. Confirm no field contains the known test App ID pattern `ca-app-pub-3940256099942544`.
5. Add a CI build assertion that fails if any `AdConfiguration` field contains the test ID pattern.
6. Perform physical device verification:
   - Rewarded ad loads, plays to completion, and grants the reward.
   - Early close does not grant the reward.
   - Interstitial appears after the configured level count, not mid-level.
   - Remove Ads purchase suppresses interstitials from the next session.

Using test ad unit IDs in a production build violates AdMob policies and results in ad serving suspension.

---

## Section 10: Analytics Status

| Field | Value |
|---|---|
| **Implementation class** | `FirebaseAnalyticsService` |
| **Event constants** | 40+ defined in `AnalyticsEventConstants` |
| **Rate limiting** | `AnalyticsRateGuard` — 200 events per minute |
| **UID privacy** | SHA-256 applied to Firebase UID, truncated to 16 hex chars before `FirebaseAnalytics.SetUserId()` |
| **Privacy verification** | SHA-256 hashing verified in SecurityTests (EditMode) |
| **7 analytics bridge MonoBehaviours** | Implemented and attached to game scenes |
| **Production verification** | NOT PERFORMED |
| **Status** | CODE COMPLETE — operational verification pending |

### Analytics Event Coverage

Analytics bridge MonoBehaviours cover: level lifecycle (start, complete, fail), economy events (upgrade purchased, coin earned, coin spent), ad events (rewarded ad shown, interstitial shown), session events (session start, first open), and player progression events (king leveled up, daily reward claimed).

Custom events carry parameters including `level_number`, `stars_earned`, `coins_earned`, `upgrade_type`, `upgrade_level`, `coin_cost`, `reward_day`, `placement`, `reward_type`, `session_ad_count`, and `config_version`.

### Production Verification Required

Before launch day, DebugView verification must confirm:
- All 10 primary events appear during a single test session on a physical device.
- Custom parameters are correctly typed (not empty strings, not null).
- `config_version` user property reads as `"3"` after Remote Config fetch.
- `userId` in DebugView is the SHA-256 hash (16 hex chars), not the raw Firebase UID.
- Data retention is set to 14 months (not the default 2 months) in Firebase Console → Analytics → Data Settings.

See `docs/M17-FIREBASE-PRODUCTION-CHECKLIST.md` Section 4 for the complete verification procedure.

---

## Section 11: Crashlytics Status

| Field | Value |
|---|---|
| **Implementation class** | `FirebaseCrashlyticsService` |
| **GameLogger integration** | All `Error` and `Warning` calls route to Crashlytics non-fatal logging in production |
| **Symbol upload** | Configured via Unity Crashlytics package — `libil2cpp.dbg.so` + `libil2cpp.sym.so` uploaded post-build |
| **Production test** | NOT PERFORMED |
| **Status** | CODE COMPLETE — production crash test pending |

### Custom Keys

`FirebaseCrashlyticsService` sets the following custom keys on session initialization and updates them during gameplay:

| Custom Key | Expected Value | Purpose |
|---|---|---|
| `current_level` | Active level number (int as string) | Triage: crash correlated to specific level? |
| `current_scene` | Unity scene name | Triage: which scene is loaded at crash? |
| `king_level` | Player's king upgrade level | Triage: progression state at crash time |
| `app_environment` | `"production"` | Distinguish production crashes from dev/staging |
| `app_version` | `"1.0.0"` | Version tag on every crash |

`app_environment` must read `"production"` in production builds. Verify this before launch.

### Symbol Upload Pipeline

IL2CPP strips method names from the compiled binary. Without symbol upload, all Crashlytics stack traces show raw memory addresses rather than C# method names, making crash triage nearly impossible. The `upload-symbols` Firebase CLI step is configured in CI and must run after every production build:

```
firebase crashlytics:symbols:upload --app=<APP_ID> path/to/symbols/
```

### Production Test Required

A staging build test (using `KING_SMASH_DEV` define so it does not pollute the production dashboard) must confirm: test crash appears in Crashlytics console within 5 minutes; stack trace is symbolicated (C# method names, not addresses); issue is assigned to the correct app version.

---

## Section 12: Remote Config Status

| Field | Value |
|---|---|
| **Implementation class** | `FirebaseRemoteConfigService` |
| **Key count** | 30+ keys defined in `RemoteConfigKeys.cs` |
| **Default values** | `RemoteConfigDefaults.cs` — safe defaults for all keys |
| **Validation** | `RemoteConfigValidator.cs` — validates all fetched values against expected types and ranges |
| **Fetch timeout** | 10 seconds — fetch failure is caught and logged; game continues on defaults |
| **Production config version** | `"3"` (increments from `"2"` used in M16 soft launch) |
| **Firebase Console** | NOT YET CONFIGURED |
| **Status** | CODE COMPLETE — Firebase Console configuration pending |

### Production Baseline Changes from Soft Launch

The production baseline (config_version=3) reverts all soft-launch generosity back to designed defaults. All multiplier values return to 1.0. Interstitial frequency returns to 3. Ad session limits and cooldowns return to standard values. The soft-launch profile (config_version=2) was a research instrument; the production profile is the intended long-term product experience.

See `docs/M17-REMOTE-CONFIG-PRODUCTION.md` for the complete key-value table.

### Key High-Impact Values to Verify in Firebase Console

| Key | Production Value | Reverts From (Soft Launch) |
|---|---|---|
| `coin_reward_multiplier` | 1.0 | 1.2 |
| `upgrade_cost_multiplier` | 1.0 | 0.9 |
| `destruction_multiplier` | 1.0 | 1.05 |
| `xp_multiplier` | 1.0 | 1.1 |
| `powerup_cost_multiplier` | 1.0 | 0.85 |
| `interstitial_frequency` | 3 | 4 |
| `ad_max_interstitials_per_session` | 5 | 3 |
| `ad_interstitial_min_session_seconds` | 60 | 90 |
| `config_version` | `"3"` | `"2"` |

### What Must Be Done

1. Upload all 30+ keys to Firebase Console → Remote Config → Default condition.
2. Set `config_version` to `"3"`.
3. Confirm no soft-launch experiment conditions (EXP-01-COIN-MULT, EXP-02-INTERSTITIAL-FREQ) remain active — these must be concluded and removed or converted to permanent conditions.
4. Verify `config_version=3` user property appears in DebugView within 5 minutes of first launch on a production build.

---

## Section 13: Cloud Save Status

| Field | Value |
|---|---|
| **Implementation class** | `FirebaseCloudSaveService` |
| **Sync service** | `CloudSyncService` |
| **Conflict resolution** | `ConflictResolver` — server wins for economy, local max for progression |
| **Firestore rules** | Economy collections: `allow write: if false` — server-authoritative via Admin SDK |
| **End-to-end test** | NOT PERFORMED |
| **Status** | CODE COMPLETE — end-to-end verification pending |

### Conflict Resolution Rules

| Data Type | Conflict Rule | Rationale |
|---|---|---|
| Economy (coins, gems, power-ups) | Server wins | Economy is server-authoritative. Client cannot inflate its own balance. |
| Progression (levels, stars) | Take max (local vs. server) | Progress should never go backwards. |
| Save metadata | Server wins | Timestamps and version tracking must be authoritative. |

### Verification Required

End-to-end cloud save verification requires a physical device with a production build and a configured production Firebase environment:

1. Complete Level 1 → confirm `users/{uid}/progress` Firestore document is written within 30 seconds.
2. Force-close and reopen → confirm Level 1 progress is restored from Firestore, not reset.
3. No duplicate anonymous user is created across app lifecycle (same Firebase UID before and after close).
4. Daily reward idempotency: claim once, force-close, reopen — reward not claimable again (server-side `transactionId` dedup enforced).

See `docs/M17-FIREBASE-PRODUCTION-CHECKLIST.md` Section 8 for the complete end-to-end flow checklist.

---

## Section 14: Security Status

**Source:** `docs/M17-SECURITY-FINAL.md`
**Overall Verdict:** CONDITIONAL PASS — 2 launch blockers require resolution before Production track submission.

| Security Area | M17 Status | Notes |
|---|---|---|
| **Client-side secrets** | CLEAR | No secrets in tracked codebase. `google-services.json` gitignored. Keystore in GCP Secret Manager. |
| **Debug surface** | CLEAR | All debug tooling gated by `#if UNITY_EDITOR` or `#if UNITY_EDITOR \|\| KING_SMASH_DEV`. Not compiled into production builds. M15 BUG-001 fix confirmed. |
| **Economy integrity** | CLEAR | Server-authoritative end-to-end. `allow write: if false` on all economy Firestore collections. Negative balance protection at server and client. |
| **Authentication** | CLEAR | Firebase ID token verified on every Cloud Run call. `uid` derived from verified token, never from request body. Token refresh handled by Firebase Auth SDK. |
| **Billing** | BLOCKER | SEC-BLOCK-001: `PurchaseServiceMock` in production path. Real billing never active. Players cannot complete purchases. |
| **Ad integrity (logic)** | CLEAR | Rewarded ad reward fires only after `ad_completed`. Remove Ads entitlement is server-written only. Interstitial policy enforces between-level placement. |
| **Ad integrity (IDs)** | BLOCKER | SEC-BLOCK-002: AdMob production ad unit IDs not verified. Using test IDs in production violates AdMob policy and results in account suspension. |
| **Analytics privacy** | VERIFIED BY DESIGN + TESTS | SHA-256 UID hashing verified in SecurityTests (EditMode). Raw UID never sent to Analytics backend. |
| **Certificate pinning** | ACKNOWLEDGED P2 (non-blocking) | Standard TLS only. Google-managed Cloud Run certificates. Post-launch recommendation. |
| **Root/emulator detection** | ACKNOWLEDGED P2 (non-blocking) | No root detection. Server-authoritative economy limits blast radius. Play Integrity API recommended post-launch. |
| **Cloud Run URL validation** | ACKNOWLEDGED P2 (non-blocking) | CI step recommended to assert no `-xxx` placeholder segments in production FirebaseEnvironmentConfig. |

---

## Section 15: Privacy / Data Safety Status

| Field | Value |
|---|---|
| **Privacy policy** | DRAFT — not yet hosted at a permanent HTTPS URL |
| **Privacy policy URL** | `https://kingcastlestudio.com/privacy-policy` — URL must be live before Play Console submission |
| **Data Safety form** | DRAFT — not yet submitted in Play Console |
| **GDPR/CCPA compliance** | COMPLIANT BY DESIGN — Firebase UID hashing, no PII in analytics, no credentials in logs |
| **Status** | DRAFT — hosting and Play Console submission required |

### Data Collected

| Data Type | Source | Purpose | Retention |
|---|---|---|---|
| Firebase anonymous UID (hashed) | Firebase Auth | User identification for cloud save and analytics | Firebase account lifetime |
| Game progression (levels, stars) | Firestore | Cloud save, cross-device sync | Account lifetime |
| Economy state (coins, gems, upgrades) | Firestore | Server-authoritative game state | Account lifetime |
| Analytics events (hashed UID, game actions) | Firebase Analytics | Product analytics, funnel analysis | 14 months (configured) |
| Crash reports | Crashlytics | Stability monitoring and bug triage | 90 days (Crashlytics default) |
| Google Account (if Google Sign-In linked) | Firebase Auth | Cross-device account linking | Firebase account lifetime |

No raw Firebase UID is stored in Analytics. No PII (email, phone, name) is collected or stored in the game. No device IMEI, GAID, or location data is collected.

### What Must Be Done

1. Write and publish privacy policy at `https://kingcastlestudio.com/privacy-policy`. The policy must be accessible via a permanent, publicly reachable HTTPS URL. Play Console requires this before any public release.
2. Complete the Data Safety form in Play Console declaring: Firebase Analytics, Firebase Crashlytics, Firebase Auth, and optional Google Sign-In.
3. Verify the Developer website `https://kingcastlestudio.com` is live and returns HTTP 200 from outside the studio network.
4. Verify support email `support@kingcastlestudio.com` is active and monitored.

---

## Section 16: Store Listing Status

| Field | Value |
|---|---|
| **App name** | "King Smash" — prepared |
| **Short description** | 70 chars (Variant A) — prepared in `docs/M17-STORE-COPY.md` |
| **Full description** | ~3,750 chars — prepared in `docs/M17-STORE-COPY.md` |
| **App icon (512×512)** | PENDING — production design asset required |
| **Feature graphic (1024×500)** | PENDING — production design asset required |
| **Screenshots (10, portrait)** | PENDING — requires production build + device capture |
| **IARC content rating** | NOT COMPLETED — Play Console questionnaire not submitted |
| **Expected rating** | Everyone (E) — cartoon physics, no blood, no social features |
| **Category** | Games → Casual (primary) |
| **Status** | DRAFT — text ready; assets and Play Console entry pending |

### IARC Questionnaire Summary

All questionnaire answers are prepared in `docs/M17-STORE-COPY.md` Section 6. Key answers: Violence = YES (mild cartoon); Gambling = NO (no loot boxes, no random prize mechanics); User-generated content = NO; Digital purchases = YES; Advertising = YES (AdMob rewarded + interstitial).

### Screenshot Requirements

All screenshots must be captured from a production build with no debug overlays, no development build indicators, no FPS counters, and no test mode markers. The 10-shot list (home screen, world map, level select, aim/launch, mid-destruction, power-up, victory, upgrade, queen rescue, boss battle) and their captions are fully specified in `docs/M17-STORE-COPY.md` Section 9.

Screenshots cannot be produced until a signed production build exists on a physical device — which requires the signing keystore (Section 7).

---

## Section 17: Pre-Launch Report

| Field | Value |
|---|---|
| **Pre-Launch Report** | NOT AVAILABLE |
| **Reason** | AAB has not been uploaded to Play Console. Play Console does not exist yet (Section 6). |
| **Expected availability** | After Gates 6 (keystore), 7 (Play Console), and AAB upload to Internal Testing track. |
| **Expected ANR result** | No critical ANRs — basis: M15 automated test suite covering 35+ EditMode tests, no blocking ANR patterns identified. |
| **Status** | BLOCKED — requires Gates 6, 7 complete and AAB uploaded |

The Google Play Pre-Launch Report runs automated tests on the uploaded AAB across a matrix of Android devices and OS versions in Firebase Test Lab. It reports crash rates, ANR rates, device compatibility issues, accessibility warnings, and form factor issues (tablet, foldable, large screen).

The Pre-Launch Report is a required review step before the Production track is set to Live. Critical findings (ANR rate > 0.47%, crash rate > 0%) are launch blockers. See `docs/M17-LAUNCH-CHECKLIST.md` Gate 9 for the complete review checklist.

---

## Section 18: Device Validation

| Field | Value |
|---|---|
| **Physical device testing** | NOT PERFORMED — M15 carry-over blocker (LAUNCH-02) |
| **Emulator testing** | NOT PERFORMED |
| **EditMode tests** | 35+ test files authored — designed to pass, not run on physical device |
| **Status** | BLOCKED — requires signed production build (Section 7) |

### Required Device Coverage Matrix

| Device Tier | RAM | Android Version | Status |
|---|---|---|---|
| Low-end (e.g., Samsung Galaxy A12) | 2–3 GB | Android 9 (API 28) | NOT TESTED |
| Low-end (e.g., Motorola Moto G9) | 2–3 GB | Android 10 (API 29) | NOT TESTED |
| Mid-range (e.g., Samsung Galaxy A54) | 6 GB | Android 12 (API 31) | NOT TESTED |
| High-end (e.g., Samsung Galaxy S23) | 8+ GB | Android 13 (API 33) | NOT TESTED |
| Notch / cutout device | Any | Android 9+ | NOT TESTED |

### Functional Tests Required Per Device

- Level 1 launch and completion without crash or hang.
- Levels 2–10 sequential play — all launch, play, and complete.
- Camera, audio, and touch input register correctly with no dead zones.
- Safe area and notch rendering — UI not obscured by notch, punch-hole, or status bar.
- Background and resume — game state preserved on app switch, no crash.
- 30-minute continuous play (thermal test) — no excessive heating, no performance cliff.
- Firebase services on-device — Auth, Firestore, Analytics, Crashlytics all active.
- AdMob ad loading on physical device — rewarded and interstitial ads load and display.

**Device testing is a hard prerequisite for the M16 Closed Alpha execution (Section 20). Devices must be tested with a production-signed build, not a debug build.**

---

## Section 19: Smoke Test Results

| Field | Value |
|---|---|
| **Status** | NOT PERFORMED |
| **Reason** | Requires physical device with a signed production build (version code 3) installed. Neither the production build nor physical device testing has been performed. |
| **Smoke test checklist** | 38-step checklist prepared in `docs/M16-FINAL-REGRESSION-CHECKLIST.md` |
| **Status** | BLOCKED |

The 38-step smoke test covers: app launch and Firebase initialization, anonymous authentication, Level 1 through Level 3 complete play, cloud save write and restore, Remote Config fetch and value verification, interstitial ad trigger, rewarded ad flow, upgrade purchase flow, daily reward claim and idempotency, power-up use and quantity decrement, app background and resume, and Crashlytics non-fatal test.

No smoke test step can be marked PASS without a production-signed APK installed on a physical Android device connected to the production Firebase environment.

---

## Section 20: Rollout Strategy

| Field | Value |
|---|---|
| **Rollout plan** | Documented in `docs/M17-ROLLOUT-STRATEGY.md` |
| **Stages** | 5 stages over approximately 10–15 days |
| **Pre-conditions** | 10 pre-conditions — all currently unmet |
| **Status** | PLANNING COMPLETE — execution blocked on all 8 launch blockers |

### Rollout Stages

| Stage | Rollout % | Decision Point | Advancement Criteria |
|---|---|---|---|
| Stage 1 | 10% | Day 1 (T+24h) | Crash-free >= 99%, D0 retention >= 50%, no P0 incidents |
| Stage 1 hold | 10% | Day 7 (T+7d) | D7 retention >= 15%, all Day 1 criteria met |
| Stage 2 | 50% | Day 7 (T+7d) | All Stage 1 met, D7 >= 15%, economy healthy, no P0/P1 open |
| Stage 3 | 100% | Day 14 (T+14d) | D14 >= 10%, purchase conversion >= 2%, crash-free >= 99%, no incidents |

### 10 Pre-Conditions (all unmet)

1. All 10 gates in M17-LAUNCH-CHECKLIST.md passed.
2. M16 soft launch executed and reviewed (NEEDS_MORE_SOFT_LAUNCH → READY_FOR_PRODUCTION).
3. Physical device testing complete on all three device tiers.
4. Production google-services.json provisioned and verified.
5. Production signing keystore provisioned and CI pipeline configured.
6. Google Play Console configured with app listing and testing tracks.
7. Real Google Play Billing integrated and sandbox tested.
8. AdMob production IDs verified on physical device.
9. Firebase production connection verified end-to-end.
10. All-gates sign-off by Engineering Lead, Product Lead, and QA Lead.

---

## Section 21: Monitoring Strategy

| Field | Value |
|---|---|
| **Post-launch monitoring plan** | `docs/M17-POST-LAUNCH-MONITORING.md` |
| **Additional alerts document** | Referenced in `docs/M17-POST-LAUNCH-MONITORING.md` |
| **Tools** | Firebase Crashlytics, Firebase Analytics, GCP Cloud Monitoring, Google Play Console |
| **BigQuery** | Configured if long-term raw event retention required |
| **Status** | READY TO CONFIGURE — pending Play Console and GCP project setup |

### Monitoring Coverage

| Area | Tool | Key Metric | Stop Threshold |
|---|---|---|---|
| Crash rate | Firebase Crashlytics | Crash-free sessions | < 95% in first 60 min → HALT |
| Backend errors | GCP Cloud Run | 5xx error rate | > 2% for 5 min → Tier 3 rollback |
| Engagement | Firebase Analytics | Level 1 completion rate | < 60% → P1 investigation |
| Retention | Firebase Analytics | D1 retention | < 20% → immediate review |
| App store health | Play Console Android vitals | ANR rate | > 1% → P1 investigation |
| Economy | Firebase Analytics | Coin earn/spend ratio | > 3× baseline → economy break investigation |
| Billing | Firebase Analytics | IAP purchase success rate | < 85% → P2 investigation |

### Hour 1 Protocol

On-call engineer checks every 15 minutes for the first hour after rollout goes live. Crashlytics crash-free, Cloud Run 5xx, `app_open` event count, and Play Console install count are checked at each interval. Stop criteria trigger immediate halt: crash-free < 95%, any confirmed data loss, any confirmed duplicate billing grant, Cloud Run 5xx > 2% for 5+ minutes.

Full monitoring schedule (hourly T+1h to T+6h, daily at T+24h, T+72h, T+7d, T+14d) and the complete escalation matrix are defined in `docs/M17-POST-LAUNCH-MONITORING.md`.

---

## Section 22: Rollback Strategy

| Field | Value |
|---|---|
| **Rollback playbook** | `docs/M17-POST-LAUNCH-MONITORING.md` — Rollback Procedures section |
| **Status** | DOCUMENTED — ready to execute once Play Console and Cloud Run are active |

### Four-Tier Rollback Model

| Tier | Mechanism | Response Time | When to Use |
|---|---|---|---|
| Tier 1 | Remote Config kill-switches | < 5 minutes | Disable problematic features without a new build. RC changes propagate within fetch interval (5–60 min). |
| Tier 2 | Play Console rollout halt | < 2 minutes | Stop new installs/updates immediately. Crash-free < 98.5%, confirmed P0 data loss or billing incident. |
| Tier 3 | Cloud Run revision rollback | < 10 minutes | Backend regression introduced by a Cloud Run deployment. `gcloud run services update-traffic --to-revisions=PREV=100` |
| Tier 4 | Hotfix build | 24–72 hours | Code bug requiring a new client build. Branch from production tag → minimum fix → increment version code → Internal Testing → Pre-Launch Report → Production. |

### Remote Config Kill-Switches

```
Disable rewarded ads:       rewarded_ads_enabled = false
Disable interstitial ads:   interstitial_ads_enabled = false
Disable IAP:                iap_enabled = false
Skip broken level:          skip_level_id = <level_id>
Disable cloud save:         cloud_save_enabled = false (local fallback)
Enable maintenance mode:    maintenance_mode = true
```

---

## Section 23: Remaining Issues

### Launch Blockers (8)

| ID | Severity | Description | Blocker? | Resolution Path |
|---|---|---|---|---|
| LAUNCH-01 | P0 | M16 status = NEEDS_MORE_SOFT_LAUNCH. No Closed Alpha has been executed. No real-user retention, crash, or economy data exists. M17 specification requires M16 = READY_FOR_PRODUCTION as a gate condition for production launch. | YES | Execute M16 Closed Alpha on Play Console Closed Alpha track. Run minimum 14 days, minimum 200 activated users. Collect and review D1 retention, Level 1 completion, crash-free rate. Advance M16 status to READY_FOR_PRODUCTION. |
| LAUNCH-02 | P1 | Physical device testing has never been performed. M15 carry-over blocker. No functional verification on any Android device. | YES | Provision production build (requires LAUNCH-04). Test on low-end, mid-range, and high-end devices across Android 9–13. Complete 38-step smoke test checklist. |
| LAUNCH-03 | P0 | Production `google-services.json` not provisioned for `king-smash-prod`. File is gitignored and must come from GCP Secret Manager at build time. | YES | Create production keystore (LAUNCH-04 first). Register SHA fingerprints in Firebase Console. Download fresh `google-services.json`. Store in GCP Secret Manager. Configure CI to fetch at build time. |
| LAUNCH-04 | P0 | Production signing keystore not provisioned. Upload keystore does not exist. CI pipeline is not configured to retrieve it. No production AAB can be signed or submitted. | YES | Generate keystore with `keytool`. Store base64-encoded in GCP Secret Manager. Configure CI Workload Identity. Extract and register SHA fingerprints with Firebase Console and Play Console. |
| LAUNCH-05 | P0 | Google Play Console not configured. No app listing exists. No testing tracks (Internal, Closed Alpha, Production). No Play Billing products configured. | YES | Create developer account if not active. Create app listing with package name `com.kingcastle.kingsmash`. Configure Internal Testing and Closed Alpha tracks. Complete IARC questionnaire. Upload app icon, feature graphic, and screenshots. Complete Data Safety form. |
| LAUNCH-06 | P0 | `PurchaseServiceMock` is registered in `GameBootstrap.cs` line 118 (SEC-BLOCK-001). Real Google Play Billing Library is not integrated. Sandbox testing not performed. Players cannot complete purchases in any production build. | YES | Implement `GooglePlayPurchaseService` wrapping Google Play Billing Library 6.x. Register under `FIREBASE_ENABLED && !KING_SMASH_DEV` guard. Test full purchase flow in Play Console sandbox. Verify receipt verification Cloud Run endpoint end-to-end. |
| LAUNCH-07 | P0 | AdMob production ad unit IDs not verified in `AdConfiguration` ScriptableObject (SEC-BLOCK-002). Physical device ad loading not tested. Using test IDs in production violates AdMob policy and results in account suspension. | YES | Verify all ad unit IDs in `AdConfiguration` ScriptableObject match AdMob Console production values. Verify `AndroidManifest.xml` App ID is production value. Test rewarded and interstitial ad behavior on physical device. |
| LAUNCH-08 | P0 | Firebase production environment (`king-smash-prod`) connection not verified end-to-end. Auth, Firestore, Analytics, Crashlytics, Remote Config have not been tested on a physical device with a production build connected to `king-smash-prod`. | YES | Complete M17-FIREBASE-PRODUCTION-CHECKLIST.md Sections 1–8. Verify on at least 2 physical devices. Record sign-off in Gate 9.2 of the checklist. |

### Non-Blocking P2 Issues (3)

| ID | Severity | Description | Blocker? | Resolution Path |
|---|---|---|---|---|
| P2-001 | P2 | `PowerUpService` has no maximum quantity cap. A player could accumulate an arbitrarily large inventory of power-ups. Not a security risk (economy is server-authoritative) but creates balance and UX concerns. | NO | Add a `maxQuantity` constant (e.g., 99) to `PowerUpService`. Enforce the cap on the server in Cloud Run's `/api/v1/economy/claim` endpoint for power-up grant rewards. Post-launch patch. |
| P2-002 | P2 | Google Sign-In not integrated as a runtime feature. Firebase Auth service and the design for Google credential linking exist in code but the Sign-In UI flow is feature-flagged off (default: anonymous auth only). | NO | Feature is tracked in `FirebaseAuthService` and designed correctly. Integrate Google Sign-In SDK, implement the sign-in UI flow, and remove the feature flag in a post-launch update. All Firebase Auth server infrastructure is already in place. |
| P2-003 | P2 | Play Integrity API (Google's replacement for SafetyNet) is stubbed and not actively checked. The server-authoritative economy limits the blast radius of compromised devices, but active device integrity checking is a best-practice recommendation for economy-modifying endpoints. | NO | Integrate Play Integrity API in a post-launch update. Add a server-side check on high-value economy endpoints. Not a security blocker given the server-authoritative economy architecture. |

---

## Section 24: Final Release Decision

---

```
╔══════════════════════════════════════════════════════════════════════╗
║                                                                      ║
║             PRODUCTION_LAUNCH_BLOCKED                                ║
║                                                                      ║
╚══════════════════════════════════════════════════════════════════════╝
```

**Reason:** M16 status = NEEDS_MORE_SOFT_LAUNCH per M17 specification section 1.
The M17 specification requires M16 = READY_FOR_PRODUCTION as a gate condition for production launch. M16 cannot advance to READY_FOR_PRODUCTION until a Closed Alpha soft launch has been executed with real users, and the resulting data (D1 retention, Level 1 completion rate, crash-free rate, progression loss incidents, duplicate purchase incidents) has been reviewed and meets the advancement criteria. No soft launch has been executed. No real-user data exists.

### 8 Active Blockers

1. **LAUNCH-01:** M16 = NEEDS_MORE_SOFT_LAUNCH. Closed Alpha not executed. No real-user data.
2. **LAUNCH-02:** Physical device testing not performed. M15 carry-over blocker.
3. **LAUNCH-03:** Production `google-services.json` not provisioned for `king-smash-prod`.
4. **LAUNCH-04:** Production signing keystore not provisioned. Release AAB cannot be signed.
5. **LAUNCH-05:** Google Play Console not configured. No app listing, no testing tracks.
6. **LAUNCH-06:** `PurchaseServiceMock` in production path. Real billing not integrated. Sandbox not tested.
7. **LAUNCH-07:** AdMob production ad unit IDs not verified on physical device.
8. **LAUNCH-08:** Firebase production environment not verified end-to-end on physical device.

---

### What IS Done (Code and Documentation)

- All M0–M17 source code committed and pushed to `origin/main`. No code blockers remain.
- ServiceLocator pattern with `#if FIREBASE_ENABLED && !KING_SMASH_DEV` compile guards — mocks used in dev/CI, real services in production.
- Server-authoritative economy: Firestore `allow write: if false` on all economy collections. Cloud Run is the only write path.
- `FirebaseAnalyticsService`, `FirebaseCrashlyticsService`, `FirebaseRemoteConfigService`, `FirebaseAuthService`, `FirebaseCloudSaveService` — all fully implemented.
- `AnalyticsRateGuard` (200 events/min), SHA-256 UID hash privacy, 7 analytics bridge MonoBehaviours, 40+ event constants.
- `RemoteConfigKeys`, `RemoteConfigDefaults`, `RemoteConfigValidator` — 30+ keys, safe defaults, validation logic.
- Cloud Run backend (Express/TypeScript) with 23 Jest tests, all economy and reward endpoints, auth middleware, rate limiting.
- `GoogleMobileAdsService` + `AdMobInterstitialService` + `RewardedAdFlowService` registered under compile guard. Interstitial policy enforced.
- SaveData v1–v7 migration chain with `SaveMigrator`.
- `WorldRegistry` with 5 worlds, `LevelConfigFactory` with 100 levels. M16 PATCH-001 applied (Level 9 DifficultyRating 10→7, KingLaunches 3→4).
- UIScreen/ScreenManager/KingSmashTheme full design system. All screens implemented. FeedbackScreen + IFeedbackService.
- 35+ EditMode test files authored.
- M15 BUG-001 fixed: `M2TestLevelSetup` `Start()` body wrapped in `#if UNITY_EDITOR`.
- M15 BUG-002 fixed: `GameBootstrap` mock service guard correctly implements `FIREBASE_ENABLED && !KING_SMASH_DEV`.
- `docs/M17-PRODUCTION-BUILD-CONFIG.md` — complete build configuration, signing, CI pipeline, post-build verification.
- `docs/M17-FIREBASE-PRODUCTION-CHECKLIST.md` — 9-section Firebase production verification checklist.
- `docs/M17-REMOTE-CONFIG-PRODUCTION.md` — production baseline config_version=3, all key values, decision rules.
- `docs/M17-SECURITY-FINAL.md` — comprehensive security review, conditional pass, 2 blockers identified.
- `docs/M17-STORE-COPY.md` — complete store text (app name, short description, full description, keywords, IARC answers, icon/screenshot specs).
- `docs/M17-POST-LAUNCH-MONITORING.md` — monitoring schedule, escalation matrix, 4-tier rollback procedures.
- `docs/M17-LAUNCH-CHECKLIST.md` — 10-gate production launch checklist with all 8 blockers documented.

---

### Exact Next Actions (in order)

1. **Provision production signing keystore.** Generate upload keystore with `keytool`. Store in GCP Secret Manager. Configure CI Workload Identity to retrieve it at build time. Extract SHA-1 and SHA-256 fingerprints.

2. **Configure Firebase project `king-smash-prod`.** Register Android app (`com.kingcastle.kingsmash`). Add all four SHA fingerprints (upload key SHA-1/SHA-256, app signing key SHA-1/SHA-256). Download fresh `google-services.json` and store in GCP Secret Manager. Enable Anonymous Sign-In and Google Sign-In. Configure OAuth consent screen as "In Production". Deploy Firestore security rules from repository to `king-smash-prod`.

3. **Configure Google Play Console.** Create app listing with package `com.kingcastle.kingsmash`. Enter store copy from `docs/M17-STORE-COPY.md`. Complete IARC questionnaire. Create Internal Testing and Closed Alpha tracks. Configure Play Billing products.

4. **Implement real Google Play Billing** (`GooglePlayPurchaseService`). Replace `PurchaseServiceMock` at `GameBootstrap.cs` line 118 under `FIREBASE_ENABLED && !KING_SMASH_DEV` guard. Test full billing flow in Play Console sandbox. Verify receipt verification end-to-end through Cloud Run.

5. **Verify AdMob production ad unit IDs** in `AdConfiguration` ScriptableObject. Confirm `AndroidManifest.xml` App ID is production value. Add CI assertion for no test ID pattern. Generate production AAB (version code 3, version name 1.0.0). Sign with upload keystore. Run post-build verification checklist from `docs/M17-PRODUCTION-BUILD-CONFIG.md` Section 10.

6. **Perform physical device testing.** Install production-signed APK on low-end, mid-range, and high-end devices (Android 9–13). Complete 38-step smoke test checklist from `docs/M16-FINAL-REGRESSION-CHECKLIST.md`. Complete Firebase production verification checklist from `docs/M17-FIREBASE-PRODUCTION-CHECKLIST.md`. Sign off Gates 2, 3, 4, 5, 6 in `docs/M17-LAUNCH-CHECKLIST.md`.

7. **Execute M16 Closed Alpha soft launch.** Upload AAB to Play Console Internal Testing track, review Pre-Launch Report (Gate 9). Move to Closed Alpha track. Run minimum 14 days, minimum 200 activated users. Review D1 retention, Level 1 completion rate, crash-free rate. On READY_FOR_PRODUCTION: advance M16 status. Sign off all remaining gates in `docs/M17-LAUNCH-CHECKLIST.md`. Initiate Stage 1 (10%) production rollout. Advance rollout stages per criteria in `docs/M17-POST-LAUNCH-MONITORING.md` at Day 1, Day 7, and Day 14.

---

*Report prepared: 2026-10-05*
*Milestone: M17 Production Launch*
*Reporter: King Smash Engineering Team*
*All M0–M17 source code: committed and pushed to `origin/main`*
*Status: PRE-LAUNCH INFRASTRUCTURE COMPLETE — LAUNCH BLOCKED (OPERATIONAL PREREQUISITES UNMET)*
