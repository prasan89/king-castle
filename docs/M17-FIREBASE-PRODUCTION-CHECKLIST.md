# King Smash M17 — Firebase Production Verification Checklist

**Version:** 1.0.0 (version code 3)
**Firebase Project:** king-smash-prod
**Milestone:** M17 Production Launch
**Overall Status:** NOT YET PERFORMED — requires physical device and production deployment
**Prepared:** M17 Milestone
**References:** docs/M17-PRODUCTION-BUILD-CONFIG.md, docs/M15-SECURITY-REVIEW.md, docs/M16-FINAL-REPORT.md

---

## How to Use This Document

Each section represents a discrete verification area. Work through sections in order — earlier sections are prerequisites for later ones. A single FAIL in any checklist item is a launch blocker unless explicitly marked `[NON-BLOCKING]`.

**Verification requires:**
1. A physical Android device (not emulator for Crashlytics and AdMob validation)
2. A signed production AAB installed as APK via `bundletool extract-apks`
3. Active Firebase Console access with Editor or Owner role on `king-smash-prod`
4. Google Play Console access (for SHA fingerprint retrieval)
5. A developer account enrolled in the production Firebase project

Mark each item with one of:
- `[PASS]` — verified, meets criteria
- `[FAIL]` — does not meet criteria, launch blocked
- `[SKIP]` — explicitly deferred (document why and confirm it is non-blocking)

---

## Section 1: Firebase Console — Android App Registration

### 1.1 Android App Present

- [ ] Firebase Console → Project `king-smash-prod` → Project Settings → Your Apps → Android app is present
- [ ] App nickname matches: "King Smash Android"
- [ ] Package name matches exactly: `com.kingcastle.kingsmash`
  - **If package name does not match:** stop. The google-services.json in the build will not work. This is a build blocker.

### 1.2 SHA Certificate Fingerprints

Google Play App Signing means two sets of SHA fingerprints must be registered:

| Type | Source | Status |
|---|---|---|
| SHA-1 (upload key) | Play Console → App Integrity → Upload key certificate | [ ] |
| SHA-256 (upload key) | Play Console → App Integrity → Upload key certificate | [ ] |
| SHA-1 (app signing key) | Play Console → App Integrity → App signing key certificate | [ ] |
| SHA-256 (app signing key) | Play Console → App Integrity → App signing key certificate | [ ] |

Both the upload key AND the app signing key fingerprints must be registered in Firebase Console → Project Settings → Your Apps → Android app → Add fingerprint. Registering only the upload key will cause Firebase Authentication (Google Sign-In) to fail for users who install via the Play Store, because the installed APK is signed with Google's distribution key, not the upload key.

**Verification command for upload key:**
```bash
keytool -list -v -keystore king-smash-upload.jks -alias [KEY_ALIAS] \
    | grep -E "SHA1:|SHA256:"
```

- [ ] Upload key SHA-1 registered in Firebase Console
- [ ] Upload key SHA-256 registered in Firebase Console
- [ ] App signing key SHA-1 registered in Firebase Console
- [ ] App signing key SHA-256 registered in Firebase Console

### 1.3 google-services.json Currency

- [ ] Download `google-services.json` fresh from Firebase Console after adding all SHA fingerprints
- [ ] Verify the downloaded file is the one used in the production build (SHA fingerprints are embedded in `google-services.json`)
- [ ] The CI pipeline fetches this file from GCP Secret Manager — verify Secret Manager version matches the downloaded file

---

## Section 2: Firebase Authentication

### 2.1 Sign-In Methods Enabled

Navigate to Firebase Console → Authentication → Sign-in method.

| Method | Required State | Verified |
|---|---|---|
| Anonymous | **Enabled** | [ ] |
| Google | **Enabled** | [ ] |
| Email/Password | Disabled (not used) | [ ] |
| Phone | Disabled (not used) | [ ] |

- [ ] Anonymous sign-in is enabled — this is the first auth step on app launch
- [ ] Google Sign-In is enabled — required for cloud save account linking

### 2.2 Google Sign-In OAuth Configuration

- [ ] OAuth consent screen is configured in Google Cloud Console for project `king-smash-prod`
- [ ] OAuth consent screen status is **In Production** (not Testing — Testing limits OAuth users to 100 test accounts)
- [ ] App domain is set in OAuth consent screen (matches privacy policy URL)
- [ ] The Web client ID in `google-services.json` is present and valid (the Firebase Unity SDK uses this for Google Sign-In token exchange)

