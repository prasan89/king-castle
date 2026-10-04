# King Smash M15 — Security Review

**Review Date:** October 2026
**Reviewer:** Senior Android/Unity Security & Build Engineer
**Scope:** M0–M14 codebase, pre-soft-launch readiness
**Build Target:** Android (ARM64), Unity 6 LTS (6000.0.47f1), IL2CPP Release

---

## Executive Summary

King Smash is in good security shape for soft launch. The architecture makes the right foundational choices: a server-authoritative economy enforced via Firestore security rules and Cloud Run, Firebase ID token verification on every server call, SHA-256-hashed user IDs in Analytics, and a compile-guard system that keeps debug tooling out of release builds.

One issue was identified and fixed during this review cycle (M2TestLevelSetup — see P2 finding below). No P0 or P1 findings were identified.

The primary outstanding item is that `IPurchaseService` remains a mock pending store listing approval (deferred to M16). This is a documented, intentional deferral, not a security gap — the server-side receipt verification infrastructure is already complete.

**Overall verdict: APPROVED FOR SOFT LAUNCH** with the fixes noted below applied and the pre-soft-launch checklist completed.

---

## Client-Side Review

### Findings Summary

| Area | Finding | Risk | Status |
|---|---|---|---|
| Debug surface | M2TestLevelSetup MonoBehaviour had no editor guard | Low | Fixed (M15) |
| Mock services in production | GameBootstrap registered mocks for all Firebase services | Medium (functional, not security) | Fixed (M15) |
| Embedded secrets | No hardcoded API keys, tokens, or passwords found in source | None | PASS |
| Debug endpoints | No debug HTTP endpoints or test server URLs in production path | None | PASS |
| Client-authoritative economy | Economy writes go through Cloud Run only | None | PASS |
| Insecure reward logic | Reward grants are idempotent and server-validated | None | PASS |
| Purchase token logging | No purchase token found in Analytics or Logger calls | None | PASS |
| Raw UID in Analytics | FirebaseAnalyticsService hashes with SHA-256 before SetUserId | None | PASS |
| Certificate pinning | Not implemented (acceptable for soft launch; revisit for GA) | Low | Acknowledged |
| Root/emulator detection | Not implemented (acceptable for soft launch; revisit for GA) | Low | Acknowledged |

---

## Economy Security

### Architecture

The economy layer is server-authoritative. Coins and gems are stored in Firestore under the `economy` collection. The only write path is through Cloud Run endpoints which:

1. Validate a Firebase ID token on every request.
2. Apply server-side reward logic.
3. Use a transaction UUID to enforce idempotency (prevents double-grants on network retry).
4. Write to Firestore with a server-side timestamp.

### Firestore Security Rules

The following collections are locked to server-write-only (Cloud Run uses the Firebase Admin SDK which bypasses client rules):

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
```

The client can read its own economy state for UI display but cannot modify it. Any attempt to write from the client is rejected by Firestore rules.

### DebugEconomyTools Assessment

`DebugEconomyTools.cs` uses Unity Editor menu items to add coins and gems directly via `CurrencyService` (local save only, no server write). This is guarded by `#if UNITY_EDITOR` and is never compiled into any APK. Confirmed safe.

---

## Auth Security

- **Anonymous auth:** The client obtains a Firebase anonymous ID token. All server calls include this token in the `Authorization: Bearer <token>` header. Cloud Run verifies the token using the Firebase Admin SDK on every request.
- **Google Sign-In:** When the player signs in with Google, the resulting credential is exchanged for a Firebase ID token server-side. The raw Google OAuth token is not stored or logged.
- **User ID in Analytics:** `FirebaseAnalyticsService.SetUserId` hashes the raw Firebase UID with SHA-256 and truncates to 16 hex characters (64 bits) before calling `FirebaseAnalytics.SetUserId`. The raw UID is never sent to the Analytics backend.
- **Logs:** `GameLogger` in release builds (`#if !DEVELOPMENT_BUILD && !UNITY_EDITOR`) clamps the minimum log level to `Warning`. Debug and Info messages — which could include state machine transitions or session context — are suppressed entirely in production APKs.
- **Token lifecycle:** The Firebase Auth SDK handles token refresh automatically. The client never stores raw tokens to disk (they are held in memory by the Firebase Auth SDK).

