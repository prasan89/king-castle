using UnityEngine;

namespace KingSmash.Save
{
    public static class SaveMigrator
    {
        public static SaveData Migrate(SaveData data)
        {
            if (data.version == SaveData.CurrentVersion) return data;
            GameLogger.Info("SaveMigrator", $"Migrating save from v{data.version} to v{SaveData.CurrentVersion}");

            if (data.version < 2)
            {
                if (data.kingProgression == null)
                    data.kingProgression = new KingSmash.Progression.KingProgression();

                data.kingProgression.KingLevel        = Mathf.Max(1, data.kingLevel);
                data.kingProgression.PowerLevel       = 1;
                data.kingProgression.SpeedLevel       = 1;
                data.kingProgression.SmashRadiusLevel = 1;
                data.kingProgression.ArmorLevel       = 1;

                GameLogger.Info("SaveMigrator",
                    $"v1->v2: kingProgression initialised (KingLevel={data.kingProgression.KingLevel})");
            }

            if (data.version < 3)
            {
                if (data.ownedPowerUps == null)
                    data.ownedPowerUps = new System.Collections.Generic.List<string>();
                if (data.powerUpInventory == null)
                    data.powerUpInventory = new KingSmash.PowerUps.PowerUpInventory();

                GameLogger.Info("SaveMigrator", "v2->v3: powerUpInventory initialised");
            }

            if (data.version < 4)
            {
                if (data.economyVersion == 0)
                    data.economyVersion = 1;

                GameLogger.Info("SaveMigrator", "v3->v4: economyVersion initialised");
            }

            data.version = SaveData.CurrentVersion;
            return data;
        }
    }
}
