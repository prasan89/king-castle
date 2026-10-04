using System;
using System.Threading.Tasks;
using KingSmash.Cloud;
using KingSmash.Core;
using KingSmash.Save;

namespace KingSmash.Services.Mocks
{
    public class CloudSaveServiceMock : ICloudSaveService
    {
        private bool     _simulateOffline;
        private SaveData _configuredCloudData;
        private int      _revision = 0;

        public void SimulateOffline(bool offline)
        {
            _simulateOffline = offline;
            GameLogger.Info("CloudSaveServiceMock", $"SimulateOffline = {offline}");
        }

        public void ConfigureCloudData(SaveData data)
        {
            _configuredCloudData = data;
            GameLogger.Info("CloudSaveServiceMock", "Cloud data configured.");
        }

        public Task<bool> IsOnlineAsync()
        {
            return Task.FromResult(!_simulateOffline);
        }

        public Task<CloudLoadResult> LoadAsync(string playerId)
        {
            if (_simulateOffline)
                return Task.FromResult(new CloudLoadResult { Success = false, Error = "offline" });

            if (_configuredCloudData != null)
            {
                GameLogger.Info("CloudSaveServiceMock", $"LoadAsync returning configured data for {playerId}");
                return Task.FromResult(new CloudLoadResult
                {
                    Success  = true,
                    Data     = _configuredCloudData,
                    Revision = _configuredCloudData.cloudRevision
                });
            }

            GameLogger.Info("CloudSaveServiceMock", $"LoadAsync: no cloud data for {playerId}");
            return Task.FromResult(new CloudLoadResult { Success = true, Data = null, Revision = 0 });
        }

        public Task<CloudSaveResult> SaveAsync(string playerId, CloudSavePayload payload)
        {
            if (_simulateOffline)
                return Task.FromResult(new CloudSaveResult { Success = false, Error = "offline" });

            _revision = payload.revision + 1;
            GameLogger.Info("CloudSaveServiceMock", $"SaveAsync: revision -> {_revision} for {playerId}");
            return Task.FromResult(new CloudSaveResult { Success = true, NewRevision = _revision });
        }

        public Task<CloudSyncResult> SyncAsync(string playerId, SaveData localData)
        {
            if (_simulateOffline)
                return Task.FromResult(new CloudSyncResult { Success = false, Error = "offline" });

            GameLogger.Info("CloudSaveServiceMock", $"SyncAsync (stub) for {playerId}");
            return Task.FromResult(new CloudSyncResult
            {
                Success      = true,
                HadConflict  = false,
                Strategy     = ConflictResolutionStrategy.Merged,
                ResolvedData = localData
            });
        }
    }
}
