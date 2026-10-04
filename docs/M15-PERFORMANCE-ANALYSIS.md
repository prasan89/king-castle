# King Smash M15 — Performance Analysis

## Overview

This document captures the static performance analysis for King Smash M15.  All
findings are derived from code review of the M0–M14 implementation.  No physical
device profiling data is available for this pass; all metrics marked
**NOT TESTED** require validation on target Android hardware.

---

## Methodology

| Item | Detail |
|---|---|
| Analysis method | Static code review + hot-path identification |
| Profiling tool | Unity Profiler (Editor), Xcode Instruments / Android Profiler (device — not yet run) |
| Testing status | **AUTOMATED ANALYSIS — no physical device profiling data** |
| Target platform | Android (primary), iOS (secondary) |
| Unity version | Unity 6 LTS |
| Target device tier | Low-end Android (2 GB RAM, Snapdragon 4xx) |

---

## Hot-Path Analysis

The following table lists all paths that execute every frame or on every
physics/collision event.  "Allocations" refers to managed GC allocations.

| Path | Component | Allocations | GC Risk | Status |
|---|---|---|---|---|
| `FixedUpdate` physics step | Rigidbody2D / BoxCollider2D | Zero (engine-internal) | None | OK |
| `DestructibleObject.TakeDamage` | DestructibleObject | Zero — no LINQ, no string format | Low | OK |
| `KingProjectile.OnCollisionEnter2D` | KingProjectile | Zero — static event invocation | Low | OK |
| `VFXService.PlayEffect` | VFXService | `Queue<T>.Dequeue` — boxed struct avoided | Minimal | OK |
| `AudioService.Play(SoundId)` | AudioService | Array index lookup, enum cast | Zero | OK |
| `GameLogger.Debug` in hot paths | GameLogger | String interpolation allocates | **DEV ONLY** | Suppressed in release (Warning+) |
| `CastleBlock.HandleDestroyed` | CastleBlock | Event subscription check, no lambda capture per frame | Zero | OK |
| `UIAnimationController` coroutines | UIAnimationController | `yield return new WaitForSeconds(t)` — one alloc per call | Minor | Acceptable (not per-frame) |
| `DestructibleObject.OnDamaged?.Invoke` | DestructibleObject | Delegate is cached — no alloc | Zero | OK |
| `DebrisPool.Spawn()` | DebrisPool | Pool dequeue path — no `Instantiate` | Minimal | OK |
| `LaunchController.OnKingLaunched` event | LaunchController | Static event — no alloc | Zero | OK |
| `ServiceLocator.Get<T>()` | ServiceLocator | `Dictionary<Type,object>` lookup — no per-frame alloc | Zero | OK |

---

## GC Allocation Hotspots

The following are the known managed allocation sources identified through static
analysis, ranked by estimated impact.

### 1 — GameLogger string formatting (ELIMINATED in release)

`GameLogger.Debug` and `GameLogger.Info` use C# string interpolation
(`$"..."`) which allocates a new string on every call.  These are called in
several hot paths during development builds.

**Mitigation (already applied):**
`GameLogger.Initialize(LogLevel.Warning)` is called in `GameBootstrap` for
non-development builds.  All `Debug`/`Info` log calls short-circuit before
string construction because `_minLevel > LogLevel.Debug`.  Zero allocation in
release.

**Remaining risk:** Any `Warning` or `Error` log called per-frame still
allocates.  Audit these calls if they appear on the profiler timeline.

### 2 — Physics callback overhead (Unity-internal, no mitigation needed)

`OnCollisionEnter2D` and `OnTriggerEnter2D` are invoked by the engine.  Unity
6's physics system does not allocate managed objects for the `Collision2D`
parameter on the hot path; the struct is reused internally.  No action required.

### 3 — Coroutine allocations in UIAnimationController

