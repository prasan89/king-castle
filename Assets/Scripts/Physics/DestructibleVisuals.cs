using System.Collections;
using UnityEngine;

namespace KingSmash.Physics
{
    [RequireComponent(typeof(DestructibleObject))]
    public class DestructibleVisuals : MonoBehaviour
    {
        [SerializeField] private MaterialVFXConfig _visualConfig;
        [SerializeField] private SpriteRenderer    _renderer;

        private DestructibleObject _destructible;
        private Coroutine _flashCoroutine;

        private const float FlashDuration = 0.12f;

        private void Awake()
        {
            _destructible = GetComponent<DestructibleObject>();

            if (_renderer == null)
                _renderer = GetComponent<SpriteRenderer>();
        }

        private void OnEnable()
        {
            DestructibleObject.OnDamaged   += HandleDamaged;
            DestructibleObject.OnDestroyed += HandleDestroyed;
            ApplyIntactVisuals();
        }

        private void OnDisable()
        {
            DestructibleObject.OnDamaged   -= HandleDamaged;
            DestructibleObject.OnDestroyed -= HandleDestroyed;

            if (_flashCoroutine != null)
            {
                StopCoroutine(_flashCoroutine);
                _flashCoroutine = null;
            }
        }

        private void HandleDamaged(DestructibleObject obj, float damage)
        {
            if (obj != _destructible) return;
            if (_renderer == null || _visualConfig == null) return;

            float healthNorm = _destructible.HealthNormalized;
            Color targetColor = Color.Lerp(_visualConfig.damagedTint, _visualConfig.intactTint, healthNorm);
            _renderer.color = targetColor;

            if (healthNorm <= 0.5f)
                ApplyDamagedSprite();
        }

        private void HandleDestroyed(DestructibleObject obj)
        {
            if (obj != _destructible) return;

            if (_flashCoroutine != null)
                StopCoroutine(_flashCoroutine);

            _flashCoroutine = StartCoroutine(DestroyedFlash());
        }

        private void ApplyIntactVisuals()
        {
            if (_renderer == null || _visualConfig == null) return;
            _renderer.color = _visualConfig.intactTint;

            if (_visualConfig.intactSprites != null && _visualConfig.intactSprites.Length > 0)
                _renderer.sprite = _visualConfig.intactSprites[
                    Random.Range(0, _visualConfig.intactSprites.Length)];
        }

        private void ApplyDamagedSprite()
        {
            if (_renderer == null || _visualConfig == null) return;
            if (_visualConfig.damagedSprites == null || _visualConfig.damagedSprites.Length == 0) return;

            _renderer.sprite = _visualConfig.damagedSprites[
                Random.Range(0, _visualConfig.damagedSprites.Length)];
        }

        private IEnumerator DestroyedFlash()
        {
            if (_renderer != null && _visualConfig != null)
                _renderer.color = _visualConfig.destroyedFlash;

            yield return new WaitForSeconds(FlashDuration);

            if (_renderer != null)
                _renderer.color = Color.clear;

            _flashCoroutine = null;
        }
    }
}
