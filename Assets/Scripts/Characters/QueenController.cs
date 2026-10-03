using System;
using UnityEngine;
using KingSmash.Core;
using KingSmash.Services;

namespace KingSmash.Characters
{
    public class QueenController : MonoBehaviour
    {
        [SerializeField] private QueenConfig _config;
        [SerializeField] private Physics.CastleStructure _imprisoningCastle;
        [SerializeField] private SpriteRenderer _spriteRenderer;

        private QueenState _state = QueenState.Captured;

        public static event Action<QueenController> OnQueenRescued;

        public QueenState State => _state;

        private void OnEnable()
        {
            Physics.CastleStructure.OnCastleDestroyed += HandleCastleDestroyed;
        }

        private void OnDisable()
        {
            Physics.CastleStructure.OnCastleDestroyed -= HandleCastleDestroyed;
        }

        private void HandleCastleDestroyed(Physics.CastleStructure castle)
        {
            if (castle != _imprisoningCastle || _state == QueenState.Rescued) return;
            _state = QueenState.Waiting;
            Rescue();
        }

        private void Rescue()
        {
            _state = QueenState.Rescued;
            if (_spriteRenderer != null && _config != null)
                _spriteRenderer.color = _config.rescuedColor;

            GameLogger.Info("QueenController", "Queen rescued!");

            if (ServiceLocator.TryGet<IAnalyticsService>(out var analytics))
                analytics.LogEvent(AnalyticsEvents.QueenRescued);

            if (ServiceLocator.TryGet<IAudioService>(out var audio))
                audio.PlaySfx("queen_rescued");

            OnQueenRescued?.Invoke(this);
        }
    }
}
