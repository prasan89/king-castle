# King Smash M15 — Manual QA Checklist

**Format:** Check each item. Mark Pass (P), Fail (F), or Not Applicable (N/A).

**Build:** _________________ **Date:** _________________ **Tester:** _________________

**Device:** _________________ **OS:** _________________ **Environment:** QA / Staging

---

## Authentication

| # | Test Case | Steps | P | F | N/A |
|---|-----------|-------|---|---|-----|
| AUTH-01 | Anonymous auth on clean install | Fresh install. Launch app. Observe auth state before onboarding. Verify anonymous UID assigned in Firestore. | | | |
| AUTH-02 | Anonymous UID persists across restart | Complete AUTH-01. Close app. Reopen. Verify same anonymous UID in logs. | | | |
| AUTH-03 | Google Sign-In links anonymous account | From home screen, open Settings > Account. Tap "Sign in with Google". Complete Google auth. Verify prior progress retained. | | | |
| AUTH-04 | Google Sign-In: cancel does not crash | Tap "Sign in with Google". Cancel Google account picker. Verify app returns to settings without crash. | | | |
| AUTH-05 | Auth failure on offline launch | Enable airplane mode. Launch app. Verify app proceeds to local play (anonymous fallback); no crash or infinite spinner. | | | |
| AUTH-06 | Re-auth after token expiry | Set system clock +48h (if testable). Reopen app. Verify silent re-auth succeeds or prompts user gracefully. | | | |

---

## Onboarding

| # | Test Case | Steps | P | F | N/A |
|---|-----------|-------|---|---|-----|
| ONB-01 | Onboarding plays on first launch | Clean install. Launch. Verify tutorial sequence fires before Level 1 is accessible. | | | |
| ONB-02 | Onboarding does not replay on second launch | Complete onboarding. Close app. Reopen. Verify main menu shown directly; no onboarding replay. | | | |
| ONB-03 | Tap targets highlighted correctly | During onboarding, verify each highlighted UI element is visible and not obscured by notch or nav bar (SafeAreaHandler). | | | |
| ONB-04 | Onboarding completes to Level 1 ready state | Follow onboarding to completion. Verify Level 1 is unlocked and Level 2 is locked. | | | |
| ONB-05 | Onboarding cannot be skipped in mid-flow | During onboarding, press Android back button. Verify onboarding cannot be dismissed prematurely. | | | |

---

## Home Screen

| # | Test Case | Steps | P | F | N/A |
|---|-----------|-------|---|---|-----|
| HOME-01 | Home screen loads after onboarding | After onboarding, verify home screen shows coin balance, gem balance, XP bar, and player level. | | | |
| HOME-02 | Coin and gem balance correct | Note values from Firestore (debug). Compare against displayed balance on home screen. | | | |
| HOME-03 | Navigation to World Map | Tap "Play" button on home screen. Verify world map loads within 3 seconds. | | | |
| HOME-04 | Navigation to Shop | Tap Shop icon. Verify shop screen loads. Tap back. Verify home screen is restored. | | | |
| HOME-05 | Daily reward banner displays | If a daily reward is available, verify banner or indicator appears on home screen. | | | |
| HOME-06 | Home screen layout on 20:9 aspect ratio | On a 20:9 device/emulator, verify no UI elements are clipped or hidden by gesture nav bar. | | | |
| HOME-07 | Home screen layout on notch device | On a notched device/emulator, verify no UI elements overlap the notch cutout. | | | |

---

## World Map

| # | Test Case | Steps | P | F | N/A |
|---|-----------|-------|---|---|-----|
| WMAP-01 | World 1 fully accessible from fresh save | From a save with levels 1–20 completed, verify all 20 World 1 nodes are accessible or locked correctly. | | | |
| WMAP-02 | Locked worlds show correct lock state | From a fresh save (Level 1 only unlocked), verify Worlds 2–5 are locked and show lock icon. | | | |
| WMAP-03 | World completion badge | Complete all levels in a world. Return to world map. Verify completion badge appears on world node. | | | |
| WMAP-04 | Scroll between worlds | Swipe/tap left and right to navigate between world nodes. Verify smooth scroll with no glitch. | | | |
| WMAP-05 | World map persists correct star counts | Verify that star icons on each level node match the saved star count for that level. | | | |