Each `StartCoroutine` call allocates approximately 16–32 bytes for the
state-machine enumerator and the `Coroutine` reference.  `UIAnimationController`
starts coroutines only on user-triggered transitions (button tap, scene change),
not per-frame.  This is acceptable; a coroutine-free alternative (DOTween
sequences) is listed as a P3 recommendation below.

### 4 — `WaitForSeconds` in screen-transition coroutines

`ScreenTransitionService.FadeToBlack` creates `new WaitForSeconds(duration)` on
each call.  This allocates a small managed object.  A cached
`WaitForSeconds` instance would eliminate this.  See P3 recommendations.

### 5 — Event unsubscription in `OnDisable`

Delegate comparison for `-=` unsubscription allocates a temporary delegate
wrapper in some Unity versions.  This occurs only on `OnDisable`, which is
infrequent and off the hot path.  No action required.

---

## Object Pool Analysis

| System | Pool Type | Pre-warmed | Zero-alloc on Play | Status |
|---|---|---|---|---|
| VFXService | `Queue<GameObject>` per `VFXId` | Yes — `PreWarm(count)` | Yes (dequeue/enqueue) | Correct |
| AudioService | Fixed `AudioSource[8]` array | Yes — created in `Awake` | Yes (array index) | Correct |
| DebrisPool | `Queue<GameObject>` | Yes | Yes (dequeue/enqueue) | Correct |
| Projectile | Instantiated per launch | No pool | Minor (one launch per round) | Acceptable |

**Assessment:** The three highest-frequency spawn systems (VFX, audio, debris)
are correctly pooled.  Projectile instantiation is acceptable given its
infrequency (once per user action).

---

## Startup Performance

The following is the critical initialisation sequence inferred from
`GameBootstrap.cs` and related services.  Times are **estimated**, not measured.

| Step | Service | Estimated Cost | Async? |
|---|---|---|---|
| 1 | `GameBootstrap.Awake → Initialize()` | <1 ms (ServiceLocator dict fill) | No |
| 2 | `LocalSaveService.Load()` | ~5–20 ms (JSON deserialization, disk read) | No |
| 3 | `IConfigService.Initialize()` | ~200–800 ms (RemoteConfig network fetch) | Yes — non-blocking |
| 4 | `DailyRewardService.CheckAndResetIfNewDay()` | <1 ms (date comparison) | No |
| 5 | `SceneLoader.LoadScene(MainMenu)` | ~100–300 ms (async scene load) | Yes |
| **Total (blocking path)** | | **~6–25 ms synchronous**, remainder async | |

**Budget:** `PerformanceBudget.StartupBudgetMs = 5000` ms (cold launch to main
menu visible).

**Risk areas:**
- `LocalSaveService.Load()` is synchronous.  On low-end devices with slow flash
  storage, JSON parse of a large save file can exceed 50 ms and cause a visible
  hitch.  Consider moving to an async load with a loading spinner.
- RemoteConfig fetch failure must not block the main thread; verify timeout and
  fallback are implemented.

---

## Scene Load Performance

| Step | Estimated Cost | Budget |
|---|---|---|
| Async scene load (Unity) | 50–200 ms | — |
| `LevelSceneInitializer.Initialize()` | 10–40 ms (DestructibleObject setup) | — |
| `VFXService.PreWarm()` during level init | 5–15 ms | — |
| **Total** | **65–255 ms** | **3000 ms** (`SceneLoadBudgetMs`) |

The level load path is well within budget on mid-range devices.  On very
low-end devices with many destructibles, `LevelSceneInitializer` should stagger
`ResetHealth` calls across frames (see P2 recommendation).

---

## Performance Recommendations

The following improvements are identified from static analysis and ranked by
priority.

