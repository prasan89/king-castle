# King Smash M15 — QA Strategy & Release Gate

---

## 1. Objective

### 1.1 Purpose of M15 QA

M15 is the final pre-soft-launch quality assurance milestone for King Smash. The objective is to validate that every implemented system (M0–M14) meets production quality standards before the title enters a monitored soft-launch territory (e.g., a single Android market such as Philippines or Canada).

M15 QA is not a feature milestone. It is a gate milestone: no new features are added during M15; the entire effort goes toward finding, documenting, and resolving defects discovered across all prior milestones.

The QA strategy covers:
- Functional correctness of all gameplay systems
- Data integrity of cloud save, economy, and progression
- Billing and reward integrity (zero exploit tolerance)
- Performance on the defined device matrix
- Analytics event fidelity for Firebase Analytics dashboards
- Firebase Remote Config live-ops readiness
- Crashlytics symbol upload and crash-free session rate baseline
- Manual and automated regression coverage
- Formal release gate pass/fail criteria

### 1.2 Soft Launch Readiness Standard

A build is considered soft-launch-ready when all of the following are true:

1. Zero open P0 defects.
2. All P1 defects that affect gameplay progression, billing, cloud save, or reward systems are fixed and regression-verified.
3. P2 defects are documented in the tracker with acknowledged owners and target-fix milestones.
4. P3 defects are logged; no P3 blocks the release.
5. The full Critical Path Regression sequence (Section 6) passes end-to-end.
6. The Release Gate Checklist (Section 8) is signed off with no open blocking items.
7. Crashlytics baseline: crash-free session rate ≥ 99.0% on emulator/CI run.
8. Firebase Analytics: all Tier-1 events (level_start, level_complete, purchase, ad_reward_claimed) fire with correct parameter sets.
9. Firebase Remote Config: remote override verified for at least coin_multiplier, daily_reward_enabled, and feature_flag_megaking.
10. Performance thresholds (Section 5) pass on at least one mid-range emulator configuration.

---

## 2. Severity Definitions

| Severity | Description | Examples | Gate Impact |
|----------|-------------|----------|-------------|
| **P0** | Game-breaking: the player cannot progress, the game crashes at launch or during a critical flow, data is permanently lost, or financial integrity is violated | Cold-start crash before main menu; progression lock after Level 19–20 world transition; billing charge without reward delivery; coin/gem duplication exploit; cloud save data wipe; IAP purchase that grants items for free | **Hard block.** Build cannot proceed to soft launch until all P0s are closed and regression-verified. |
| **P1** | Major functional defect: a feature is broken or produces incorrect output but the game can still be played | Daily reward granted twice on same UTC day; wrong gem amount from IAP; achievement never unlocks even when criteria met; rewarded ad watched but coins not credited; offline play corrupts subsequent cloud sync; wrong XP shown in end-of-level screen; Firebase Auth sign-in fails and blocks onboarding | **Soft block.** All P1s in gameplay, billing, save, economy, and ad reward flows must be fixed before soft launch. P1s in non-critical flows (e.g., cosmetic unlock order) may be deferred with PM sign-off. |
| **P2** | Visible defect that degrades experience but does not break functionality | Hit-stop frame skips in rare device/level combination; UI element misaligned on 20:9 aspect ratio; audio pitch incorrect on one power-up; level clear star animation stutters on low-end; text truncated in German locale; single-frame VFX pop at level start | **Non-blocking.** Must be logged with repro steps. Target fix milestone assigned before full worldwide launch. |
| **P3** | Cosmetic or polish item with negligible player impact | Slight colour mismatch vs design spec; shadow z-order on HUD element; button press sound 2 dB quieter than design target; minor particle clipping on edge of screen; coin label drops one frame behind animation on very slow devices | **Non-blocking.** Logged and triaged. May be deferred to post-launch patch. |

### 2.1 Release Criteria Summary

