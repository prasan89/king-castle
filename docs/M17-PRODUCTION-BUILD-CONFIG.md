# King Smash M17 — Production Build Configuration

**Version:** 1.0.0 (version code 3)
**Build Type:** Release AAB — Google Play Store
**Milestone:** M17 Production Launch
**Follows:** M16 Soft-Launch (1.0.0-sl1, version code 2, status: NEEDS_MORE_SOFT_LAUNCH)
**Firebase Project:** king-smash-prod
**Unity Version:** 6000.0.47f1 (Unity 6 LTS)
**Prepared:** M17 Milestone

---

## Overview

This document is the authoritative reference for all build configuration, scripting define symbols, environment switching, signing, security controls, and CI/CD pipeline steps required to produce a production-quality Android App Bundle (AAB) for King Smash global launch.

All CI/CD operators and release engineers must follow this document exactly before submitting a production build to Google Play. Any deviation from this configuration invalidates the security posture established in M15 and M16 and must be reviewed and approved before proceeding.

**Prior milestones that inform this document:**
- M15-BUILD-CONFIGURATION.md — foundational build settings, IL2CPP stripping, ProGuard rules
- M16-SOFT-LAUNCH-STRATEGY.md — environment switching, version code rationale
- M15-SECURITY-REVIEW.md — credential hygiene, debug surface audits

---

## 1. Version and Build Identity

| Setting | Value | Notes |
|---|---|---|
| Version Name | `1.0.0` | Clean semver. No `-sl1` suffix. Never reuse after first Play Console upload. |
| Version Code | `3` | sl1 was code 2; rc1 was code 1. Code 3 signals the first global production build. |
| Application Name | King Smash | As shown on the Play Store listing. |
| Package Name | `com.kingcastle.kingsmash` | See Section 1.1 — permanent, never change. |
| Build Type | Release AAB | AAB for Play Store submission. APK only for direct device testing. |
| Unity Version | 6000.0.47f1 | Unity 6 LTS. Never build production with any non-LTS or patch release without explicit approval. |

### 1.1 Package Name Lock

**`com.kingcastle.kingsmash` must never be changed after the first Google Play Console upload.**

Google Play treats the package name as a permanent identifier for the app. A package name change creates a new, separate app in the Play Store — it cannot be merged with the original. All user installs, reviews, ratings, and Play Console history are tied to the package name. Changing it means starting over with zero installs and losing all prior users.

**Verification command (post-build):**
```bash
aapt dump badging path/to/KingSmash.aab | grep package:
# Expected: package: name='com.kingcastle.kingsmash' versionCode='3' versionName='1.0.0'
```

---

## 2. Release Build Settings

| Setting | Value | Why |
|---|---|---|
| Scripting Backend | IL2CPP | Required for ARM64. Converts C# to C++ for ahead-of-time compilation, improving startup time and resistance to reflection-based cheating. |
| Target Architecture | ARM64 + ARMv7 | ARM64 for modern devices (required by Play since 2019). ARMv7 for compatibility with older Android 8.0 devices. |
| API Compatibility Level | .NET Standard 2.1 | Consistent with all prior builds. Do not change. |
| Managed Stripping Level | High | Removes all unused Unity engine code from the binary. Reduces APK size and reduces attack surface. Requires `link.xml` validation (see Section 6). |
| IL2CPP Code Generation | Faster (smaller) | Optimizes for binary size over code generation speed. Correct choice for mobile. |
| Compression Method | LZ4HC | Best compression ratio for Unity asset bundles. Consistent with sl1 build. |
| Development Build | **OFF** | Must be OFF in all production and staging builds. Development builds expose the Profiler connection socket and output verbose logs. |
| Autoconnect Profiler | **OFF** | Dependent on Development Build; also explicitly OFF. |
| Deep Profiling | **OFF** | Dependent on Development Build; also explicitly OFF. |
| Script Debugging | **OFF** | Must be OFF. Script debugging enables a TCP/IP debug server on the device. |
| Allow Downloads Over HTTP | **Never** | All network traffic must use HTTPS. Firebase SDK enforces this; the Unity setting adds a second layer. |
| Internet Access | Require | The app requires network access; declare this explicitly. |
| Orientation | Portrait only | Set in Player Settings → Resolution and Presentation and in AndroidManifest.xml. |

### 2.1 Orientation Lock in AndroidManifest

```xml
<activity
    android:screenOrientation="portrait"
    android:configChanges="orientation|screenSize|keyboardHidden">
```

---

## 3. Scripting Define Symbols

