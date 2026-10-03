using System.Collections.Generic;
using UnityEngine;
using KingSmash.Physics;
using KingSmash.Characters;
using KingSmash.Core;
namespace KingSmash.Levels
{
    public class LevelResetController : MonoBehaviour
    {
        [Header("Roots — assign in Inspector or auto-collect")]
        [SerializeField] private Transform _castleRoot;
        [SerializeField] private Transform _enemyRoot;
        [SerializeField] private Transform _queenRoot;
        [SerializeField] private Transform _kingSpawnPoint;

        // Snapshot data
        private struct TransformSnapshot { public Vector3 pos; public Quaternion rot; public Vector3 scale; }
        private readonly List<(GameObject go, TransformSnapshot snap)> _snapshots = new();
        private readonly List<(Rigidbody2D rb, Vector2 vel, float angVel)> _rbSnapshots = new();

        private void Awake()
        {
            CaptureSnapshots(_castleRoot);
            CaptureSnapshots(_enemyRoot);
            CaptureSnapshots(_queenRoot);
        }

        private void CaptureSnapshots(Transform root)
        {
            if (root == null) return;
            foreach (Transform t in root.GetComponentsInChildren<Transform>(includeInactive: true))
            {
                _snapshots.Add((t.gameObject, new TransformSnapshot
                {
                    pos   = t.position,
                    rot   = t.rotation,
                    scale = t.localScale,
                }));
                var rb = t.GetComponent<Rigidbody2D>();
                if (rb != null) _rbSnapshots.Add((rb, Vector2.zero, 0f));
            }
        }

        public void ResetLevel()
        {
            GameLogger.Info("LevelResetController", "Resetting level...");

            // Re-enable and reposition all tracked GameObjects
            foreach (var (go, snap) in _snapshots)
            {
                if (go == null) continue;
                go.SetActive(true);
                go.transform.position   = snap.pos;
                go.transform.rotation   = snap.rot;
                go.transform.localScale = snap.scale;
            }

            // Zero out all rigidbodies
            foreach (var (rb, _, __) in _rbSnapshots)
            {
                if (rb == null) continue;
                rb.linearVelocity = Vector2.zero;
                rb.angularVelocity = 0f;
            }

            // Reset destructible health
            foreach (var d in GetComponentsInChildren<DestructibleObject>(includeInactive: true))
                d.ResetHealth();

            GameLogger.Info("LevelResetController", "Level reset complete.");
        }
    }
}
