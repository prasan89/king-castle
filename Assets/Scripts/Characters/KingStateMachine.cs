using System;
using UnityEngine;
using KingSmash.Core;

namespace KingSmash.Characters
{
    public class KingStateMachine
    {
        private static readonly bool[,] _validTransitions;

        static KingStateMachine()
        {
            int n = Enum.GetValues(typeof(KingState)).Length;
            _validTransitions = new bool[n, n];
            void Allow(KingState f, KingState t) => _validTransitions[(int)f, (int)t] = true;

            Allow(KingState.Idle,             KingState.PreparingLaunch);
            Allow(KingState.PreparingLaunch,  KingState.Launched);
            Allow(KingState.PreparingLaunch,  KingState.Idle);        // aim cancelled
            Allow(KingState.Launched,         KingState.Flying);
            Allow(KingState.Flying,           KingState.Impact);
            Allow(KingState.Impact,           KingState.Stunned);
            Allow(KingState.Impact,           KingState.Flying);       // bounce
            Allow(KingState.Impact,           KingState.Defeated);
            Allow(KingState.Stunned,          KingState.Flying);       // recover mid-air
            Allow(KingState.Stunned,          KingState.Idle);
            Allow(KingState.Stunned,          KingState.Defeated);
            Allow(KingState.Flying,           KingState.Idle);         // landed
            Allow(KingState.Idle,             KingState.Victory);
            Allow(KingState.Stunned,          KingState.Victory);
        }

        public KingState Current { get; private set; } = KingState.Idle;

        public static event Action<KingState, KingState> OnStateChanged;

        public bool TryTransition(KingState next)
        {
            if (!_validTransitions[(int)Current, (int)next])
            {
                GameLogger.Warning("KingStateMachine", $"Invalid: {Current} -> {next}");
                return false;
            }
            var prev = Current;
            Current = next;
            OnStateChanged?.Invoke(prev, next);
            return true;
        }

        public void ForceTransition(KingState next)
        {
            var prev = Current;
            Current = next;
            OnStateChanged?.Invoke(prev, next);
        }
    }
}
