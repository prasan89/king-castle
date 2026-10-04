# King Smash M15 — Device Matrix

---

## 1. Overview

This document defines the device matrix for King Smash M15 QA. It describes the target device coverage, the rationale for device and Android version selection, screen resolution and aspect ratio coverage, and the tested vs untested configuration status for this M15 pass.

King Smash targets Android API 24 (Android 7.0) as the minimum and Android API 33 (Android 13) as the primary target. The game is built with IL2CPP, ARM64, and Unity 6000.0.47f1.

**Important:** Physical device testing was NOT performed during this M15 pass. All Android testing was conducted using the Android Studio emulator. The coverage table in Section 6 specifies the exact status of each configuration.

---

## 2. Device Tier Definitions

| Tier | RAM | Typical SoC | Android Version | Screen | Notes |
|------|-----|-------------|----------------|--------|
| LOW-END | 2–3 GB | Snapdragon 460 / MediaTek Helio G85 | Android 9–10 (API 28–29) | 720 × 1520 – 720 × 1600 (HD+) | Budget segment; represents a significant portion of the addressable market in Southeast Asia, India, and Latin America. Performance floor for the game. |
| MID-RANGE | 4–6 GB | Snapdragon 695 / MediaTek Dimensity 700 | Android 11–12 (API 30–32) | 1080 × 2400 (FHD+) | Primary target class. 60fps target must be met here. Represents the median device across target soft-launch markets. |
| HIGH-END | 8–12 GB | Snapdragon 8 Gen 1 / Exynos 2200 | Android 13–14 (API 33–34) | 1080 × 2340 – 1440 × 3088 | Premium segment. Validates that the game does not waste resources or cap at 30fps on high-refresh hardware. |

---

## 3. Device Examples by Tier

### 3.1 Low-End Devices

| Device | RAM | SoC | Android | Screen Resolution | Aspect Ratio | Display Type | Nav Type |
|--------|-----|-----|---------|-------------------|--------------|--------------|----------|
| Samsung Galaxy A03 | 2 GB | MediaTek MT6739W | Android 11 | 720 × 1600 | 20:9 | V-notch | 3-button |
| Motorola Moto E40 | 4 GB | MediaTek Helio G35 | Android 11 | 720 × 1600 | 20:9 | V-notch | 3-button |
| Xiaomi Redmi 9A | 2 GB | MediaTek Helio G25 | Android 10 | 720 × 1520 | 19:9 | Dewdrop notch | 3-button |
| Samsung Galaxy A02 | 3 GB | Snapdragon 450 | Android 10 | 720 × 1600 | 20:9 | Infinity-V notch | 3-button |
| Motorola Moto G9 Play | 4 GB | Snapdragon 662 | Android 10 | 720 × 1600 | 20:9 | Waterdrop notch | 3-button |

**Low-end testing rationale:** These devices have the most constrained memory and CPU. They define the game's performance floor. If the game crashes or drops below 30fps consistently on these, it must be investigated before launch.

### 3.2 Mid-Range Devices

| Device | RAM | SoC | Android | Screen Resolution | Aspect Ratio | Display Type | Nav Type |
|--------|-----|-----|---------|-------------------|--------------|--------------|----------|
| Samsung Galaxy A52 | 6 GB | Snapdragon 720G | Android 11 | 1080 × 2400 | 20:9 | Infinity-O (centered punch-hole) | Gesture / 3-button |
| Samsung Galaxy A53 | 6 GB | Exynos 1280 | Android 12 | 1080 × 2400 | 20:9 | Infinity-O (centered punch-hole) | Gesture |
| Google Pixel 4a | 6 GB | Snapdragon 730G | Android 12 | 1080 × 2340 | 19.5:9 | Corner punch-hole (top-right) | Gesture |
| Xiaomi Redmi Note 11 | 4 GB | Snapdragon 680 | Android 11 | 1080 × 2400 | 20:9 | Centered punch-hole | 3-button |
| OnePlus Nord CE 2 | 8 GB | Dimensity 900 | Android 12 | 1080 × 2400 | 20:9 | Centered punch-hole | Gesture / 3-button |
| Realme 9 Pro+ | 6 GB | Dimensity 920 | Android 12 | 1080 × 2400 | 20:9 | Centered punch-hole | Gesture |

**Mid-range testing rationale:** The Galaxy A52/A53 and Pixel 4a are the reference devices for 60fps validation. They represent the median hardware profile for Southeast Asian and European launch territories.

### 3.3 High-End Devices

