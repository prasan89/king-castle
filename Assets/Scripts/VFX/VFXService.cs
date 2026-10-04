using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace KingSmash.VFX
{
    /// <summary>
    /// MonoBehaviour VFX service.
    /// Pre-warms per-VFXId object pools at Awake so Play() never allocates.
    /// </summary>
    [DefaultExecutionOrder(-800)]
    public class VFXService : MonoBehaviour, IVFXService
    {
        [SerializeField] private VFXConfig _vfxConfig;

        // Pools keyed by VFXId — pre-warmed in InitializePools().
        private readonly Dictionary<VFXId, Queue<GameObject>> _pools
            = new Dictionary<VFXId, Queue<GameObject>>();

        // A flat list of all pooled instances for StopAll().
        private readonly List<GameObject> _allInstances = new List<GameObject>();

        // ─────────────────────────────────────────────────────────────────────
        #region Unity lifecycle

        private void Awake()
        {
            if (_vfxConfig == null)
            {
                Core.GameLogger.Warning("VFXService", "VFXConfig not assigned.");
                return;
            }
            InitializePools();
        }

        #endregion

        // ─────────────────────────────────────────────────────────────────────
        #region Pool initialisation

        private void InitializePools()
        {
            var entries = _vfxConfig.Entries;
            for (int i = 0; i < entries.Count; i++)
            {
                VFXEntry entry = entries[i];
                if (entry == null || entry.prefab == null) continue;

                if (!_pools.TryGetValue(entry.id, out Queue<GameObject> pool))
                {
                    pool = new Queue<GameObject>(entry.poolSize);
                    _pools[entry.id] = pool;
                }

                for (int j = 0; j < entry.poolSize; j++)
                {
                    GameObject instance = CreateInstance(entry);
                    pool.Enqueue(instance);
                }
            }
        }

        private GameObject CreateInstance(VFXEntry entry)
        {
            GameObject go = Instantiate(entry.prefab, transform);
            go.name = $"{entry.prefab.name}_{entry.id}";
            go.SetActive(false);
            _allInstances.Add(go);
            return go;
        }

        #endregion

        // ─────────────────────────────────────────────────────────────────────
        #region IVFXService

        public void Play(VFXId id, Vector3 position, Quaternion rotation = default)
        {
            GameObject go = GetFromPool(id);
            if (go == null) return;

            go.transform.SetParent(transform, false);
            go.transform.position = position;
            go.transform.rotation = rotation == default ? Quaternion.identity : rotation;
            go.SetActive(true);

            VFXEntry entry = _vfxConfig.GetEntry(id);
            if (entry != null && entry.autoReturn)
                StartCoroutine(ReturnAfterDelay(go, id, entry.lifetime));
        }

        public void Play(VFXId id, Transform parent, bool worldPositionStays = true)
        {
            GameObject go = GetFromPool(id);
            if (go == null) return;

            go.transform.SetParent(parent, worldPositionStays);
            if (!worldPositionStays)
                go.transform.localPosition = Vector3.zero;
            go.SetActive(true);

            VFXEntry entry = _vfxConfig.GetEntry(id);
            if (entry != null && entry.autoReturn)
                StartCoroutine(ReturnAfterDelay(go, id, entry.lifetime));
        }

        public void StopAll()
        {
            for (int i = 0; i < _allInstances.Count; i++)
            {
                if (_allInstances[i] != null && _allInstances[i].activeSelf)
                    _allInstances[i].SetActive(false);
            }
        }

        #endregion

        // ─────────────────────────────────────────────────────────────────────
        #region Internal helpers

        /// <summary>
        /// Gets a pooled instance. Does NOT allocate in the steady state.
        /// Expands pool by one if empty (safety valve, should not happen normally).
        /// </summary>
        private GameObject GetFromPool(VFXId id)
        {
            if (!_pools.TryGetValue(id, out Queue<GameObject> pool))
            {
                Core.GameLogger.Warning("VFXService", $"No pool for VFXId: {id}");
                return null;
            }

            if (pool.Count > 0)
                return pool.Dequeue();

            // Safety: expand pool if exhausted
            VFXEntry entry = _vfxConfig.GetEntry(id);
            if (entry == null || entry.prefab == null) return null;

            Core.GameLogger.Warning("VFXService", $"Pool exhausted for VFXId: {id} — expanding.");
            return CreateInstance(entry);
        }

        private IEnumerator ReturnAfterDelay(GameObject go, VFXId id, float delay)
        {
            yield return new WaitForSeconds(delay);
            ReturnToPool(go, id);
        }

        private void ReturnToPool(GameObject go, VFXId id)
        {
            if (go == null) return;
            go.SetActive(false);
            go.transform.SetParent(transform, false);

            if (_pools.TryGetValue(id, out Queue<GameObject> pool))
                pool.Enqueue(go);
        }

        #endregion
    }
}
