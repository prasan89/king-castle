using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
namespace KingSmash.UI.Widgets
{
    public class PowerUpCardWidget : MonoBehaviour
    {
        [SerializeField] private Button _button;
        [SerializeField] private Image _iconImage;
        [SerializeField] private TextMeshProUGUI _nameLabel;
        [SerializeField] private TextMeshProUGUI _quantityLabel;
        [SerializeField] private GameObject _selectedOverlay;
        [SerializeField] private GameObject _disabledOverlay;

        private bool _isSelected;

        public void Setup(Sprite icon, string displayName, int quantity, bool isDisabled, Action onTap)
        {
            if (_iconImage    != null) _iconImage.sprite    = icon;
            if (_nameLabel    != null) _nameLabel.text      = displayName;
            if (_quantityLabel != null) _quantityLabel.text = $"x{quantity}";
            if (_disabledOverlay != null) _disabledOverlay.SetActive(isDisabled);
            if (_button != null)
            {
                _button.interactable = !isDisabled && quantity > 0;
                _button.onClick.RemoveAllListeners();
                _button.onClick.AddListener(() => { ToggleSelect(); onTap?.Invoke(); });
            }
        }

        private void ToggleSelect()
        {
            _isSelected = !_isSelected;
            if (_selectedOverlay != null) _selectedOverlay.SetActive(_isSelected);
        }
    }
}
