# KING SMASH

## Project Overview
KING SMASH is a physics-destruction mobile game for Android. The King is launched toward enemy castles to rescue the captured Queen. Developed in Unity 6 LTS targeting Android, portrait orientation, 60 FPS.

## Unity Version
Unity 6 LTS (6000.x). Exact version pinned in ProjectSettings/ProjectVersion.txt.

## Prerequisites
- Unity Hub + Unity 6 LTS with Android Build Support module
- Android SDK, NDK, JDK (via Unity Hub)
- JDK 17+
- TextMeshPro (auto-imported on first open)

## Project Structure
```
Assets/
  Scripts/
    Core/          GameBootstrap, GameManager, SceneLoader, GameLogger, ServiceLocator
    Services/      Interfaces (IAuthService, ISaveService, ...) + Firebase impls + Mocks
    Save/          SaveData, LocalSaveService, SaveMigrator
    Gameplay/      AimController, LaunchController, DestructionController, CameraFollow, interfaces
    Physics/       DestructibleObject, CastleStructure
    Characters/    KingProjectile
    Levels/        LevelController, LevelConfig, WorldConfig
    Economy/       EconomyConfig, EconomyCalculator, PowerUpConfig
    Progression/   ProgressionManager
    UI/            MainMenuUI, GameHUD
    Analytics/     AnalyticsEvents
    ScriptableObjects/  All [CreateAssetMenu] config classes
  Tests/EditMode/  Unit tests (NUnit, EditMode)
  Scenes/          Bootstrap, MainMenu, Level
```

## Namespaces
| Namespace | Purpose |
|---|---|
| KingSmash.Core | Bootstrap, GameManager, SceneLoader, Logger, ServiceLocator |
| KingSmash.Services | All service interfaces + mock/Firebase implementations |
| KingSmash.Save | SaveData model, migration, local persistence |
| KingSmash.Gameplay | Aim, launch, destruction controllers, interfaces |
| KingSmash.Physics | DestructibleObject, CastleStructure, material configs |
| KingSmash.Characters | KingProjectile |
| KingSmash.Levels | LevelController, LevelConfig, WorldConfig |
| KingSmash.Economy | EconomyConfig, EconomyCalculator, PowerUpConfig |
| KingSmash.Progression | ProgressionManager |
| KingSmash.UI | MainMenuUI, GameHUD |

## Firebase Setup
### Required packages (Firebase Unity SDK)
- com.google.firebase.auth
- com.google.firebase.firestore
- com.google.firebase.analytics
- com.google.firebase.remote-config
- com.google.firebase.crashlytics

### Configuration files (NOT in source control)
- `Assets/google-services.json` — Android, from Firebase console
- `Assets/GoogleService-Info.plist` — iOS, from Firebase console

**Download from**: Firebase Console → Project Settings → Your apps

### Environment switching
Set Unity Scripting Define Symbols:
- Development: `KING_SMASH_DEV`
- Staging: `KING_SMASH_STAGING`
- Production: (no symbol)

## GCP Architecture
```
Android Client (Unity)
    ↓ Firebase Auth (anonymous + email)
    ↓ Cloud Firestore (player data: players/{playerId})
    ↓ Firebase Remote Config (feature flags, balance tweaks)
    ↓ Firebase Analytics (events defined in AnalyticsEvents.cs)
    ↓ Firebase Crashlytics (crash reporting)
    ↓ AdMob (rewarded + interstitial ads)
    ↓ Google Play Billing (IAP)
    ↓ Cloud Run (server-side purchase validation, leaderboards)
    ↓ Cloud Storage (remotely managed assets/AB tests)
```

## Local Development
1. Clone repo
2. Open in Unity Hub with Unity 6 LTS
3. TextMeshPro import prompt → Import TMP Essentials
4. Open Scenes/Bootstrap — this is the entry point
5. Without Firebase credentials, all services run on mock implementations
6. No credentials needed for local gameplay development

## Android Build
1. File → Build Settings → Android → Switch Platform
2. Player Settings:
   - Company Name: your studio
   - Package Name: com.yourstudio.kingsmash
   - Minimum API: 26 (Android 8.0)
   - Target API: 35+
   - Scripting Backend: IL2CPP
   - Architecture: ARM64
3. Create/reference your keystore in Player Settings → Publishing Settings
4. Place `google-services.json` in Assets/ (from Firebase console)
5. Build & Run or Build APK/AAB

## Environment Configuration
Never commit credentials. Use:
- `google-services.json` (local only, gitignored)
- Unity Scripting Define Symbols for environment selection
- Firebase Remote Config for runtime value overrides
- CI/CD: inject `google-services.json` as a secret, set define symbols per build target

## Testing
### Run EditMode tests
Window → General → Test Runner → EditMode tab → Run All

### Test coverage (M0)
- SaveDataTests — serialization, migration, unique IDs
- EconomyCalculatorTests — reward math, affordability checks
- LevelConfigTests — validation, star thresholds
- ProgressionTests — level completion, coin awards, star totals

### Running via CLI
```bash
/path/to/Unity -runTests -testPlatform EditMode -projectPath . -testResults results.xml -batchmode -nographics
```

## Coding Conventions
- Namespaces: KingSmash.<Module>
- Interfaces: I-prefixed (IAuthService)
- ScriptableObjects: [CreateAssetMenu] on every config class
- Events: static Action<T> on MonoBehaviours, subscribed in OnEnable/OnDisable
- No direct Firebase calls from gameplay code — always via service interfaces
- No hard-coded balance values — all in ScriptableObject configs
- No secrets in source control

## M1 Roadmap
M1 will implement the full AIM → LAUNCH → FLIGHT → COLLISION → DAMAGE → DESTRUCTION loop:
- Complete AimController with dotted trajectory preview
- KingProjectile physics tuning
- DestructionController particle VFX
- Level win/fail state UI
- Camera follow polish
