using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace KingSmash.UI.Components
{
    public class KSTab : MonoBehaviour
    {
        [SerializeField] private Button          _button;
        [SerializeField] private Image           _activeIndicator;
        [SerializeField] private TextMeshProUGUI _label;
        [SerializeField] private Image           _background;
        [SerializeField] private KingSmashTheme  _theme;

        public event Action OnTabSelected;

        private void Awake()
        {
            if (_button != null)
                _button.onClick.AddListener(HandleTabClick);
        }

        public void SetActive(bool active)
        {
            if (_activeIndicator != null)
                _activeIndicator.gameObject.SetActive(active);

            if (_label != null && _theme != null)
                _label.color = active ? _theme.textPrimary : _theme.textSecondary;

            if (_background != null && _theme != null)
                _background.color = active
                    ? new Color(_theme.primaryButton.r, _theme.primaryButton.g, _theme.primaryButton.b, 0.15f)
                    : Color.clear;
        }

        private void HandleTabClick()
        {
            OnTabSelected?.Invoke();
        }
    }
}