| Device | RAM | SoC | Android | Screen Resolution | Aspect Ratio | Display Type | Nav Type |
|--------|-----|-----|---------|-------------------|--------------|--------------|----------|
| Samsung Galaxy S22 | 8 GB | Exynos 2200 | Android 13 | 1080 × 2340 | 19.5:9 | Centered punch-hole; 120Hz | Gesture |
| Samsung Galaxy S22 Ultra | 12 GB | Snapdragon 8 Gen 1 | Android 13 | 1440 × 3088 | 19.3:9 | Centered punch-hole; 120Hz | Gesture |
| Google Pixel 7 | 8 GB | Google Tensor G2 | Android 13 | 1080 × 2400 | 20:9 | Centered punch-hole; 90Hz | Gesture |
| OnePlus 10 Pro | 12 GB | Snapdragon 8 Gen 1 | Android 13 | 1440 × 3216 | 20.1:9 | Centered punch-hole; 120Hz | Gesture |
| Samsung Galaxy Z Fold 4 (inner) | 12 GB | Snapdragon 8+ Gen 1 | Android 12 | 2176 × 1812 | ~6:5 near-square | Crease visible; no notch | Gesture |

**High-end testing rationale:** These devices validate that the game does not waste battery or produce heat issues, and that the IL2CPP build does not inadvertently cap frame rate at 30fps via `Application.targetFrameRate`.

---

## 4. Android Version Coverage Rationale

### 4.1 Market Share by Android Version (approximate, 2026)

| Android Version | API Level | Approx. Market Share | Included |
|----------------|-----------|----------------------|----------|
| Android 7.0–7.1 | 24–25 | < 2% | Min SDK (crash-gate only) |
| Android 8.0–8.1 | 26–27 | < 3% | Min SDK (crash-gate only) |
| Android 9 | 28 | ~6% | Low-end tier |
| Android 10 | 29 | ~12% | Low-end tier |
| Android 11 | 30 | ~16% | Mid-range primary |
| Android 12 | 31–32 | ~18% | Mid-range primary |
| Android 13 | 33 | ~22% | High-end primary; **target API** |
| Android 14 | 34 | ~14% | High-end secondary |
| Android 15 | 35 | ~7% | Future; not in scope for M15 |

### 4.2 Android Version-Specific Behavioural Notes

| Android Version | Key Behaviour Change | Impact on King Smash |
|----------------|---------------------|---------------------|
| Android 9 (API 28) | Cleartext HTTP blocked by default | All network calls must use HTTPS. Firebase SDK complies. |
| Android 10 (API 29) | Scoped storage; background location restricted | No impact (no file system access; no location). |
| Android 11 (API 30) | Package visibility restrictions; one-time permissions | No impact on current feature set. |
| Android 12 (API 31) | Splash screen API enforced (white screen replaced by system splash) | UnityActivity splash theme must set `windowSplashScreenBackground`. |
| Android 12 (API 32) | Notification permission not yet required (added in API 33) | No impact. |
| Android 13 (API 33) | `POST_NOTIFICATIONS` permission required at runtime | Firebase Cloud Messaging (if used for re-engagement) must request permission. |
| Android 14 (API 34) | Partial photo picker; background activity launch restrictions | No impact for M15 scope. |

---

## 5. Screen Resolution & Aspect Ratio Coverage

### 5.1 Aspect Ratio Matrix

| Aspect Ratio | Example Resolution | Device Class | Notes |
|-------------|-------------------|--------------|-------|
| 16:9 | 1920 × 1080 | Legacy / Emulator default | Standard widescreen. Android Studio default emulator. UI must not assume this is the only ratio. |
| 18:9 | 1440 × 720 | Low-end 2018–2019 | Tall screen; bottom nav bar eats into 16:9 layout space. |
| 19:9 | 1520 × 720 | Low-end Xiaomi/Realme 2019–2020 | Dewdrop/teardrop notch common at this resolution. |
| 19.5:9 | 2340 × 1080 | Mid-range Google Pixel, Samsung S21 | Most common mid-range ratio 2020–2022. |
| 20:9 | 2400 × 1080 | Mid-range Samsung A series 2021+ | Most common mid-range ratio 2021+. Primary target. |
| 20.1:9 | 3216 × 1440 | High-end OnePlus 10 Pro | Very tall; HUD score label may appear low relative to centre. |
| 20.9:9 | 3088 × 1440 | High-end Samsung S22 Ultra | Extreme tall ratio; verify no UI element too close to edge. |
| ~6:5 (near-square) | 2176 × 1812 | Samsung Galaxy Z Fold 4 inner | Out of primary scope. Layout must not crash; P3 severity for visual oddities. |