---

## Level Select

| # | Test Case | Steps | P | F | N/A |
|---|-----------|-------|---|---|-----|
| LSEL-01 | Unlocked levels are tappable | Tap any unlocked level node. Verify level select detail panel (or direct load) appears. | | | |
| LSEL-02 | Locked levels are not tappable | Tap a locked level. Verify it is not selectable and a "locked" message or visual is shown. | | | |
| LSEL-03 | Star display on level node | Verify 1, 2, and 3 star levels each show the correct star fill on the node. | | | |
| LSEL-04 | World 2 unlock after World 1 boss | Save injection: set all World 1 levels complete. Verify World 2 levels are accessible. | | | |
| LSEL-05 | Level 100 (final) node accessible | Save injection: set all levels 1–99 complete. Verify Level 100 node is accessible. | | | |

---

## Gameplay

| # | Test Case | Steps | P | F | N/A |
|---|-----------|-------|---|---|-----|
| GAME-01 | Level loads and initializes | Tap any unlocked level. Verify all elements load: player character, blocks, enemies, HUD (coins, lives, power-up slots). | | | |
| GAME-02 | Input registers correctly | Perform standard move/attack input. Verify player character responds without input lag. | | | |
| GAME-03 | Pause menu accessible in-level | During gameplay, tap pause button. Verify pause overlay appears; game time stops. | | | |
| GAME-04 | Resume from pause | From pause overlay, tap Resume. Verify gameplay resumes from exact state. | | | |
| GAME-05 | Quit to menu from pause | From pause overlay, tap Quit. Verify return to level select or world map; no in-progress save corruption. | | | |
| GAME-06 | HUD elements not obscured | During gameplay, verify all HUD elements (score, lives, power-ups, timer if applicable) are visible and not behind notch/nav bar. | | | |
| GAME-07 | Level timer (if applicable) counts down correctly | Observe timer during play. Verify it counts down at real-time rate; no double-speed or freeze. | | | |
| GAME-08 | Multiple levels in sequence | Complete Level 1. Proceed to Level 2. Verify Level 2 loads correctly with no residual state from Level 1. | | | |

---

## Physics & Destruction

| # | Test Case | Steps | P | F | N/A |
|---|-----------|-------|---|---|-----|
| PHYS-01 | Block destruction responds to hit | Attack a destructible block. Verify it deforms or breaks within expected hit count. | | | |
| PHYS-02 | Chain destruction propagates | Destroy a supporting block. Verify connected blocks fall and interact via physics. | | | |
| PHYS-03 | Debris does not persist beyond cleanup window | After block destruction, verify debris objects are pooled/destroyed within 2–3 seconds. No memory leak from uncleaned debris. | | | |
| PHYS-04 | Physics correct at level boundaries | Push/throw objects near level edge. Verify no tunnelling through walls or floor. | | | |
| PHYS-05 | Hit-stop effect fires on strong hit | Deliver a heavy attack. Verify brief frame-freeze (hit-stop) effect fires; gameplay resumes normally. | | | |
| PHYS-06 | Camera shake on destruction | Destroy a significant structure. Verify camera shake fires and returns to idle position smoothly. | | | |

---

## Enemies

| # | Test Case | Steps | P | F | N/A |
|---|-----------|-------|---|---|-----|
| ENEMY-01 | Enemies spawn at correct positions | Load a level with enemies. Verify all enemies spawn at their defined positions. | | | |
| ENEMY-02 | Enemy HP depletes on hit | Attack an enemy. Verify HP bar decrements with each hit. | | | |
| ENEMY-03 | Enemy death triggers reward | Kill an enemy. Verify coin/XP reward appears and is added to balance. | | | |
| ENEMY-04 | Enemy AI transitions states correctly | Let an enemy idle, then approach within aggro range. Verify enemy transitions to chase/attack state. | | | |
| ENEMY-05 | Enemy does not leave level bounds | Observe enemy patrol AI. Verify enemy does not walk off-screen or through solid walls. | | | |
| ENEMY-06 | Enemy respawn (if applicable) | If the level has respawning enemies, verify they respawn at correct intervals and positions. | | | |

