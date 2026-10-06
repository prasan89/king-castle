using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using KingSmash.Core;
using KingSmash.Economy;
using KingSmash.Services;
using KingSmash.Audio;
using KingSmash.UI.Components;

namespace KingSmash.UI.Screens
{
    /// <summary>
    /// Polished modal shown when the player lacks sufficient currency.
    /// UIScreen subclass — use ScreenManager to show/hide.
    /// </summary>
    public class InsufficientCurrencyPopup : UIScreen
    {
        // -- Original fields --------------------------------------------------
        [SerializeField] private TextMeshProUGUI _titleLabel;
        [SerializeField] private TextMeshProUGUI _currentLabel;
        [SerializeField] private TextMeshProUGUI _requiredLabel;
        [SerializeField] private TextMeshProUGUI _shortfallLabel;
        [SerializeField] private Button          _earnCoinsButton;
        [SerializeField] private Button          _closeButton;

        // -- M13 polish fields ------------------------------------------------
        [SerializeField] private KingSmashTheme    _themeConfig;
        [SerializeField] private RectTransform     _popupPanel;
        [SerializeField] private KSCurrencyDisplay _currentAmountDisplay;
        [SerializeField] private KSCurrencyDisplay _requiredAmountDisplay;

        public static event Action OnEarnCoinsRequested;

        private Coroutine _animCoroutine;

        protected override void Awake()
        {
            base.Awake();

            if (_earnCoinsButton != null)
                _earnCoinsButton.onClick.AddListener(OnShopClicked);

            if (_closeButton != null)
                _closeButton.onClick.AddListener(Dismiss);
        }

        private void OnDestroy()
        {
            if (_earnCoinsButton != null)
                _earnCoinsButton.onClick.RemoveListener(OnShopClicked);

            if (_closeButton != null)
                _closeButton.onClick.RemoveListener(Dismiss);
        }

        /// <summary>
        /// Populate the popup with currency context.
        /// Call before or after Show().
        /// </summary>
        public void Populate(CurrencyType type, long current, long required)
        {
            if (_titleLabel     != null) _titleLabel.text     = "Not Enough " + type;
            if (_currentLabel   != null) _currentLabel.text   = "You have: "  + CurrencyFormatter.Format(current);
            if (_requiredLabel  != null) _requiredLabel.text  = "You need: "  + CurrencyFormatter.Format(required);
            if (_shortfallLabel != null) _shortfallLabel.text = "Short by: " + CurrencyFormatter.Format(required - current);
        }

        // -- UIScreen overrides -----------------------------------------------

        protected override void OnShow()
        {
            if (ServiceLocator.TryGet<IAudioService>(out var audio))
                audio.Play(SoundId.PopupOpen);

            if (_popupPanel != null)
            {
                if (_animCoroutine != null) StopCoroutine(_animCoroutine);
                _animCoroutine = StartCoroutine(UIAnimationController.BounceReveal(
                    _popupPanel,
                    _themeConfig != null ? _themeConfig.durationBounce : 0.35f));
            }
        }

        protected override void OnHide() { }

        // -- Dismiss (animated) -----------------------------------------------

        public void Dismiss()
        {
            if (_animCoroutine != null) StopCoroutine(_animCoroutine);
            _animCoroutine = StartCoroutine(DismissCoroutine());
        }

        private IEnumerator DismissCoroutine()
        {
            if (ServiceLocator.TryGet<IAudioService>(out var audio))
                audio.Play(SoundId.PopupClose);

            if (_popupPanel != null)
                yield return UIAnimationController.SlideOut(_popupPanel);

            Hide();
        }

        // -- Button handlers --------------------------------------------------

        private void OnShopClicked()
        {
            OnEarnCoinsRequested?.Invoke();
            ScreenManager.Instance?.Show<ShopScreen>();
            Dismiss();
        }
    }
}
