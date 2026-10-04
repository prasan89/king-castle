using UnityEngine;
using UnityEngine.UI;
using KingSmash.Services;
using KingSmash.Audio;

namespace KingSmash.UI.Components
{
    /// <summary>
    /// Reusable Android back / hardware-back handler.
    /// Listens for the Escape key each frame and optionally wires an on-screen Button.
    /// </summary>
    public class KSBackButton : MonoBehaviour
    {
        [SerializeField] private Button _uiButton;

        private void Awake()
        {
            if (_uiButton != null)
                _uiButton.onClick.AddListener(OnBackPressed);
        }

        private void OnDestroy()
        {
            if (_uiButton != null)
                _uiButton.onClick.RemoveListener(OnBackPressed);
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
                OnBackPressed();
        }

        public void OnBackPressed()
        {
            if (ServiceLocator.TryGet<IAudioService>(out var audio))
                audio.Play(SoundId.Back);

            ScreenManager.Instance?.Back();
        }
    }
}
