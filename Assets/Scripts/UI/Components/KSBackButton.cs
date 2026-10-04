using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using KingSmash.Core;
using KingSmash.Services;
using KingSmash.Audio;

namespace KingSmash.UI.Components
{
    [RequireComponent(typeof(Button))]
    public class KSBackButton : MonoBehaviour, IPointerClickHandler
    {
        [SerializeField] private bool _playSound = true;

        private Button _button;

        private void Awake()
        {
            _button = GetComponent<Button>();
            _button.onClick.AddListener(OnBack);
        }

        private void OnDestroy()
        {
            _button.onClick.RemoveListener(OnBack);
        }

        public void OnPointerClick(PointerEventData eventData) { }

        private void OnBack()
        {
            if (_playSound)
            {
                if (ServiceLocator.TryGet<IAudioService>(out var audio))
                    audio.Play(SoundId.Back);
            }

            ScreenManager.Instance?.Back();
        }
    }
}
