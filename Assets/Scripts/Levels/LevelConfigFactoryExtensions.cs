using System.Collections.Generic;

namespace KingSmash.Levels
{
    public static class LevelConfigFactoryExtensions
    {
        public static LevelDefinition Get(this IReadOnlyList<LevelDefinition> levels, int levelIndex)
        {
            for (int i = 0; i < levels.Count; i++)
            {
                if (levels[i].LevelIndex == levelIndex)
                    return levels[i];
            }
            return null;
        }

        public static int MaxPowerUpsForLevel(this IReadOnlyList<LevelDefinition> levels, int levelIndex)
        {
            var def = levels.Get(levelIndex);
            return def != null ? def.MaxPowerUpsAllowed : 2;
        }
    }
}
