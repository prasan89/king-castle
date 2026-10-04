using UnityEngine;
using KingSmash.Core;
using KingSmash.Gameplay;
using KingSmash.Characters;
using KingSmash.Levels;
using KingSmash.Services;
using KingSmash.Audio;
using KingSmash.VFX;

namespace KingSmash.UI
{
    /// <summary>
    /// Bridges gameplay events to UI services (audio, VFX, toast, screen transitions).
    /// Attach once to a persistent GameObject in the level scene.
    /// </summary>
    public class UIEventBridge : MonoBehaviour
    {
        private void OnEnable()
        {
            LevelStateMachine.OnStateChanged   += HandleLevelState;
            KingProjectile.OnKingLanded        += HandleKingLanded;
        }

        private void OnDisable()
        {
            LevelStateMachine.OnStateChanged   -= HandleLevelState;
            KingProjectile.OnKingLanded        -= HandleKingLanded;
        }

        private void HandleLevelState(LevelState state)
        {
            switch (state)
            {
                case LevelState.Complete:
                    if (ServiceLocator.TryGet<IAudioService>(out var audioWin))
                        audioWin.Play(SoundId.LevelComplete);
                    break;

                case LevelState.Failed:
                    if (ServiceLocator.TryGet<IAudioService>(out var audioFail))
                        audioFail.Play(SoundId.Defeat);
                    break;
            }
        }

        private void HandleKingLanded(KingProjectile king)
        {
            if (ServiceLocator.TryGet<IAudioService>(out var audio))
                audio.Play(SoundId.KingLand);
        }
    }
}