- **No P0s** — zero open, zero "fixed but not verified".
- **All gameplay/billing/save/economy/ad-reward P1s** — fixed, regression-verified, and closed.
- **All P1s** in any category — either fixed+closed or explicitly deferred by PM with documented rationale.
- **P2/P3** — fully documented in tracker; no block on release.

---

## 3. Test Categories

The following 40 test categories align with the M15 test specification. Each category is assigned a priority, and the coverage method is noted.

| # | Category | Priority | Automated? | Manual? |
|---|----------|----------|-----------|--------|
| 1 | Cold Launch & Splash Screen | P0 | EditMode/PlayMode | Yes |
| 2 | Firebase Authentication — Anonymous | P0 | PlayMode | Yes |
| 3 | Firebase Authentication — Google Sign-In | P1 | No | Yes |
| 4 | Onboarding Flow (first-run tutorial) | P1 | PlayMode | Yes |
| 5 | Main Menu / Home Screen | P1 | PlayMode | Yes |
| 6 | World Map Navigation | P1 | PlayMode | Yes |
| 7 | Level Select Screen | P1 | PlayMode | Yes |
| 8 | Level Load & Initialization | P0 | PlayMode | Yes |
| 9 | Core Gameplay — Input & Physics | P0 | PlayMode | Yes |
| 10 | Core Gameplay — Block Destruction | P0 | PlayMode | Yes |
| 11 | Enemy Behaviour & AI | P1 | PlayMode | Yes |
| 12 | Boss Encounter Logic | P1 | PlayMode | Yes |
| 13 | Power-up: Bomb | P1 | PlayMode | Yes |
| 14 | Power-up: Fire | P1 | PlayMode | Yes |
| 15 | Power-up: Ice | P1 | PlayMode | Yes |
| 16 | Power-up: Lightning | P1 | PlayMode | Yes |
| 17 | Power-up: Mega King | P1 | PlayMode | Yes |
| 18 | Victory Flow & Star Rewards | P0 | PlayMode | Yes |
| 19 | Failure Flow & Continue/Retry | P1 | PlayMode | Yes |
| 20 | Economy — Coins & XP | P0 | EditMode/PlayMode | Yes |
| 21 | Economy — Gems | P0 | EditMode/PlayMode | Yes |
| 22 | Upgrades System | P1 | PlayMode | Yes |
| 23 | Shop Screen | P1 | PlayMode | Yes |
| 24 | Google Play Billing — IAP | P0 | No | Yes (manual + sandbox) |
| 25 | AdMob — Rewarded Ads | P0 | No | Yes |
| 26 | AdMob — Interstitial Ads | P1 | No | Yes |
| 27 | Daily Rewards | P1 | PlayMode | Yes |
| 28 | Missions System | P1 | PlayMode | Yes |
| 29 | Achievements | P1 | PlayMode | Yes |
| 30 | Firestore Cloud Save — Write | P0 | PlayMode | Yes |
| 31 | Firestore Cloud Save — Read & Conflict Resolution | P0 | PlayMode | Yes |
| 32 | Offline Mode | P1 | PlayMode | Yes |
| 33 | Remote Config — Fetch & Apply | P1 | PlayMode | Yes |
| 34 | Firebase Analytics — Event Fidelity | P1 | PlayMode | Yes |
| 35 | Firebase Crashlytics — Symbol Upload & Non-Fatal | P1 | EditMode | Yes |
| 36 | Settings Screen | P2 | PlayMode | Yes |
| 37 | Audio System | P2 | No | Yes |
| 38 | Android Back Button & Navigation | P1 | No | Yes |
| 39 | Background/Resume (ALT-TAB, incoming call) | P1 | No | Yes |
| 40 | Performance & Memory Profiling | P0 | PlayMode (headless) | Yes |

---

## 4. Device Matrix

### 4.1 Device Tiers

