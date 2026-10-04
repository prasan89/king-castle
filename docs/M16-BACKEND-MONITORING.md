# King Smash M16 — Backend Monitoring Reference

**Version:** 1.0.0-sl1
**Build Date:** 2026-10-05
**Milestone:** M16 Soft Launch
**Depends on:** M14 Analytics implementation, M16 Soft-Launch Strategy, M16 Analytics Playbook

---

## Overview

This document covers the Cloud backend monitoring setup for King Smash's M16 soft launch. The backend consists of:
- **Cloud Run** — Node.js REST API handling economy, rewards, purchases, and cloud save operations
- **Firestore** — NoSQL document store for player data (progression, economy, entitlements)
- **Firebase Auth** — Player identity; UID hashing in server logs (no raw PII stored in logs)
- **Firebase Analytics + Crashlytics** — Client-side telemetry (covered in M16-ANALYTICS-PLAYBOOK.md)

All monitoring is set up in the `king-smash-prod` GCP project. Do not monitor staging data during soft launch.

---

## 1. Cloud Run Monitoring Setup

### GCP Monitoring Alerts (Create in Cloud Console Before Day 0)

Navigate to: GCP Console → Monitoring → Alerting → Create Policy

| Alert Name | Metric | Condition | Threshold | Window | Notification Channel |
|---|---|---|---|---|---|
| High Request Latency | `run.googleapis.com/request_latencies` (p95) | ABOVE | 2000ms | 5-minute rolling | Slack + email |
| High Request Latency P99 | `run.googleapis.com/request_latencies` (p99) | ABOVE | 5000ms | 5-minute rolling | Slack + email (urgent) |
| High 5xx Error Rate | `run.googleapis.com/request_count` filtered by response_code_class=5xx / total | ABOVE | 0.5% | 5-minute rolling | Slack + email (treat as P0) |
| High 4xx Error Rate | `run.googleapis.com/request_count` filtered by response_code_class=4xx / total | ABOVE | 5% | 5-minute rolling | Slack (may indicate client auth issues) |
| Request Volume Spike | `run.googleapis.com/request_count` | ABOVE | 1000/min sustained | 2-minute rolling | Email (possible retry storm) |
| High Instance Count | `run.googleapis.com/container/instance_count` | ABOVE | 5 sustained | 2-minute rolling | Email (unexpected for < 2000 users) |

### Notification Channels Setup

Configure two notification channels before Day 0:
1. **Email:** Team distribution list (all engineers + producer)
2. **Slack:** `#king-smash-monitoring` channel via Slack webhook (GCP Notification Channel → Slack)

### Log-Based Alerts

The Cloud Run `authMiddleware` logs SHA-256 hashed UIDs (no raw PII in logs). Log-based alerts catch issues before metrics aggregate:

**Alert: Any server-side ERROR log in production**

```
# Cloud Logging filter
resource.type="cloud_run_revision"
resource.labels.service_name="king-smash-api"
resource.labels.location="us-central1"
severity=ERROR
```

Configure as a log-based alert with 1-minute notification delay to batch related errors.

**Alert: Economy handler ERROR**

```
resource.type="cloud_run_revision"
resource.labels.service_name="king-smash-api"
severity=ERROR
jsonPayload.handler=("rewardsHandler" OR "economyHandler" OR "purchaseHandler")
```

Any economy-related server error must be investigated as a potential H2/H3/H4 hard requirement issue.

### Cloud Run Dashboard Configuration

Create a custom GCP Monitoring dashboard (`King Smash — Soft Launch`) with these panels:

1. **Request rate** (requests/minute, last 24h)
2. **Error rate by status class** (2xx / 4xx / 5xx stacked, last 24h)
3. **Request latency** (p50 / p95 / p99 lines, last 24h)
4. **Instance count** (active instances over time)
5. **Container CPU utilization** (average across instances)
6. **Container memory utilization** (average — watch for memory leaks)

---

## 2. Firestore Security Rules Verification

### Pre-Launch Rules Audit

Before uploading the Internal Testing build, verify the deployed Firestore security rules match the M14 security design. Run:

```bash
firebase --project king-smash-prod firestore:rules:get
```

Confirm the following rules are in effect:

| Collection Path | Client Write Rule | Expected Value | Notes |
|---|---|---|---|
| `players/{uid}/economy/main` | `allow write` | `if false` | Economy state — server writes only via Cloud Run |
| `players/{uid}/transactions/{txId}` | `allow write` | `if false` | Idempotency keys — server writes only |
| `players/{uid}/dailyRewards/main` | `allow write` | `if false` | Daily reward state — server-managed |
| `players/{uid}/purchases/{purchaseId}` | `allow write` | `if false` | Receipt records — Cloud Run after verification |
| `players/{uid}/entitlements/{entitlementId}` | `allow write` | `if false` | Granted entitlements — Cloud Run only |
| `players/{uid}/progression/main` | `allow write` | `if isOwner()` | Player progression — client writes, UID-scoped |
| `players/{uid}/cloudSave/main` | `allow write` | `if isOwner()` | Cloud save document — client writes, UID-scoped |

**P0 violation:** If any economy-related collection (`economy/main`, `transactions/`, `dailyRewards/`, `purchases/`, `entitlements/`) allows client writes, do not proceed with soft launch until corrected.

### Rules Deployment Command

```bash
firebase --project king-smash-prod deploy --only firestore:rules
```

After deployment, verify via the Firebase Console → Firestore → Rules tab → Rules Playground. Test with a client UID attempting to write to `economy/main` — should return `PERMISSION_DENIED`.

---

## 3. Backend Endpoint Health Reference

### Cloud Run Endpoints in Production

| Endpoint | Method | Handler | Purpose | Expected Latency p95 |
|---|---|---|---|---|
| `/health` | GET | healthHandler | Health check | < 100ms |
| `/economy/earn` | POST | economyHandler | Server-side coin earn | < 500ms |
| `/economy/spend` | POST | economyHandler | Server-side coin spend | < 500ms |
| `/rewards/daily` | POST | rewardsHandler | Daily reward claim | < 500ms |
| `/rewards/level` | POST | rewardsHandler | Post-level reward | < 500ms |
| `/purchase/verify` | POST | purchaseHandler | Receipt verification + entitlement grant | < 2000ms |
| `/purchase/restore` | POST | purchaseHandler | Restore purchases | < 2000ms |
| `/cloud/save` | POST | cloudSaveHandler | Write player cloud save | < 1000ms |
| `/cloud/load` | GET | cloudSaveHandler | Read player cloud save | < 1000ms |

**Note on `/purchase/verify`:** This endpoint calls the Google Play Developer API for receipt verification. The 2000ms p95 target accounts for external API latency. If p95 exceeds 3000ms, check Google Play API status.

### Uptime Monitoring

Configure a GCP Uptime Check on the `/health` endpoint:

```
URL: https://king-smash-api-[hash]-uc.a.run.app/health
Check frequency: every 1 minute
Regions: Iowa (US-Central1) + Oregon (US-West1)
Alert: if 2 consecutive failures
```

---

## 4. Firestore Monitoring

### Firestore Metrics Dashboard

Add to the King Smash soft launch dashboard:

| Panel | Metric | Alert |
|---|---|---|
| Document reads/sec | `firestore.googleapis.com/document/read_count` | Baseline; watch for spikes |
| Document writes/sec | `firestore.googleapis.com/document/write_count` | Spike may indicate retry storm |
| Read latency p95 | `firestore.googleapis.com/api/request_latencies` (Read) | > 1000ms → investigate |
| Failed reads (PERMISSION_DENIED) | Log-based metric on PERMISSION_DENIED read errors | > 10/min → ALERT |
| Failed writes (PERMISSION_DENIED) | Log-based metric on PERMISSION_DENIED write errors | Any on economy collections → P0 |

### Firestore Index Verification

Before soft launch, verify the composite indexes required for Cloud Run queries are deployed:

```bash
firebase --project king-smash-prod firestore:indexes
```

Missing indexes cause query failures that appear as 5xx errors in Cloud Run. Common required indexes:
- `players/{uid}/transactions` — ordered by `timestamp` descending (idempotency window queries)
- `players/{uid}/purchases` — ordered by `createdAt` descending

---

## 5. Incident Response

### P0 Backend Incident Procedure

A P0 backend incident is any condition that meets one or more of:
- Cloud Run 5xx error rate > 5% for > 5 minutes
- Any verified player progression loss
- Any verified duplicate reward or purchase grant
- Firestore data corruption

**P0 Response Steps:**

1. **Notify:** Page all engineers via the `#king-smash-p0` Slack channel + direct message. Producer must be notified within 15 minutes.
2. **Assess:** Open GCP Console → Cloud Run logs. Is the issue ongoing or intermittent?
3. **Rollback (deployment issue):**
   - Identify the last stable Cloud Run revision: `gcloud run revisions list --service king-smash-api --region us-central1`
   - Roll back: `gcloud run services update-traffic king-smash-api --to-revisions [STABLE_REVISION]=100 --region us-central1`
   - Confirm traffic routing change takes effect (< 30 seconds for Cloud Run)