### 2.3 Authorized Domains

- [ ] Firebase Console → Authentication → Settings → Authorized domains contains the Cloud Run game-api domain
- [ ] No test or development domains remain in the authorized list for production

---

## Section 3: Cloud Firestore

### 3.1 Database Provisioned

- [ ] Firestore database exists in `king-smash-prod` project (not Realtime Database)
- [ ] Database is in **Native mode** (not Datastore mode — Native mode is required for real-time listeners)
- [ ] Database region is set and cannot be changed after creation — confirm region is appropriate for target market (recommend `us-central1` for global reach or `europe-west1` for EU-first launch)

### 3.2 Security Rules Deployed

Navigate to Firestore → Rules tab. The deployed rules must match `firestore.rules` in the repository.

- [ ] Rules are deployed and active (check "Last deployed" timestamp)
- [ ] Rules version in console matches rules version in `firestore.rules` in source control

**Critical rules to verify manually:**

Economy collections (coins, gems, upgrades) — must be server-authoritative:
```
// Economy collections must NOT allow client writes
match /users/{userId}/economy/{doc} {
  allow read: if isOwner(userId);
  allow write: if false;  // Server-only via Admin SDK / Cloud Run
}
```

- [ ] `allow write: if false` is present on all economy collections (coins, gems, upgrades, purchases)
- [ ] Economy writes only go through Cloud Run game-api (Admin SDK bypass)

Progression collections — client writes allowed only for owned data:
```
match /users/{userId}/progress/{doc} {
  allow read, write: if isOwner(userId);
}
```

- [ ] `isOwner(userId)` predicate correctly checks `request.auth.uid == userId`
- [ ] No collection allows `allow write: if true` (catch-all write access)
- [ ] Cloud Save collection (`/users/{userId}/savedata`) is owner-gated

### 3.3 Rules Test Coverage

- [ ] `firestore.rules` has corresponding test cases in `tests/firestore.rules.test.js` (or equivalent)
- [ ] All rule tests pass: `firebase emulators:exec "npm test --prefix functions"`

---

## Section 4: Firebase Analytics

### 4.1 Analytics Enabled

- [ ] Firebase Console → Analytics → Dashboard is accessible and not showing "Analytics not configured"
- [ ] `google-services.json` contains `analytics_service` section

### 4.2 DebugView Pre-Launch Verification

Before launching production, verify all custom events fire correctly using DebugView on a physical device with a production build installed.

**Enable DebugView:**
```bash
adb shell setprop debug.firebase.analytics.app com.kingcastle.kingsmash
```

**Events to verify in DebugView:**

| Event Name | Trigger | Custom Parameters Expected |
|---|---|---|
| `level_start` | Level begins | `level_number`, `config_version` |
| `level_complete` | Level ends, 1+ star | `level_number`, `stars_earned`, `coins_earned` |
| `level_fail` | Level ends, 0 stars | `level_number`, `fail_reason` |
| `upgrade_purchased` | Upgrade bought | `upgrade_type`, `upgrade_level`, `coin_cost` |
| `daily_reward_claimed` | Daily reward collected | `reward_day`, `coins_awarded` |
| `ad_rewarded_shown` | Rewarded ad completes | `placement`, `reward_type` |
| `ad_interstitial_shown` | Interstitial fires | `level_number`, `session_ad_count` |
| `king_leveled_up` | King XP threshold crossed | `new_king_level` |
| `session_start` | App foregrounds | (automatic) |
| `first_open` | First launch | (automatic) |

- [ ] All 10 events appear in DebugView during a single test session
- [ ] Custom parameters are present and correctly typed (not empty strings, not null)
- [ ] `config_version` user property appears in DebugView as `"3"` after Remote Config fetch

### 4.3 User ID Hashing

- [ ] `userId` sent to Analytics is SHA-256 hashed, not the raw Firebase UID
- [ ] Verified in code: `FirebaseAnalyticsService.SetUserId()` calls `HashUserId(uid)` before `FirebaseAnalytics.SetUserId()`
- [ ] Raw UID does not appear in any Analytics event parameters (grep source: `FirebaseAnalytics.SetUserId\|analytics.*userId` — confirm no raw UID)