| Device Tier | Example Devices | RAM | Android Version | Screen Resolution | Notes |
|-------------|----------------|-----|-----------------|-------------------|------|
| **LOW-END** | Samsung Galaxy A03, Motorola Moto E40, Xiaomi Redmi 9A | 2–3 GB | Android 9–10 (API 28–29) | 720 × 1600 (HD+), 720p | Heavy memory pressure; GC spikes expected; minimum target frame rate 30fps acceptable |
| **LOW-END (notch)** | Redmi 9, Motorola Moto G9 Play | 3 GB | Android 10 (API 29) | 720 × 1520, waterdrop notch | SafeAreaHandler.cs must keep HUD out of notch; gesture nav bar at bottom |
| **MID-RANGE** | Samsung Galaxy A52, Google Pixel 4a, Xiaomi Redmi Note 11 | 4–6 GB | Android 11–12 (API 30–32) | 1080 × 2400 (FHD+) | Primary target class; 60fps target must be met here |
| **MID-RANGE (punch-hole)** | Samsung Galaxy A53, OnePlus Nord CE 2 | 6 GB | Android 12 (API 32) | 1080 × 2400, centered punch-hole | Camera cutout requires SafeAreaHandler.cs validation |
| **MID-RANGE (gesture nav)** | Google Pixel 4a (gesture mode enabled) | 6 GB | Android 12 | 1080 × 2340 | Gesture nav bar overlaps bottom HUD unless insets applied; verify no buttons hidden |
| **HIGH-END** | Samsung Galaxy S22, Google Pixel 7, OnePlus 10 Pro | 8–12 GB | Android 13–14 (API 33–34) | 1080 × 2340 – 1440 × 3088 | 120Hz display; IL2CPP must not cap at 30fps; verify vsync target |
| **HIGH-END (foldable)** | Samsung Galaxy Z Fold 4 (inner display) | 12 GB | Android 13 | 2176 × 1812 (inner), near-square | Layout must not break on near-square aspect ratio; SafeAreaHandler hinge handling |
| **TABLET** | Samsung Galaxy Tab A8 | 3–4 GB | Android 11 | 1920 × 1200 (WUXGA), 16:10 | Out of primary scope for soft launch; P3 severity for layout issues |

### 4.2 Android Version Coverage Rationale

| Android Version | API Level | Market Share Rationale | Included in Matrix |
|----------------|-----------|----------------------|-------------------|
| Android 7.0–8.1 | 24–27 | Min SDK target; very small share but must not crash | Crash-only gate (must not crash on install/launch) |
| Android 9 | 28 | ~8% of active Android; low-end devices | Low-end device tier |
| Android 10 | 29 | ~15% of active Android; important mid-low overlap | Low-end + low-mid device tier |
| Android 11 | 30 | ~18% of active Android; scoped storage enforcement | Mid-range primary |
| Android 12 | 31–32 | ~20% of active Android; splash screen API enforced | Mid-range primary |
| Android 13 | 33 | ~22% of active Android; per-app notification permissions | High-end primary; **target API** |
| Android 14 | 34 | ~12% of active Android; partial photo picker | High-end secondary |

### 4.3 Screen Resolution & Aspect Ratio Coverage

| Aspect Ratio | Resolution Examples | Devices | Notes |
|-------------|--------------------|---------|-|
| 16:9 | 1920 × 1080 | Older mid-range | Standard; rarely seen on new devices but emulator default |
| 18:9 | 2160 × 1080, 1440 × 720 | Galaxy A series 2019–2020 | Long screen; UI safe area top/bottom |
| 19.5:9 | 2340 × 1080 | Pixel 4a, Galaxy S21 | Common punch-hole/teardrop notch class |
| 20:9 | 2400 × 1080 | Galaxy A52, Redmi Note 11 | Tall screen; most common mid-range 2021+ |
| 20.9:9 | 3088 × 1440 | Galaxy S22 Ultra | Very tall; HUD may be pushed far from screen centre |

### 4.4 Notch / Punch-hole Handling

