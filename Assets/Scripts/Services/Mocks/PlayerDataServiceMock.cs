using System.Threading.Tasks;
using KingSmash.Core;
using KingSmash.Save;

namespace KingSmash.Services
{
    public class PlayerDataServiceMock : IPlayerDataService
    {
        public Task<SaveData> LoadPlayerDataAsync(string playerId)
        {
            GameLogger.Info("PlayerDataServiceMock", $"LoadPlayerData: {playerId}");
            return Task.FromResult(SaveData.CreateNew());
        }

        public Task SavePlayerDataAsync(string playerId, SaveData data)
        {
            GameLogger.Info("PlayerDataServiceMock", $"SavePlayerData: {playerId}");
            return Task.CompletedTask;
        }

        public Task<bool> DeletePlayerDataAsync(string playerId)
        {
            GameLogger.Info("PlayerDataServiceMock", $"DeletePlayerData: {playerId}");
            return Task.FromResult(true);
        }
    }
}