### 4.4 Data Retention

- [ ] Firebase Console → Analytics → Data Settings → Data retention is set to the maximum available retention period (14 months recommended)
  - Default is 2 months; changing this requires explicit action in the console
- [ ] BigQuery export is configured if long-term raw event retention is needed

---

## Section 5: Firebase Crashlytics

### 5.1 Crashlytics Enabled

- [ ] Firebase Console → Crashlytics → Dashboard is accessible for `king-smash-prod`
- [ ] `FirebaseCrashlyticsService.Initialize()` is called in `GameBootstrap` before any game code runs
- [ ] `FirebaseCrashlyticsService` is registered in the ServiceLocator for the production build (confirmed via `GameBootstrap.cs` audit)

### 5.2 dSYM / IL2CPP Symbol Upload

IL2CPP strips method names and replaces them with addresses. Without symbol upload, Crashlytics stack traces are unreadable in production.

- [ ] `upload-symbols` script is configured in CI post-build step
- [ ] `libil2cpp.dbg.so` (ARM64) and `libil2cpp.sym.so` are uploaded after each production build
- [ ] The upload step uses the Firebase CLI:
  ```bash
  firebase crashlytics:symbols:upload \
    --app=1:XXXXXXXXXXXXX:android:XXXXXXXXXXXXXXXXXXXXXXXX \
    path/to/symbols/
  ```
- [ ] Verify in Crashlytics dashboard: after the test crash (Section 5.3), the stack trace shows C# method names, not raw addresses

### 5.3 Test Crash Verification

**This step requires a physical device with a debug or staging build — do NOT perform on the production AAB that will be submitted to Play Store, as it creates a crash event in your production dashboard before launch.**

Use a staging build to verify Crashlytics plumbing works. Only confirm the `[PASS]` here after a staging-build test passes.