---

## Boss Encounters

| # | Test Case | Steps | P | F | N/A |
|---|-----------|-------|---|---|-----|
| BOSS-01 | Boss spawns on Level 20 (World 1 boss) | Play to Level 20. Verify boss character spawns and intro sequence plays. | | | |
| BOSS-02 | Boss HP bar visible and accurate | During boss fight, verify boss HP bar is displayed in HUD; it decrements correctly. | | | |
| BOSS-03 | Boss phase transition triggers | Deplete boss HP to phase threshold (e.g., 50%). Verify phase transition animation and new attack pattern. | | | |
| BOSS-04 | Boss defeat triggers victory flow | Deplete boss HP to 0. Verify defeat animation plays and victory screen is shown. | | | |
| BOSS-05 | Boss fight save checkpoint | Begin boss fight. Force-close app. Reopen. Verify player returns to start of boss level (not mid-fight). | | | |

---

## Power-ups

| # | Test Case | Steps | P | F | N/A |
|---|-----------|-------|---|---|-----|
| PU-01 | Bomb: purchase and use | Buy Bomb from shop. Enter level. Activate Bomb. Verify explosion radius destroys surrounding blocks/enemies. | | | |
| PU-02 | Fire: purchase and use | Buy Fire. Enter level. Activate. Verify fire trail VFX and burn damage over time on enemies/blocks. | | | |
| PU-03 | Ice: purchase and use | Buy Ice. Enter level. Activate. Verify freeze effect on enemies (slow or stasis). Verify thaw after duration. | | | |
| PU-04 | Lightning: purchase and use | Buy Lightning. Enter level. Activate. Verify chain lightning hits multiple enemies. | | | |
| PU-05 | Mega King: purchase and use | Buy Mega King (if `feature_flag_megaking = true`). Enter level. Activate. Verify mega form VFX, boosted stats, and duration timer. | | | |
| PU-06 | Power-up inventory decrements | Use any power-up. Verify inventory count decrements by exactly 1. | | | |
| PU-07 | Power-up not usable when inventory = 0 | With 0 of a power-up in inventory, verify the UI button is disabled; no crash on tap. | | | |
| PU-08 | Power-up disabled by Remote Config | Set `feature_flag_megaking = false` in Remote Config. Verify Mega King is not shown in shop or level UI. | | | |

---

## Victory Screen

| # | Test Case | Steps | P | F | N/A |
|---|-----------|-------|---|---|-----|
| VIC-01 | Victory screen shows correct star count | Complete level at various performance levels. Verify 1, 2, or 3 stars awarded per design targets. | | | |
| VIC-02 | Coin reward displayed and credited | Note coins before level. Complete level. Verify coin delta on victory screen matches LevelData.coinReward × multiplier. | | | |
| VIC-03 | XP reward credited | Note XP before level. Complete level. Verify XP delta matches LevelData.xpReward. | | | |
| VIC-04 | "Next Level" button advances correctly | On victory screen, tap Next Level. Verify Level N+1 loads. | | | |
| VIC-05 | Replay level from victory screen | Tap Replay on victory screen. Verify same level restarts with fresh state. | | | |
| VIC-06 | No duplicate reward on retry | Fail level after first play. Retry. Complete. Verify reward granted once, not twice. | | | |

---

## Failure / Game Over

| # | Test Case | Steps | P | F | N/A |
|---|-----------|-------|---|---|-----|
| FAIL-01 | Failure screen shown on loss condition | Trigger loss condition (lives depleted or time expired). Verify failure screen appears. | | | |
| FAIL-02 | Retry from failure restarts level cleanly | Tap Retry on failure screen. Verify level restarts with fresh state; no residual HP/enemy state. | | | |
| FAIL-03 | Continue with ad (if implemented) | On failure screen, tap "Continue" (watch ad). Verify rewarded ad plays. After ad, player continues with restored lives. | | | |
| FAIL-04 | Quit from failure returns to level select | Tap Quit on failure screen. Verify return to level select; level star count unchanged. | | | |
| FAIL-05 | No coin penalty on failure | Note coins before entering level. Fail level. Tap Quit. Verify coin balance is unchanged (no deduction for loss). | | | |