Scripting define symbols are set in **Edit → Project Settings → Player → Other Settings → Scripting Define Symbols**.

### 3.1 Production Symbol Set

| Symbol | Production Value | Effect |
|---|:---:|---|
| `FIREBASE_ENABLED` | **SET** | Enables all Firebase SDK calls. Without this define, GameBootstrap registers mock services. This define enables real Firebase Auth, Analytics, Crashlytics, Remote Config, and Firestore. |
| `KING_SMASH_DEV` | **NOT SET** | If set, GameBootstrap routes to mock services even when `FIREBASE_ENABLED` is present. Must be absent in all production builds. |
| `KING_SMASH_STAGING` | **NOT SET** | Staging environment guard. Must be absent in production builds. |
| `DEVELOPMENT_BUILD` | **NOT SET** | Unity sets this automatically when "Development Build" is checked. Must be absent. Controls GameLogger minimum output level. |
| `GOOGLE_MOBILE_ADS` | **SET** | Required to compile GoogleMobileAdsService against the AdMob Unity plugin. Must be set for any build that shows ads. |

**Production defines string (copy-paste into Player Settings):**
```
FIREBASE_ENABLED;GOOGLE_MOBILE_ADS
```

### 3.2 Service Routing — Net Effect of Production Defines

The central routing guard in `GameBootstrap.cs` is:

```csharp
#if FIREBASE_ENABLED && !KING_SMASH_DEV
    // Real Firebase services registered
#else
    // Mock services registered
#endif
```

With the production symbol set (`FIREBASE_ENABLED` set, `KING_SMASH_DEV` absent), the condition evaluates to `true` and all real Firebase services are registered. The mock service path is compiled out by IL2CPP.

**Debug and overlay systems deactivated in production:**

| System | Guard | Production State |
|---|---|---|
| `AnalyticsDebugOverlay` | `#if UNITY_EDITOR \|\| KING_SMASH_DEV` | **Inactive** — not compiled into APK |
| `AnalyticsCrashTest` | `#if UNITY_EDITOR \|\| KING_SMASH_DEV` | **Inactive** — not compiled into APK |
| `DebugEconomyTools` | `#if UNITY_EDITOR` | **Inactive** — not compiled into APK |
| `GCAllocLogger` | `#if UNITY_EDITOR \|\| KING_SMASH_DEV` | **Inactive** — not compiled into APK |
| `FrameTimingMonitor` | `#if UNITY_EDITOR \|\| KING_SMASH_DEV` | **Inactive** — not compiled into APK |
| `MemoryWatchdog` | `#if UNITY_EDITOR \|\| KING_SMASH_DEV` | **Inactive** — not compiled into APK |
| `M2TestLevelSetup` | Start() is no-op outside editor | **Inactive** in production builds |
| `GameLogger` | Min level: Warning in non-DEVELOPMENT_BUILD | Only `Warning` and `Error` logs emit |

All `#if UNITY_EDITOR || KING_SMASH_DEV` guards evaluate to `false` in production, removing the enclosed code from the IL2CPP-compiled binary. This is verified automatically by checking that the final AAB size does not include debug texture atlases or overlay canvases.

### 3.3 Symbol Verification Script

Run this before every production build to catch accidental symbol inclusion:

```bash
# Check ProjectSettings.asset for stray dev symbols
grep -E "KING_SMASH_DEV|KING_SMASH_STAGING|DEVELOPMENT_BUILD" \
    ProjectSettings/ProjectSettings.asset
# Expected: No output (empty match). Any match is a build blocker.
```

---

## 4. Android App Bundle vs APK

| Format | Use Case | Notes |
|---|---|---|
| **AAB** (Android App Bundle) | Google Play Store submission | Required. Play Console will not accept APKs from new apps after August 2021. Play App Signing handles the final APK split generation. |
| **APK** | Direct device testing, QA sideload | Build via Unity → Build Settings → Build (not Build And Run) with APK selected. Sign with release keystore for release testing; debug keystore for dev testing. |

**Never submit an APK to Google Play.** Only use AAB for Play Console uploads.

---

## 5. Firebase Configuration

### 5.1 google-services.json

| Environment | Firebase Project | File Source |
|---|---|---|
| Production | `king-smash-prod` | GCP Secret Manager, fetched by CI |

The `google-services.json` file is placed in `Assets/Plugins/Android/` before the Unity build step. The CI pipeline fetches it from GCP Secret Manager and writes it to this path. It is never committed to the repository.

