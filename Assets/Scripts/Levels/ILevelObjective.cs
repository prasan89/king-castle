namespace KingSmash.Levels
{
    public enum ObjectiveStatus { InProgress, Completed, Failed }
    public interface ILevelObjective
    {
        string DisplayName { get; }
        ObjectiveStatus Status { get; }
        float ProgressNormalized { get; }   // 0..1 for HUD
        void Initialize();
        void Tick();                         // called each frame while level is Playing
    }
}
