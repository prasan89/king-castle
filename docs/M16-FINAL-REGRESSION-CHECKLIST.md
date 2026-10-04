# King Smash M16 — Final Regression Checklist

Run this checklist before promoting any build to Closed Alpha.
All items in the **Critical Path** section must be checked off by a human tester
on a physical device. Do not substitute emulator results for Critical Path items.

**Build under test:** _______________
**Tester:** _______________
**Device:** _______________
**Android API:** _______________
**Date:** _______________

---

## Critical Path

Complete these steps end-to-end in a single session on a fresh install where noted.

- [ ] Fresh install on test device — uninstall any previous version first (no prior app data)
- [ ] Launch app — no crash on startup, loading screen completes, no stuck state
- [ ] Onboarding / tutorial completes without errors or UI overlap
- [ ] Home screen shows correctly: king art, currency, level progress, navigation tabs
- [ ] Level 1: start, aim, launch, destroy target, complete level
- [ ] Level 1: rewards shown (coins, stars), coin balance updates correctly
- [ ] Level 2: starts automatically after Level 1 reward screen OR via level select
- [ ] Level 3: completes — upgrade screen or upgrade prompt becomes accessible
- [ ] Upgrade screen: purchase one stat upgrade (confirm coins deducted, stat increases)
- [ ] Power-up screen: acquire one power-up (confirm inventory updates)
- [ ] Level 5: use a power-up in level — effect applies, inventory decrements
- [ ] Level 9: complete (PATCH-001 — verify difficulty is accessible, not excessively hard)
- [ ] Level 10: starts correctly — world progression continues from Level 9
- [ ] Level 19 (boss level): completes — World 2 (Desert) unlocks
- [ ] Level 20: starts correctly — Desert World scene loads without errors
- [ ] Fail a level intentionally: failure screen appears with correct attempt count
- [ ] Retry after failure: attempt counter updates, level restarts cleanly
- [ ] Rewarded ad offered on failure: accept offer — ad loads and plays
- [ ] Rewarded continue: after watching ad to completion, level continues (not restart) with same state
- [ ] Interstitial ad: fires after appropriate number of level completions — NOT during active gameplay
- [ ] Daily reward: claim day 1 reward — reward granted, streak counter shows 1
- [ ] Close app, reopen next day (or simulate time skip in dev build): daily reward is available again
- [ ] Mission: complete one active mission — claim reward, mission marked done
- [ ] Achievement: trigger one achievement condition — achievement notification appears
- [ ] Shop screen: open, browse items, close — no crash, prices display correctly
- [ ] Test IAP purchase (sandbox): select a product, complete payment flow, reward delivered to account
- [ ] Test IAP cancel: cancel purchase mid-flow — no reward granted, balance unchanged
- [ ] Cloud save: play 5 levels, force-quit app, reopen — progress is fully preserved
- [ ] Offline mode: enable airplane mode, play 2 levels, rewards show — no crash or hang
- [ ] Reconnect online: disable airplane mode — progress syncs to cloud within 60 seconds
- [ ] Google Sign-In (if enabled in build): link account, verify UID is preserved after re-login
- [ ] Settings screen: mute audio — verify silence; unmute — verify audio resumes
- [ ] Android back button from Settings screen: navigates to Home (does not exit app)
- [ ] Android back button from Level Select screen: navigates to World Map (does not exit app)
- [ ] Android back button during gameplay: opens Pause menu (does not exit or close app)
- [ ] Background app mid-level: press Home, wait 30s, return to app — game resumes correctly
- [ ] Level 99 (The Final Battle): verify scene loads and level can be completed
- [ ] Verify no debug UI visible: no cheat menu, no developer currency grant button, no internal overlay

---

## Analytics Verification

Complete these checks independently of the gameplay session above. Use a device
running a **staging** build connected to the Firebase DebugView panel, or confirm
via BigQuery events within 10 minutes of the test session.

- [ ] `level_start` event fires with correct `level_id` and `world_id` on Level 1 start
- [ ] `level_complete` event fires with correct `stars`, `coins`, and `completion_time` on Level 1 complete
- [ ] `rewarded_ad_reward_granted` event fires **only after** `ad_completed` — verify it does NOT fire on ad close/skip
- [ ] `purchase_success` event fires with correct `product_id` after sandbox IAP
- [ ] Crashlytics: no new crash signatures introduced by this build — check 1 hour after fresh install session

---

## Sign-Off

| Role | Name | Signature | Date |
|------|------|-----------|------|
| QA Lead | | | |
| Dev Lead | | | |
| Producer | | | |

**Promotion approved:** Yes / No

**Notes / Blockers:**

---

*Document version: M16-sl1 — update header fields for each build.*