### 5.2 SafeAreaHandler Coverage

`SafeAreaHandler.cs` reads `Screen.safeArea` at runtime and repositions the HUD canvas anchors. The following configurations exercise distinct safe area behaviours and must all be verified:

| Configuration | Safe Area Behaviour | Verification Approach |
|--------------|--------------------|-----------------------|
| No notch (standard 3-button nav) | Safe area = full screen minus bottom bar (48dp) | Emulator: default with 3-button nav |
| Waterdrop/teardrop notch (top-centre) | Safe area top inset matches notch height (~35dp) | Emulator: configure notch in AVD settings |
| Centered punch-hole (top-centre) | Small top inset (~30dp) for camera | Emulator: API 33 device with punch-hole emulation |
| Corner punch-hole (top-right) | Right/top inset applied; Pixel 4a style | Emulator: Pixel 4a AVD |
| Gesture navigation (no 3-button bar) | Bottom inset (~20dp) for gesture handle | Emulator: enable gesture navigation in Android settings |
| Gesture navigation + notch | Both top and bottom insets active | Emulator: Pixel 4a AVD in gesture mode |

**HUD elements that must respect SafeArea:**
- Score label (top-left)
- Lives/HP indicator (top-right)
- Power-up slots (bottom-left)
- Pause button (top-right)
- Coin/gem balance (top-centre or top-left)

---

## 6. Emulator vs Physical Test Status

### 6.1 Tested Configurations (M15 Pass)

| Configuration | Device Profile | Android API | Resolution | Nav Type | Test Status |
|--------------|----------------|-------------|-----------|----------|
| Android Studio Emulator — Primary | Pixel 7 AVD (x86_64) | API 33 | 1080 × 2400, 20:9 | Gesture | **EMULATOR TESTED** |
| Android Studio Emulator — Low-end | Generic Phone AVD (x86) | API 29 | 720 × 1520, 19:9 | 3-button | **EMULATOR TESTED** |
| Unity EditMode Tests | N/A (Unity Editor host) | N/A | N/A | N/A | **AUTOMATED TESTED** |
| Unity PlayMode Tests | N/A (Unity headless runner) | N/A | N/A | N/A | **AUTOMATED TESTED** |

### 6.2 Not Tested (Physical Device Required)

| Configuration | Reason Not Tested | Risk Level | Required Before |
|--------------|------------------|-----------|----------------|
| Samsung Galaxy A52 (physical) | No physical device available | HIGH | Worldwide launch |
| Samsung Galaxy A03 (physical, low-end) | No physical device available | HIGH | Worldwide launch |
| Samsung Galaxy S22 (physical, 120Hz) | No physical device available | MEDIUM | Worldwide launch |
| Google Pixel 4a (physical, corner punch-hole) | No physical device available | MEDIUM | Worldwide launch |
| Samsung Galaxy Z Fold 4 (physical, inner display) | No physical device available | LOW (P3 scope) | Post-launch patch |
| Any device with real AdMob ad delivery | Physical device + AdMob mediation required | HIGH | Soft launch |
| Any device for Google Play Billing sandbox | Play-signed APK on physical device required | HIGH | Soft launch |
| Device with 120Hz refresh rate | Emulator does not simulate refresh rate | MEDIUM | Worldwide launch |
| Device under thermal throttling | Emulator does not simulate heat | MEDIUM | Worldwide launch |
| Device with incoming call interruption | Requires telephony on physical device | MEDIUM | Worldwide launch |

### 6.3 Coverage Summary

| Coverage Dimension | Target | M15 Pass Actual | Gap |
|-------------------|--------|----------------|-----|
| Android version range | API 24–34 | API 29 (emulator) + API 33 (emulator) | API 24–28 and API 30–32, 34 untested |
| Device tiers covered | Low / Mid / High | Low (emulator) + Mid (emulator primary) | High-end physical untested |
| Aspect ratios tested | 16:9, 18:9, 19.5:9, 20:9, 20.9:9 | 19:9 (emulator) + 20:9 (emulator) | 18:9, 19.5:9, 20.9:9 untested |
| Notch/punch-hole types | Waterdrop, centre punch-hole, corner punch-hole, gesture nav | Centre punch-hole emulated (Pixel 7 AVD) | Waterdrop, corner punch-hole untested |
| Billing | Emulator init + real sandbox | Emulator BillingClient init only | Real sandbox on physical untested |
| AdMob real ads | Physical device | Test ad unit on emulator only | Real ad fill untested |

---

## 7. Target vs Actual Coverage for M15 Pass

### 7.1 What This Pass Validates

This M15 automated/emulator pass validates:

