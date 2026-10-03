// M1 Test Level — Editor Setup Script
// Run this once from the Unity Editor menu to scaffold the M1_TestLevel scene.
// After running, save the scene manually.
//
// SCENE HIERARCHY (create by hand or run this):
//
// M1_TestLevel (scene)
//  ├─ LevelRoot                    (LevelSceneInitializer, LevelController, DestructionController)
//  ├─ LaunchArea
//  │   ├─ LaunchPoint              (Transform — King spawns here)
//  │   ├─ AimController            (AimController component)
//  │   └─ LaunchController         (LaunchController component)
//  ├─ Castle
//  │   ├─ CastleStructure          (CastleStructure component, autoCollectChildren=true)
//  │   ├─ Block_Wood_01            (CastleBlock, DestructibleObject, Rigidbody2D, BoxCollider2D)
//  │   ├─ Block_Wood_02
//  │   ├─ Block_Wood_03
//  │   ├─ Block_Stone_01
//  │   ├─ Block_Stone_02
//  │   ├─ Block_Metal_01
//  │   ├─ Enemy_01                 (EnemyPlaceholder, Rigidbody2D, Collider2D)
//  │   └─ Queen_01                 (QueenPlaceholder — imprisoned by Castle CastleStructure ref)
//  ├─ Ground                       (Static Rigidbody2D or no RB, BoxCollider2D)
//  ├─ LevelBoundary                (LevelBoundary, EdgeCollider2D trigger — surrounds level)
//  ├─ DebrisPool                   (DebrisPool component)
//  ├─ TrajectoryPreview            (TrajectoryPreview, LineRenderer)
//  ├─ Main Camera                  (CameraController — replaces default Camera)
//  ├─ Canvas (Screen Space Overlay)
//  │   └─ GameHUD                  (GameHUD component)
//  └─ PauseManager                 (PauseManager component)
//
// CASTLE BLOCK RECOMMENDED POSITIONS (units):
//   Ground Y = 0
//   Block_Wood_01   pos=(6, 0.5)   size=(1, 1)
//   Block_Wood_02   pos=(7, 0.5)   size=(1, 1)
//   Block_Wood_03   pos=(6.5, 1.5) size=(1, 1)  <- top of wood tower
//   Block_Stone_01  pos=(8.5, 0.5) size=(1.5, 1)
//   Block_Stone_02  pos=(8.5, 1.5) size=(1.5, 1)
//   Block_Metal_01  pos=(8.5, 2.5) size=(1, 0.5) <- metal cap
//   Enemy_01        pos=(7, 2.5)   size=(0.6, 0.9)
//   Queen_01        pos=(8.5, 3.2) size=(0.5, 0.8)
//
// LAUNCH AREA:
//   LaunchPoint     pos=(-2, 0.5)
//
// CAMERA:
//   Default pos=(-1, 3, -10), size=6 (orthographic)
//   Bounds: minX=-2, maxX=14, minY=-1, maxY=8
//
// ASSIGNED SCRIPTABLE OBJECTS:
//   AimController._projectileConfig → ProjectileConfig asset
//   LaunchController._kingPrefab    → KingProjectile prefab
//   CastleBlock._material           → Wood/Stone/Metal MaterialConfig asset
//   DestructibleObject._materialConfig → same MaterialConfig
//   LevelController._levelConfig    → M1TestLevelConfig asset
//
// PHYSICS LAYER SETUP (Project Settings → Physics 2D → Layer Collision Matrix):
//   Create layers: Ground, Castle, King, Debris
//   Debris should NOT collide with Debris (avoids debris pile physics cost)

using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace KingSmash.Levels
{
    public static class M1TestLevelSetup
    {
#if UNITY_EDITOR
        [MenuItem("KingSmash/Setup M1 Test Level")]
        public static void SetupScene()
        {
            UnityEngine.Debug.Log("[M1TestLevelSetup] See M1TestLevelSetup.cs for scene hierarchy instructions.");
            UnityEngine.Debug.Log("[M1TestLevelSetup] Create the M1_TestLevel scene manually following the documented hierarchy.");
        }
#endif
    }
}
