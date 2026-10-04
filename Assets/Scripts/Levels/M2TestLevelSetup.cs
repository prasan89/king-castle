using UnityEngine;
using KingSmash.Core;

namespace KingSmash.Levels
{
    // Attach to a GameObject in the M2_TestLevel scene.
    // Logs a summary after scene load to confirm all M2 systems initialised.
    //
    // M15 SECURITY FIX: Wrapped all debug event subscriptions and info logging
    // in #if UNITY_EDITOR so that the component is a safe no-op in any non-editor
    // build (Development, Staging, Production).  The class itself is kept so that
    // any scene that already holds a reference to it does not break; outside the
    // editor the Start() method simply does nothing.
    public class M2TestLevelSetup : MonoBehaviour
    {
        [Header("Scene Object Counts (for validation logging)")]
        [SerializeField] private int _expectedEnemyCount = 5; // 2 Guards + 1 Shield + 1 Archer + 1 EnemyKing
        [SerializeField] private bool _queenPresent = true;

#if UNITY_EDITOR
        private void Start()
        {
            GameLogger.Info("M2TestLevel", $"M2 test level loaded. Expected enemies: {_expectedEnemyCount}, Queen: {_queenPresent}");

            // Subscribe to high-level events to surface in log
            Characters.EnemyController.OnEnemyDied    += e => GameLogger.Info("M2TestLevel", $"Enemy defeated: {e.name}");
            Characters.QueenController.OnQueenRescued  += q => GameLogger.Info("M2TestLevel", "QUEEN RESCUED — M2 objective complete!");
            Characters.KingController.OnKingStateChanged += (prev, next) =>
                GameLogger.Debug("M2TestLevel", $"King state: {prev} -> {next}");
        }
#else
        // Non-editor builds: Start is a deliberate no-op.
        // This prevents internal scene counts, event names, and state transitions
        // from appearing in logs or being discoverable via reverse engineering.
        private void Start() { }
#endif
    }
}
