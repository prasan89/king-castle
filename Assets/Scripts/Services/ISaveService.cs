using KingSmash.Save;

namespace KingSmash.Services
{
    public interface ISaveService
    {
        SaveData Current { get; }
        void Load();
        void Save();
        void Delete();
        void AddCoins(long amount);
        void SpendCoins(long amount);
        bool TrySpendCoins(long amount);
        void AddGems(int amount);
        void RecordLevelComplete(int levelIndex, int stars);
    }
}
