using UnityEngine;
using UnityEngine.UI;
using TMPro;
using KingSmash.Physics;
namespace KingSmash.UI.Widgets
{
    public class CastleMaterialCardWidget : MonoBehaviour
    {
        [SerializeField] private Image _materialIcon;
        [SerializeField] private TextMeshProUGUI _nameLabel;
        [SerializeField] private TextMeshProUGUI _hpLabel;
        [SerializeField] private Image _colorSwatch;

        public void Setup(MaterialConfig config)
        {
            if (config == null) return;
            if (_nameLabel  != null) _nameLabel.text  = config.name;
            if (_hpLabel    != null) _hpLabel.text    = $"HP: {config.maxHealth:F0}";
            if (_colorSwatch != null) _colorSwatch.color = config.debugColor;
        }
    }
}
