# King Smash M15 — Build Configuration Reference

---

## Overview

This document is the authoritative reference for all build configuration, scripting define symbols, environment switching, and security settings required to produce a release-quality APK for King Smash soft launch.

All CI/CD steps must follow this document before submitting a build for QA sign-off.

---

## 1. Release Build Settings

| Setting | Value |
|---|---|
| Package ID | `com.kingcastle.kingsmash` (set in Player Settings → Other Settings) |
| Application Name | King Smash |
| Version Name | 1.0.0 |
| Version Code | 1 (increment on every Play Store submission) |
| Build Type | Release |
| Scripting Backend | IL2CPP |
| Target Architecture | ARM64 (primary) + ARMv7 (compatibility) |
| API Compatibility Level | .NET Standard 2.1 |
| Managed Stripping Level | High |
| IL2CPP Code Generation | Faster (smaller) |
| Development Build | OFF for all non-dev builds |
| Autoconnect Profiler | OFF |
| Deep Profiling | OFF |
| Script Debugging | OFF |
| Allow Downloads Over HTTP | Never |
| Internet Access | Require |

### 1.1 Orientation

Portrait only. Set in Player Settings → Resolution and Presentation.

Lock in AndroidManifest.xml as well:

```xml
android:screenOrientation="portrait"
```

---

## 2. Scripting Define Symbols

Define symbols control which services are compiled into each environment. They are set in **Edit → Project Settings → Player → Other Settings → Scripting Define Symbols**.

| Symbol | DEV | STAGING | PROD | Purpose |
|---|:---:|:---:|:---:|---|
| `FIREBASE_ENABLED` | Y | Y | Y | Gates all Firebase SDK calls. Without this the project compiles without the Firebase package (CI, editor). |
| `KING_SMASH_DEV` | Y | — | — | Activates mock services, debug overlays, and dev-only tooling in GameBootstrap. |
| `KING_SMASH_STAGING` | — | Y | — | Activates staging Firebase project. No mocks. All real services. |
| `DEVELOPMENT_BUILD` | Y | Y | — | Unity-controlled flag set automatically when "Development Build" is checked. Enables GameLogger Debug/Info output. |
| `GOOGLE_MOBILE_ADS` | — | Y | Y | Required to compile GoogleMobileAdsService against the AdMob Unity plugin. |

**Minimum symbol sets by environment:**

- **Development:** `FIREBASE_ENABLED;KING_SMASH_DEV`
- **Staging:** `FIREBASE_ENABLED;KING_SMASH_STAGING;GOOGLE_MOBILE_ADS`
- **Production:** `FIREBASE_ENABLED;GOOGLE_MOBILE_ADS`

### 2.1 Service Routing Summary

The GameBootstrap `#if FIREBASE_ENABLED && !KING_SMASH_DEV` guard ensures:

- Dev builds (even with `FIREBASE_ENABLED`) always use mocks.
- Staging and Production builds with `FIREBASE_ENABLED` always use real Firebase services.
- CI builds without any symbols compile and register mocks only.

---

## 3. Firebase Configuration

### 3.1 google-services.json

Each environment has its own Firebase project and its own `google-services.json`:

| Environment | Firebase Project ID | File Location |
|---|---|---|
| Development | `king-smash-dev` | `Assets/google-services.json` |
| Staging | `king-smash-staging` | `Assets/google-services.json` (swapped by CI) |
| Production | `king-smash-prod` | `Assets/google-services.json` (swapped by CI) |

The file lives at `Assets/google-services.json` in all cases. The CI/CD pipeline swaps it from GCP Secret Manager before running the build.

**`google-services.json` is gitignored.** Never commit it.

```
# .gitignore
Assets/google-services.json
Assets/google-services.json.meta
```

### 3.2 Credentials that must never be committed

- `google-services.json`
- GCP service account JSON keys
- Firebase Admin SDK private keys
- API secrets and signing key passwords
- AdMob App ID overrides that contain production unit IDs (use AdConfiguration ScriptableObject instead)

All of the above are stored in **GCP Secret Manager** and fetched by the CI build step.

### 3.3 Remote Config

- Remote Config is fetched on app start (non-blocking, async) via `FirebaseRemoteConfigService.Initialize()` then `FetchAsync()`.
- Default values for all keys are compiled into `RemoteConfigDefaults.GetAll()` to ensure the game functions if the fetch fails or times out.
- Fetch timeout: 10 seconds. On timeout the game proceeds with cached or default values.
- Config version is tracked as a user property in Analytics (`config_version`).

---

## 4. AdMob Configuration

### 4.1 Ad Unit IDs

Ad unit IDs are loaded from the `AdConfiguration` ScriptableObject located at `Resources/AdConfiguration`. They are **never hardcoded** in C# source files.

| Build Type | Ad Unit IDs | Test Mode Flag |
|---|---|---|
| `KING_SMASH_DEV` | AdMob test unit IDs from AdConfiguration.TestRewardedAdUnitId / TestInterstitialAdUnitId | Enabled in AdConfiguration |
| Staging / Production | Production unit IDs from AdConfiguration | Disabled |

The `AdConfiguration` asset has separate fields for test and production unit IDs. The active ID is chosen by the `GetRewardedAdUnitId()` / `GetInterstitialAdUnitId()` methods, which read the `KING_SMASH_DEV` guard at runtime.

### 4.2 App ID

The AdMob App ID must be declared in `AndroidManifest.xml`:

```xml
<meta-data
    android:name="com.google.android.gms.ads.APPLICATION_ID"
    android:value="ca-app-pub-XXXXXXXXXXXXXXXX~XXXXXXXXXX"/>
```

