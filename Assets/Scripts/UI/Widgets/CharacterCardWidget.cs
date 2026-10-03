using UnityEngine;
using UnityEngine.UI;
using TMPro;
namespace KingSmash.UI.Widgets
{
    public class CharacterCardWidget : MonoBehaviour
    {
        [SerializeField] private Image _characterImage;
        [SerializeField] private TextMeshProUGUI _nameLabel;
        [SerializeField] private TextMeshProUGUI _descriptionLabel;

        public void Setup(Sprite characterSprite, string characterName, string description = "")
        {
            if (_characterImage     != null) { _characterImage.sprite = characterSprite; _characterImage.enabled = characterSprite != null; }
            if (_nameLabel          != null) _nameLabel.text          = characterName;
            if (_descriptionLabel   != null) _descriptionLabel.text   = description;
        }
    }
}