SafeAreaHandler.cs reads `Screen.safeArea` and repositions the HUD canvas anchors at runtime. The following configurations must be verified:

- Waterdrop / teardrop notch (top-centre): verify score label and top HUD row sit below the notch cutout.
- Centred punch-hole (top-centre): same as teardrop but smaller; verify no UI element clips into the hole.
- Corner punch-hole (top-right or top-left): verify star counter and settings button anchor do not overlap.
- Gesture navigation bar (bottom inset): verify the bottom HUD, retry button, and shop shortcut are above the gesture bar inset.
- 3-button navigation bar: verify the bottom bar adds layout padding; buttons must not be partially hidden.

### 4.5 Test Status for This M15 Pass

| Configuration | Test Status |
|--------------|-------------|
| Unity EditMode tests (all categories) | **AUTOMATED TESTED** — Unity EditMode runner on CI |
| Unity PlayMode tests (all categories) | **AUTOMATED TESTED** — Unity PlayMode runner on CI |
| Android Emulator — API 33 x86_64, 1080 × 2400 FHD+ | **EMULATOR TESTED** — Android Studio emulator |
| Android Emulator — API 29 x86, 720 × 1520 HD+ | **EMULATOR TESTED** — Android Studio emulator |
| Physical device — any tier | **PHYSICAL DEVICE TESTED — None available for this pass** |
| Foldable inner display | **NOT TESTED** — requires physical hardware |
| 120Hz display (high-end) | **NOT TESTED** — emulator does not simulate 120Hz |
| Thermal throttling under sustained play | **NOT TESTED** — requires physical hardware |
| Real AdMob ad delivery | **NOT TESTED** — requires physical device + AdMob mediation |
| Google Play Billing (live or sandbox) | **NOT TESTED** — requires Play-signed APK on physical device |
| Incoming call interruption | **NOT TESTED** — requires physical hardware |

---

## 5. Performance Thresholds

All measurements use Unity Profiler (PlayMode), Android Logcat, and Firebase Performance (where instrumented).

| Metric | Target | Warning | Fail | Measurement Method |
|--------|--------|---------|------|--------------------|
| Gameplay frame rate (mainstream mid-range device) | ≥ 60 fps stable | 45–59 fps sustained | < 30 fps for > 2 consecutive seconds | Unity Profiler frame time; `Application.targetFrameRate = 60` |
| Cold launch time (app icon tap → main menu interactive) | < 5 seconds | 5–8 seconds | > 10 seconds | ADB `am start -W` timestamps |
| Gameplay scene memory (RSS) | < 350 MB | 350–500 MB | > 600 MB | Unity Profiler Memory snapshot; Android `dumpsys meminfo` |
| GC allocations per gameplay frame | 0 bytes/frame | > 0 and < 1 KB/frame | > 5 KB/frame | Unity Profiler Allocation tracker; zero-alloc hot path mandate |
| Level scene load time (LevelManager.LoadLevel → first gameplay frame) | < 3 seconds | 3–5 seconds | > 7 seconds | Unity Profiler Timeline; `Time.realtimeSinceStartup` delta log |
| Asset bundle decompression (first-run) | < 2 seconds | 2–4 seconds | > 6 seconds | Unity Profiler I/O timeline |
| Crash-free session rate (automated run) | ≥ 99.5% | 99.0–99.5% | < 99.0% | Firebase Crashlytics dashboard; CI test session aggregate |
| ANR rate | 0% | N/A | Any ANR | ADB logcat `am_anr` tag; Crashlytics ANR reporting |
| Battery draw during 10-min gameplay session | Baseline (no measurement on emulator) | N/A | N/A | Physical device only; not testable this pass |

### 5.1 Notes on Performance Testing Without Physical Hardware

