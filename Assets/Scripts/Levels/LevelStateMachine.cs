using System;
using UnityEngine;
using KingSmash.Core;

namespace KingSmash.Levels
{
    public enum LevelState
    {
        LevelStart,
        Playing,
        KingFlying,
        Resolving,
        LevelComplete,
        LevelFailed
    }

    /// Lightweight state machine for in-level state.
    /// Distinct from GameState (which tracks app-level state).
    public class LevelStateMachine
    {
        public LevelState Current { get; private set; } = LevelState.LevelStart;

        public static event Action<LevelState, LevelState> OnStateChanged;

        private static readonly bool[,] _validTransitions = BuildTransitionTable();

        public bool TryTransition(LevelState next)
        {
            if (!_validTransitions[(int)Current, (int)next])
            {
                GameLogger.Warning("LevelStateMachine", $"Invalid transition {Current} -> {next}");
                return false;
            }
            var prev = Current;
            Current = next;
            GameLogger.Info("LevelStateMachine", $"Level state: {prev} -> {next}");
            OnStateChanged?.Invoke(prev, next);
            return true;
        }

        public void ForceTransition(LevelState next)
        {
            var prev = Current;
            Current = next;
            OnStateChanged?.Invoke(prev, next);
        }

        private static bool[,] BuildTransitionTable()
        {
            int n = Enum.GetValues(typeof(LevelState)).Length;
            var t = new bool[n, n];

            Allow(t, LevelState.LevelStart,    LevelState.Playing);
            Allow(t, LevelState.Playing,       LevelState.KingFlying);
            Allow(t, LevelState.Playing,       LevelState.LevelFailed);
            Allow(t, LevelState.KingFlying,    LevelState.Resolving);
            Allow(t, LevelState.KingFlying,    LevelState.LevelComplete);
            Allow(t, LevelState.Resolving,     LevelState.Playing);
            Allow(t, LevelState.Resolving,     LevelState.LevelComplete);
            Allow(t, LevelState.Resolving,     LevelState.LevelFailed);
            // Allow restart from terminal states
            Allow(t, LevelState.LevelComplete, LevelState.LevelStart);
            Allow(t, LevelState.LevelFailed,   LevelState.LevelStart);

            return t;
        }

        private static void Allow(bool[,] t, LevelState from, LevelState to)
            => t[(int)from, (int)to] = true;
    }
}