---

## Rewards

| # | Test Case | Steps | P | F | N/A |
|---|-----------|-------|---|---|-----|
| REW-01 | Level clear coins credited to balance | See VIC-02. Verify coins shown on home screen match post-victory balance. | | | |
| REW-02 | Gem drop from special level credited | Complete a level with gem reward. Verify gems shown on home screen match post-victory balance. | | | |
| REW-03 | Reward granted once across restart | Complete level. Force-close before home screen. Reopen. Verify reward not granted twice. | | | |
| REW-04 | Reward animation plays correctly | On victory screen, verify coin/gem fly-in animation plays without stutter. | | | |

---

## Economy

| # | Test Case | Steps | P | F | N/A |
|---|-----------|-------|---|---|-----|
| ECON-01 | Coin balance updates immediately after level | Complete level. Verify coin balance on home screen updates without requiring full app restart. | | | |
| ECON-02 | Gem balance updates immediately after reward | Receive gem reward. Verify gem balance updates immediately. | | | |
| ECON-03 | Insufficient coins: purchase blocked | In shop, with insufficient coins, tap a coin-priced item. Verify purchase is blocked and error/insufficient funds UI shown. | | | |
| ECON-04 | Insufficient gems: purchase blocked | With insufficient gems, tap a gem-priced item. Verify purchase blocked. | | | |
| ECON-05 | Economy state persists across session | Note coin/gem/XP. Close app. Reopen. Verify exact same values. | | | |
| ECON-06 | coin_multiplier Remote Config applies | Set `coin_multiplier = 2`. Complete level. Verify coin reward is exactly 2× base value. | | | |

---

## Upgrades

| # | Test Case | Steps | P | F | N/A |
|---|-----------|-------|---|---|-----|
| UPG-01 | Upgrade available when prerequisites met | Verify upgrade option appears in upgrades screen after meeting level/cost requirements. | | | |
| UPG-02 | Upgrade cost deducted on purchase | Note coins. Purchase upgrade. Verify correct coin amount deducted. | | | |
| UPG-03 | Upgrade effect applies in next level | Purchase a damage upgrade. Enter level. Verify damage output increased per spec. | | | |
| UPG-04 | Max upgrade level cannot be exceeded | Purchase upgrades to max level. Verify upgrade button disabled at max; no additional cost taken. | | | |
| UPG-05 | Upgrade state persists across session | Purchase an upgrade. Close app. Reopen. Verify upgrade level retained. | | | |

---

## Shop

| # | Test Case | Steps | P | F | N/A |
|---|-----------|-------|---|---|-----|
| SHOP-01 | Shop loads all items | Open shop. Verify all expected categories (power-ups, upgrades, gem bundles) are displayed. | | | |
| SHOP-02 | Item prices displayed correctly | Verify coin and gem prices for each item match design values. | | | |
| SHOP-03 | Purchase power-up from shop | Buy a power-up with sufficient coins. Verify inventory incremented; coin balance decremented. | | | |
| SHOP-04 | Shop reflects correct inventory counts | Open shop after buying a power-up. Verify current inventory count displayed per item. | | | |
| SHOP-05 | IAP gem bundle navigates to billing | Tap a gem bundle (IAP). Verify Google Play billing sheet appears (or test mode equivalent). | | | |
| SHOP-06 | Shop closes and returns to home | Tap back/close on shop. Verify return to home screen without crash. | | | |

---

## Billing (Google Play IAP)

