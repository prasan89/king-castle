using System;
using System.Collections.Generic;
using UnityEngine;
using KingSmash.Core;

namespace KingSmash.Physics
{
    /// Tracks all DestructibleObject children and emits events as they break.
    /// Auto-collects children unless list is manually populated in Inspector.
    public class CastleStructure : MonoBehaviour
    {
        [SerializeField] private List<DestructibleObject> _destructibles = new();
        [SerializeField] private bool _autoCollectChildren = true;

        public static event Action<CastleStructure> OnCastleDestroyed;
        public static event Action<CastleStructure, float> OnDestructionProgress;

        private int _totalPieces;
        private int _destroyedPieces;

        public int TotalPieces => _totalPieces;
        public int DestroyedPieces => _destroyedPieces;
        public float DestructionRatio => _totalPieces > 0 ? (float)_destroyedPieces / _totalPieces : 0f;
        public bool IsFullyDestroyed => _destroyedPieces >= _totalPieces && _totalPieces > 0;

        private void Awake()
        {
            if (_autoCollectChildren)
            {
                _destructibles.Clear();
                _destructibles.AddRange(GetComponentsInChildren<DestructibleObject>());
            }
            _totalPieces = _destructibles.Count;
            GameLogger.Debug("CastleStructure", $"{name}: {_totalPieces} pieces registered.");
        }

        private void OnEnable() => DestructibleObject.OnDestroyed += HandlePieceDestroyed;
        private void OnDisable() => DestructibleObject.OnDestroyed -= HandlePieceDestroyed;

        private void HandlePieceDestroyed(DestructibleObject piece)
        {
            if (!_destructibles.Contains(piece)) return;
            _destroyedPieces++;
            GameLogger.Debug("CastleStructure", $"{name}: {_destroyedPieces}/{_totalPieces} destroyed. Ratio={DestructionRatio:P0}");
            OnDestructionProgress?.Invoke(this, DestructionRatio);

            if (IsFullyDestroyed)
            {
                GameLogger.Info("CastleStructure", $"{name} fully destroyed!");
                OnCastleDestroyed?.Invoke(this);
            }
        }
    }
}
