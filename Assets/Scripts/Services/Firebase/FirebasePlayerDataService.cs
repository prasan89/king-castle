// Cloud Firestore implementation of IPlayerDataService.
// SETUP REQUIRED: Firebase Firestore Unity SDK + credentials (see FirebaseAuthService.cs)
// Collection path: players/{playerId}
//
// WARNING: Never trust data read from Firestore for purchase validation.
// Purchases must be validated server-side via Cloud Run.

using System.Threading.Tasks;
using KingSmash.Core;
using KingSmash.Save;

namespace KingSmash.Services.Firebase
{
    public class FirebasePlayerDataService : IPlayerDataService
    {
        private const string Collection = "players";

        public async Task<SaveData> LoadPlayerDataAsync(string playerId)
        {
            // var doc = await FirebaseFirestore.DefaultInstance
            //     .Collection(Collection).Document(playerId).GetSnapshotAsync();
            // return doc.Exists ? doc.ConvertTo<SaveData>() : SaveData.CreateNew();
            GameLogger.Warning("FirebasePlayerDataService", "Firebase SDK not yet imported — returning new save.");
            return await Task.FromResult(SaveData.CreateNew());
        }

        public async Task SavePlayerDataAsync(string playerId, SaveData data)
        {
            // await FirebaseFirestore.DefaultInstance
            //     .Collection(Collection).Document(playerId).SetAsync(data);
            GameLogger.Warning("FirebasePlayerDataService", "Firebase SDK not yet imported — save skipped.");
            await Task.CompletedTask;
        }

        public async Task<bool> DeletePlayerDataAsync(string playerId)
        {
            // await FirebaseFirestore.DefaultInstance
            //     .Collection(Collection).Document(playerId).DeleteAsync();
            GameLogger.Warning("FirebasePlayerDataService", "Firebase SDK not yet imported — delete skipped.");
            return await Task.FromResult(true);
        }
    }
}
