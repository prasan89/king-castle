using UnityEngine;
using KingSmash.Core;
using KingSmash.Gameplay;
using KingSmash.Physics;
using KingSmash.UI;

namespace KingSmash.Levels
{
    /// Wires up all scene references at startup.
    /// Place on a single "LevelRoot" GameObject in the Level/M1_TestLevel scene.
    public class LevelSceneInitializer : MonoBehaviour
    {
        [Header("Config")]
        [SerializeField] private LevelConfig _levelConfig;

        [Header("Controllers")]
        [SerializeField] private LevelController _levelController;
        [SerializeField] private LaunchController _launchController;
        [SerializeField] private DestructionController _destructionController;
        [SerializeField] private AimController _aimController;
        [SerializeField] private CameraController _cameraController;

        [Header("Castle Structures")]
        [SerializeField] private CastleStructure[] _castles;

        [Header("UI")]
        [SerializeField] private GameHUD _hud;

        private void Awake()
        {
            // Register castles with destruction tracker
            foreach (var castle in _castles)
                if (castle != null) _destructionController?.RegisterCastle(castle);
        }

        private void Start()
        {
            int levelIndex = _levelConfig != null ? _levelConfig.levelIndex : 0;
            int launches   = _levelConfig != null ? _levelConfig.kingLaunches : 3;
            _hud?.Initialize(levelIndex, launches, _levelController);
        }
    }
}
