# King Smash M17 — Production Security Final Review

**Review Date:** 2026-10-05
**Reviewer:** Production Security Engineer
**Scope:** Full codebase M0–M17, pre-production-launch readiness
**Build Target:** Android (ARM64), Unity 6 LTS (6000.0.47f1), IL2CPP Release, `FIREBASE_ENABLED;GOOGLE_MOBILE_ADS` define set
**Previous Review:** M15-SECURITY-REVIEW.md (pre-soft-launch, October 2026)
**Status:** CONDITIONAL PASS — 2 launch blockers require resolution before Production track submission

---

## Executive Summary

King Smash has maintained strong security fundamentals from M0 through M17. The server-authoritative economy architecture, Firebase ID token verification on every Cloud Run call, SHA-256 UID hashing for Analytics, and compile-guard-protected debug surface are all intact and verified across the milestone history.

Two blockers remain that prevent a clean Production launch:

1. `IPurchaseService` is still registered as `PurchaseServiceMock` in `GameBootstrap.cs` (line 118). Real billing has never been active. Google Play Billing purchases will silently succeed on the client without any server-side receipt verification or entitlement grant. This is a functional and revenue-integrity blocker.

2. AdMob production ad unit IDs require final verification in the `AdConfiguration` ScriptableObject before the production AAB is signed. Using test ad unit IDs in a production build violates AdMob policy and results in account suspension.

All other areas reviewed below are CLEAR or have documented, non-blocking deferrals tracked with explicit recommendations.

**Overall verdict: LAUNCH BLOCKED — resolve SEC-BLOCK-001 and SEC-BLOCK-002 before Production track submission.**

---

## 1. Client-Side Secrets Audit

Scope: all tracked files in the repository. Verified against `git ls-files` output and manual review of service configuration files.

| File / Area | Finding | Risk | Status |
|---|---|---|---|
| `Assets/google-services.json` | File is gitignored. Confirmed absent from `git ls-files`. Provisioned by CI from GCP Secret Manager at build time. | None | CLEAR |
| Firebase API key (inside `google-services.json`) | The `current_key` field in `google-services.json` is the Android-SDK public API key. It is not a server credential. It identifies the Firebase project to the Android SDK and is intentionally public. Embedding it in the APK is correct and expected behavior. | None — this is by design | CLEAR |
| Cloud Run base URL (`FirebaseEnvironmentConfig.cs`) | The `CloudRunBaseUrl` field holds a public HTTPS endpoint (`https://api-[hash].run.app`). Cloud Run endpoints are public by design; authentication is enforced server-side via Firebase ID token verification on every request. The URL is not a secret. | None | CLEAR |
| AdMob App ID (`AdConfiguration` ScriptableObject) | The AdMob App ID is a public identifier required in the `AndroidManifest.xml` and embedded in every Android binary. Google's policy explicitly requires it to be present in the manifest. It is not a secret. | None — public by design | CLEAR |
| GCP Secret Manager | The production signing keystore, service account JSON, and all backend environment secrets are stored in GCP Secret Manager. None are committed to the repository. CI retrieves them at build time via Workload Identity Federation. | None | CLEAR |
| Signing keystore (`.keystore` / `.jks`) | Not present in the repository. Confirmed via `git ls-files` search. Stored in GCP Secret Manager. CI injects at build time only. | None | CLEAR |
| Service account JSON | Not present in the repository. Confirmed via `git ls-files` search. Cloud Run uses Workload Identity; local development uses Application Default Credentials. | None | CLEAR |
| Hardcoded API keys, tokens, passwords | `grep -rE '(api_key|apiKey|api-key|password|secret|token)\s*=\s*"[A-Za-z0-9+/]{20,}"' Assets/Scripts/` returns no matches in production code paths. | None | CLEAR |
| `.env` files | No `.env` files committed. Confirmed via `git ls-files`. | None | CLEAR |

**Verdict: NO embedded production secrets in the tracked codebase. CLEAR.**

---

## 2. Debug Surface Audit

Scope: all files with debug tooling, test utilities, or dev-only behavior. Verified that each is guarded against compilation in production builds (release build with no `UNITY_EDITOR`, no `DEVELOPMENT_BUILD`, no `KING_SMASH_DEV` define).

