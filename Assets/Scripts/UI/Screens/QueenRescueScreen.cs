using UnityEngine;
using UnityEngine.UI;
using TMPro;
using KingSmash.Characters;
using KingSmash.Core;
namespace KingSmash.UI.Screens
{
    public class QueenRescueScreen : UIScreen
    {
        [Header("Art")]
        [SerializeField] private GameObject _kingCharacterArt;
        [SerializeField] private GameObject _queenCharacterArt;

        [Header("Text")]
        [SerializeField] private TextMeshProUGUI _messageLabel;

        [Header("Buttons")]
        [SerializeField] private Button _continueButton;

        protected override void Awake()
        {
            base.Awake();
            _continueButton?.onClick.AddListener(OnContinueClicked);
        }

        private void OnEnable()  => QueenController.OnQueenRescued += HandleQueenRescued;
        private void OnDisable() => QueenController.OnQueenRescued -= HandleQueenRescued;

        private void HandleQueenRescued(QueenController _) => Show();

        protected override void OnShow()
        {
            if (_messageLabel != null) _messageLabel.text = "The Queen is Rescued!";
            StartCoroutine(UIAnimationController.BounceReveal(transform, 0.4f));
            if (ServiceLocator.TryGet<KingSmash.Services.IAudioService>(out var audio))
                audio.PlaySfx("queen_rescued");
        }

        private void OnContinueClicked()
        {
            Hide();
        }

        private void OnDestroy() => _continueButton?.onClick.RemoveListener(OnContinueClicked);
    }
}