1. **Logic correctness** — all gameplay, economy, progression, and retention logic via Unity EditMode/PlayMode tests.
2. **Firebase integration** — Auth, Firestore, Remote Config, Analytics, and Crashlytics integration tests against the `king-smash-dev` Firebase project.
3. **Basic rendering** — level renders and UI is navigable on emulator at API 33 (FHD+) and API 29 (HD+).
4. **SafeAreaHandler basic function** — on the Pixel 7 AVD (centered punch-hole, gesture nav).
5. **Performance regression detection** — frame time and memory regressions detected by comparing against baseline on emulator.
6. **Billing client init** — BillingClient connects without crash on emulator.
7. **Ads test unit** — AdMob test ad unit loads and callbacks fire correctly.

### 7.2 What This Pass Does NOT Validate

The following must be tested on physical devices before soft launch or worldwide launch:

| Item | Required For |
|------|-------------|
| Absolute 60fps on mid-range device | Soft launch gate |
| Absolute memory under 350MB on real hardware | Soft launch gate |
| AdMob real ad fill, rendering, and reward callbacks | Soft launch gate |
| Google Play Billing sandbox purchase end-to-end | Soft launch gate |
| Receipt validation via Cloud Run `/validate-purchase` (live flow) | Soft launch gate |
| Thermal throttling performance (sustained 20-min session) | Worldwide launch gate |
| 120Hz display: correct vsync target and no over-rendered frames | Worldwide launch gate |
| Waterdrop notch SafeArea: HUD not clipped into notch area | Worldwide launch gate |
| Corner punch-hole (Pixel 4a): Settings/score button not behind camera | Worldwide launch gate |
| Near-square display (Fold inner): no layout crash | Post-launch patch |
| Incoming call interruption handling | Worldwide launch gate |
| Push notification delivery and deep-link re-engagement | Worldwide launch gate |

---

## 8. SafeAreaHandler.cs Verification Notes

The game ships `SafeAreaHandler.cs` which is applied to the HUD canvas. The following notes guide physical device verification when hardware becomes available:

- **What it does:** Reads `Screen.safeArea` (a `Rect` in screen-space pixels) at `Awake()` and `OnRectTransformDimensionsChange()`. Computes anchor offsets proportional to screen size and applies them to the canvas `RectTransform`. Intended to push HUD elements inward to avoid notch and navigation bar regions.
- **Failure mode 1:** If `Screen.safeArea` returns `(0, 0, screenWidth, screenHeight)` (no insets), the handler is a no-op — correct for devices with no cutout or nav bar.
- **Failure mode 2:** If the canvas is not in Screen Space — Camera mode, safe area adjustments may not match pixel-perfect positions. Verify canvas render mode.
- **Failure mode 3:** On foldable devices, `OnRectTransformDimensionsChange` must fire on fold/unfold. Verify HUD repositions when the inner display expands.
- **Verification checklist for physical device:**
  - [ ] Score label fully visible and not clipped by notch on Galaxy A03.
  - [ ] Pause button fully visible and not behind punch-hole on Galaxy A52.
  - [ ] Power-up slot row fully visible above gesture bar on Pixel 4a (gesture mode).
  - [ ] No HUD element clipped by corner punch-hole on Pixel 4a (top-right camera).
  - [ ] Fold/unfold on Galaxy Z Fold 4 does not freeze or mis-position HUD.

---

## 9. Gesture Navigation vs 3-Button Navigation Notes

Android devices may use either gesture navigation (swipe-based, introduced in Android 10) or the legacy 3-button navigation bar.

| Navigation Mode | Bottom Inset | Side Insets | Impact on King Smash |
|----------------|-------------|------------|----------------------|
| 3-button nav | ~48dp (always visible) | None | Bottom HUD must sit above the 48dp bar. `Screen.safeArea.y` will reflect this. |
| Gesture nav (pill) | ~20dp (gesture handle area) | None | Smaller inset; more vertical screen space. HUD sits lower; verify power-up row is still tappable. |
| Gesture nav (full immersive) | 0 (hidden until swipe) | None | Full screen available; game may be in immersive mode. Verify swipe-up does not trigger android home unexpectedly. |

**Recommendation:** For M16 (if implemented), request that the game run in `SYSTEM_UI_FLAG_IMMERSIVE_STICKY` mode to hide both status bar and nav bar during gameplay, with bars restored on pause screen. This maximises gameplay screen real estate on all devices.

---

*Document version: M15-DEVICE-MATRIX-v1.0*
*Last updated: 2026-10-04*
*Owner: QA Lead — King Smash*