| File | Guard Condition | Production Build Impact | Status |
|---|---|---|---|
| `Assets/Scripts/Economy/DebugEconomyTools.cs` | `#if UNITY_EDITOR` | Not compiled into any APK. Unity Editor menu items only. | CLEAR |
| `Assets/Scripts/Levels/M1TestLevelSetup.cs` | `#if UNITY_EDITOR` | Not compiled into any APK. | CLEAR |
| `Assets/Scripts/Levels/M2TestLevelSetup.cs` | `#if UNITY_EDITOR` on entire `Start()` body — **fixed M15 BUG-001** | `Start()` is an explicit empty method in production. Zero runtime cost; zero information disclosure. Class retained for scene reference compatibility. | CLEAR — fixed M15 |
| `Assets/Scripts/Analytics/AnalyticsDebugOverlay.cs` | `#if UNITY_EDITOR \|\| KING_SMASH_DEV` | Not visible or functional in production builds. Only active in dev builds with `KING_SMASH_DEV` define explicitly set. | CLEAR |
| `Assets/Scripts/Analytics/AnalyticsCrashTest.cs` | `#if UNITY_EDITOR \|\| KING_SMASH_DEV` | Not compiled into production builds. Never callable from production runtime. | CLEAR |
| `Assets/Scripts/Core/GameLogger.cs` | `#if !DEVELOPMENT_BUILD && !UNITY_EDITOR` clamps minimum log level to `Warning` | Debug and Info log calls are compiled into the binary but produce no output. Warning and above are emitted. No internal state is disclosed via logcat on production devices. | CLEAR |
| `Assets/Scripts/Core/GameBootstrap.cs` (service mocks) | `#if FIREBASE_ENABLED && !KING_SMASH_DEV` gates real service registration; `#else` path uses mocks | Production builds compiled with `FIREBASE_ENABLED` and without `KING_SMASH_DEV` correctly register real Firebase services. Mocks are active only in dev/CI. Fixed M15 P2-002. | CLEAR — fixed M15 |
| `Assets/Scripts/Services/Firebase/FirebaseEnvironmentConfig.cs` | Placeholder `-xxx` segments in Cloud Run URLs | Not a debug guard issue — see SEC-P2-003 in Known Issues. | See SEC-P2-003 |

**Verdict: Debug surface is fully guarded. No debug tooling reachable in production builds. CLEAR.**

---

## 3. Economy Integrity

### Architecture

The economy layer is server-authoritative end-to-end. The client cannot modify its own economy state.

**Write path:** Client → Cloud Run (Firebase ID token verified) → Firebase Admin SDK → Firestore

**Client Firestore rules (all economy collections):**

```
match /users/{uid}/economy/{document=**} {
  allow read: if request.auth.uid == uid;
  allow write: if false;
}

match /users/{uid}/transactions/{document=**} {
  allow read: if request.auth.uid == uid;
  allow write: if false;
}

match /users/{uid}/purchases/{document=**} {
  allow read: if request.auth.uid == uid;
  allow write: if false;
}

match /users/{uid}/entitlements/{document=**} {
  allow read: if request.auth.uid == uid;
  allow write: if false;
}

match /users/{uid}/dailyRewards/{document=**} {
  allow read: if request.auth.uid == uid;
  allow write: if false;
}
```

Every collection that holds economy state is `allow write: if false` for client access. Cloud Run uses the Firebase Admin SDK, which bypasses client security rules.

### Idempotency

All Cloud Run reward endpoints accept a client-generated `transactionId` UUID. The server stores the `transactionId` in Firestore and rejects any request with a previously-seen `transactionId` for that `uid`. Network retries from the client are safe — a reward cannot be double-granted regardless of retry count or timing.

### Negative balance protection

`CurrencyService` enforces a floor of 0 for both coins and gems on all deduction operations. The server validates the current balance before applying any spend. A client that sends a crafted spend request for more coins than it holds receives a `400 Bad Request` response; no balance modification occurs.

`PowerUpService` enforces an equivalent floor — power-up count cannot go below 0 on any deduct operation.

### DebugEconomyTools assessment

`DebugEconomyTools.cs` calls `CurrencyService` methods directly (local in-memory mutation only — no server write). These calls are behind `#if UNITY_EDITOR` and are never compiled into any APK. No direct Firestore write path exists from debug tooling.

**Verdict: Economy integrity is sound. CLEAR.**

---

## 4. Authentication Security

### Firebase ID token verification

All Cloud Run endpoints require a valid Firebase ID token in the `Authorization: Bearer <token>` header. The Firebase Admin SDK verifies the token on every request. There is no unauthenticated endpoint in the production backend.

### Server-side user identity

The server extracts the `uid` from the verified ID token payload. No endpoint accepts a client-supplied `playerId`, `userId`, or `uid` parameter that could be forged. The `uid` used for all Firestore reads and writes is always derived from the verified token, never from the request body.

### Anonymous authentication

Players begin with Firebase anonymous authentication. The anonymous session generates a Firebase UID that persists across app launches (stored by the Firebase Auth SDK on-device). Anonymous sessions are full Firebase auth users with the same ID token structure as Google Sign-In users.

### Google Sign-In account linking

