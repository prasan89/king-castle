using System;
using UnityEngine;
using KingSmash.Core;
using KingSmash.Services;

namespace KingSmash.Characters
{
    /// Queen — the rescue target.
    /// Freed when the CastleStructure she's inside is fully destroyed.
    public class QueenPlaceholder : MonoBehaviour
    {
        [SerializeField] private Physics.CastleStructure _imprisoningCastle;
        [SerializeField] private SpriteRenderer _spriteRenderer;
        [SerializeField] private Color _freedColor = Color.yellow;

        private bool _freed;

        public static event Action<QueenPlaceholder> OnQueenFreed;

        private void OnEnable()
        {
            if (_imprisoningCastle != null)
                Physics.CastleStructure.OnCastleDestroyed += HandleCastleDestroyed;
        }

        private void OnDisable()
        {
            Physics.CastleStructure.OnCastleDestroyed -= HandleCastleDestroyed;
        }

        private void HandleCastleDestroyed(Physics.CastleStructure castle)
        {
            if (castle != _imprisoningCastle || _freed) return;
            Free();
        }

        private void Free()
        {
            _freed = true;
            if (_spriteRenderer != null) _spriteRenderer.color = _freedColor;
            GameLogger.Info("QueenPlaceholder", "Queen freed!");
            ServiceLocator.Get<IAnalyticsService>().LogEvent(AnalyticsEvents.QueenRescued);
            OnQueenFreed?.Invoke(this);
        }
    }
}