| Priority | Area | Recommendation | Expected Impact |
|---|---|---|---|
| P1 | Logging | Audit all `Warning`/`Error` log calls for per-frame occurrence; move any found onto `#if` guards or rate-limit them | Eliminates potential per-frame string alloc in release builds |
| P2 | Level init | Stagger `DestructibleObject.ResetHealth()` calls on level restart across multiple frames using a coroutine or `IEnumerator` job | Eliminates frame spike on large level resets |
| P2 | UI panels | Apply object pooling or `SetActive(false)` caching for frequently-toggled UI panels (combo counter, power-up HUD) | Avoids repeated `Instantiate`/`Destroy` GC pressure |
| P3 | Coroutines | Cache `WaitForSeconds` instances in `ScreenTransitionService` as `private static readonly` fields | Eliminates 1 small alloc per screen transition |
| P3 | Frame rate cap | Set `Application.targetFrameRate = 60` on mid-range, `30` on low-end devices (detect via `SystemInfo.systemMemorySize`) | Reduces battery drain and thermal pressure |
| P3 | Coroutine → DOTween | Replace `UIAnimationController` coroutine fades with DOTween cached sequences | Reduces coroutine state-machine alloc; smoother animation |
| P4 | Physics sleep | Call `Rigidbody2D.Sleep()` on debris bodies that have been stationary for >2 s | Reduces active physics body count below `MaxActivePhysicsBodies` budget |

---

## Performance Budget Reference

The following table mirrors the constants defined in `PerformanceBudget.cs`.

| Metric | Budget | Notes |
|---|---|---|
| Target frame time | 16.67 ms | 60 fps |
| Low-end frame time | 33.33 ms | 30 fps |
| Max destructibles per frame | 5 | Stagger large collapses |
| Max debris particles active | 50 | VFXService soft cap |
| Max active physics bodies | 100 | Rigidbody2D instances |
| Max audio sources active | 8 | Matches AudioService pool |
| Startup budget | 5,000 ms | Cold launch → main menu |
| Scene load budget | 3,000 ms | Tap Play → gameplay interactive |
| Max managed heap | 256 MB | Mono/GC soft limit |

---

## Missing Data — Requires Physical Device

The following metrics **cannot** be determined from static analysis alone.
They must be collected via Unity Profiler attached to a physical Android device
or via Android GPU Inspector / Snapdragon Profiler.

| Metric | Status |
|---|---|
| Real FPS on low-end Android (Snapdragon 4xx, 2 GB RAM) | NOT TESTED |
| Real FPS on mid-range Android (Snapdragon 7xx, 4 GB RAM) | NOT TESTED |
| Memory profiler snapshot during large castle cascade (10+ blocks) | NOT TESTED |
| Managed heap size at peak gameplay | NOT TESTED |
| Thermal throttling behaviour during extended session (>10 min) | NOT TESTED |
| GPU frame timing during VFX-heavy levels (max particle count) | NOT TESTED |
| Actual cold-launch time (tap icon → main menu visible) | NOT TESTED |
| Actual level-load time (tap Play → first physics frame) | NOT TESTED |
| Battery drain rate during 30-minute session | NOT TESTED |
| GC.Collect frequency under sustained gameplay | NOT TESTED |

**All physical device metrics are: NOT TESTED**
No physical Android device was available for the M15 performance pass.
Recommendations above are based solely on static code analysis.

---

## Monitoring Components (M15)

Three new dev-only MonoBehaviours are provided to assist with future profiling
sessions on physical devices.  All are compiled only under
`UNITY_EDITOR || KING_SMASH_DEV`.

| Component | Purpose | Key Threshold |
|---|---|---|
| `GCAllocLogger` | Logs per-frame GC allocation spikes | Default: 1 KB/frame |
| `FrameTimingMonitor` | Rolling average frame-time warnings/errors | Warning: 33 ms, Error: 66 ms |
| `MemoryWatchdog` | Periodic managed-heap and total-memory checks | Warning: 256 MB heap, Error: 600 MB total |

Attach all three to a `DebugTools` GameObject in the dev scene, or instantiate
via `GameBootstrap` under the `KING_SMASH_DEV` define.

---

*Document generated: M15 static analysis pass — King Smash, Unity 6 LTS, Android.*
