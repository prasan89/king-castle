using UnityEngine;

namespace KingSmash.VFX
{
    /// <summary>
    /// Describes a single VFX prefab entry used inside VFXConfig.
    /// Not a ScriptableObject — it lives as an embedded serialized element in the config list.
    /// </summary>
    [System.Serializable]
    public class VFXEntry
    {
        public VFXId    id;
        public GameObject prefab;
        [Min(1)]
        public int      poolSize   = 4;
        public float    lifetime   = 2f;
        public bool     autoReturn = true;
    }
}
