# M11 Architecture — Production GCP/Firebase Backend + Cloud Save

## System Overview

```
┌─────────────────────────────────────────────────────────────────────────┐
│  Unity Client (Android)                                                  │
│                                                                          │
│  GameBootstrap                                                           │
│    ├── IAuthService          (AuthServiceMock / FirebaseAuthService)     │
│    ├── ICloudSaveService     (CloudSaveServiceMock / FirebaseCloudSave)  │
│    ├── CloudSyncService      (orchestrates full sync)                    │
│    └── FirebaseRemoteConfigService                                       │
└──────────────┬──────────────────────────────────────────────────────────┘
               │  HTTPS + Firebase ID Token (Bearer)
               ▼
┌─────────────────────────────────────────────────────────────────────────┐
│  Cloud Run — game-api (Express / TypeScript)                             │
│                                                                          │
│  POST /api/v1/player/init          POST /api/v1/player/link-google       │
│  GET  /api/v1/progression/load     POST /api/v1/progression/save         │
│  POST /api/v1/progression/sync     GET  /api/v1/economy/balance          │
│  POST /api/v1/economy/claim        GET  /api/v1/rewards/daily/status     │
│  POST /api/v1/rewards/daily/claim  POST /api/v1/rewards/mission/claim    │
│  POST /api/v1/rewards/achievement/claim                                  │
│                                                                          │
│  Middleware: verifyFirebaseToken → requestLogger → rateLimiter           │
└──────────────┬──────────────────────────────────────────────────────────┘
               │  Firebase Admin SDK (ADC)
               ▼
┌─────────────────────────────────────────────────────────────────────────┐
│  Firebase / GCP                                                          │
│                                                                          │
│  Firebase Auth          — ID token issuance + verification               │
│  Cloud Firestore        — player data store (see schema below)           │
│  Firebase Remote Config — feature flags + game config                    │
│  GCP Secret Manager     — backend secrets (not committed to git)         │
│  Cloud Logging          — structured JSON request logs                   │
└─────────────────────────────────────────────────────────────────────────┘
```

## Firestore Schema

```
players/{uid}                          ← client read+write
  ├── economy/main                     ← server-write-only
  ├── transactions/{txId}              ← server-write-only (idempotency log)
  ├── dailyRewards/main                ← server-write-only
  ├── purchases/{purchaseId}           ← server-write-only
  ├── entitlements/{entitlementId}     ← server-write-only
  ├── progression/main                 ← client read+write
  ├── missions/{missionId}             ← client read+write
  └── achievements/{achievementId}     ← client read+write
```

**Server-write-only collections** are written exclusively by Cloud Run using the Firebase Admin SDK, which bypasses security rules. The Firestore rules set `allow write: if false` on these paths — this prevents a compromised client from manipulating coins, gems, or reward state directly.

## API Endpoints

| Method | Path | Auth | Rate limit | Description |
|--------|------|------|------------|-------------|
| POST | /api/v1/player/init | Bearer | 60/min | Idempotent player profile creation |
| GET | /api/v1/player/profile | Bearer | 60/min | Fetch profile |
| POST | /api/v1/player/link-google | Bearer | 60/min | Link anonymous → Google account |
| GET | /api/v1/progression/load | Bearer | 60/min | Load server progression |
| POST | /api/v1/progression/save | Bearer | 60/min | Save progression (revision check) |
| POST | /api/v1/progression/sync | Bearer | 60/min | Merge client+server progression |
| GET | /api/v1/economy/balance | Bearer | 60/min | Get coins+gems balance |
| POST | /api/v1/economy/claim | Bearer | 60/min | Server-authoritative economy transaction |
| GET | /api/v1/rewards/daily/status | Bearer | 60/min | Daily reward status |
| POST | /api/v1/rewards/daily/claim | Bearer | 10/min | Claim daily reward |
| POST | /api/v1/rewards/mission/claim | Bearer | 10/min | Claim mission reward |
| POST | /api/v1/rewards/achievement/claim | Bearer | 10/min | Claim achievement tier reward |

All requests must include: `Authorization: Bearer <Firebase ID Token>`

## Conflict Resolution

