using System;
using UnityEngine;
using KingSmash.Services;
using KingSmash.Core;

namespace KingSmash.Save
{
    public class LocalSaveService : ISaveService
    {
        private const string SaveKey = "KingSmash_SaveData";
        private SaveData _current;

        public SaveData Current => _current;

        public void Load()
        {
            try
            {
                var json = PlayerPrefs.GetString(SaveKey, null);
                if (string.IsNullOrEmpty(json))
                {
                    _current = SaveData.CreateNew();
                    GameLogger.Info("LocalSaveService", "No save found — creating new save.");
                }
                else
                {
                    _current = JsonUtility.FromJson<SaveData>(json);
                    _current = SaveMigrator.Migrate(_current);
                    GameLogger.Info("LocalSaveService", $"Save loaded. Player: {_current.playerId}, Level: {_current.currentLevel}");
                }
            }
            catch (Exception ex)
            {
                GameLogger.Error("LocalSaveService", "Save load failed — creating new save.", ex);
                _current = SaveData.CreateNew();
            }
        }

        public void Save()
        {
            try
            {
                _current.lastSavedTimestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
                var json = JsonUtility.ToJson(_current, prettyPrint: false);
                PlayerPrefs.SetString(SaveKey, json);
                PlayerPrefs.Save();
                GameLogger.Debug("LocalSaveService", "Save written.");
            }
            catch (Exception ex)
            {
                GameLogger.Error("LocalSaveService", "Save write failed.", ex);
            }
        }

        public void Delete()
        {
            PlayerPrefs.DeleteKey(SaveKey);
            _current = SaveData.CreateNew();
            GameLogger.Info("LocalSaveService", "Save deleted.");
        }

        public void AddCoins(long amount)
        {
            _current.coins += amount;
            Save();
        }

        public void SpendCoins(long amount)
        {
            if (_current.coins < amount) throw new InvalidOperationException("Insufficient coins.");
            _current.coins -= amount;
            Save();
        }

        public bool TrySpendCoins(long amount)
        {
            if (_current.coins < amount) return false;
            SpendCoins(amount);
            return true;
        }

        public void AddGems(int amount)
        {
            _current.gems += amount;
            Save();
        }

        public void RecordLevelComplete(int levelIndex, int stars)
        {
            if (!_current.completedLevels.Contains(levelIndex))
                _current.completedLevels.Add(levelIndex);

            var existing = _current.GetStarsForLevel(levelIndex);
            if (stars > existing)
                _current.SetStarsForLevel(levelIndex, stars);

            if (levelIndex >= _current.currentLevel)
                _current.currentLevel = levelIndex + 1;

            Save();
        }
    }
}
