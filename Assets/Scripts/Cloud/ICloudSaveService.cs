using System;
using System.Threading.Tasks;
using KingSmash.Save;

namespace KingSmash.Cloud
{
    public enum ConflictResolutionStrategy
    {
        ServerWins,
        ClientWins,
        Merged,
        ManualRequired
    }

    public class CloudLoadResult
    {
        public bool     Success;
        public SaveData Data;
        public string   Error;
        public int      Revision;
    }

    public class CloudSaveResult
    {
        public bool   Success;
        public int    NewRevision;
        public string Error;
    }

    public class CloudSyncResult
    {
        public bool                      Success;
        public bool                      HadConflict;
        public ConflictResolutionStrategy Strategy;
        public SaveData                  ResolvedData;
        public string                    Error;
    }

    public class CloudSavePayload
    {
        public int    revision;
        public int    currentLevel;
        public long   coins;
        public int    gems;
        public int    kingLevel;
        public float  kingPower;
        public float  kingSpeed;
        public float  kingSmashRadius;
        public float  kingArmor;
        public long   lastSavedTimestamp;
    }

    public interface ICloudSaveService
    {
        Task<CloudLoadResult> LoadAsync(string playerId);
        Task<CloudSaveResult> SaveAsync(string playerId, CloudSavePayload payload);
        Task<CloudSyncResult> SyncAsync(string playerId, SaveData localData);
        Task<bool>            IsOnlineAsync();
    }

    public static class CloudSaveServiceEvents
    {
        public static event Action<CloudSyncResult> OnSyncCompleted;
        public static void FireOnSyncCompleted(CloudSyncResult result) => OnSyncCompleted?.Invoke(result);
    }
}