4. **Data issue:**
   - Do NOT manually modify production Firestore data without a full backup.
   - Export affected subcollection first: `gcloud firestore export gs://king-smash-backup/incident-[DATE]/`
   - Investigate via read-only queries only until root cause is confirmed.
5. **Player communication:**
   - If progression loss is confirmed or suspected: halt new Closed Alpha invitations immediately.
   - If > 10 players are affected: draft a player communication. Soft launch track allows email to testers.
6. **Post-incident:** Write a post-mortem within 48 hours. Include: timeline, root cause, player impact count, remediation, prevention steps.

### P1 Backend Incident Procedure

A P1 incident is any condition where:
- Cloud Run 5xx error rate is 0.5–5% for > 15 minutes
- A Cloud Save failure rate > 1% is confirmed
- A single device model or Android version has > 10% crash rate

**P1 Response Steps:**

1. Acknowledge in `#king-smash-monitoring` within 1 hour.
2. Assign to an engineer (on-call rotation during soft launch).
3. Investigate within 4 hours.
4. Fix deployed within 48 hours or a workaround (e.g., Remote Config feature flag) applied within 24 hours.

### Economy Integrity Check Procedure

If a duplicate reward or purchase grant is suspected (player reports, transaction log anomaly):

1. Query Firestore `transactions` collection for duplicate `idempotencyKey` values:
   ```javascript
   // Via Firebase Admin SDK (Cloud Shell or local)
   const dups = await admin.firestore()
     .collectionGroup('transactions')
     .where('idempotencyKey', '==', suspectedKey)
     .get();
   // Should return exactly 1 document. 2+ = duplicate grant.
   ```
2. Cross-reference Cloud Run logs for the `rewardsHandler` or `purchaseHandler` that issued the grant.
3. If a duplicate is confirmed: this is H3/H4 hard requirement violation. P0 escalation.
4. **Never manually modify `economy/main`** — always go through the Cloud Run admin endpoint or a verified migration script reviewed by a second engineer.

---

## 6. Cloud Save Integrity Monitoring

### Client-Side Event Monitoring

Analytics events `cloud_save_started`, `cloud_save_success`, `cloud_save_failed` provide the primary client-visible signal:

| Metric | Formula | Target | Alert |
|---|---|---|---|
| Save success rate | `cloud_save_success` / `cloud_save_started` | > 99% | < 99% → investigate |
| Load success rate | `cloud_load_success` / `cloud_load_started` | > 99% | < 99% → investigate |
| Save failure error categories | `error_category` param on `cloud_save_failed` | No PERMISSION_DENIED | Any PERMISSION_DENIED → P0 (auth regression) |
| Offline save rate | `cloud_save_started` with `is_online=false` | Baseline | Spike may indicate connectivity issue in market |

### Server-Side Verification

Cloud Run `/cloud/save` handler logs `saveResult.success=true/false` for each write attempt. If `success=false` appears in logs but `cloud_save_failed` is not firing in Analytics, there is a client-side gap in error reporting — add to the M17 analytics cleanup list.

**Progression loss verification procedure:**

If a player reports data loss after reinstall or device switch:
1. Get the player's Firebase UID (via support email or in-game ID).
2. Check Firestore `players/{uid}/cloudSave/main` — is the document present?
3. Check Cloud Run logs for `/cloud/save` calls from that UID — was the last save successful?
4. Check `cloud_save_failed` events for that user's `user_pseudo_id` in Firebase Analytics.
5. If the Firestore document is missing but Cloud Run logged a successful write: escalate to Firestore support (rare but possible Firestore internal issue).
6. If Cloud Run never received a save call for that UID: the client did not trigger a save. Check Unity `CloudSaveService` trigger conditions.

---

## 7. Soft-Launch Backend Capacity Planning

### M16 Expected Load (500–2,000 Active Users)

| Traffic Pattern | Estimate | Notes |
|---|---|---|
| Peak DAU active simultaneously | ~200 concurrent | Casual game, peak around evenings in PH/CA/AU |
| API requests/concurrent user | ~0.5/min | Level completions, economy, daily reward |
| Peak requests/min | ~100/min | Well within default Cloud Run capacity |
| Expected Cloud Run instances | 1–2 | Auto-scaling handles this trivially |
| Firestore reads/min | ~500–1000 | Well within Firestore free tier + quota |

### Scaling Limits (Not Expected to Be Reached in M16)