**`.gitignore` entries (must be present):**
```
Assets/Plugins/Android/google-services.json
Assets/Plugins/Android/google-services.json.meta
Assets/google-services.json
Assets/google-services.json.meta
```

**CI fetch step:**
```bash
gcloud secrets versions access latest \
    --secret="king-smash-prod-google-services-json" \
    --project="king-smash-prod" \
    > Assets/Plugins/Android/google-services.json
```

### 5.2 FirebaseEnvironmentConfig.cs

Verify that `FirebaseEnvironmentConfig.cs` has the Cloud Run service URL set to the production endpoint before the production build:

```csharp
// Production
public static string GameApiBaseUrl => "https://game-api-[hash]-uc.a.run.app";
```

The staging URL must not appear in a production binary. The `#if KING_SMASH_STAGING` guard handles this; verify the guard is correctly applied before building.

---

## 6. IL2CPP Code Stripping and link.xml

High managed stripping level removes unused types from the IL2CPP-compiled binary. This requires `Assets/link.xml` to preserve all types that are accessed reflectively or via serialization at runtime.

**Required `Assets/link.xml`:**
```xml
<linker>
  <assembly fullname="KingSmash" preserve="all"/>
  <assembly fullname="Firebase.Analytics"/>
  <assembly fullname="Firebase.Auth"/>
  <assembly fullname="Firebase.Crashlytics"/>
  <assembly fullname="Firebase.RemoteConfig"/>
  <assembly fullname="Firebase.Firestore"/>
  <assembly fullname="Google.MobileAds"/>
  <assembly fullname="Unity.Services.Core"/>
</linker>
```

If `ExecutionEngineException` appears in Crashlytics after a production build, it indicates a type was stripped. Add the affected assembly to `link.xml` and rebuild.

---

## 7. ProGuard / R8 Rules

Unity generates `proguard-user.txt` via `Assets/Plugins/Android/proguard-user.txt`. The following rules must be present:

```
-keep class com.google.firebase.** { *; }
-keep class com.google.android.gms.ads.** { *; }
-keep class com.google.android.gms.** { *; }
-dontwarn com.google.firebase.**
-dontwarn com.google.android.gms.**
```

---

## 8. App Signing

### 8.1 Signing Architecture

King Smash uses **Google Play App Signing**. This separates two keys:

| Key | Purpose | Location |
|---|---|---|
| Upload key | Signs the AAB you upload to Play Console | GCP Secret Manager |
| Distribution key | Signs the final APK delivered to users (managed by Google) | Google Play infrastructure |

This means if the upload key is ever compromised, it can be rotated by Google Play support without changing the distribution key that users' devices trust. **This is strongly recommended for all new apps.**

### 8.2 Keystore Credential Sources

| Credential | Storage | How CI Fetches It |
|---|---|---|
| Release keystore (`.jks`) | GCP Secret Manager, base64-encoded | Decoded and written to temp path |
| Key alias | GCP Secret Manager | Environment variable |
| Keystore password | GCP Secret Manager | Environment variable |
| Key alias password | GCP Secret Manager | Environment variable |

**CI step (example):**
```bash
# Fetch keystore
gcloud secrets versions access latest \
    --secret="king-smash-upload-keystore-b64" \
    --project="king-smash-prod" \
    | base64 --decode > /tmp/king-smash-upload.jks

export KEYSTORE_PATH=/tmp/king-smash-upload.jks
export KEY_ALIAS=$(gcloud secrets versions access latest --secret="king-smash-key-alias")
export KEYSTORE_PASS=$(gcloud secrets versions access latest --secret="king-smash-keystore-pass")
export KEY_PASS=$(gcloud secrets versions access latest --secret="king-smash-key-pass")
```

After the build, the keystore file is deleted:
```bash
rm -f /tmp/king-smash-upload.jks
```

### 8.3 What Must NEVER Be Committed to the Repository

This list is absolute. No exceptions. No "temporary" commits.

| File / Pattern | Why |
|---|---|
| `*.jks`, `*.keystore` | Release signing key — loss = inability to update the app |
| Keystore passwords (any format) | Allow signing new builds impersonating the app |
| `google-services.json` (production) | Contains Firebase project config including Analytics measurement ID and API keys |
| `service-account.json`, `*-serviceAccount-*.json` | GCP service account keys — full admin access to Firebase project |
| Firebase Admin SDK private keys | Same as above |
| `.env` files containing API keys or secrets | Any `.env` with non-public values |
| `AdConfiguration` assets with production Ad Unit IDs hardcoded as strings | Play Store policy violation if production unit IDs are exposed |