The production App ID is stored in GCP Secret Manager and injected by CI. The test App ID (`ca-app-pub-3940256099942544~3347511713`) is safe to commit and is used in development builds.

---

## 5. Signing

| Item | Location | Committed? |
|---|---|---|
| Release keystore file (`.jks`) | GCP Secret Manager | No |
| Key alias | GCP Secret Manager | No |
| Keystore password | GCP Secret Manager | No |
| Key alias password | GCP Secret Manager | No |
| Signing config template | `ci/build-settings-template.json` | Yes (no secrets) |

The CI step fetches the keystore and injects signing credentials into the Unity build settings at build time. The keystore is never written to disk in the repository.

---

## 6. IL2CPP and Code Stripping

High managed stripping removes unused Unity engine code. To prevent required types from being stripped, maintain `Assets/link.xml`:

```xml
<linker>
  <assembly fullname="KingSmash" preserve="all"/>
  <assembly fullname="Firebase.Analytics"/>
  <assembly fullname="Firebase.Auth"/>
  <assembly fullname="Firebase.Crashlytics"/>
  <assembly fullname="Firebase.RemoteConfig"/>
  <assembly fullname="Firebase.Firestore"/>
</linker>
```

Test each release candidate with stripping enabled. IL2CPP exceptions caused by stripped types produce `ExecutionEngineException` at runtime.

---

## 7. ProGuard / R8 (Android)

Unity generates `mainTemplate.gradle` and `proguard-user.txt`. Ensure the following are kept:

```
-keep class com.google.firebase.** { *; }
-keep class com.google.android.gms.ads.** { *; }
-dontwarn com.google.firebase.**
```

Place rules in `Assets/Plugins/Android/proguard-user.txt`.

---

## 8. Security Checklist

Complete this checklist before every release candidate build is submitted for QA sign-off.

### 8.1 Source and Credentials

- [ ] `google-services.json` is absent from the repository (verify with `git status`)
- [ ] No GCP service account keys anywhere in the repository (`find . -name "*.json" | xargs grep -l "private_key"` returns empty)
- [ ] No hardcoded API keys, passwords, or tokens in C# source files (`grep -r "apikey\|password\|secret\|Bearer " Assets/Scripts` returns empty)
- [ ] AdMob App ID in AndroidManifest is the test App ID for dev builds and the production App ID for release builds
- [ ] Production Ad Unit IDs are in the `AdConfiguration` ScriptableObject, not in source code

### 8.2 Build Flags

- [ ] `Development Build` checkbox is OFF
- [ ] `Autoconnect Profiler` is OFF
- [ ] `Deep Profiling` is OFF
- [ ] `Script Debugging` is OFF
- [ ] Scripting define symbols match the target environment (see Section 2)
- [ ] `KING_SMASH_DEV` is absent from production symbol list
- [ ] `GOOGLE_MOBILE_ADS` is present in staging/production symbol list

### 8.3 Debug Surface

- [ ] `M2TestLevelSetup` Start() is a no-op in non-editor builds (M15 fix applied)
- [ ] `DebugEconomyTools` is `#if UNITY_EDITOR` — confirmed never ships in APK
- [ ] `AnalyticsDebugOverlay` is `#if UNITY_EDITOR || KING_SMASH_DEV` — not visible in production
- [ ] `AnalyticsCrashTest` is `#if UNITY_EDITOR || KING_SMASH_DEV` — not visible in production
- [ ] `GameLogger` minimum level is Warning or higher in release builds (automatic via `#if !DEVELOPMENT_BUILD && !UNITY_EDITOR`)

### 8.4 Firebase Services

- [ ] `FirebaseAnalyticsService` is registered (not `AnalyticsServiceMock`) in production build
- [ ] `FirebaseRemoteConfigService` is registered in production build
- [ ] `FirebaseCrashlyticsService` is registered and `Initialize()` called in production build
- [ ] `FirebaseAuthService` is registered in production build
- [ ] `FirebaseCloudSaveService` is registered in production build
- [ ] `GoogleMobileAdsService` is registered in production build
- [ ] Crashlytics dSYM/symbol upload step is configured in CI for each release build

### 8.5 Economy and Billing

- [ ] Economy write paths verified as server-authoritative (client cannot call economy Firestore collection with `allow write: if true`)
- [ ] Purchase token is not logged anywhere (grep for `purchaseToken` in Analytics calls)
- [ ] `PurchaseServiceMock` is expected for soft launch (billing deferred to M16); document explicitly in release notes

---

## 9. CI/CD Pipeline Notes

### 9.1 Unity Cloud Build or GitHub Actions

Recommended build pipeline steps (in order):

1. Checkout repository
2. Fetch `google-services.json` from GCP Secret Manager and write to `Assets/`
3. Fetch signing keystore from GCP Secret Manager and write to a temp path
4. Set scripting define symbols for target environment
5. Run Unity build with IL2CPP, Release configuration
6. Sign the APK with the fetched keystore
7. Delete keystore temp file
8. Upload dSYM symbols to Crashlytics
9. Archive APK artifact
10. Run post-build size and security checks

### 9.2 Environment Branch Mapping

| Git branch / tag | Environment | Symbols |
|---|---|---|
| `develop` | Development | `FIREBASE_ENABLED;KING_SMASH_DEV` |
| `staging` | Staging | `FIREBASE_ENABLED;KING_SMASH_STAGING;GOOGLE_MOBILE_ADS` |
| `release/*` / `main` | Production | `FIREBASE_ENABLED;GOOGLE_MOBILE_ADS` |

### 9.3 Version Code

Increment `versionCode` in `ProjectSettings/ProjectSettings.asset` for every Play Store submission. Never reuse a version code. Automate incrementing in the CI step before building.

---

*Last updated: M15 — October 2026*
