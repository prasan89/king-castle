using UnityEngine;
using KingSmash.Audio;
using KingSmash.VFX;

namespace KingSmash.World
{
    [CreateAssetMenu(fileName = "WorldTheme", menuName = "KingSmash/World/WorldTheme")]
    public class WorldTheme : ScriptableObject
    {
        [Header("Identity")]
        public WorldAtmosphereController.WorldId worldId = WorldAtmosphereController.WorldId.Forest;
        public string worldName = "Forest Kingdom";

        [Header("Sky Gradient")]
        public Color skyColorTop    = new Color(0.35f, 0.65f, 0.30f, 1f);
        public Color skyColorBottom = new Color(0.70f, 0.88f, 0.55f, 1f);

        [Header("Ambient Lighting")]
        public Color ambientLightColor     = new Color(0.85f, 0.95f, 0.75f, 1f);
        [Range(0f, 2f)]
        public float ambientLightIntensity = 1f;

        [Header("Fog")]
        public Color fogColor  = new Color(0.75f, 0.90f, 0.65f, 1f);
        public bool  enableFog = false;

        [Header("Audio")]
        public SoundId worldMusic    = SoundId.World1;
        public SoundId bossMusic     = SoundId.BossBattle;
        [Range(0f, 1f)]
        public float   ambientVolume = 0.3f;

        [Header("Castle Accent Colors")]
        [Tooltip("Exactly 3 entries: [0] main, [1] shadow, [2] highlight.")]
        public Color[] castleAccentColors = new Color[3]
        {
            new Color(0.55f, 0.40f, 0.22f, 1f),
            new Color(0.30f, 0.22f, 0.10f, 1f),
            new Color(0.80f, 0.65f, 0.42f, 1f)
        };

        [Header("Terrain")]
        public Color terrainTint = new Color(0.55f, 0.75f, 0.35f, 1f);

        [Header("Ambient Particles")]
        [Tooltip("Exactly 3 entries forming a gradient: start, mid, end.")]
        public Color[] particleColors = new Color[3]
        {
            new Color(0.45f, 0.75f, 0.25f, 0.80f),
            new Color(0.60f, 0.88f, 0.40f, 0.50f),
            new Color(0.75f, 0.95f, 0.55f, 0.15f)
        };

        [Range(0f, 1f)]
        public float particleDensity = 0.5f;
    }
}
