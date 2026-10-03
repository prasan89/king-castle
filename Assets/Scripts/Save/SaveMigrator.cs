using UnityEngine;

namespace KingSmash.Save
{
    public static class SaveMigrator
    {
        public static SaveData Migrate(SaveData data)
        {
            if (data.version == SaveData.CurrentVersion) return data;

            GameLogger.Info("SaveMigrator", $"Migrating save from v{data.version} to v{SaveData.CurrentVersion}");

            // Version migration chain — add cases here as save schema evolves
            // Example: if (data.version < 2) MigrateV1ToV2(data);

            data.version = SaveData.CurrentVersion;
            return data;
        }
    }
}