| # | Test Case | Steps | P | F | N/A |
|---|-----------|-------|---|---|-----|
| BILL-01 | Billing client initializes without crash | Launch app. Check logcat for BillingClient connected. No crash or ANR. | | | |
| BILL-02 | Product details fetched for all SKUs | In shop, verify all IAP items have price and title (not placeholder). | | | |
| BILL-03 | Purchase flow opens correctly | Tap IAP item. Verify Google Play billing UI (or test overlay) appears. | | | |
| BILL-04 | [Physical] Sandbox purchase credits gems once | On physical device: complete sandbox purchase. Verify gems credited exactly once in both local and Firestore. | | | |
| BILL-05 | [Physical] Duplicate receipt rejected | Re-submit purchase receipt. Verify Cloud Run validates and rejects duplicate; gems not doubled. | | | |
| BILL-06 | Purchase cancelled does not grant reward | Dismiss billing UI without completing purchase. Verify gems not credited. | | | |
| BILL-07 | Billing unavailable: graceful degradation | Simulate billing unavailable (airplane mode). Tap IAP. Verify error message shown; no crash. | | | |

---

## Ads (AdMob)

| # | Test Case | Steps | P | F | N/A |
|---|-----------|-------|---|---|-----|
| ADS-01 | Rewarded ad button appears at correct locations | Verify "Watch Ad" buttons appear at: failure screen (continue), shop (free power-up), daily reward bonus. | | | |
| ADS-02 | Rewarded ad loads test ad | Tap "Watch Ad". Verify test ad (AdMob test ad ID) loads and plays. | | | |
| ADS-03 | Reward granted after full ad view | Watch full rewarded ad. Verify reward (coins/power-up) credited exactly once. | | | |
| ADS-04 | Reward NOT granted if ad closed early | If skip/close option available before ad completion, close early. Verify no reward granted. | | | |
| ADS-05 | Interstitial shown at correct interval | Play through levels. Verify interstitial ad shown after N levels (per `interstitial_interval` config). | | | |
| ADS-06 | Interstitial cooldown respected | Verify interstitial not shown again within `interstitial_cooldown_seconds`. | | | |
| ADS-07 | No ad shown during boss fight | Trigger any automatic interstitial timing during boss fight. Verify interstitial is suppressed. | | | |
| ADS-08 | Ad failure: graceful message | Simulate no ad fill (use test "no fill" ID). Verify "No ad available" message shown; no crash. | | | |

---

## Daily Rewards

| # | Test Case | Steps | P | F | N/A |
|---|-----------|-------|---|---|-----|
| DR-01 | Daily reward claimable on first day | Fresh install. Open daily reward screen. Verify Day 1 reward is available to claim. | | | |
| DR-02 | Correct reward for day streak | Claim rewards on Day 1, Day 2, Day 3. Verify reward values match the streak schedule. | | | |
| DR-03 | Reward credited to balance | Claim daily reward. Verify coins/gems added to balance. | | | |
| DR-04 | Reward not claimable twice on same day | Claim reward. Close app. Reopen same day. Verify claim button is greyed out; countdown shows time until next. | | | |
| DR-05 | Streak resets if day missed | Advance system clock by 2+ days (skipping a day). Verify streak resets to Day 1. | | | |
| DR-06 | `daily_reward_enabled = false` disables feature | Set Remote Config `daily_reward_enabled = false`. Verify daily reward UI is hidden on home screen. | | | |

---

## Missions

| # | Test Case | Steps | P | F | N/A |
|---|-----------|-------|---|---|-----|
| MIS-01 | At least one active mission at all times | Open missions screen. Verify at least one mission is shown with progress bar. | | | |
| MIS-02 | Mission progress updates after level | Complete a level that counts toward a mission (e.g., "Complete 3 levels"). Verify mission counter increments. | | | |
| MIS-03 | Mission complete reward granted once | Complete mission requirements. Tap claim. Verify reward granted. Verify claim not available again. | | | |
| MIS-04 | New mission replaces completed mission | After claiming a mission reward, verify a new mission populates. | | | |
| MIS-05 | Mission progress persists across session | Note mission progress. Close app. Reopen. Verify same progress shown. | | | |

---

## Achievements

