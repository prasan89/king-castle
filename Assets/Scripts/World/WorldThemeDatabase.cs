using System.Collections.Generic;
using UnityEngine;
using KingSmash.VFX;

namespace KingSmash.World
{
    [CreateAssetMenu(fileName = "WorldThemeDatabase", menuName = "KingSmash/World/WorldThemeDatabase")]
    public class WorldThemeDatabase : ScriptableObject
    {
        [SerializeField] private List<WorldTheme> _themes = new List<WorldTheme>();

        public IReadOnlyList<WorldTheme> Themes => _themes;

        public WorldTheme GetTheme(int worldIndex)
        {
            var targetId = (WorldAtmosphereController.WorldId)worldIndex;
            return GetTheme(targetId);
        }

        public WorldTheme GetTheme(WorldAtmosphereController.WorldId worldId)
        {
            for (int i = 0; i < _themes.Count; i++)
            {
                if (_themes[i] != null && _themes[i].worldId == worldId)
                    return _themes[i];
            }
            return null;
        }
    }
}