When a player signs in with Google, the Google credential is exchanged for a Firebase ID token server-side. The raw Google OAuth access token is not stored on disk, not logged, and not sent to any analytics endpoint. The Firebase Auth SDK handles credential lifecycle.

### UID hashing for Analytics

`FirebaseAnalyticsService.SetUserId` applies SHA-256 to the raw Firebase UID and truncates the result to 16 hex characters (64 bits of entropy) before calling `FirebaseAnalytics.SetUserId`. The raw UID is never sent to the Firebase Analytics backend. This prevents cross-referencing Analytics events with the Firebase Auth user table via raw UID.

### Token and credential logging

`GameLogger` in release builds (`#if !DEVELOPMENT_BUILD && !UNITY_EDITOR`) suppresses all Debug and Info messages. Manual review of all `GameLogger` call sites confirmed: no call site passes a Firebase ID token, a Google OAuth token, a purchase receipt token, or any credential value as a log parameter.

### Token refresh

The Firebase Auth SDK handles ID token refresh automatically (tokens expire every 60 minutes). The client never stores raw tokens to disk — they are held in memory by the Firebase Auth SDK's internal credential store.

**Verdict: Authentication security is sound. CLEAR.**

---

## 5. Billing Security

### Current state

`IPurchaseService` is currently registered as `PurchaseServiceMock` in `GameBootstrap.cs` at line 118. This is documented with a TODO comment in the source. The mock implementation accepts all purchase requests and returns a synthetic success response without making any Google Play Billing calls.

**This is a launch blocker (SEC-BLOCK-001).**

In a production build with `PurchaseServiceMock` active:
- A player who taps a purchase button will see the mock complete immediately without triggering the Google Play purchase dialog.
- No Google Play receipt is generated.
- No server-side receipt verification occurs.
- No entitlement is granted in Firestore.
- The player receives no purchased content and no money is charged — but the UX implies a purchase was attempted.

If a real billing library is wired but `PurchaseServiceMock` is still registered (an easy misconfiguration), the reverse problem occurs: the Google Play dialog appears, the player is charged, but no receipt verification runs and no entitlement is granted.

### Server-side receipt verification infrastructure

`IReceiptVerificationService` is implemented and wired into `ShopService`. When real billing is enabled, the verification flow is:

1. Client receives a signed Google Play purchase token from the Google Play Billing Library.
2. Client sends the purchase token to the Cloud Run `/verify-purchase` endpoint.
3. Cloud Run verifies the token against the Google Play Developer API server-side.
4. On verification success, Cloud Run writes the entitlement to Firestore and acknowledges the purchase.
5. The purchase token is never logged or sent to Analytics.

The server-side infrastructure is complete. Only the client-side `IPurchaseService` binding needs to be replaced.

### Entitlement write path

The `entitlements` Firestore collection is `allow write: if false` for client access (see Section 3). Entitlements can only be written by Cloud Run. The client cannot grant itself an entitlement directly.

### Purchase token handling

Confirmed by search across all `GameLogger`, `IAnalyticsService`, and remote logging call sites: no purchase token value is passed to any logging or analytics function.

**Verdict: BLOCKED — SEC-BLOCK-001 must be resolved before Production launch.**

---

## 6. Ad Integrity

### Ad service registration

`GoogleMobileAdsService` is registered in `GameBootstrap.cs` under the `#if FIREBASE_ENABLED && !KING_SMASH_DEV` guard. In production builds with the correct define set, the real AdMob service is active. In dev builds and CI, the mock ad service is used.

### Rewarded ad reward integrity

The rewarded ad reward callback fires only after the ad SDK signals `ad_completed` (the full ad was watched). The reward is not granted on ad skip, ad failure, or early close. The reward amount is defined server-side in Remote Config and not accepted from client parameters.

### Interstitial ad policy

Interstitial ads fire between levels, not mid-level. The interstitial policy guard checks:
- The player has not purchased Remove Ads.
- The minimum interval since the last interstitial has elapsed (configured via Remote Config, default 60 seconds).
- The player is not in a tutorial or onboarding flow.

The Remove Ads entitlement is read from Firestore (server-written only). A client cannot self-grant the Remove Ads entitlement to suppress interstitials.

### AdMob App ID

The AdMob App ID in the `AdConfiguration` ScriptableObject and in `AndroidManifest.xml` must be the production App ID before the production AAB is signed.

**This is a launch blocker (SEC-BLOCK-002).**

Using test App IDs or test ad unit IDs in a production build violates AdMob policies and will result in ad serving suspension. Verify that all ad unit IDs (rewarded, interstitial) and the App ID in the ScriptableObject match the production values registered in AdMob Console.

**Verdict: BLOCKED on ad unit ID verification (SEC-BLOCK-002). Ad integrity logic itself is sound.**

