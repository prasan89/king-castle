using KingSmash.Characters;

namespace KingSmash.Levels
{
    public class RescueQueenObjective : UnityEngine.MonoBehaviour, ILevelObjective
    {
        private bool _rescued;

        public string DisplayName => "Rescue the Queen";
        public ObjectiveStatus Status { get; private set; } = ObjectiveStatus.InProgress;
        public float ProgressNormalized => _rescued ? 1f : 0f;

        public void Initialize()
        {
            _rescued = false;
            Status = ObjectiveStatus.InProgress;
        }

        public void Tick() { }

        private void OnEnable()  => QueenController.OnQueenRescued += HandleRescued;
        private void OnDisable() => QueenController.OnQueenRescued -= HandleRescued;

        private void HandleRescued(QueenController _)
        {
            _rescued = true;
            Status = ObjectiveStatus.Completed;
        }
    }
}