Without a physical device, performance thresholds are evaluated on the Android Studio emulator (API 33, x86_64, 4 GB RAM allocation). Emulator CPU/GPU performance does not reflect real device performance. Results from emulator runs are used for regression detection only (i.e., a test that was 4.9s last run and is now 6.1s indicates a regression), not as absolute pass/fail evidence. Absolute pass/fail evidence requires physical device testing before worldwide launch.

---

## 6. Critical Path Regression

The following sequence must be executed end-to-end after every fix that touches gameplay, economy, save, auth, analytics, or Remote Config. A regression is confirmed if any step produces an unexpected result.

| Step | Action | Expected Result | Regression Signal |
|------|--------|----------------|-------------------|
| 1 | Clean install (no prior data). Launch app. | Splash → anonymous Firebase auth → onboarding tutorial → Level 1 unlocked. | Crash, auth failure, or onboarding loop. |
| 2 | Complete Level 1. Collect reward. Upgrade one power-up. Select Level 2. | Level 2 loads; coins deducted correctly for upgrade; XP incremented. | Wrong coin/XP delta; Level 2 locked; upgrade UI does not reflect purchase. |
| 3 | Close app (home button). Reopen. Navigate to home screen. | Coins, XP, upgrade level, and Level 2 unlock all persisted from step 2. | Any value reset to zero or pre-upgrade state. |
| 4 | Play to Level 10 (World 1 checkpoint). Complete it. | World 1 complete badge shown; World 2 levels unlock. | World 2 remains locked; checkpoint badge absent. |
| 5 | Complete Level 19. Confirm Level 20 (world boss) unlocks. Complete Level 20. World 2 unlocks. | World 2 map visible and at least Level 21 unlocked. | World 2 never unlocks; Level 20 boss does not spawn; save corruption. |
| 6 | Purchase a power-up (coins). Enter any level. Activate the purchased power-up. | Power-up activates; inventory decremented by 1 after use. | Coins deducted but power-up not available; double deduction; power-up activates without deduction. |
| 7 | Claim daily reward (simulate new UTC day via Remote Config override or system clock advance). Close and reopen app. | Daily reward not claimable again on same UTC day; streak incremented. | Reward claimable twice; streak resets. |
| 8 | Enable airplane mode. Play Level 1. Complete it. | Level completes; result saved locally; no crash on Firestore write attempt. | Crash; progress lost; infinite loading spinner. |
| 9 | Re-enable network. Reopen app. | Local save syncs to Firestore; no data loss; no duplicate coins. | Duplicate reward; local progress overwritten by stale cloud data; merge conflict crash. |
| 10 | Open Firebase Analytics DebugView (or logcat tag FA). Complete Level 1. | `level_start`, `level_complete` events visible with `level_id`, `world_id`, `stars`, `coins_earned`, `duration_ms` parameters. | Events missing; events fire with null or zero parameters; events fire multiple times. |
| 11 | In Firebase Remote Config console (or test override), set `coin_multiplier` = 2. Force fetch in app. Complete Level 1. | Coin reward is exactly 2× the base value defined in LevelData. | Multiplier not applied; multiplier applied to wrong value; multiplier applied more than once. |

---

## 7. Test Environment

| Parameter | Value |
|-----------|-------|
| Unity version | 6000.0.47f1 (Unity 6 LTS) |
| Scripting backend | IL2CPP |
| Architecture | ARM64 (release) |
| Target API level | Android API 33 (Android 13) |
| Minimum API level | Android API 24 (Android 7.0) |
| Build type | Release build (not Development Build) |
| Scripting defines (QA environment) | `FIREBASE_ENABLED; KING_SMASH_DEV` |
| Scripting defines (RC candidate) | `FIREBASE_ENABLED; KING_SMASH_STAGING` |
| Firebase project (QA) | king-smash-dev (separate from production) |
| Firebase project (RC) | king-smash-staging |
| CI platform | Unity Cloud Build / GitHub Actions (Unity Test Runner) |
| Test framework | Unity Test Framework 1.4.x (NUnit 3.x) |
| EditMode test assembly | KingSmash.Tests.EditMode |
| PlayMode test assembly | KingSmash.Tests.PlayMode |
| Analytics debug mode | `adb shell setprop debug.firebase.analytics.app com.kingsmash.game` |
| Remote Config fetch interval override | 0 seconds (MinimumFetchInterval = 0 in QA build) |