| # | Test Case | Steps | P | F | N/A |
|---|-----------|-------|---|---|-----|
| ACH-01 | Achievements screen lists all achievements | Open achievements screen. Verify all expected achievements are listed with descriptions and progress. | | | |
| ACH-02 | Progress counter increments correctly | Perform achievement action (e.g., destroy 10 blocks). Verify achievement counter increments. | | | |
| ACH-03 | Achievement unlocks at correct threshold | Reach the unlock threshold. Verify achievement badge changes from locked to unlocked. | | | |
| ACH-04 | Achievement reward granted once | Claim achievement reward. Verify reward granted. Attempt to claim again — verify not claimable. | | | |
| ACH-05 | Achievement state persists across session | Unlock an achievement. Close app. Reopen. Verify achievement still shows as unlocked. | | | |

---

## Cloud Save

| # | Test Case | Steps | P | F | N/A |
|---|-----------|-------|---|---|-----|
| SAVE-01 | Save written to Firestore after level | Complete Level 1. Check Firestore console. Verify player document updated with level 1 complete, correct stars. | | | |
| SAVE-02 | Save loaded on app reopen | Note save state. Close app. Reopen. Verify exact same state loaded from Firestore. | | | |
| SAVE-03 | Conflict resolution: server timestamp wins | Modify Firestore document directly (simulate server-side update with later timestamp). Reopen app. Verify server version used. | | | |
| SAVE-04 | Conflict resolution: higher stars preserved | Simulate local 3-star save vs server 1-star save for same level. Sync. Verify 3-star version is kept. | | | |
| SAVE-05 | Cloud save does not reset progress | Play several levels. Force-close. Reopen. Verify no levels lost or reset. | | | |
| SAVE-06 | Save migration on schema version bump | Inject an old-version save document. Open app. Verify migration completes without crash; progress preserved. | | | |

---

## Offline Mode

| # | Test Case | Steps | P | F | N/A |
|---|-----------|-------|---|---|-----|
| OFF-01 | Game playable on airplane mode | Enable airplane mode after auth. Navigate to Level 1. Complete it. Verify completion saved locally. | | | |
| OFF-02 | No crash on Firestore write while offline | Complete level while offline. Verify no crash from Firestore write attempt. | | | |
| OFF-03 | Online sync after reconnect | Complete a level offline. Re-enable network. Verify local progress syncs to Firestore without data loss. | | | |
| OFF-04 | No duplicate reward on offline → online sync | Complete level offline. Reconnect. Verify reward (coins/XP) was not doubled by sync. | | | |
| OFF-05 | Offline → online does not overwrite newer cloud save | While offline, complete Level 5. Meanwhile (simulated), cloud has Level 6 completion. Reconnect. Verify both Level 5 and Level 6 are present in final state. | | | |

---

## Remote Config

| # | Test Case | Steps | P | F | N/A |
|---|-----------|-------|---|---|-----|
| RC-01 | Remote Config fetches all keys on launch | Launch app (with fetch interval = 0). Check logs for successful fetch. Verify all 25+ keys present. | | | |
| RC-02 | Defaults used when offline | Enable airplane mode. Launch app. Verify all features use default values; no crash. | | | |
| RC-03 | coin_multiplier override applies to reward | Set `coin_multiplier = 3`. Complete level. Verify coin reward = base × 3. | | | |
| RC-04 | feature_flag_megaking disables power-up | Set `feature_flag_megaking = false`. Verify Mega King not shown in shop; not available in level. | | | |
| RC-05 | daily_reward_enabled controls UI | Set `daily_reward_enabled = false`. Verify daily reward entry point removed from home screen. | | | |
| RC-06 | interstitial_interval controls ad frequency | Set `interstitial_interval = 1`. Play 2 levels. Verify interstitial shown after every level. | | | |
| RC-07 | Config refresh during session (if applicable) | Force a background fetch. Verify new values applied on next app activate. | | | |

---

## Analytics

