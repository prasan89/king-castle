using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using KingSmash.Audio;
using KingSmash.Services;
using KingSmash.Core;

namespace KingSmash.UI.Components
{
    public enum ButtonStyle
    {
        Primary,
        Secondary,
        Danger,
        Ghost,
        Icon
    }

    [RequireComponent(typeof(Button))]
    public class KSButton : MonoBehaviour
    {
        [SerializeField] private KingSmashTheme  _theme;
        [SerializeField] private ButtonStyle     _style = ButtonStyle.Primary;
        [SerializeField] private Image           _background;
        [SerializeField] private TextMeshProUGUI _label;
        [SerializeField] private Image           _icon;
        [SerializeField] private bool            _playSound = true;

        private Button    _button;
        private Coroutine _scaleCoroutine;
        private bool      _isPressed;

        private void Awake()
        {
            _button = GetComponent<Button>();
            _button.onClick.AddListener(OnButtonClicked);

            var trigger = GetComponent<EventTrigger>();
            if (trigger == null) trigger = gameObject.AddComponent<EventTrigger>();

            var pointerDown = new EventTrigger.Entry { eventID = EventTriggerType.PointerDown };
            pointerDown.callback.AddListener(_ => OnPress());
            trigger.triggers.Add(pointerDown);

            var pointerUp = new EventTrigger.Entry { eventID = EventTriggerType.PointerUp };
            pointerUp.callback.AddListener(_ => OnRelease());
            trigger.triggers.Add(pointerUp);

            if (_theme != null) ApplyStyle(_style);
        }

        private void OnPress()
        {
            if (_scaleCoroutine != null) StopCoroutine(_scaleCoroutine);
            _isPressed = true;
            float targetScale = _theme != null ? _theme.buttonScalePressed : 0.92f;
            _scaleCoroutine = StartCoroutine(LerpScale(targetScale, 0.08f));

            if (_playSound)
            {
                if (ServiceLocator.TryGet<IAudioService>(out var audio))
                    audio.Play(SoundId.ButtonClick);
            }
        }

        private void OnRelease()
        {
            if (!_isPressed) return;
            _isPressed = false;
            if (_scaleCoroutine != null) StopCoroutine(_scaleCoroutine);
            _scaleCoroutine = StartCoroutine(LerpScale(1.0f, 0.08f));
        }

        private void OnButtonClicked()
        {
            if (_playSound)
            {
                if (ServiceLocator.TryGet<IAudioService>(out var audio))
                    audio.Play(SoundId.ButtonConfirm);
            }
        }

        public void ApplyStyle(ButtonStyle style)
        {
            _style = style;
            if (_theme == null || _background == null) return;

            switch (style)
            {
                case ButtonStyle.Primary:   _background.color = _theme.primaryButton;  break;
                case ButtonStyle.Secondary: _background.color = _theme.secondaryButton; break;
                case ButtonStyle.Danger:    _background.color = _theme.dangerButton;   break;
                case ButtonStyle.Ghost:     _background.color = new Color(1f, 1f, 1f, 0.1f); break;
                case ButtonStyle.Icon:      _background.color = Color.clear; break;
            }

            if (_label != null) _label.color = _theme.textPrimary;
        }

        public void SetLabel(string text)
        {
            if (_label != null) _label.text = text;
        }

        public void SetInteractable(bool interactable)
        {
            if (_button != null)
            {
                _button.interactable = interactable;
                if (_background != null)
                    _background.color = interactable
                        ? (_theme != null ? GetStyleColor(_style) : _background.color)
                        : (_theme != null ? _theme.disabledColor  : Color.gray);
            }
        }

        public void SetIcon(Sprite sprite)
        {
            if (_icon != null)
            {
                _icon.sprite  = sprite;
                _icon.enabled = sprite != null;
            }
        }

        private Color GetStyleColor(ButtonStyle style)
        {
            if (_theme == null) return Color.white;
            switch (style)
            {
                case ButtonStyle.Primary:   return _theme.primaryButton;
                case ButtonStyle.Secondary: return _theme.secondaryButton;
                case ButtonStyle.Danger:    return _theme.dangerButton;
                case ButtonStyle.Ghost:     return new Color(1f, 1f, 1f, 0.1f);
                case ButtonStyle.Icon:      return Color.clear;
                default:                    return _theme.primaryButton;
            }
        }

        private IEnumerator LerpScale(float targetUniform, float duration)
        {
            Vector3 start   = transform.localScale;
            Vector3 end     = Vector3.one * targetUniform;
            float   elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                transform.localScale = Vector3.Lerp(start, end, elapsed / duration);
                yield return null;
            }
            transform.localScale = end;
        }
    }
}