### 7.1 Build Flavour Configuration

```
QA Build:
  - Development Build: OFF
  - Scripting Define: FIREBASE_ENABLED;KING_SMASH_DEV
  - Firebase project: king-smash-dev
  - Remote Config fetch interval: 0s
  - Crashlytics: enabled, debug symbols uploaded

RC Candidate Build:
  - Development Build: OFF
  - Scripting Define: FIREBASE_ENABLED;KING_SMASH_STAGING
  - Firebase project: king-smash-staging
  - Remote Config fetch interval: 43200s (12h, production default)
  - Crashlytics: enabled, debug symbols uploaded
  - AAB format: enabled for Play Store upload
```

---

## 8. Release Gate Checklist

All items must be checked and signed off before the build proceeds to soft launch. Items marked **BLOCKING** must be resolved; items marked **NON-BLOCKING** must be documented.

### 8.1 Authentication & Auth Persistence

- [ ] **[BLOCKING]** Cold launch on clean install completes anonymous Firebase Auth within 5 seconds.
- [ ] **[BLOCKING]** Anonymous UID persisted across app close/reopen without re-authentication.
- [ ] **[BLOCKING]** Google Sign-In links anonymous account; prior progress is retained.
- [ ] **[BLOCKING]** Auth failure (network offline at launch) handled gracefully — local play available.

### 8.2 Onboarding & Tutorial

- [ ] **[BLOCKING]** First-run onboarding completes without skip-able progression block.
- [ ] **[BLOCKING]** Onboarding does not replay on second launch.
- [ ] **[NON-BLOCKING]** Tutorial highlight arrows render correctly on all tested aspect ratios.

### 8.3 Progression & Level Unlock

- [ ] **[BLOCKING]** Level 1 unlocked after onboarding; Level 2 unlocked after Level 1 completion.
- [ ] **[BLOCKING]** World transition (Level 19→20 boss→World 2) completes and persists.
- [ ] **[BLOCKING]** All 100 levels are accessible given correct completion prerequisites (verified by save injection test).
- [ ] **[BLOCKING]** Stars (1–3) awarded correctly based on level targets; no over-award or under-award.

### 8.4 Economy Integrity

- [ ] **[BLOCKING]** Coins awarded from level completion match LevelData.coinReward × Remote Config coin_multiplier.
- [ ] **[BLOCKING]** Gems awarded from level completion match LevelData.gemReward.
- [ ] **[BLOCKING]** XP awarded correctly; level-up triggers at correct XP thresholds.
- [ ] **[BLOCKING]** No duplicate reward on level retry after failure.
- [ ] **[BLOCKING]** No duplicate reward on offline→online sync.
- [ ] **[BLOCKING]** Power-up purchase deducts correct coin/gem cost; inventory incremented by exactly 1.
- [ ] **[BLOCKING]** Power-up use in level decrements inventory by 1; no double-use within single level.

### 8.5 IAP / Billing

- [ ] **[BLOCKING]** Google Play Billing client initialises without crash (emulator).
- [ ] **[BLOCKING]** All IAP product IDs resolve via `BillingClient.queryProductDetailsAsync`.
- [ ] **[BLOCKING — Physical Device Required]** IAP sandbox purchase completes; gems credited exactly once.
- [ ] **[BLOCKING — Physical Device Required]** IAP purchase receipt validated server-side via Cloud Run `/validate-purchase`; no reward without valid receipt.
- [ ] **[BLOCKING — Physical Device Required]** Duplicate purchase attempt (re-send receipt) does not grant double gems.

### 8.6 Ads

