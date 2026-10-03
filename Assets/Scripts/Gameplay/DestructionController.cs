using System;
using System.Collections.Generic;
using UnityEngine;
using KingSmash.Physics;
using KingSmash.Core;
using KingSmash.Services;

namespace KingSmash.Gameplay
{
    /// Tracks destruction across all registered castles.
    /// Reports per-castle and global destruction stats.
    public class DestructionController : MonoBehaviour
    {
        private readonly List<CastleStructure> _castles = new();
        private int _destroyedCastleCount;
        private int _destroyedPieces;
        private int _totalPieces;

        public static event Action<int> OnCastleDestroyed;
        public static event Action OnAllCastlesDestroyed;
        public static event Action<float> OnGlobalDestructionChanged; // 0-1

        public int DestroyedCastles => _destroyedCastleCount;
        public int TotalCastles => _castles.Count;
        public bool AllCastlesDestroyed => _castles.Count > 0 && _destroyedCastleCount >= _castles.Count;
        public float GlobalDestructionRatio => _totalPieces > 0 ? (float)_destroyedPieces / _totalPieces : 0f;

        private void OnEnable()
        {
            CastleStructure.OnCastleDestroyed    += HandleCastleDestroyed;
            CastleStructure.OnDestructionProgress += HandleDestructionProgress;
            DestructibleObject.OnDestroyed        += HandlePieceDestroyed;
        }

        private void OnDisable()
        {
            CastleStructure.OnCastleDestroyed    -= HandleCastleDestroyed;
            CastleStructure.OnDestructionProgress -= HandleDestructionProgress;
            DestructibleObject.OnDestroyed        -= HandlePieceDestroyed;
        }

        public void RegisterCastle(CastleStructure castle)
        {
            if (_castles.Contains(castle)) return;
            _castles.Add(castle);
            _totalPieces += castle.TotalPieces;
        }

        private void HandlePieceDestroyed(DestructibleObject _)
        {
            _destroyedPieces++;
            OnGlobalDestructionChanged?.Invoke(GlobalDestructionRatio);
        }

        private void HandleDestructionProgress(CastleStructure _, float ratio)
        {
            OnGlobalDestructionChanged?.Invoke(GlobalDestructionRatio);
        }

        private void HandleCastleDestroyed(CastleStructure castle)
        {
            _destroyedCastleCount++;
            ServiceLocator.Get<IAnalyticsService>().LogEvent(
                AnalyticsEvents.CastleDestroyed,
                ("castle_index", _destroyedCastleCount),
                ("destruction_ratio", GlobalDestructionRatio));
            OnCastleDestroyed?.Invoke(_destroyedCastleCount);

            if (AllCastlesDestroyed)
            {
                GameLogger.Info("DestructionController", "All castles destroyed!");
                OnAllCastlesDestroyed?.Invoke();
            }
        }
    }
}
