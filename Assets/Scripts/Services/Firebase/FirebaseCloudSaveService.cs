// FirebaseCloudSaveService — implements ICloudSaveService.
// Real implementation gated on #if FIREBASE_ENABLED.
// Stub always compiles so editor builds succeed without the Firebase SDK.

using System;
using System.Text;
using System.Threading.Tasks;
using KingSmash.Cloud;
using KingSmash.Core;
using KingSmash.Save;
using KingSmash.Services;

namespace KingSmash.Services.Firebase
{
    public class FirebaseCloudSaveService : ICloudSaveService
    {
        private readonly IAuthService _auth;

        public FirebaseCloudSaveService(IAuthService auth)
        {
            _auth = auth;
        }

#if FIREBASE_ENABLED
        public async Task<CloudLoadResult> LoadAsync(string playerId)
        {
            try
            {
                var db = global::Firebase.Firestore.FirebaseFirestore.DefaultInstance;
                var progressionRef = db.Document($"players/{playerId}/progression/main");
                var snap = await progressionRef.GetSnapshotAsync();

                if (!snap.Exists)
                    return new CloudLoadResult { Success = true, Data = null, Revision = 0 };

                var json = snap.GetValue<string>("saveDataJson");
                var data = UnityEngine.JsonUtility.FromJson<SaveData>(json);
                int revision = snap.ContainsField("revision") ? snap.GetValue<int>("revision") : 0;

                return new CloudLoadResult { Success = true, Data = data, Revision = revision };
            }
            catch (Exception ex)
            {
                GameLogger.Error("FirebaseCloudSave", $"LoadAsync error: {ex.Message}");
                return new CloudLoadResult { Success = false, Error = ex.Message };
            }
        }

        public async Task<CloudSaveResult> SaveAsync(string playerId, CloudSavePayload payload)
        {
            try
            {
                var db = global::Firebase.Firestore.FirebaseFirestore.DefaultInstance;
                int newRevision = payload.revision + 1;
                var progressionRef = db.Document($"players/{playerId}/progression/main");

                var updates = new System.Collections.Generic.Dictionary<global::Firebase.Firestore.FieldPath, object>
                {
                    [new global::Firebase.Firestore.FieldPath("revision")]          = newRevision,
                    [new global::Firebase.Firestore.FieldPath("currentLevel")]      = payload.currentLevel,
                    [new global::Firebase.Firestore.FieldPath("coins")]             = payload.coins,
                    [new global::Firebase.Firestore.FieldPath("gems")]              = payload.gems,
                    [new global::Firebase.Firestore.FieldPath("kingLevel")]         = payload.kingLevel,
                    [new global::Firebase.Firestore.FieldPath("lastSavedTimestamp")] = payload.lastSavedTimestamp,
                    [new global::Firebase.Firestore.FieldPath("updatedAt")]         = global::Firebase.Firestore.FieldValue.ServerTimestamp
                };

                await progressionRef.UpdateAsync(updates);
                return new CloudSaveResult { Success = true, NewRevision = newRevision };
            }
            catch (Exception ex)
            {
                GameLogger.Error("FirebaseCloudSave", $"SaveAsync error: {ex.Message}");
                return new CloudSaveResult { Success = false, Error = ex.Message };
            }
        }

        public async Task<CloudSyncResult> SyncAsync(string playerId, SaveData localData)
        {
            try
            {
                var url   = $"{FirebaseEnvironmentConfig.CloudRunGameApiUrl}/api/v1/progression/sync";
                var token = await GetIdTokenAsync();
                var json  = UnityEngine.JsonUtility.ToJson(localData);
                var body  = Encoding.UTF8.GetBytes(json);

                using var req = UnityEngine.Networking.UnityWebRequest.Put(url, body);
                req.method = "POST";
                req.SetRequestHeader("Content-Type", "application/json");
                req.SetRequestHeader("Authorization", $"Bearer {token}");
                req.SetRequestHeader("X-Idempotency-Key", $"{playerId}-{localData.cloudRevision}");
                await req.SendWebRequest();

                if (req.result != UnityEngine.Networking.UnityWebRequest.Result.Success)
                    return new CloudSyncResult { Success = false, Error = req.error };

                return new CloudSyncResult { Success = true, Strategy = ConflictResolutionStrategy.Merged, ResolvedData = localData };
            }
            catch (Exception ex)
            {
                GameLogger.Error("FirebaseCloudSave", $"SyncAsync error: {ex.Message}");
                return new CloudSyncResult { Success = false, Error = ex.Message };
            }
        }

        public async Task<bool> IsOnlineAsync()
        {
            try
            {
                var url = $"{FirebaseEnvironmentConfig.CloudRunGameApiUrl}/health";
                using var req = UnityEngine.Networking.UnityWebRequest.Get(url);
                await req.SendWebRequest();
                return req.result == UnityEngine.Networking.UnityWebRequest.Result.Success;
            }
            catch
            {
                return false;
            }
        }

        private async Task<string> GetIdTokenAsync()
        {
            var user = global::Firebase.Auth.FirebaseAuth.DefaultInstance.CurrentUser;
            if (user == null) return string.Empty;
            return await user.TokenAsync(false);
        }
#else
        public Task<CloudLoadResult> LoadAsync(string playerId)
        {
            GameLogger.Warning("FirebaseCloudSave", "Firebase not enabled — stub LoadAsync.");
            return Task.FromResult(new CloudLoadResult { Success = false, Error = "firebase_disabled" });
        }

        public Task<CloudSaveResult> SaveAsync(string playerId, CloudSavePayload payload)
        {
            GameLogger.Warning("FirebaseCloudSave", "Firebase not enabled — stub SaveAsync.");
            return Task.FromResult(new CloudSaveResult { Success = false, Error = "firebase_disabled" });
        }

        public Task<CloudSyncResult> SyncAsync(string playerId, SaveData localData)
        {
            GameLogger.Warning("FirebaseCloudSave", "Firebase not enabled — stub SyncAsync.");
            return Task.FromResult(new CloudSyncResult { Success = false, Error = "firebase_disabled" });
        }

        public Task<bool> IsOnlineAsync()
            => Task.FromResult(false);
#endif
    }
}