| Field | Strategy |
|-------|----------|
| `highestUnlockedLevel` | max(local, server) |
| `completedLevels` | union |
| `completedWorlds` | union |
| `defeatedBosses` | union |
| `starsPerLevel[i]` | max per level |
| `coins` / `gems` | server wins |
| `powerUpInventory` | server wins |
| `dailyRewardState` | server wins |
| `missionProgress` | server wins |
| `achievementProgress` | server wins |
| `adSessionStats` | local wins (session-scoped) |
| `cloudRevision` | server revision + 1 on merge |

## Environment Setup

### GCP Projects

| Env | Project ID | Cloud Run URL |
|-----|-----------|---------------|
| DEV | king-smash-dev | https://game-api-dev-xxx.run.app |
| STAGING | king-smash-staging | https://game-api-staging-xxx.run.app |
| PROD | king-smash-prod | https://game-api-xxx.run.app |

Unity scripting define symbols:
- DEV: `KING_SMASH_DEV`
- Staging: `KING_SMASH_STAGING`
- Prod: (no symbol — `#else` branch)

### Firebase Emulator (local dev)

```bash
cd king-castle
firebase emulators:start
# Auth:      localhost:9099
# Firestore: localhost:8080
# UI:        localhost:4000
```

Set `FIRESTORE_EMULATOR_HOST=localhost:8080` and `FIREBASE_AUTH_EMULATOR_HOST=localhost:9099` before running the backend locally.

### Backend Local Run

```bash
cd backend/game-api
cp .env.example .env          # fill in FIREBASE_PROJECT_ID etc.
npm install
npm run dev                   # ts-node watch mode
npm test                      # 23 Jest tests
npm run build                 # tsc → dist/
```

### Cloud Run Deploy

```bash
gcloud run deploy game-api \
  --source backend/game-api \
  --project king-smash-prod \
  --region us-central1 \
  --allow-unauthenticated   # Firebase token auth handled in middleware
```

## Secret Manager

Backend secrets are stored in GCP Secret Manager and accessed at runtime via the `secretmanager` client — they are **never** committed to git or baked into the container image.

Required secrets:
- `PACKAGE_NAME` — Android package name for Play Integrity validation
- `ALLOWED_ORIGINS` — comma-separated CORS whitelist

Access pattern in Cloud Run:
```typescript
import { SecretManagerServiceClient } from '@google-cloud/secret-manager';
const client = new SecretManagerServiceClient();
const [version] = await client.accessSecretVersion({ name: `projects/${PROJECT_ID}/secrets/PACKAGE_NAME/versions/latest` });
```

The Cloud Run service account needs the `roles/secretmanager.secretAccessor` IAM role.

## Security Model

1. **Client identity**: Every API request must carry a valid Firebase ID token. The server calls `admin.auth().verifyIdToken()` — the `uid` from the decoded token is the canonical player identity. Client-supplied `playerId` fields are never trusted.

2. **Economy integrity**: `coins` and `gems` are only modified by Cloud Run via the Admin SDK. The client cannot write to `economy/main` or `transactions/{txId}` (Firestore rules: `allow write: if false`).

3. **Idempotency**: Economy transactions and reward claims use a UUID idempotency key. The server checks `transactions` for an existing record before applying. Safe to retry on network failure.

4. **No secrets in code**: `FirebaseEnvironmentConfig.cs` stores only Cloud Run URLs (not credentials). Unity Firebase SDK uses `google-services.json` / `GoogleService-Info.plist` which are gitignored and provisioned by CI.

5. **Logging**: Structured JSON logs hash the `uid` with SHA-256 before writing. Tokens, passwords, and purchase receipts are never logged.

## Known Limitations / Future Work

- Google Sign-In (`SignInWithGoogleAsync`) requires the Google Sign-In Unity SDK (com.google.signin) which must be added to `Packages/manifest.json` and configured per-platform. The current implementation has a TODO placeholder.
- Play Integrity API (receipt/install attestation) is stubbed — requires Cloud Run integration with `https://playintegrity.googleapis.com`.
- Firestore offline persistence (`FirestoreSettings.IsPersistenceEnabled`) should be enabled on the Unity client for full offline read support.
- Rate limits (60 req/min general, 10 req/min rewards) are per-IP. For authenticated endpoints, consider per-UID rate limiting via a Redis-backed store on Cloud Memorystore.
