using System.Collections.Generic;
using UnityEngine;
using KingSmash.Core;

namespace KingSmash.Physics
{
    /// Lightweight object pool for debris particles.
    /// Android-safe: fixed pool size, no GC allocation during gameplay.
    public class DebrisPool : MonoBehaviour
    {
        [SerializeField] private GameObject _debrisPrefab;
        [SerializeField] private int _poolSize = 32;
        [SerializeField] private float _debrisLifetime = 2.5f;

        private readonly Queue<GameObject> _available = new();
        private readonly List<(GameObject obj, float expiry)> _active = new();

        public static DebrisPool Instance { get; private set; }

        private void Awake()
        {
            if (Instance != null) { Destroy(gameObject); return; }
            Instance = this;
            Prewarm();
        }

        private void Prewarm()
        {
            if (_debrisPrefab == null)
            {
                GameLogger.Warning("DebrisPool", "No debris prefab assigned — pool disabled.");
                return;
            }
            for (int i = 0; i < _poolSize; i++)
            {
                var go = Instantiate(_debrisPrefab, transform);
                go.SetActive(false);
                _available.Enqueue(go);
            }
        }

        private void Update()
        {
            float now = Time.time;
            for (int i = _active.Count - 1; i >= 0; i--)
            {
                if (now >= _active[i].expiry)
                {
                    Return(_active[i].obj);
                    _active.RemoveAt(i);
                }
            }
        }

        /// Spawn a debris particle at position with random velocity in a cone.
        public void Spawn(Vector2 position, MaterialConfig material, int count)
        {
            int spawnCount = Mathf.Min(count, _available.Count);
            for (int i = 0; i < spawnCount; i++)
            {
                var go = _available.Dequeue();
                go.transform.position = position;
                go.transform.localScale = Vector3.one * (material?.breakParticleScale ?? 1f);

                // Set colour to match material
                var sr = go.GetComponent<SpriteRenderer>();
                if (sr != null && material != null) sr.color = material.debugColor;

                // Random scatter velocity
                var rb = go.GetComponent<Rigidbody2D>();
                if (rb != null)
                {
                    var angle = Random.Range(-80f, 80f);
                    var speed = Random.Range(2f, 6f);
                    rb.linearVelocity = Quaternion.Euler(0, 0, angle) * Vector2.up * speed;
                    rb.angularVelocity = Random.Range(-360f, 360f);
                }

                go.SetActive(true);
                _active.Add((go, Time.time + _debrisLifetime));
            }
        }

        private void Return(GameObject go)
        {
            go.SetActive(false);
            var rb = go.GetComponent<Rigidbody2D>();
            if (rb != null) { rb.linearVelocity = Vector2.zero; rb.angularVelocity = 0f; }
            _available.Enqueue(go);
        }
    }
}
