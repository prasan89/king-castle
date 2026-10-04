using System;
using System.Collections;
using UnityEngine;
using KingSmash.Core;
using KingSmash.Characters;

namespace KingSmash.PowerUps
{
    public class PowerUpController : MonoBehaviour
    {
        public static event Action<PowerUpType>        OnEffectStarted;
        public static event Action<PowerUpType>        OnEffectEnded;
        public static event Action<PowerUpType, float> OnEffectTick;

        private PowerUpEffect _activeEffect;
        private PowerUpType   _activeType;
        private Coroutine     _effectRoutine;

        public bool HasActiveEffect => _activeEffect != null && _activeEffect.IsRunning;

        public void ApplyEffect(PowerUpType type, KingProjectile king)
        {
            if (HasActiveEffect)
            {
                GameLogger.Warning("PowerUpController", "Effect already active — skipping.");
                return;
            }

            var config = PowerUpFactory.LoadConfig(type);
            if (config == null)
            {
                GameLogger.Warning("PowerUpController", $"No config found for {type}.");
                return;
            }

            var effect = PowerUpFactory.CreateEffect(type, gameObject);
            effect.Initialize(config, king);
            _activeEffect  = effect;
            _activeType    = type;
            _effectRoutine = StartCoroutine(RunEffect(config, effect, type));
        }

        private IEnumerator RunEffect(PowerUpConfig config, PowerUpEffect effect, PowerUpType type)
        {
            effect.Begin();
            OnEffectStarted?.Invoke(type);

            if (config.effectDuration > 0f)
            {
                float elapsed = 0f;
                while (elapsed < config.effectDuration)
                {
                    elapsed += Time.deltaTime;
                    OnEffectTick?.Invoke(type, elapsed / config.effectDuration);
                    yield return null;
                }
            }

            FinishEffect(effect, type);
        }

        private void FinishEffect(PowerUpEffect effect, PowerUpType type)
        {
            effect.End();
            Destroy(effect);
            _activeEffect  = null;
            _effectRoutine = null;
            OnEffectEnded?.Invoke(type);
        }

        public void CancelActiveEffect()
        {
            if (_effectRoutine != null) { StopCoroutine(_effectRoutine); _effectRoutine = null; }
            if (_activeEffect  != null) FinishEffect(_activeEffect, _activeType);
        }

        private void OnDisable() => CancelActiveEffect();
    }
}