---

## Billing Security

- **Purchase flow:** `IPurchaseService` is currently `PurchaseServiceMock`. This is a documented deferral (M16) pending Google Play store listing approval. The mock does not make any real billing calls.
- **Receipt verification:** The server-side receipt verification service (`IReceiptVerificationService`) is implemented and wired into `ShopService`. When real billing is enabled in M16 the verification will be server-side via Cloud Run, matching the economy write pattern.
- **Purchase token logging:** Confirmed by grep that no purchase token value is passed to `GameLogger`, `IAnalyticsService`, or any remote logging call.
- **Entitlement state:** `entitlements` Firestore collection is server-write-only (see Firestore rules above). The client reads entitlement state but cannot grant itself entitlements.
- **Soft launch note:** Soft launch without real billing is standard practice. Revenue collection begins after store approval is confirmed.

---

## Debug Surface Review

| File | Guard | Production Build Impact | Status |
|---|---|---|---|
| `Assets/Scripts/Economy/DebugEconomyTools.cs` | `#if UNITY_EDITOR` | Not compiled into any APK | SAFE |
| `Assets/Scripts/Levels/M1TestLevelSetup.cs` | `#if UNITY_EDITOR` | Not compiled into any APK | SAFE |
| `Assets/Scripts/Levels/M2TestLevelSetup.cs` | None (was missing) — **FIXED M15** | Start() is now a no-op outside the editor | FIXED |
| `Assets/Scripts/Analytics/AnalyticsDebugOverlay.cs` | `#if UNITY_EDITOR \|\| KING_SMASH_DEV` | Invisible in production; only visible in dev builds | SAFE |
| `Assets/Scripts/Analytics/AnalyticsCrashTest.cs` | `#if UNITY_EDITOR \|\| KING_SMASH_DEV` | Invisible in production; only visible in dev builds | SAFE |
| `Assets/Scripts/Core/GameLogger.cs` | `#if !DEVELOPMENT_BUILD && !UNITY_EDITOR` clamps minimum to Warning | Debug/Info logs never emit in release APK | SAFE |

### M2TestLevelSetup Detail

The original file was a `MonoBehaviour` with no editor guard. Its `Start()` method:
- Called `GameLogger.Info` with internal enemy counts and scene configuration.
- Subscribed to `EnemyController.OnEnemyDied`, `QueenController.OnQueenRescued`, and `KingController.OnKingStateChanged` to log game events.

**Risk assessment:** Low. The logger suppresses Info messages in release builds anyway, so no information would appear in logcat on a production device. However the event subscriptions would remain active and consume memory/CPU. Additionally, a reverse engineer decompiling the IL2CPP output could discover internal event names and game object structure.

**Fix applied (M15):** The entire `Start()` body is now inside `#if UNITY_EDITOR`. In non-editor builds `Start()` is an explicit empty method. The class itself is retained so that scenes referencing it do not break on load.

---

## Findings

### P0 — Critical (Production Blocking)

None identified.

### P1 — High (Must Fix Before Soft Launch)

None identified.

### P2 — Medium (Fixed This Milestone)

**P2-001: M2TestLevelSetup — missing editor guard on debug MonoBehaviour**

- **File:** `Assets/Scripts/Levels/M2TestLevelSetup.cs`
- **Finding:** The MonoBehaviour had no `#if UNITY_EDITOR` guard. In a production build it would subscribe to three internal game events and log scene configuration details.
- **Risk:** Low information disclosure; minor runtime overhead from active event subscriptions.
- **Fix:** Wrapped all `Start()` logic in `#if UNITY_EDITOR`. Replaced with an empty `Start()` in the `#else` branch. Class retained for scene reference compatibility.
- **Status:** FIXED in M15.

**P2-002: GameBootstrap — all production services registered as mocks**

