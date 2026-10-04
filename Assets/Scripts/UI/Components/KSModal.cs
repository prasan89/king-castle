using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using KingSmash.Audio;
using KingSmash.Services;
using KingSmash.Core;

namespace KingSmash.UI.Components
{
    public class KSModal : MonoBehaviour
    {
        [SerializeField] private KingSmashTheme  _theme;
        [SerializeField] private CanvasGroup     _canvasGroup;
        [SerializeField] private RectTransform   _panel;
        [SerializeField] private Button          _closeButton;
        [SerializeField] private TextMeshProUGUI _titleLabel;
        [SerializeField] private TextMeshProUGUI _bodyLabel;
        [SerializeField] private Button          _confirmButton;
        [SerializeField] private Button          _cancelButton;
        [SerializeField] private TextMeshProUGUI _confirmLabel;
        [SerializeField] private TextMeshProUGUI _cancelLabel;

        private Action _onConfirm;
        private Action _onCancel;
        private Coroutine _animCoroutine;

        private static KSModal _instance;
        public static KSModal Instance
        {
            get
            {
                if (_instance == null)
                    _instance = FindObjectOfType<KSModal>(true);
                return _instance;
            }
        }

        private void Awake()
        {
            if (_instance == null) _instance = this;

            if (_closeButton   != null) _closeButton.onClick.AddListener(OnCancelClicked);
            if (_confirmButton != null) _confirmButton.onClick.AddListener(OnConfirmClicked);
            if (_cancelButton  != null) _cancelButton.onClick.AddListener(OnCancelClicked);

            gameObject.SetActive(false);
        }

        public void Show(
            string title,
            string body,
            string confirm   = "OK",
            string cancel    = null,
            Action onConfirm = null,
            Action onCancel  = null)
        {
            _onConfirm = onConfirm;
            _onCancel  = onCancel;

            if (_titleLabel != null) _titleLabel.text = title;
            if (_bodyLabel  != null) _bodyLabel.text  = body;
            if (_confirmLabel != null) _confirmLabel.text = confirm;

            bool hasCancelButton = !string.IsNullOrEmpty(cancel);
            if (_cancelButton  != null) _cancelButton.gameObject.SetActive(hasCancelButton);
            if (_cancelLabel   != null && hasCancelButton) _cancelLabel.text = cancel;

            gameObject.SetActive(true);

            if (_animCoroutine != null) StopCoroutine(_animCoroutine);
            _animCoroutine = StartCoroutine(ShowAnimation());

            if (ServiceLocator.TryGet<IAudioService>(out var audio))
                audio.Play(SoundId.PopupOpen);
        }

        public void Hide()
        {
            if (_animCoroutine != null) StopCoroutine(_animCoroutine);
            _animCoroutine = StartCoroutine(HideAnimation());

            if (ServiceLocator.TryGet<IAudioService>(out var audio))
                audio.Play(SoundId.PopupClose);
        }

        private IEnumerator ShowAnimation()
        {
            if (_canvasGroup != null)
            {
                _canvasGroup.interactable   = false;
                _canvasGroup.blocksRaycasts = false;
                yield return UIAnimationController.Fade(_canvasGroup, 0f, 1f, 0.2f);
                _canvasGroup.interactable   = true;
                _canvasGroup.blocksRaycasts = true;
            }

            if (_panel != null)
                StartCoroutine(UIAnimationController.BounceReveal(_panel,
                    _theme != null ? _theme.durationBounce : 0.35f));
        }

        private IEnumerator HideAnimation()
        {
            if (_canvasGroup != null)
            {
                _canvasGroup.interactable   = false;
                _canvasGroup.blocksRaycasts = false;
                yield return UIAnimationController.Fade(_canvasGroup, 1f, 0f, 0.2f);
            }
            gameObject.SetActive(false);
        }

        private void OnConfirmClicked()
        {
            var cb = _onConfirm;
            Hide();
            cb?.Invoke();
        }

        private void OnCancelClicked()
        {
            var cb = _onCancel;
            Hide();
            cb?.Invoke();
        }
    }
}
