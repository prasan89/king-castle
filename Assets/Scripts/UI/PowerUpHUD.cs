using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using KingSmash.Audio;
using KingSmash.Core;
using KingSmash.PowerUps;
using KingSmash.Services;
using KingSmash.VFX;

namespace KingSmash.UI
{
    /// Displays available power-ups during gameplay and provides touch-accessible activation.
    public class PowerUpHUD : MonoBehaviour
    {
        // ── Power-up card inner data ──────────────────────────────────────────
        [Serializable]
        private class PowerUpCard
        {
            public PowerUpType   Type;
            public RectTransform Root;
            public Image         IconImage;
            public TextMeshProUGUI CountLabel;
            public Image         LockOverlay;
            public CanvasGroup   CardCg;
            public Button        Button;
        }

        // ── Fields ────────────────────────────────────────────────────────────
        [Header("Cards (assign in Inspector)")]
        [SerializeField] private List<PowerUpCard> _cards = new();

        [Header("M13 Polish")]
        [SerializeField] private KingSmashTheme    _themeConfig;
        [SerializeField] private ScreenFlashEffect _screenFlash;

        // ── Private state ─────────────────────────────────────────────────────
        private PowerUpService _service;
        private PowerUpType    _activeType = PowerUpType.None;
        private Coroutine      _pulseCoroutine;

        // ── Lifecycle ─────────────────────────────────────────────────────────

        private void Awake()
        {
            foreach (var card in _cards)
            {
                if (card.Root == null) continue;
                EnsureMinTouchTarget(card.Root, 88f);
                var capturedType = card.Type;
                card.Button?.onClick.AddListener(() => OnCardClicked(capturedType));
            }
        }

        private void Start()
        {
            ServiceLocator.TryGet<PowerUpService>(out _service);
            RefreshAll();
        }

        private void OnEnable()
        {
            PowerUpService.OnPowerUpActivated += HandleActivated;
            PowerUpService.OnInventoryChanged += HandleInventoryChanged;
        }

        private void OnDisable()
        {
            PowerUpService.OnPowerUpActivated -= HandleActivated;
            PowerUpService.OnInventoryChanged -= HandleInventoryChanged;
        }

        // ── Event handlers ────────────────────────────────────────────────────

        private void HandleActivated(PowerUpType type)
        {
            GameLogger.Debug("PowerUpHUD", $"Activated: {type}");
            _activeType = type;
            UpdatePulse(type);

            if (_screenFlash != null)
                _screenFlash.Flash(new Color(1f, 0.9f, 0.2f, 0.5f), 0.08f);

            var card = FindCard(type);
            if (card?.Root != null)
                StartCoroutine(UIAnimationController.BounceReveal(card.Root, 0.25f));

            if (ServiceLocator.TryGet<IAudioService>(out var audio))
                audio.Play(SoundId.PowerUpActivate);
        }

        private void HandleInventoryChanged(PowerUpType type, int newCount)
        {
            GameLogger.Debug("PowerUpHUD", $"Inventory changed: {type} → {newCount}");
            RefreshCard(type, newCount);
        }

        // ── Card click ────────────────────────────────────────────────────────

        private void OnCardClicked(PowerUpType type)
        {
            if (_service == null) return;
            if (!_service.Has(type)) return;
            _service.Select(type);
        }

        // ── Refresh helpers ───────────────────────────────────────────────────

        private void RefreshAll()
        {
            foreach (var card in _cards)
            {
                int count = _service != null ? _service.GetCount(card.Type) : 0;
                RefreshCard(card.Type, count);
            }
        }

        private void RefreshCard(PowerUpType type, int count)
        {
            var card = FindCard(type);
            if (card == null) return;

            bool available = count > 0;

            if (card.CountLabel != null)
                card.CountLabel.text = count > 0 ? count.ToString() : "0";

            if (card.CardCg != null)
                card.CardCg.alpha = available ? 1f : 0.4f;

            if (card.LockOverlay != null)
                card.LockOverlay.gameObject.SetActive(!available);

            if (card.Button != null)
                card.Button.interactable = available;
        }

        // ── Active pulse ──────────────────────────────────────────────────────

        private void UpdatePulse(PowerUpType type)
        {
            if (_pulseCoroutine != null)
            {
                StopCoroutine(_pulseCoroutine);
                _pulseCoroutine = null;
            }

            foreach (var c in _cards)
                if (c.Root != null) c.Root.localScale = Vector3.one;

            if (type == PowerUpType.None) return;

            var activeCard = FindCard(type);
            if (activeCard?.Root != null)
                _pulseCoroutine = StartCoroutine(PulseActive(activeCard.Root));
        }

        private IEnumerator PulseActive(RectTransform rt)
        {
            if (rt == null) yield break;
            const float minScale   = 1.0f;
            const float maxScale   = 1.05f;
            const float halfPeriod = 0.6f;

            while (true)
            {
                float elapsed = 0f;
                while (elapsed < halfPeriod)
                {
                    elapsed += Time.unscaledDeltaTime;
                    float t = Mathf.PingPong(elapsed / halfPeriod, 1f);
                    float s = Mathf.Lerp(minScale, maxScale, t);
                    rt.localScale = Vector3.one * s;
                    yield return null;
                }
            }
        }

        // ── Helpers ───────────────────────────────────────────────────────────

        private PowerUpCard FindCard(PowerUpType type)
        {
            foreach (var c in _cards)
                if (c.Type == type) return c;
            return null;
        }

        private static void EnsureMinTouchTarget(RectTransform rt, float minPx)
        {
            if (rt == null) return;
            Vector2 size = rt.sizeDelta;
            if (size.x < minPx) size.x = minPx;
            if (size.y < minPx) size.y = minPx;
            rt.sizeDelta = size;
        }

        private void OnDestroy()
        {
            if (_pulseCoroutine != null) StopCoroutine(_pulseCoroutine);
        }
    }
}
