# King Smash M16 — RC Iteration Process

This document governs how builds progress from RC-tagged commits to the Closed Alpha
testing track on Google Play. Follow this process for every build promoted beyond the
Internal testing track.

---

## 1. Build Versioning

All soft-launch builds follow Semantic Versioning with a build-type suffix.

| Build ID   | Version Name    | Version Code | Date       | Changes                                              | Status         |
|------------|-----------------|-------------|------------|------------------------------------------------------|----------------|
| rc-1       | 1.0.0-rc1       | 100001      | —          | M15 release candidate; code complete, no device soak | Superseded     |
| sl-1       | 1.0.0-sl1       | 100010      | —          | Soft-launch build: BUG-001/002 fixed, PATCH-001 (L9) applied, FIREBASE_ENABLED=production | Active |
| sl-1p1     | 1.0.0-sl1p1     | 100011      | —          | Reserved — patch build if P0/P1 found in soft launch  | Pending        |

**Version code rules:**
- Increment version code by 1 for every new upload to Play Console.
- Version code must never be reused, even after a rejected build.
- The `versionCode` field in `build.gradle` is the single source of truth; do not set it in Unity's Player Settings manually.

---

## 2. Change Log Template

For every build uploaded to the Play Console, record the following in
`docs/CHANGELOG.md` before tagging:

```
## [version-name] — version-code — YYYY-MM-DD

### Changes
- …

### Bug Fixes
- …

### Known Issues
- …

### Testing Notes
- Build tested on: [device list]
- Min Android API: 24
- Target Android API: 35
- Firebase project: king-smash-prod / king-smash-staging
```

---

## 3. P0 Hotfix Process

**Definition:** P0 = crash or data-loss bug affecting more than one player.

**Target SLA:** fix shipped within 24 hours of confirmation.

1. Confirm the crash in Firebase Crashlytics:
   - At least 3 distinct reports on the affected version code.
   - Issue is reproducible on at least one test device.
2. Create a hotfix branch:
   ```
   git checkout -b hotfix/sl1-p0-<issue-id>
   ```
3. Implement the fix. Every P0 fix **must** be accompanied by a unit test or
   integration test that reproduces the failure condition.
4. Build an Internal track APK (Unity → Build → Android → Internal).
   - Do not increment the public version name; use the same name with an
     incremented version code (`sl1p1`, `sl1p2`, etc.).
5. Distribute to the Internal testing team (minimum 5 devices, covering at
   least 2 distinct Android API levels).
6. **24-hour soak period.** Monitor Crashlytics for:
   - Recurrence of the original issue.
   - New regressions introduced by the fix.
7. If the soak is clean, promote to Closed Alpha by uploading the same APK
   to the Closed Alpha track with the same version code.
8. Monitor Crashlytics for a further 24-hour window post-promotion.
9. Document the incident:
   ```
   ## P0-<issue-id> — [title]
   - Reported: [date]
   - Confirmed: [date] — [number] reports, version code [x]
   - Root cause: …
   - Fix: …
   - Verification: unit test added in [TestClassName], soak on [devices]
   - Closed: [date]
   ```

---

## 4. P1 Fix Process

**Definition:** P1 = significant feature regression or user-visible bug without data loss.

**Target SLA:** fix shipped within 72 hours of confirmation.

The process mirrors P0 with the following differences:
- Branch prefix: `fix/sl1-p1-<issue-id>` (no "hotfix" prefix).
- Soak period: **48 hours** minimum (vs. 24 for P0).
- Promotion target: Closed Alpha on the next scheduled build window (not an
  emergency promotion).
- Documentation in `docs/CHANGELOG.md` under the next patch version entry.

---

## 5. Remote Config Tuning Process

Remote Config changes take effect without a new build. Use this process for any
numeric parameter change in Firebase Remote Config (difficulty multipliers, ad
frequency caps, economy values, etc.).

**No code deploy required.** Changes are fetched by the running game within the
active fetch interval (default: 12 hours in production, 1 minute in dev/staging).

### Step-by-step

1. **Identify the metric** indicating imbalance. Example:
   - L9 30-day completion rate below 40% in BigQuery.
   - D1 retention below target after world-1 paywall.

2. **Define the change precisely.** Example:
   ```
   Parameter:    level_9_destruction_multiplier
   Old value:    1.0
   New value:    1.1
   Rationale:    L9 completion at 38% (target >= 55%); +10% destruction bonus
                 should close the gap without making the level trivial.
   ```

3. **Document the change** in `docs/M16-REMOTE-CONFIG-PROFILE.md` under the
   active environment section before applying it.

4. **Apply the change** in the Firebase Remote Config console:
   - Open the correct project (king-smash-prod for Closed Alpha).
   - Edit the parameter value.
   - Publish changes.

5. **Increment `config_version`** to the next integer in the same publish
   operation. The game reads this value on fetch and logs it to analytics,
   allowing before/after cohort analysis.

6. **Monitor the affected metric** for at least 48 hours:
   - Pull BigQuery dashboard (or Looker Studio report) every 12 hours.
   - If the metric moves in the wrong direction, revert immediately (step 4
     in reverse).

7. **Document the outcome:**
   ```
   ## RC Change — config_version N -> N+1 — YYYY-MM-DD
   - Parameter:   level_9_destruction_multiplier
   - Old -> New:  1.0 -> 1.1
   - Hypothesis:  completion rate will increase by >= 5 pp within 48h
   - Outcome:     completion rate moved from 38% -> 52% (day 2 sample: 214 sessions)
   - Action:      keep
   ```

---

## 6. Build Distribution

- All soft-launch builds are distributed **exclusively** via the
  **Google Play Closed Testing (Closed Alpha)** track.
- **No direct APK distribution.** Side-loading bypasses Play Protect and
  invalidates crash attribution in Crashlytics.
- **No TestFlight.** iOS is not in scope for M16 soft launch.
- Internal team builds (for soak tests) use the **Internal testing** track.
- QA devices must be added to the Closed Alpha tester group before a build
  is promoted.

---

## 7. Build Promotion Gate

Before promoting any build from Internal to Closed Alpha, verify:

- [ ] `docs/M16-FINAL-REGRESSION-CHECKLIST.md` is complete with no open blockers.
- [ ] Crashlytics shows zero new crash-free sessions below 99% for the Internal
      soak build over the 24-hour soak window.
- [ ] All PATCH entries in `docs/M16-LEVEL-BALANCE-PATCH.md` are applied and
      verified on device.
- [ ] `FIREBASE_ENABLED` scripting define is set to the **production** project for
      Closed Alpha builds.
- [ ] Version code is unique and incrementally higher than the previous Closed
      Alpha upload.
- [ ] Change log entry is written and committed to `docs/CHANGELOG.md`.

---

## 8. Emergency Rollback

If a build causes a significant P0 regression in Closed Alpha and a hotfix cannot
be delivered within the SLA:

1. Open Play Console -> Closed Testing -> Halt rollout for the affected build.
2. Re-promote the previous clean build (same version code) to restore the track.
3. File the P0 issue per section 3 above.
4. Communicate the rollback to all Closed Alpha testers via the Play Console
   release notes mechanism.

> Note: Play Console does not support instant rollback by version code once a build
> reaches Closed Alpha at 100% rollout. Halting the rollout stops new installs but
> does not downgrade existing installs. Design hotfixes to be forward-compatible.