**Automated detection — run before every release commit:**
```bash
# Check for accidental private key material
find . -name "*.json" -not -path "./.git/*" \
    | xargs grep -l "private_key" 2>/dev/null
# Expected: no output

# Check for hardcoded secrets in source
grep -r "apikey\|password\|secret\|Bearer \|AIza\|ya29\." Assets/Scripts/ 2>/dev/null
# Expected: no output

# Check for keystore files
find . -name "*.jks" -o -name "*.keystore" 2>/dev/null | grep -v ".git"
# Expected: no output
```

---

## 9. Unity CLI Batch Build (CI Reference)

Production builds must be run headlessly via the Unity CLI to eliminate human error and ensure repeatability.

### 9.1 Build Method

The CI pipeline calls a static `BuildScript.BuildProductionAAB()` method defined in `Assets/Editor/BuildScript.cs`:

```bash
/path/to/Unity \
  -quit \
  -batchmode \
  -nographics \
  -projectPath "$PROJECT_PATH" \
  -buildTarget Android \
  -executeMethod BuildScript.BuildProductionAAB \
  -logFile build.log \
  -customBuildPath "$OUTPUT_PATH/KingSmash-1.0.0.aab"
```

### 9.2 BuildScript.BuildProductionAAB() Required Actions

The build method must perform, in order:

1. Set `PlayerSettings.applicationIdentifier` — verify it equals `com.kingcastle.kingsmash`
2. Set `PlayerSettings.bundleVersion` = `"1.0.0"`
3. Set `PlayerSettings.Android.bundleVersionCode` = `3`
4. Set scripting defines: `FIREBASE_ENABLED;GOOGLE_MOBILE_ADS`
5. Set `PlayerSettings.Android.targetArchitectures` = `AndroidArchitecture.ARM64 | AndroidArchitecture.ARMv7`
6. Set `EditorUserBuildSettings.buildAppBundle` = `true`
7. Set `PlayerSettings.Android.useCustomKeystore` = `true` (inject keystore from env vars)
8. Set `EditorUserBuildSettings.androidBuildSystem` = `AndroidBuildSystem.Gradle`
9. Call `BuildPipeline.BuildPlayer(...)` with `BuildOptions.None` (never `Development`)
10. Exit with code `0` on success, `1` on failure (CI reads exit code)

### 9.3 CI Pipeline Step Order

The complete production build pipeline in order:

1. Checkout repository at tagged commit (e.g., `v1.0.0`)
2. Verify no secrets in repository (`grep` checks from Section 8.3)
3. Fetch `google-services.json` from GCP Secret Manager → `Assets/Plugins/Android/`
4. Fetch signing keystore from GCP Secret Manager → `/tmp/king-smash-upload.jks`
5. Set keystore environment variables
6. Run Unity batch build (`BuildScript.BuildProductionAAB`)
7. Delete keystore temp file
8. Run post-build verification checklist (Section 10)
9. Upload Crashlytics mapping file (IL2CPP symbols)
10. Upload AAB artifact to GCP artifact storage
11. (Manual gate) Engineer approves artifact upload to Play Console

---

## 10. Post-Build Verification Checklist

Complete this checklist for every build before it is uploaded to Google Play Console. A single failure is a build blocker — do not upload.

### 10.1 Version and Package Identity

- [ ] Version name is exactly `1.0.0` (no `-sl1`, no `-rc`, no extra characters)
  ```bash
  aapt dump badging KingSmash.aab | grep "versionName"
  # Expected: versionName='1.0.0'
  ```
- [ ] Version code is exactly `3`
  ```bash
  aapt dump badging KingSmash.aab | grep "versionCode"
  # Expected: versionCode='3'
  ```
- [ ] Package name is exactly `com.kingcastle.kingsmash`
  ```bash
  aapt dump badging KingSmash.aab | grep "package: name"
  # Expected: package: name='com.kingcastle.kingsmash'
  ```

### 10.2 Signing

- [ ] AAB is signed with the release (upload) keystore, NOT the debug keystore
  ```bash
  apksigner verify --verbose KingSmash.aab 2>&1 | grep "Verified using"
  # Must NOT contain "cn=Android Debug" in the signer chain
  ```
- [ ] Certificate SHA-256 matches the registered upload key in Firebase Console (Android app settings)

### 10.3 Build Configuration

- [ ] Development Build is OFF (no Profiler socket, no debug logs)
  ```bash
  strings KingSmash.aab | grep -i "profiler\|autoconnect" | wc -l
  # Expected: very low count (strings from SDK docs only, no Unity Profiler references)
  ```