- **File:** `Assets/Scripts/Core/GameBootstrap.cs`
- **Finding:** All Firebase and AdMob services (Analytics, Config, Crashlytics, Auth, Cloud Save, Ads) were registered as mocks unconditionally. A production build with `FIREBASE_ENABLED` would silently no-op all telemetry, crash reporting, and cloud save.
- **Risk:** Medium functional risk (blind production deployment with no observability). Not a direct security vulnerability but a significant operational risk — a crashing production build would generate zero Crashlytics reports.
- **Fix:** Added `#if FIREBASE_ENABLED && !KING_SMASH_DEV` guards for all seven service registrations. Mocks remain as the `#else` path for dev and CI.
- **Status:** FIXED in M15.

### P3 — Low (Acknowledge and Track)

**P3-001: No certificate pinning**

- **Finding:** HTTP calls to Cloud Run use standard TLS without certificate pinning.
- **Risk:** Very low. Cloud Run endpoints use Google-managed TLS certificates. Pinning would protect against a compromised CA, which is not a realistic threat for this title at soft launch.
- **Recommendation:** Evaluate pinning for GA if the title enters markets where this is a regulatory or platform expectation. Use Unity's `HttpClient` with a custom `HttpMessageHandler` or a library such as TrustKit.
- **Status:** Acknowledged — defer to GA.

**P3-002: No root or emulator detection**

- **Finding:** The client does not detect rooted devices or emulator environments.
- **Risk:** Low at soft launch territory scale. A motivated attacker on a rooted device could intercept network traffic or modify local save data, but the server-authoritative economy limits the blast radius.
- **Recommendation:** Implement Google Play Integrity API (replaces SafetyNet) checks server-side before economy-modifying operations, post-soft-launch.
- **Status:** Acknowledged — defer to M16.

**P3-003: IPurchaseService is a mock**

- **Finding:** Billing is not active for soft launch.
- **Risk:** None — this is an intentional deferral documented in GameBootstrap with a TODO comment. The server-side billing infrastructure is complete.
- **Recommendation:** Complete M16 billing implementation before monetisation launch.
- **Status:** Acknowledged — scheduled for M16.

**P3-004: CloudRunBaseUrl contains placeholder segments (`-xxx`)**

- **File:** `Assets/Scripts/Services/Firebase/FirebaseEnvironmentConfig.cs`
- **Finding:** The Cloud Run URLs contain `-xxx` placeholder segments (e.g., `https://api-xxx.run.app`). These are placeholders for the real service hash segments assigned by Cloud Run.
- **Risk:** None if the real URLs are injected at build time. If the placeholder strings shipped in a production APK, API calls would fail but no secrets would be exposed.
- **Recommendation:** Confirm that CI injects real Cloud Run URLs via scripting define symbols or a separate config asset before each production build, and add a CI validation step that asserts the production URL does not contain `-xxx`.
- **Status:** Acknowledged — add CI validation.

---

## Recommendations

### Pre-Soft-Launch Actions (Required)

1. Apply M15 source changes: `M2TestLevelSetup.cs` editor guard fix and `GameBootstrap.cs` production service switching. (**Done in M15.**)
2. Confirm `google-services.json` is in `.gitignore` and absent from the repository (`git ls-files Assets/google-services.json` returns empty).
3. Verify production Firestore security rules are deployed: economy/transactions/purchases/entitlements collections must be `allow write: if false` for client access.
4. Run a staging build with `FIREBASE_ENABLED;KING_SMASH_STAGING;GOOGLE_MOBILE_ADS` and confirm in Firebase Console that: Analytics events appear, Crashlytics initialises, Remote Config fetch succeeds, Auth tokens are issued.
5. Confirm Crashlytics symbol upload is configured in CI for the staging build; force-crash with `AnalyticsCrashTest` and verify the crash appears symbolicated in the Firebase Console.
6. Run the M15-BUILD-CONFIGURATION.md Security Checklist in full against the release candidate APK.
7. Confirm `Development Build` is OFF in the production build settings.
8. Confirm production Cloud Run URLs do not contain `-xxx` placeholder segments.

### Post-Soft-Launch / M16 Actions (Tracked)

- Implement real billing (`IPurchaseService`) with Google Play Billing Library.
- Add Google Play Integrity API checks server-side for economy-modifying endpoints.
- Evaluate certificate pinning requirement for target markets.
- Add CI validation asserting no `-xxx` segments in production Cloud Run URLs.
- Add proguard/R8 keep rules for Firebase and AdMob classes if not already present.

---

*Last updated: M15 — October 2026*