- [ ] **[BLOCKING]** Rewarded ad unit initialises without crash (test ad ID in QA build).
- [ ] **[BLOCKING]** Rewarded ad reward callback fires exactly once after full ad view; coins credited once.
- [ ] **[BLOCKING]** Skipping rewarded ad before completion does not grant reward.
- [ ] **[BLOCKING]** Interstitial ad shown at correct intervals (defined by Remote Config interstitial_interval); no crash.
- [ ] **[NON-BLOCKING]** Ad frequency capping (interstitial_cooldown_seconds) enforced correctly.

### 8.7 Cloud Save

- [ ] **[BLOCKING]** Firestore write after level completion completes within 5 seconds on good network.
- [ ] **[BLOCKING]** Firestore read on app launch loads correct player state.
- [ ] **[BLOCKING]** Conflict resolution (server timestamp wins for completed levels; local wins for higher star count) verified.
- [ ] **[BLOCKING]** Offline play writes to local cache; sync on reconnect does not lose progress.
- [ ] **[BLOCKING]** Cloud save migration (version field mismatch) does not crash or reset progress.

### 8.8 Remote Config

- [ ] **[BLOCKING]** RemoteConfigManager fetches all 25+ keys on app start.
- [ ] **[BLOCKING]** Default values used correctly when fetch fails or times out.
- [ ] **[BLOCKING]** `coin_multiplier` override applies to level reward calculation.
- [ ] **[BLOCKING]** `daily_reward_enabled = false` disables the daily reward UI and claim path.
- [ ] **[BLOCKING]** `feature_flag_megaking = false` disables Mega King power-up in shop and level select.

### 8.9 Analytics

- [ ] **[BLOCKING]** `level_start` fires with `level_id`, `world_id` parameters.
- [ ] **[BLOCKING]** `level_complete` fires with `level_id`, `stars`, `coins_earned`, `duration_ms`, `attempts`.
- [ ] **[BLOCKING]** `purchase` event fires with `item_id`, `currency`, `value` on IAP completion.
- [ ] **[BLOCKING]** `ad_reward_claimed` fires with `ad_unit`, `reward_type`, `reward_amount`.
- [ ] **[NON-BLOCKING]** All M14 Tier-2 events fire with correct parameter types.

### 8.10 Crashlytics

- [ ] **[BLOCKING]** Debug symbols uploaded to Crashlytics for the RC candidate build.
- [ ] **[BLOCKING]** `Crashlytics.RecordException` for non-fatal errors visible in Firebase console.
- [ ] **[BLOCKING]** Crash-free session rate ≥ 99.0% across automated test run.

### 8.11 Performance

- [ ] **[BLOCKING]** Level load time < 5 seconds on emulator (API 33).
- [ ] **[BLOCKING]** Gameplay memory < 500 MB on emulator (API 33, 4 GB allotted).
- [ ] **[BLOCKING]** No GC allocations > 5 KB/frame during 60-second gameplay session.
- [ ] **[NON-BLOCKING — Physical Device Required]** 60 fps sustained on mid-range physical device.

### 8.12 Retention Features

- [ ] **[BLOCKING]** Daily reward: correct reward for day streak; no duplicate grant on same UTC day.
- [ ] **[BLOCKING]** Missions: at least one mission visible at all times; completion grants correct reward.
- [ ] **[BLOCKING]** Achievements: progression counters update correctly; completion reward granted once.

---

## 9. Known Test Limitations

The following limitations apply to this M15 automated/emulator-only QA pass. Each item notes the risk level and mitigation plan.

