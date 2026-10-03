using UnityEngine;
using KingSmash.Characters;

namespace KingSmash.Levels
{
    public class DefeatEnemiesObjective : MonoBehaviour, ILevelObjective
    {
        [SerializeField] private int _targetCount = 3;

        private int _defeatedCount;

        public string DisplayName => $"Defeat {_targetCount} Enemies";
        public ObjectiveStatus Status { get; private set; } = ObjectiveStatus.InProgress;
        public float ProgressNormalized => _targetCount > 0 ? (float)_defeatedCount / _targetCount : 0f;

        public void Initialize()
        {
            _defeatedCount = 0;
            Status = ObjectiveStatus.InProgress;
        }

        public void Tick() { }    // driven by events

        private void OnEnable()  => EnemyController.OnEnemyDied += HandleEnemyDied;
        private void OnDisable() => EnemyController.OnEnemyDied -= HandleEnemyDied;

        private void HandleEnemyDied(EnemyController enemy)
        {
            _defeatedCount++;
            if (_defeatedCount >= _targetCount)
                Status = ObjectiveStatus.Completed;
        }
    }
}