| Resource | Default Limit | Action If Approaching |
|---|---|---|
| Cloud Run max instances | 10 | Set to 20 before Closed Alpha; request quota increase for M17 |
| Cloud Run concurrency | 80 req/instance | Current default; adequate for M16 load |
| Firestore reads/day | 50,000 (free tier) | Will exceed on Day 1 — ensure billing is enabled |
| Firestore writes/day | 20,000 (free tier) | Will exceed — ensure billing is enabled |

**Action required before Day 0:** Verify GCP billing is enabled on `king-smash-prod`. The free tier will be exhausted on the first day of real user traffic. Billing alerts should be set at $50 and $200 to monitor backend costs during soft launch.

### Cost Expectations for M16

Estimated GCP cost for M16 soft launch (500–2,000 users, 14–21 days):
- Cloud Run: < $5 total (minimal compute for small-scale traffic)
- Firestore: < $10 total (read/write operations at soft-launch scale)
- Cloud Storage (backup): < $1
- Total estimated: < $20 for the entire M16 soft launch phase

These are order-of-magnitude estimates. Set billing alerts to catch any unexpected cost spikes.

---

## 8. Remote Config Backend Integration

### Config Fetch Monitoring

Remote Config fetches are logged as `config_version` in Crashlytics custom keys. The soft-launch profile is version `2`. If players are running `config_version=1` (the RC1 defaults), they are not receiving the soft-launch parameter overrides — this is a configuration deployment issue.

**Check after Day 1 of Internal Testing:** Navigate to Firebase → Remote Config → Conditions. Verify the soft-launch condition is active and the version 2 profile is published.

### Remote Config Levers for Live Ops

These Remote Config parameters can be updated without a build during soft launch. Each is a potential lever to respond to monitoring signals:

| Parameter | Current Value | Response Scenario |
|---|---|---|
| `coin_multiplier` | 1.2 | If economy ratio < 1.5: increase to 1.4. If > 10.0: decrease to 1.0 |
| `interstitial_frequency_cap` | 4 | If post-interstitial abandonment > 30%: increase to 5 or 6 |
| `rewarded_ads_enabled` | true | If reward integrity issue confirmed: set to false immediately |
| `king_launch_power` | (level-specific) | If L9 completion < 40%: can increase to reduce effective difficulty |
| `destruction_multiplier` | 1.0 | If L9 abandonment persists post-patch: increase to 1.1 |
| `daily_reward_streak_bonuses` | [10, 25, 50, 100, 250] | If daily reward claim rate < 20%: increase values |

**RC change protocol for soft launch:**
1. Identify the metric that is out of range.
2. Determine the RC parameter that addresses it.
3. Test the change in the Firebase RC Conditions playground first.
4. Apply the change in Remote Config console (takes effect on next app foreground/fetch cycle, typically within 12 hours of client next-launch).
5. Log the change in `#king-smash-monitoring` Slack with: parameter name, old value, new value, reason, timestamp.
6. Monitor the affected metric for 48 hours after the change.

---

## 9. Soft-Launch Backend Go-Live Checklist

Complete all items before uploading Internal Testing build:

### Security
- [ ] Firestore security rules deployed and verified (all economy collections: `allow write: if false`)
- [ ] Firebase App Check enabled on `king-smash-prod`
- [ ] Cloud Run authentication middleware active (all endpoints require valid Firebase ID token except `/health`)
- [ ] No hardcoded secrets in the Unity build (`google-services.json` uses prod project, no staging keys)

### Monitoring
- [ ] GCP Monitoring alerts configured (latency, error rate, instance count)
- [ ] Slack webhook configured for `#king-smash-monitoring`
- [ ] Log-based alert on `severity=ERROR` Cloud Run logs
- [ ] Uptime check on `/health` endpoint active
- [ ] Firestore PERMISSION_DENIED alert configured
- [ ] BigQuery export enabled on Firebase Analytics (`king-smash-prod` → Analytics → BigQuery)

### Operations
- [ ] GCP billing enabled and billing alerts set ($50 + $200)
- [ ] Team on-call rotation documented for the 14-day Closed Alpha window
- [ ] `#king-smash-p0` Slack channel created with all engineers + producer as members
- [ ] Support email configured in Google Play Console and monitored
- [ ] Backup export scheduled (weekly Firestore export to Cloud Storage during soft launch)

### Capacity
- [ ] Cloud Run max instances confirmed (default 10 is sufficient for M16)
- [ ] Remote Config soft-launch profile published (version 2 active)
- [ ] Firebase Remote Config conditions verified in Firebase Console

---

*Reference version: 1.0.0-sl1 | Last updated: 2026-10-05 | Milestone: M16*
