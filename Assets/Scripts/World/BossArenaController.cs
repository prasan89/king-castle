using System;
using UnityEngine;
using KingSmash.Audio;
using KingSmash.Camera;
using KingSmash.Characters;
using KingSmash.Core;
using KingSmash.Services;
using KingSmash.VFX;

namespace KingSmash.World
{
    public class BossArenaController : MonoBehaviour
    {
        [SerializeField] private bool _isBossLevel = true;

        [Header("Optional References")]
        [SerializeField] private CameraShakeProfile _bossIntroShakeProfile;
        [SerializeField] private CameraShakeProfile _bossDefeatShakeProfile;

        public static event Action OnBossDefeated;

        private void OnEnable()
        {
            if (!_isBossLevel) return;
            EnemyHealth.OnEnemyDefeated += HandleEnemyDefeated;
        }

        private void OnDisable()
        {
            EnemyHealth.OnEnemyDefeated -= HandleEnemyDefeated;
        }

        private void Start()
        {
            if (!_isBossLevel) return;
            PlayBossIntro();
        }

        private void PlayBossIntro()
        {
            if (ServiceLocator.TryGet<IAudioService>(out var audio))
                audio.PlayMusic(SoundId.BossBattle);

            if (ServiceLocator.TryGet<ICameraEffectService>(out var cameraFx))
            {
                if (_bossIntroShakeProfile != null)
                    cameraFx.Shake(_bossIntroShakeProfile);
                else
                    cameraFx.ShakeImmediate(0.4f, 0.18f);
            }

            GameLogger.Info("BossArenaController", "Boss arena intro triggered.");
        }

        private void HandleEnemyDefeated(EnemyHealth enemyHealth)
        {
            if (enemyHealth == null) return;
            if (enemyHealth.GetComponent<EnemyKing>() == null) return;
            TriggerBossDefeat();
        }

        private void TriggerBossDefeat()
        {
            GameLogger.Info("BossArenaController", "Boss defeated — triggering defeat sequence.");

            if (ServiceLocator.TryGet<IAudioService>(out var audio))
                audio.PlayMusic(SoundId.Victory);

            if (ServiceLocator.TryGet<ICameraEffectService>(out var cameraFx))
            {
                if (_bossDefeatShakeProfile != null)
                    cameraFx.Shake(_bossDefeatShakeProfile);
                else
                    cameraFx.ShakeImmediate(0.7f, 0.30f);
            }

            if (ServiceLocator.TryGet<IVFXService>(out var vfx))
                vfx.Play(VFXId.BossDefeat, Vector3.zero);

            EnemyHealth.OnEnemyDefeated -= HandleEnemyDefeated;

            OnBossDefeated?.Invoke();
        }
    }
}
