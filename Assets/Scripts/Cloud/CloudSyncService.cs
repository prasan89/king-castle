using System;
using System.Threading.Tasks;
using KingSmash.Core;
using KingSmash.Save;
using KingSmash.Services;

namespace KingSmash.Cloud
{
    public enum SyncStatus
    {
        Idle,
        Connecting,
        Syncing,
        Synced,
        Failed,
        Offline
    }

    public sealed class CloudSyncService
    {
        public static event Action<SyncStatus> OnSyncStatusChanged;

        private readonly ICloudSaveService _cloudSave;
        private readonly ISaveService      _localSave;
        private readonly IAuthService      _auth;
        private readonly IConfigService    _config;

        private SyncStatus _status = SyncStatus.Idle;

        public SyncStatus Status => _status;

        public CloudSyncService(
            ICloudSaveService cloudSave,
            ISaveService      localSave,
            IAuthService      auth,
            IConfigService    config)
        {
            _cloudSave = cloudSave;
            _localSave = localSave;
            _auth      = auth;
            _config    = config;
        }

        public async Task<CloudSyncResult> FullSyncAsync()
        {
            if (!_auth.IsSignedIn)
            {
                SetStatus(SyncStatus.Failed);
                return new CloudSyncResult { Success = false, Error = "not_authenticated" };
            }

            var playerId = _auth.CurrentUser.UserId;

            SetStatus(SyncStatus.Connecting);

            bool online = await _cloudSave.IsOnlineAsync();
            if (!online)
            {
                SetStatus(SyncStatus.Offline);
                return new CloudSyncResult { Success = false, Error = "offline" };
            }

            SetStatus(SyncStatus.Syncing);

            CloudLoadResult loadResult;
            try
            {
                loadResult = await _cloudSave.LoadAsync(playerId);
            }
            catch (Exception ex)
            {
                GameLogger.Error("CloudSyncService", $"LoadAsync failed: {ex.Message}");
                SetStatus(SyncStatus.Failed);
                return new CloudSyncResult { Success = false, Error = ex.Message };
            }

            var localData = _localSave.Current;

            bool hadConflict = false;
            SaveData resolved;

            if (!loadResult.Success || loadResult.Data == null)
            {
                resolved = localData;
            }
            else
            {
                var cloudData = loadResult.Data;
                hadConflict = cloudData.cloudRevision > localData.cloudRevision;
                resolved = ConflictResolver.Resolve(localData, cloudData);
            }

            if (resolved != localData)
            {
                ApplyToLocalSave(resolved);
                _localSave.Save();
            }

            var payload = BuildPayload(resolved);

            CloudSaveResult saveResult;
            try
            {
                saveResult = await _cloudSave.SaveAsync(playerId, payload);
            }
            catch (Exception ex)
            {
                GameLogger.Error("CloudSyncService", $"SaveAsync failed: {ex.Message}");
                SetStatus(SyncStatus.Failed);
                return new CloudSyncResult { Success = false, Error = ex.Message };
            }

            if (saveResult.Success)
            {
                _localSave.Current.cloudRevision     = saveResult.NewRevision;
                _localSave.Current.lastSyncTimestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
                _localSave.Save();
            }

            var result = new CloudSyncResult
            {
                Success      = saveResult.Success,
                HadConflict  = hadConflict,
                Strategy     = ConflictResolutionStrategy.Merged,
                ResolvedData = resolved,
                Error        = saveResult.Success ? null : saveResult.Error
            };

            SetStatus(saveResult.Success ? SyncStatus.Synced : SyncStatus.Failed);
            CloudSaveServiceEvents.FireOnSyncCompleted(result);
            return result;
        }

        private void ApplyToLocalSave(SaveData resolved)
        {
            var local = _localSave.Current;
            local.currentLevel          = resolved.currentLevel;
            local.completedLevels       = resolved.completedLevels;
            local.starsPerLevel         = resolved.starsPerLevel;
            local.coins                 = resolved.coins;
            local.gems                  = resolved.gems;
            local.ownedProducts         = resolved.ownedProducts;
            local.kingLevel             = resolved.kingLevel;
            local.kingPower             = resolved.kingPower;
            local.kingSpeed             = resolved.kingSpeed;
            local.kingSmashRadius       = resolved.kingSmashRadius;
            local.kingArmor             = resolved.kingArmor;
            local.kingProgression       = resolved.kingProgression;
            local.ownedPowerUps         = resolved.ownedPowerUps;
            local.powerUpInventory      = resolved.powerUpInventory;
            local.missionProgress       = resolved.missionProgress;
            local.achievementProgress   = resolved.achievementProgress;
            local.dailyRewardState      = resolved.dailyRewardState;
            local.currentWorld          = resolved.currentWorld;
            local.completedWorlds       = resolved.completedWorlds;
            local.defeatedBosses        = resolved.defeatedBosses;
            local.processedLevelRewards = resolved.processedLevelRewards;
            local.economyVersion        = resolved.economyVersion;
            local.cloudRevision         = resolved.cloudRevision;
            local.cloudPlayerId         = resolved.cloudPlayerId;
            local.lastSyncTimestamp     = resolved.lastSyncTimestamp;
        }

        private static CloudSavePayload BuildPayload(SaveData data)
        {
            return new CloudSavePayload
            {
                revision           = data.cloudRevision,
                currentLevel       = data.currentLevel,
                coins              = data.coins,
                gems               = data.gems,
                kingLevel          = data.kingLevel,
                kingPower          = data.kingPower,
                kingSpeed          = data.kingSpeed,
                kingSmashRadius    = data.kingSmashRadius,
                kingArmor          = data.kingArmor,
                lastSavedTimestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
            };
        }

        private void SetStatus(SyncStatus status)
        {
            _status = status;
            OnSyncStatusChanged?.Invoke(status);
        }
    }
}