- [ ] IL2CPP backend confirmed — binary contains `libil2cpp.so`, not `libmono.so`
  ```bash
  unzip -l KingSmash.aab | grep "libil2cpp.so"
  # Expected: at least one match for each ABI (arm64-v8a, armeabi-v7a)
  ```
- [ ] Scripting defines verified — no `KING_SMASH_DEV` or `KING_SMASH_STAGING` in build artifacts
  ```bash
  grep -E "KING_SMASH_DEV|KING_SMASH_STAGING" ProjectSettings/ProjectSettings.asset
  # Expected: no output
  ```

### 10.4 File Size

- [ ] AAB file size is under 200 MB
  ```bash
  ls -lh KingSmash.aab | awk '{print $5}'
  # Expected: < 200M
  ```
  If over 200 MB, investigate asset bundles and texture compression before uploading. Play Store has a 150 MB limit for APK but 2 GB for AAB; the 200 MB budget is a performance constraint, not a store constraint.

### 10.5 Firebase Configuration

- [ ] `google-services.json` references `king-smash-prod` project (not dev or staging)
  ```bash
  python3 -c "import json; d=json.load(open('Assets/Plugins/Android/google-services.json')); \
    print(d['project_info']['project_id'])"
  # Expected: king-smash-prod
  ```
- [ ] `google-services.json` is NOT committed in git working tree
  ```bash
  git status Assets/Plugins/Android/google-services.json
  # Expected: "nothing to commit" or file not tracked
  ```

### 10.6 No Credential Leakage

- [ ] No private key material in any tracked file (see Section 8.3 grep commands)
- [ ] No service account JSON files in repository
- [ ] Build log (`build.log`) does not contain keystore password strings

### 10.7 Debug Surface Audit

- [ ] `AnalyticsDebugOverlay` MonoBehaviour is absent from all production scenes
- [ ] `AnalyticsCrashTest` MonoBehaviour is absent from all production scenes
- [ ] `M2TestLevelSetup` MonoBehaviour Start() is a no-op (confirmed via code review)
- [ ] GameLogger minimum level is `Warning` in non-DEVELOPMENT_BUILD (confirmed in `GameLogger.cs`)

---

## 11. Performance Budget Reference

Production builds are validated against `PerformanceBudget.cs` constants. These are not RC-adjustable — they are compile-time constants that define the engineering contract for device support.

| Budget | Value | Class |
|---|---|---|
| Target frame time | 16.67 ms (60 FPS) | `PerformanceBudget.TargetFrameTime` |
| Low-end frame time | 33.33 ms (30 FPS) | `PerformanceBudget.LowEndFrameTime` |
| Max managed heap | 256 MB | `PerformanceBudget.MaxManagedHeapMB` |

Low-end devices (those that sustain 30 FPS but cannot sustain 60 FPS) are supported but must not crash due to memory pressure. The 256 MB heap budget applies to all devices on the supported matrix.

---

## 12. Environment Branch and Tag Mapping

| Git Ref | Environment | Scripting Defines | Firebase Project |
|---|---|---|---|
| `feature/*`, `develop` | Development | `FIREBASE_ENABLED;KING_SMASH_DEV` | king-smash-dev |
| `staging` | Staging | `FIREBASE_ENABLED;KING_SMASH_STAGING;GOOGLE_MOBILE_ADS` | king-smash-staging |
| `release/1.0.0`, tag `v1.0.0` | Production | `FIREBASE_ENABLED;GOOGLE_MOBILE_ADS` | king-smash-prod |

The production build is always built from a tagged commit. Never build production from an untagged commit or from `main` directly — tag the commit first.

---

## 13. AdMob Configuration

Ad unit IDs are loaded from the `AdConfiguration` ScriptableObject at `Resources/AdConfiguration`. They are never hardcoded in C# source files.

The AdMob App ID must be declared in `AndroidManifest.xml`:

```xml
<meta-data
    android:name="com.google.android.gms.ads.APPLICATION_ID"
    android:value="ca-app-pub-XXXXXXXXXXXXXXXX~XXXXXXXXXX"/>
```

The production App ID is stored in GCP Secret Manager and injected by CI. Never commit production Ad Unit IDs or the production App ID in source.

---

*Document version: 1.0 — M17 Milestone*
*See also: docs/M15-BUILD-CONFIGURATION.md, docs/M16-SOFT-LAUNCH-STRATEGY.md, docs/M17-FIREBASE-PRODUCTION-CHECKLIST.md, docs/M17-REMOTE-CONFIG-PRODUCTION.md*
