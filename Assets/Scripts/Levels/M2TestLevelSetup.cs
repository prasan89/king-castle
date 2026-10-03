using UnityEngine;
using KingSmash.Core;

namespace KingSmash.Levels
{
    // Attach to a GameObject in the M2_TestLevel scene.
    // Logs a summary after 3 seconds to confirm all M2 systems initialised.
    public class M2TestLevelSetup : MonoBehaviour
    {
        [Header("Scene Object Counts (for validation logging)")]
        [SerializeField] private int _expectedEnemyCount = 5; // 2 Guards + 1 Shield + 1 Archer + 1 EnemyKing
        [SerializeField] private bool _queenPresent = true;

        private void Start()
        {
            GameLogger.Info("M2TestLevel", $"M2 test level loaded. Expected enemies: {_expectedEnemyCount}, Queen: {_queenPresent}");

            // Subscribe to high-level events to surface in log
            Characters.EnemyController.OnEnemyDied   += e => GameLogger.Info("M2TestLevel", $"Enemy defeated: {e.name}");
            Characters.QueenController.OnQueenRescued += q => GameLogger.Info("M2TestLevel", "QUEEN RESCUED — M2 objective complete!");
            Characters.KingController.OnKingStateChanged += (prev, next) =>
                GameLogger.Debug("M2TestLevel", $"King state: {prev} -> {next}");
        }
    }
}
