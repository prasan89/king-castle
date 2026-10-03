using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
namespace KingSmash.UI.Widgets
{
    public class LevelNodeWidget : MonoBehaviour
    {
        [SerializeField] private Button _button;
        [SerializeField] private TextMeshProUGUI _levelLabel;
        [SerializeField] private List<GameObject> _starIcons;
        [SerializeField] private GameObject _lockIcon;
        [SerializeField] private Image _nodeBackground;

        public void Setup(int levelNumber, int stars, bool unlocked, Action onTap)
        {
            if (_levelLabel != null) _levelLabel.text = levelNumber.ToString();
            for (int i = 0; i < _starIcons.Count; i++)
                if (_starIcons[i] != null) _starIcons[i].SetActive(i < stars);
            if (_lockIcon != null) _lockIcon.SetActive(!unlocked);
            if (_button != null)
            {
                _button.onClick.RemoveAllListeners();
                _button.interactable = unlocked;
                if (unlocked) _button.onClick.AddListener(() => onTap?.Invoke());
            }
        }
    }
}
