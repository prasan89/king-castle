using System.Collections;
using UnityEngine;
using KingSmash.Core;

namespace KingSmash.VFX
{
    public class VFXParticleEffect : MonoBehaviour
    {
        [Header("Identity")]
        [SerializeField] private VFXId _vfxId;

        [Header("Particle Systems")]
        [SerializeField] private ParticleSystem[] _particleSystems;

        [Tooltip("Total lifetime before return to pool. Use -1 to auto-detect as longest ParticleSystem duration.")]
        [SerializeField] private float _overrideLifetime = -1f;

        private float _resolvedLifetime;
        private Coroutine _returnCoroutine;

        private void Awake()
        {
            _resolvedLifetime = ResolveLifetime();
        }

        private void OnEnable()
        {
            _resolvedLifetime = ResolveLifetime();
            PlayParticles();

            if (_returnCoroutine != null)
                StopCoroutine(_returnCoroutine);

            _returnCoroutine = StartCoroutine(ReturnAfterLifetime());
        }

        private void OnDisable()
        {
            if (_returnCoroutine != null)
            {
                StopCoroutine(_returnCoroutine);
                _returnCoroutine = null;
            }
            StopParticles();
        }

        private void PlayParticles()
        {
            if (_particleSystems == null) return;
            for (int i = 0; i < _particleSystems.Length; i++)
            {
                if (_particleSystems[i] != null)
                    _particleSystems[i].Play();
            }
        }

        private void StopParticles()
        {
            if (_particleSystems == null) return;
            for (int i = 0; i < _particleSystems.Length; i++)
            {
                if (_particleSystems[i] != null)
                    _particleSystems[i].Stop();
            }
        }

        private float ResolveLifetime()
        {
            if (_overrideLifetime >= 0f)
                return _overrideLifetime;

            float longest = 0.5f;
            if (_particleSystems == null) return longest;

            for (int i = 0; i < _particleSystems.Length; i++)
            {
                if (_particleSystems[i] == null) continue;
                var main = _particleSystems[i].main;
                float dur = main.startLifetime.constantMax + main.duration;
                if (dur > longest) longest = dur;
            }

            return longest;
        }

        private IEnumerator ReturnAfterLifetime()
        {
            yield return new WaitForSeconds(_resolvedLifetime);
            gameObject.SetActive(false);
            _returnCoroutine = null;
        }
    }
}
