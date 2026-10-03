using System.Collections.Generic;
using UnityEngine;
using KingSmash.Physics;

namespace KingSmash.Levels
{
    public class DestroyCastleObjective : MonoBehaviour, ILevelObjective
    {
        [SerializeField] private List<CastleStructure> _targets;

        private int _destroyedCount;

        public string DisplayName => "Destroy the Castle";
        public ObjectiveStatus Status { get; private set; } = ObjectiveStatus.InProgress;
        public float ProgressNormalized => _targets.Count > 0 ? (float)_destroyedCount / _targets.Count : 0f;

        public void Initialize()
        {
            _destroyedCount = 0;
            Status = ObjectiveStatus.InProgress;
        }

        public void Tick() { }    // driven by events

        private void OnEnable()  => CastleStructure.OnCastleDestroyed  += HandleCastleDestroyed;
        private void OnDisable() => CastleStructure.OnCastleDestroyed  -= HandleCastleDestroyed;

        private void HandleCastleDestroyed(CastleStructure castle)
        {
            if (!_targets.Contains(castle)) return;
            _destroyedCount++;
            if (_destroyedCount >= _targets.Count)
                Status = ObjectiveStatus.Completed;
        }
    }
}