---

## 7. Known Remaining Issues

| ID | Area | Description | Severity | Blocker? |
|---|---|---|---|---|
| SEC-BLOCK-001 | Billing | `IPurchaseService` is `PurchaseServiceMock` in `GameBootstrap.cs` line 118. Real billing is not active. Players cannot complete purchases. Server-side receipt verification does not run. | P0 — Critical | YES |
| SEC-BLOCK-002 | Ads | AdMob App ID and ad unit IDs in `AdConfiguration` ScriptableObject must be verified as production values before signing the production AAB. Using test IDs in production violates AdMob policy. | P0 — Critical | YES |
| SEC-P2-001 | Network | No certificate pinning on Cloud Run HTTPS connections. Standard TLS only. Cloud Run uses Google-managed certificates. Risk is very low for this title at launch scale. | P2 — Low | NO |
| SEC-P2-002 | Device integrity | No root or emulator detection. The server-authoritative economy limits blast radius. Google Play Integrity API (replacing SafetyNet) checks are recommended for post-launch economy-modifying endpoints. | P2 — Low | NO |
| SEC-P2-003 | Build / CI | `FirebaseEnvironmentConfig.cs` Cloud Run URLs must not contain `-xxx` placeholder segments in the production build. A CI validation step asserting absence of `-xxx` in production config is recommended. | P2 — Low | NO — would cause API call failures, not a security vulnerability |

---

## 8. Recommendations Before Production

The following four actions are required before submitting to the Google Play Production track.

**1. Replace PurchaseServiceMock with a real Google Play Billing implementation (SEC-BLOCK-001)**

In `GameBootstrap.cs` at line 118, replace the `PurchaseServiceMock` registration with a real `GooglePlayPurchaseService` (or equivalent) that wraps the Google Play Billing Library 6.x. The registration should be gated identically to the other Firebase services: `#if FIREBASE_ENABLED && !KING_SMASH_DEV`. Test the full purchase flow end-to-end in a staging environment with a real Google Play test purchase: dialog appears → player confirms → receipt sent to Cloud Run → Cloud Run verifies against Play Developer API → entitlement written to Firestore → client reads entitlement → content unlocked. Confirm the acknowledgment call is made within 3 days of purchase (Play policy requirement).

**2. Verify all AdMob IDs are production values before signing the AAB (SEC-BLOCK-002)**

Open the `AdConfiguration` ScriptableObject in the Unity Editor and confirm every field (App ID, rewarded ad unit ID, interstitial ad unit ID) matches the corresponding value in AdMob Console for the `com.kingcastle.kingsmash` production app. Do not sign the production AAB until this is confirmed. Add a CI build step that asserts no field in the AdConfiguration asset contains a known test ID pattern (`ca-app-pub-3940256099942544`).

**3. Confirm production Firestore security rules are deployed to `king-smash-prod`**

Run `firebase firestore:rules:get --project king-smash-prod` and verify the deployed rules include `allow write: if false` on all five economy collections (economy, transactions, purchases, entitlements, dailyRewards). Do not assume the rules from staging are identical to production. Deploy explicitly from the verified rules file.

**4. Validate that the production AAB contains no `-xxx` placeholder Cloud Run URLs**

After building the production AAB but before uploading to Play Console, extract the APK and decompile or inspect `assets/bin/Data/` for the `FirebaseEnvironmentConfig` asset. Confirm that `CloudRunBaseUrl` does not contain `-xxx`. If it does, the production environment configuration was not injected correctly by CI. This would cause all Cloud Run calls to fail silently after launch, disabling cloud save, economy, and ad reward grants for all players.

---

## 9. Security Posture Summary

| Area | M15 Status | M17 Status | Change |
|---|---|---|---|
| Client-side secrets | CLEAR | CLEAR | No change — already clean |
| Debug surface | CLEAR (M2TestLevelSetup fixed) | CLEAR | No change |
| Economy integrity | CLEAR | CLEAR | No change |
| Authentication | CLEAR | CLEAR | No change |
| Billing | Deferred (mock, soft launch) | BLOCKED | Must resolve before production |
| Ad integrity (logic) | CLEAR | CLEAR | No change |
| Ad integrity (IDs) | N/A (test IDs acceptable in soft launch) | BLOCKED | Must verify before production AAB |
| Certificate pinning | Acknowledged — P3 | Acknowledged — P2 | Upgraded priority; still non-blocking |
| Root/emulator detection | Acknowledged — P3 | Acknowledged — P2 | Upgraded priority; still non-blocking |
| Cloud Run URL validation | Acknowledged — P3 | Acknowledged — P2 | CI step recommended; non-blocking |

---

*Last updated: 2026-10-05 — M17 Production Security Final Review*
