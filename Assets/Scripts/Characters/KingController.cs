using System;
using UnityEngine;
using KingSmash.Core;
using KingSmash.Gameplay;
using KingSmash.Services;

namespace KingSmash.Characters
{
    // Orchestrates the King's state machine, health, and audio hooks.
    [RequireComponent(typeof(KingHealth))]
    public class KingController : MonoBehaviour
    {
        [SerializeField] private KingConfig _config;
        [SerializeField] private KingAnimator _animator;

        private KingStateMachine _stateMachine;
        private KingHealth _health;

        public static event Action<KingState, KingState> OnKingStateChanged;

        public KingState State => _stateMachine.Current;

        private void Awake()
        {
            _stateMachine = new KingStateMachine();
            _health = GetComponent<KingHealth>();
        }

        private void OnEnable()
        {
            KingProjectile.OnKingCollision    += HandleCollision;
            KingProjectile.OnKingLanded       += HandleLanded;
            KingHealth.OnKingDefeated         += HandleDefeated;
            LaunchController.OnKingLaunched   += HandleLaunched;
            QueenController.OnQueenRescued    += HandleQueenRescued;
        }

        private void OnDisable()
        {
            KingProjectile.OnKingCollision    -= HandleCollision;
            KingProjectile.OnKingLanded       -= HandleLanded;
            KingHealth.OnKingDefeated         -= HandleDefeated;
            LaunchController.OnKingLaunched   -= HandleLaunched;
            QueenController.OnQueenRescued    -= HandleQueenRescued;
        }

        private void Transition(KingState next)
        {
            var prev = _stateMachine.Current;
            if (_stateMachine.TryTransition(next))
            {
                _animator?.SetState(next);
                OnKingStateChanged?.Invoke(prev, next);
                PlayAudioForState(next);
            }
        }

        private void HandleLaunched(Vector2 dir, float power)
        {
            Transition(KingState.Launched);
            Transition(KingState.Flying);
        }

        private void HandleCollision(KingProjectile king, UnityEngine.Collision2D col)
        {
            Transition(KingState.Impact);

            float speed = col.relativeVelocity.magnitude;
            if (speed > 5f)
            {
                float dmg = speed * 0.1f;
                _health.TakeDamage(dmg);
            }

            if (_config != null && speed > _config.stunDurationOnHeavyHit)
                Transition(KingState.Stunned);
            else
                Transition(KingState.Flying);
        }

        private void HandleLanded(KingProjectile king)
        {
            if (_stateMachine.Current != KingState.Defeated)
                Transition(KingState.Idle);
        }

        private void HandleDefeated(KingHealth _)
        {
            Transition(KingState.Defeated);
        }

        private void HandleQueenRescued(QueenController _)
        {
            if (_stateMachine.Current != KingState.Defeated)
            {
                var prev = _stateMachine.Current;
                _stateMachine.ForceTransition(KingState.Victory);
                _animator?.SetState(KingState.Victory);
                OnKingStateChanged?.Invoke(prev, KingState.Victory);
                PlayAudioForState(KingState.Victory);
            }
        }

        private void PlayAudioForState(KingState state)
        {
            var audio = ServiceLocator.TryGet<IAudioService>(out var svc) ? svc : null;
            if (audio == null) return;
            switch (state)
            {
                case KingState.Impact:   audio.PlaySfx("king_impact");  break;
                case KingState.Stunned:  audio.PlaySfx("king_stunned"); break;
                case KingState.Defeated: audio.PlaySfx("king_defeated"); break;
                case KingState.Victory:  audio.PlaySfx("king_victory");  break;
            }
        }
    }
}
