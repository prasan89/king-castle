using System.Collections.Generic;
using UnityEngine;

namespace KingSmash.VFX
{
    [CreateAssetMenu(fileName = "VFXConfig", menuName = "KingSmash/VFX/VFXConfig")]
    public class VFXConfig : ScriptableObject
    {
        [SerializeField] private List<VFXEntry> _entries = new List<VFXEntry>();

        public List<VFXEntry> Entries => _entries;

        /// <summary>
        /// Returns the VFXEntry matching the given VFXId via linear search.
        /// Returns null if not found.
        /// </summary>
        public VFXEntry GetEntry(VFXId id)
        {
            for (int i = 0; i < _entries.Count; i++)
            {
                if (_entries[i] != null && _entries[i].id == id)
                    return _entries[i];
            }
            return null;
        }
    }
}
