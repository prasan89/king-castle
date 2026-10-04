using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace KingSmash.UI.Components
{
    /// <summary>
    /// Prevents accidental double-taps by imposing a cooldown on a Button.
    /// Attach to the same GameObject as the Button you want to guard.
    /// During the cooldown window, the Button is set non-interactable,
    /// then automatically restored.
    /// </summary>
    [RequireComponent(typeof(Button))]
    public class KSDoubleTapGuard : MonoBehaviour
    {
        [SerializeField] private float _cooldown = 0.5f;

        private Button    _button;
        private float     _lastClickTime = -999f;
        private Coroutine _restoreCoroutine;

        private void Awake()
        {
            _button = GetComponent<Button>();
            _button.onClick.AddListener(OnButtonClicked);
        }

        private void OnDestroy()
        {
            if (_button != null)
                _button.onClick.RemoveListener(OnButtonClicked);
        }

        private void OnButtonClicked()
        {
            float now = Time.unscaledTime;
            if (now - _lastClickTime < _cooldown)
            {
                // Within cooldown — consume (already blocked by interactable = false below)
                return;
            }

            _lastClickTime = now;

            // Disable for cooldown then restore
            if (_restoreCoroutine != null) StopCoroutine(_restoreCoroutine);
            _restoreCoroutine = StartCoroutine(DisableForCooldown());
        }

        private IEnumerator DisableForCooldown()
        {
            if (_button != null) _button.interactable = false;
            yield return new WaitForSecondsRealtime(_cooldown);
            if (_button != null) _button.interactable = true;
            _restoreCoroutine = null;
        }
    }
}