| # | Test Case | Steps | P | F | N/A |
|---|-----------|-------|---|---|-----|
| ANA-01 | level_start event fires with correct params | Enable DebugView. Start Level 1. Verify `level_start` event with `level_id=1`, `world_id=1`. | | | |
| ANA-02 | level_complete event fires with correct params | Complete Level 1. Verify `level_complete` with `level_id`, `stars`, `coins_earned`, `duration_ms`, `attempts`. | | | |
| ANA-03 | level_fail event fires on failure | Fail a level. Verify `level_fail` event with `level_id`, `failure_reason`. | | | |
| ANA-04 | purchase event fires on IAP | Complete an IAP (sandbox). Verify `purchase` event with `item_id`, `currency`, `value`. | | | |
| ANA-05 | ad_reward_claimed event fires correctly | Watch rewarded ad. Verify `ad_reward_claimed` with `ad_unit`, `reward_type`, `reward_amount`. | | | |
| ANA-06 | No duplicate events on retry | Fail level and retry. Verify `level_start` fires once per attempt; `level_complete` fires once on completion. | | | |
| ANA-07 | session_start fires on cold launch | Cold launch app. Verify `session_start` event fires. | | | |
| ANA-08 | User property (player_level) set correctly | Level up player. Verify Firebase user property `player_level` updated. | | | |

---

## Crashlytics

| # | Test Case | Steps | P | F | N/A |
|---|-----------|-------|---|---|-----|
| CRASH-01 | Debug symbols uploaded for build | Check Firebase Crashlytics console. Verify dSYM/symbols uploaded for the RC build version. | | | |
| CRASH-02 | Non-fatal exception appears in console | Trigger a known non-fatal path (e.g., Firestore read with mock error). Verify `RecordException` event in Crashlytics. | | | |
| CRASH-03 | Breadcrumbs visible in crash report | Trigger a crash (test build). Verify breadcrumb trail in Crashlytics shows navigation leading to crash. | | | |
| CRASH-04 | Custom keys appear in crash report | Verify `level_id`, `world_id`, `player_level` custom keys appear in crash context. | | | |
| CRASH-05 | Crash-free session rate ≥ 99% | Run automated test suite (PlayMode). Observe crash-free rate in Crashlytics. Must be ≥ 99.0%. | | | |

---

## Settings Screen

| # | Test Case | Steps | P | F | N/A |
|---|-----------|-------|---|---|-----|
| SET-01 | Settings screen opens from home | Tap settings icon. Verify settings screen loads with all options. | | | |
| SET-02 | Music toggle persists across session | Toggle music off. Close app. Reopen. Verify music remains off. | | | |
| SET-03 | SFX toggle persists across session | Toggle SFX off. Close app. Reopen. Verify SFX remains off. | | | |
| SET-04 | Vibration toggle works | Toggle vibration. Play level. Verify vibration feedback matches toggle state. | | | |
| SET-05 | Account section shows correct auth state | In settings, verify Account section shows "Anonymous" or Google account name depending on auth state. | | | |
| SET-06 | Settings close returns to home | Close settings. Verify return to home screen; no state reset. | | | |

---

## Audio

| # | Test Case | Steps | P | F | N/A |
|---|-----------|-------|---|---|-----|
| AUD-01 | Background music plays on main menu | Open main menu. Verify background music is audible. | | | |
| AUD-02 | Background music transitions on level load | Load a level. Verify music transitions to level track without abrupt cut. | | | |
| AUD-03 | SFX plays on destruction | Destroy a block. Verify destruction SFX plays. | | | |
| AUD-04 | SFX plays on power-up activation | Activate a power-up. Verify power-up activation SFX plays. | | | |
| AUD-05 | Audio resumes after background/resume | Background app (home button). Return to app. Verify audio resumes correctly; no duplicate layers. | | | |
| AUD-06 | Music mute toggle silences music immediately | In settings, toggle music off. Verify music stops immediately without fade. | | | |
| AUD-07 | SFX mute toggle silences effects immediately | Toggle SFX off. Verify all sound effects are silent. | | | |

---

## Android Back Button

| # | Test Case | Steps | P | F | N/A |
|---|-----------|-------|---|---|-----|
| BACK-01 | Back from home screen shows exit confirmation | On home screen, press Android back button. Verify exit confirmation dialog appears. | | | |
| BACK-02 | Back from world map returns to home | On world map, press back. Verify return to home screen. | | | |
| BACK-03 | Back from level select returns to world map | On level select, press back. Verify return to world map. | | | |
| BACK-04 | Back during gameplay shows pause menu | During gameplay, press back. Verify pause menu shown; game does not quit immediately. | | | |
| BACK-05 | Back from shop returns to home | On shop screen, press back. Verify return to home screen. | | | |
| BACK-06 | Back from settings returns to home | On settings, press back. Verify return to home screen. | | | |

