using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using KingSmash.Core;
using KingSmash.PowerUps;

namespace KingSmash.UI.Screens
{
    public class PowerUpSelectionScreen : UIScreen
    {
        [Serializable]
        private class PowerUpSlotUI
        {
            public PowerUpType type;
            public Button      selectButton;
            public TextMeshProUGUI namLabel;
            public TextMeshProUGUI quantityLabel;
            public GameObject  selectedIndicator;
        }

        [SerializeField] private List<PowerUpSlotUI> _slots = new();
        [SerializeField] private TextMeshProUGUI     _selectedLabel;
        [SerializeField] private Button              _playButton;
        [SerializeField] private Button              _backButton;

        private PowerUpService _service;

        protected override void Awake()
        {
            base.Awake();
            _playButton?.onClick.AddListener(OnPlayClicked);
            _backButton?.onClick.AddListener(OnBackClicked);
            foreach (var slot in _slots)
            {
                var capturedSlot = slot;
                slot.selectButton?.onClick.AddListener(() => OnSlotClicked(capturedSlot.type));
            }
        }

        private void OnEnable()
        {
            PowerUpService.OnInventoryChanged += HandleInventoryChanged;
        }

        private void OnDisable()
        {
            PowerUpService.OnInventoryChanged -= HandleInventoryChanged;
        }

        protected override void OnShow()
        {
            ServiceLocator.TryGet<PowerUpService>(out _service);
            if (_service == null)
                GameLogger.Warning("PowerUpSelectionScreen", "PowerUpService not registered.");
            RefreshAll();
        }

        private void OnSlotClicked(PowerUpType type)
        {
            if (_service == null) return;
            if (_service.Selected == type)
                _service.ClearSelection();
            else
                _service.Select(type);
            RefreshAll();
        }

        private void OnPlayClicked()
        {
            ScreenManager.Instance.Back();
        }

        private void OnBackClicked()
        {
            _service?.ClearSelection();
            ScreenManager.Instance.Back();
        }

        private void HandleInventoryChanged(PowerUpType type, int newCount) => RefreshAll();

        private void RefreshAll()
        {
            foreach (var slot in _slots)
            {
                int  count      = _service?.GetCount(slot.type) ?? 0;
                bool isSelected = _service?.Selected == slot.type;

                if (slot.namLabel     != null) slot.namLabel.text     = slot.type.ToString();
                if (slot.quantityLabel != null) slot.quantityLabel.text = $"x{count}";
                slot.selectedIndicator?.SetActive(isSelected);
                if (slot.selectButton  != null) slot.selectButton.interactable = count > 0;
            }

            if (_selectedLabel != null)
            {
                _selectedLabel.text = _service?.Selected.HasValue == true
                    ? $"Selected: {_service.Selected.Value}"
                    : "No power-up selected";
            }
        }
    }
}
