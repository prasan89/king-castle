using UnityEngine;
using KingSmash.Characters;
using KingSmash.Core;

namespace KingSmash.Levels
{
    /// Invisible boundary trigger — returns King to base if it flies off screen.
    [RequireComponent(typeof(Collider2D))]
    public class LevelBoundary : MonoBehaviour
    {
        private void Awake()
        {
            var col = GetComponent<Collider2D>();
            col.isTrigger = true;
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            var king = other.GetComponent<KingProjectile>();
            if (king == null) return;
            GameLogger.Debug("LevelBoundary", "King exited level bounds.");
            // King.OnBecameInvisible handles landing; boundary just logs
        }
    }
}