---

## Background / Resume

| # | Test Case | Steps | P | F | N/A |
|---|-----------|-------|---|---|-----|
| BG-01 | Game pauses when backgrounded during gameplay | During active gameplay, press home button. Verify game pauses (time stops, no physics updates). | | | |
| BG-02 | Game resumes correctly after foreground | After backgrounding, return to app. Verify gameplay resumes from exact state (position, HP, timer). | | | |
| BG-03 | No audio layer duplication on resume | Background and resume 3 times in quick succession. Verify audio has no duplicate channels. | | | |
| BG-04 | [Physical] Incoming call interruption | Simulate incoming call. Accept. Return to app. Verify no crash and game state intact. | | | |
| BG-05 | Firebase auth token refresh on long background | Background app for > 1 hour (simulate). Return. Verify auth token silently refreshed; no re-auth prompt. | | | |

---

## Performance

| # | Test Case | Steps | P | F | N/A |
|---|-----------|-------|---|---|-----|
| PERF-01 | Cold launch within 5s (emulator) | Fresh launch on emulator. Measure time from app icon tap to main menu interactive. Must be < 5s. | | | |
| PERF-02 | Level load within 3s (emulator) | Tap a level. Measure time to first gameplay frame. Must be < 3s. | | | |
| PERF-03 | No hitches > 100ms during 60s gameplay | Record Unity Profiler during 60s session. Verify no frame > 100ms. | | | |
| PERF-04 | Memory < 500MB at peak gameplay | Open Unity Profiler. Play for 3 minutes. Verify peak RSS < 500 MB on emulator. | | | |
| PERF-05 | No GC allocs > 5KB per frame in hot path | Enable GC alloc tracking. Play 60s. Verify no frame in gameplay loop exceeds 5KB allocation. | | | |
| PERF-06 | Memory stable over 10 minutes (no leak) | Play for 10 minutes without returning to menu. Verify memory does not grow continuously. | | | |

---

## Memory

| # | Test Case | Steps | P | F | N/A |
|---|-----------|-------|---|---|-----|
| MEM-01 | Object pool prevents debris accumulation | Destroy many blocks. Verify particle/debris objects are returned to pool, not created indefinitely. | | | |
| MEM-02 | Level unload releases assets | Complete a level and load next. Verify previous level's assets unloaded (Profiler: no lingering textures). | | | |
| MEM-03 | Texture atlases loaded at appropriate quality | On low-end emulator (720p), verify lower-resolution atlas is loaded, not full 2K textures. | | | |
| MEM-04 | Audio clip memory managed correctly | Play through 5 levels. Verify audio clip memory stable; no unbounded AudioSource accumulation. | | | |

---

## Security

| # | Test Case | Steps | P | F | N/A |
|---|-----------|-------|---|---|-----|
| SEC-01 | No API keys in APK resources | Decompile QA APK (apktool). Search for `AIza` (Firebase API key pattern). Verify keys not hardcoded in plaintext in res/. | | | |
| SEC-02 | Firestore security rules block unauthorized reads | Use Firebase REST API with a different UID. Verify access to another player's document returns 403. | | | |
| SEC-03 | Cloud Run `/validate-purchase` requires auth | Send IAP validation request without Firebase ID token. Verify 401 returned. | | | |
| SEC-04 | No cheat: client cannot send arbitrary coin value | Intercept (proxy) level complete request. Modify `coins_earned` to an inflated value. Verify Cloud Run rejects or clamps the value. | | | |
| SEC-05 | Remote Config cannot grant negative costs | Set any price key to -9999 in Remote Config. Verify purchase flow clamps to minimum 0 or uses default. | | | |

---

*Checklist version: M15-MANUAL-QA-CHECKLIST-v1.0*
*Last updated: 2026-10-04*
*Owner: QA Lead — King Smash*