| Limitation | Affected Categories | Risk Level | Mitigation |
|-----------|--------------------|-----------:|------------|
| No physical device testing | Performance (absolute values), Audio, Haptics, Thermal throttling, 120Hz display, Notch/punch-hole visual | HIGH | Schedule physical device testing on at least 3 devices (low/mid/high) before worldwide launch. |
| No real AdMob ad delivery | Ads (real fill rate, ad format render) | HIGH | Use AdMob test ad IDs. Real ad delivery must be verified on physical device before launch. |
| No Google Play Billing sandbox on real device | IAP purchase flow, receipt validation | HIGH | IAP gate items marked "Physical Device Required". Test on physical device with Play-signed APK before launch. |
| Emulator does not simulate thermal throttling | Sustained-play performance, battery drain | MEDIUM | Physical device test over 20-minute session required. |
| Emulator does not simulate incoming call | Background/Resume category | MEDIUM | Test on physical device. |
| Emulator does not simulate push notifications | Retention re-engagement notifications | MEDIUM | Test local notification trigger via ADB on physical device. |
| Emulator audio output differs from device speaker | Audio mix, volume levels, spatial audio | LOW | Audio QA requires physical device with headphones and speaker comparison. |
| Firebase Remote Config fetch in emulator may be slow | Remote Config latency | LOW | Override MinimumFetchInterval = 0 in QA build; not representative of production fetch latency. |
| Google Sign-In not testable on emulator without Google Play Services configuration | Auth category #3 | MEDIUM | Test Google Sign-In on physical device with Play Services installed. |

---

## 10. Bug Report Template

Use this template for all defects found during M15 QA. File in the project tracker (e.g., Jira / GitHub Issues) under the `M15-QA` label.

```
## Bug Report — King Smash M15

**Title:** [One-line description — Component: Short description of wrong behaviour]

---

### Metadata

| Field | Value |
|-------|-------|
| Bug ID | BUG-XXXX |
| Severity | P0 / P1 / P2 / P3 |
| Category | [from Section 3 category list, e.g., "Economy — Coins & XP"] |
| Reporter | [Name / QA Engineer] |
| Date Found | YYYY-MM-DD |
| Build Version | [e.g., 1.0.0-rc3 (build 47)] |
| Environment | QA / Staging / Production |
| Firebase Project | king-smash-dev / king-smash-staging |
| Status | Open / In Progress / Fixed / Verified / Closed |
| Assigned To | [Developer name] |
| Target Fix Build | [build number or milestone] |

---

### Device / Platform

| Field | Value |
|-------|-------|
| Device Type | Physical / Emulator |
| Device Name | [e.g., Samsung Galaxy A52 / Android Emulator API 33] |
| OS Version | [e.g., Android 12 (API 32)] |
| RAM | [e.g., 6 GB] |
| Screen Resolution | [e.g., 1080 × 2400] |
| Unity Version | 6000.0.47f1 |
| Build Type | Debug / Release |
| Scripting Defines | [e.g., FIREBASE_ENABLED;KING_SMASH_DEV] |

---

### Summary

[2–3 sentence plain-language description of the defect. What is wrong, when does it happen, what is the impact.]

---

### Steps to Reproduce

1. [First step — be specific about taps, values, timing]
2. [Second step]
3. [Continue as needed]

**Minimum Reproduction Rate:** [e.g., 10/10, 3/10, 1/10 — intermittent]

---

### Expected Result

[What should happen, with reference to design doc, GDD, or spec section if applicable]

---

### Actual Result

[What actually happens. Include exact error message, wrong value, or crash signature.]

---

### Attachments

- [ ] Screenshot / Screen recording attached
- [ ] Unity Profiler snapshot attached
- [ ] Logcat / ADB log attached
- [ ] Firebase Crashlytics crash ID: [ID if available]
- [ ] Firebase Analytics DebugView screenshot attached

---

### Additional Context

**Regression:** [Is this a regression from a prior build? If yes, last known-good build.]
**Workaround:** [Is there a workaround? Describe if yes.]
**Related Bugs:** [BUG-XXXX, BUG-YYYY]
**Notes:** [Any additional context for the developer.]
```

---

*Document version: M15-QA-STRATEGY-v1.0*
*Last updated: 2026-10-04*
*Owner: QA Lead — King Smash*