- [ ] Test crash trigger fires (via `AnalyticsCrashTest` or `FirebaseCrashlytics.Instance.TestIt()` in staging build)
- [ ] Crash appears in Crashlytics console within 5 minutes
- [ ] Stack trace is symbolicated (C# method names visible, not address offsets)
- [ ] Issue is assigned to the correct app version

### 5.4 Custom Keys Configured

Verify `FirebaseCrashlyticsService.SetCustomKeys()` sets the following keys on every Crashlytics session initialization:

| Custom Key | Expected Value | Purpose |
|---|---|---|
| `current_level` | Active level number (int as string) | Triage: is crash correlated with a specific level? |
| `current_scene` | Unity scene name | Triage: which scene is loaded? |
| `king_level` | Player's king upgrade level | Triage: progression state at crash time |
| `app_environment` | `"production"` | Distinguish prod crashes from dev |
| `app_version` | `"1.0.0"` | Version tag on every crash |

- [ ] All 5 custom keys are present on Crashlytics session start
- [ ] `app_environment` value is `"production"` in the production build (not `"development"` or `"staging"`)
- [ ] `current_level` and `king_level` are updated in real-time as gameplay progresses (not set once at init only)

### 5.5 PII Audit — Crashlytics Logs

Crashlytics logs and custom keys must never contain PII. Verify:

- [ ] No Firebase UID in any Crashlytics log message or custom key
- [ ] No player name, email, or phone number in any Crashlytics log
- [ ] No device IMEI, GAID, or other device identifiers in Crashlytics custom keys
- [ ] `FirebaseCrashlytics.Instance.Log()` calls are grepped: `grep -r "Crashlytics.Instance.Log\|SetCustomKey" Assets/Scripts/` — review all results for PII

---

## Section 6: Firebase Remote Config

### 6.1 Remote Config Enabled

- [ ] Firebase Console → Remote Config is accessible and not showing "Remote Config not configured"
- [ ] Default condition (applies to all users) is present

### 6.2 Production Baseline Uploaded

The production baseline (config_version=3) must be uploaded to the default condition before launch. See `docs/M17-REMOTE-CONFIG-PRODUCTION.md` for the full value table.

- [ ] All 30+ keys from `RemoteConfigKeys.cs` are present in the Remote Config console
- [ ] `config_version` is set to `"3"` in the default condition
- [ ] No soft-launch experiment conditions remain active (EXP-01-COIN-MULT and EXP-02-INTERSTITIAL-FREQ must be concluded and removed or converted to a permanent condition)
- [ ] All production values match the production baseline table in `docs/M17-REMOTE-CONFIG-PRODUCTION.md`

**Key verification — check these high-impact values specifically:**

| Key | Expected Production Value |
|---|---|
| `coin_reward_multiplier` | 1.0 (reverted from 1.2) |
| `upgrade_cost_multiplier` | 1.0 (reverted from 0.9) |
| `destruction_multiplier` | 1.0 (reverted from 1.05) |
| `xp_multiplier` | 1.0 (reverted from 1.1) |
| `powerup_cost_multiplier` | 1.0 (reverted from 0.85) |
| `interstitial_frequency` | 3 (reverted from 4) |
| `ad_max_interstitials_per_session` | 5 (reverted from 3) |
| `ad_interstitial_min_session_seconds` | 60 (reverted from 90) |
| `config_version` | "3" (incremented from "2") |

- [ ] All values above are set correctly in the production Remote Config console

### 6.3 Fetch Configuration

- [ ] `FirebaseRemoteConfigService.Initialize()` sets fetch timeout to 10 seconds
- [ ] `RemoteConfigDefaults.GetAll()` returns safe defaults for all 30+ keys
- [ ] On fetch failure or timeout, game continues with defaults (verified in code: fetch failure is caught and logged, not thrown)

### 6.4 On-Device Fetch Test

Test on a physical device with a production build:

1. Install production APK
2. Launch app (anonymous auth + Remote Config fetch happens on boot)
3. Wait 15 seconds
4. Navigate to any screen that uses RC values

- [ ] `config_version` user property appears in Firebase Analytics DebugView as `"3"` within 5 minutes of first launch
- [ ] In-game economy values reflect the production baseline (coins feel slightly harder to earn than during soft launch — this is expected and correct)
- [ ] No fetch error appears in device logcat: `adb logcat | grep -i "RemoteConfig"`

---

## Section 7: Cloud Run game-api

### 7.1 Service Deployed and Running

- [ ] Cloud Run service `game-api` is deployed in project `king-smash-prod`
- [ ] Service URL is HTTPS (not HTTP)
- [ ] Service URL in `FirebaseEnvironmentConfig.cs` matches the deployed Cloud Run URL
- [ ] Cloud Run service is deployed with minimum 1 instance (prevents cold-start on launch day)

### 7.2 Authentication Middleware

- [ ] Auth middleware is active — all endpoints except `/health` require a valid Firebase ID token
- [ ] Test: unauthenticated request to `/api/economy/balance` returns HTTP 401
  ```bash
  curl -s -o /dev/null -w "%{http_code}" https://[CLOUD_RUN_URL]/api/economy/balance
  # Expected: 401
  ```
- [ ] Test: authenticated request with valid token returns HTTP 200
  ```bash
  curl -H "Authorization: Bearer [VALID_FIREBASE_ID_TOKEN]" \
    https://[CLOUD_RUN_URL]/api/economy/balance
  # Expected: 200 with balance payload
  ```

### 7.3 Rate Limiting Active

- [ ] Rate limiting middleware is active on all economy endpoints
- [ ] Rate limit is appropriate for production traffic (not set to an artificially high soft-launch test value)
- [ ] Test: 20 rapid requests from same IP/token trigger rate limit response (HTTP 429)

### 7.4 ADC and Admin SDK

- [ ] Cloud Run service account has Firestore read/write permissions on `king-smash-prod`
- [ ] Firebase Admin SDK is initialized via Application Default Credentials (ADC) — not via a downloaded service account JSON file committed to the repo
- [ ] No service account JSON file exists in the Cloud Run service container

### 7.5 Jest Test Suite Passing

- [ ] All 23 Jest tests pass in the game-api service:
  ```bash
  cd backend/game-api && npm test
  # Expected: 23 passing, 0 failing
  ```

### 7.6 Service Health Check

- [ ] `/health` endpoint returns HTTP 200 with a JSON body indicating all dependencies (Firestore, Firebase Auth) are healthy
- [ ] Cloud Run uptime check (or Cloud Monitoring alert) is configured to page on-call if `/health` returns non-200

---

## Section 8: End-to-End Flow Verification

**STATUS: NOT YET PERFORMED — requires physical device + production deployment**

This section cannot be completed until:
1. The production AAB (version code 3) is installed on a physical Android device
2. The Firebase production environment (Auth, Firestore, Remote Config) is fully configured per Sections 1–7
3. The Cloud Run game-api is deployed and accessible

**Do not mark any item in this section as [PASS] without completing the physical device test.**

### 8.1 Anonymous Authentication Flow

1. Install production APK on a physical device (fresh install, no prior data)
2. Launch the app

- [ ] App completes boot without crash
- [ ] Anonymous auth succeeds within 10 seconds (no auth error dialog)
- [ ] Firebase Console → Authentication → Users shows a new anonymous user entry within 2 minutes of launch
- [ ] Remote Config fetch completes and `config_version=3` user property is set

### 8.2 Level Complete → Firestore Write

1. Complete Level 1

- [ ] Level complete screen appears with coins awarded
- [ ] Firestore Console → `users/{uid}/progress` shows an updated document within 30 seconds
- [ ] Coins balance in Firestore matches coins shown on screen
- [ ] `level_complete` event appears in Analytics DebugView with correct `level_number` and `coins_earned` parameters

### 8.3 Close and Reopen — Progress Restore

1. Complete Level 1 (from 8.2)
2. Force-close the app
3. Reopen the app

- [ ] App relaunches without crash
- [ ] Level 1 is shown as completed (star display matches pre-close state)
- [ ] Coin balance matches the balance shown before closing
- [ ] No duplicate anonymous user is created (same Firebase UID before and after reopen: check via Analytics `user_id` or Crashlytics custom key)

### 8.4 Daily Reward Idempotency

1. Claim daily reward (Day 1)
2. Force-close and reopen the app

- [ ] Daily reward is not claimable again on reopen (server-side idempotency enforced by Cloud Run)
- [ ] Coin balance is not double-credited
- [ ] `daily_reward_claimed` event appears exactly once in Analytics for this session

### 8.5 Upgrade Purchase Flow

1. Accumulate enough coins to purchase an upgrade
2. Open the upgrade store
3. Purchase one upgrade

- [ ] Upgrade UI shows correct cost
- [ ] Coin balance decreases by correct amount
- [ ] Upgrade is reflected in gameplay on the next level
- [ ] Firestore `users/{uid}/economy` upgrade document is updated (via Cloud Run server write)
- [ ] `upgrade_purchased` Analytics event fires with correct `upgrade_type` and `coin_cost`

### 8.6 Power-Up Quantity Decrement

1. Use a power-up during a level

- [ ] Power-up quantity decrements by 1 after use
- [ ] If quantity reaches 0, the power-up is shown as unavailable (not as having 1 remaining)
- [ ] Power-up quantity is persisted in Firestore (not reset on app close)
- [ ] Power-up use does not cause a negative quantity (server-side guard)

---

## Section 9: Launch Readiness Gate

### 9.1 All Sections Complete

| Section | Status | Blocker? |
|---|---|---|
| Section 1: Android App Registration | NOT YET PERFORMED | Yes |
| Section 2: Authentication | NOT YET PERFORMED | Yes |
| Section 3: Firestore | NOT YET PERFORMED | Yes |
| Section 4: Analytics | NOT YET PERFORMED | Yes |
| Section 5: Crashlytics | NOT YET PERFORMED | Yes |
| Section 6: Remote Config | NOT YET PERFORMED | Yes |
| Section 7: Cloud Run game-api | NOT YET PERFORMED | Yes |
| Section 8: End-to-End Flows | NOT YET PERFORMED | Yes |

### 9.2 Final Gate Decision

- [ ] All sections show `[PASS]` (or documented `[SKIP]` with written justification)
- [ ] No `[FAIL]` items remain open
- [ ] End-to-end flows verified on at least 2 physical devices (different Android versions)
- [ ] Sign-off recorded: Engineer name, date, device models used

**PRODUCTION_LAUNCH_BLOCKED until this checklist is fully completed.**

---

*Document version: 1.0 — M17 Milestone*
*See also: docs/M17-PRODUCTION-BUILD-CONFIG.md, docs/M17-REMOTE-CONFIG-PRODUCTION.md, docs/M15-SECURITY-REVIEW.md*
