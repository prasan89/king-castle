// FeedbackScreen.cs — M16 soft-launch: in-game player feedback UI screen.
// Allows the player to select a category and submit a short free-text message.
//
// Privacy requirements:
//  - No PII collection. The input field is free-text; a heuristic scan warns
//    before submission and prevents it when common PII patterns are detected.
//  - Message is capped at 500 characters server-side and at input time.
//  - The screen never persists raw message text to device storage.

using UnityEngine;
using TMPro;
using KingSmash.Core;
using KingSmash.Services;
using KingSmash.UI.Components;
using KingSmash.Services.Firebase;

namespace KingSmash.UI
{
    /// <summary>
    /// In-game feedback screen.
    ///
    /// Scene wiring:
    ///  - Assign one <see cref="KSButton"/> per <see cref="FeedbackCategory"/> value
    ///    to <c>_categoryButtons</c> (same order as the enum).
    ///  - Wire <c>_submitButton.onClick</c> and <c>_closeButton.onClick</c> via
    ///    Awake (done here) — do not wire them in the Inspector to avoid duplicates.
    /// </summary>
    public class FeedbackScreen : UIScreen
    {
        // ── Inspector ────────────────────────────────────────────────────────────

        [Header("Category Selection")]
        [Tooltip("One KSButton per FeedbackCategory, in enum declaration order.")]
        [SerializeField] private KSButton[] _categoryButtons;

        [Header("Message Input")]
        [SerializeField] private TMP_InputField _messageInput;

        [Header("Actions")]
        [SerializeField] private KSButton _submitButton;
        [SerializeField] private KSButton _closeButton;

        // ── State ────────────────────────────────────────────────────────────────

        private FeedbackCategory _selectedCategory = FeedbackCategory.Gameplay;

        private const int MinMessageLength = 5;
        private const int MaxMessageLength = 500;
        private const string Tag = "FeedbackScreen";

        // Heuristic PII patterns — not exhaustive; secondary safety net.
        private static readonly string[] PiiPatterns =
        {
            "@",            // e-mail address
            "phone:",
            "tel:",
            "+1",           // NANP phone prefix
            "ssn",
            "password",
            "passwd"
        };

        // ── Lifecycle ────────────────────────────────────────────────────────────

        protected override void Awake()
        {
            base.Awake();

            if (_submitButton != null)
                _submitButton.GetComponent<UnityEngine.UI.Button>()?.onClick.AddListener(OnSubmitClicked);

            if (_closeButton != null)
                _closeButton.GetComponent<UnityEngine.UI.Button>()?.onClick.AddListener(OnCloseClicked);

            // Wire category buttons.
            var categories = System.Enum.GetValues(typeof(FeedbackCategory));
            for (int i = 0; i < _categoryButtons.Length && i < categories.Length; i++)
            {
                int captured = i; // capture for closure
                _categoryButtons[captured]
                    ?.GetComponent<UnityEngine.UI.Button>()
                    ?.onClick.AddListener(() => OnCategorySelected((FeedbackCategory)captured));
            }
        }

        protected override void OnShow()
        {
            _selectedCategory = FeedbackCategory.Gameplay;
            RefreshCategoryHighlight();

            if (_messageInput != null)
            {
                _messageInput.text = string.Empty;
                _messageInput.characterLimit = MaxMessageLength;
            }

            if (_submitButton != null)
                _submitButton.GetComponent<UnityEngine.UI.Button>().interactable = true;
        }

        protected override void OnHide()
        {
            // Clear state so no message lingers if the screen is re-shown.
            if (_messageInput != null)
                _messageInput.text = string.Empty;

            _selectedCategory = FeedbackCategory.Gameplay;
        }

        private void OnDestroy()
        {
            if (_submitButton != null)
                _submitButton.GetComponent<UnityEngine.UI.Button>()?.onClick.RemoveListener(OnSubmitClicked);

            if (_closeButton != null)
                _closeButton.GetComponent<UnityEngine.UI.Button>()?.onClick.RemoveListener(OnCloseClicked);
        }

        // ── Interaction ──────────────────────────────────────────────────────────

        private void OnCategorySelected(FeedbackCategory category)
        {
            _selectedCategory = category;
            RefreshCategoryHighlight();
        }

        private void OnSubmitClicked()
        {
            if (_messageInput == null) return;

            string msg = _messageInput.text.Trim();

            // Minimum length guard.
            if (msg.Length < MinMessageLength)
            {
                ToastService.ShowMessage("Please describe your feedback");
                return;
            }

            // PII heuristic scan — warn and block if suspicious.
            if (ContainsPiiHint(msg))
            {
                GameLogger.Warning(Tag,
                    "Feedback submission blocked: message appears to contain personal information. " +
                    "User notified. No data was sent.");
                ToastService.ShowError("Please do not include personal information.");
                return;
            }

            // Hard-cap length (defensive — TMP characterLimit should already enforce this).
            if (msg.Length > MaxMessageLength)
                msg = msg.Substring(0, MaxMessageLength);

            // Build context from live save data.
            var context = BuildContext();

            // Submit via service if available.
            if (ServiceLocator.TryGet<IFeedbackService>(out var service))
            {
                service.SubmitFeedback(_selectedCategory, msg, context);
            }
            else
            {
                GameLogger.Warning(Tag, "IFeedbackService not registered — feedback not recorded.");
            }

            ToastService.ShowSuccess("Thank you for your feedback!");
            ScreenManager.Instance.Back();
        }

        private void OnCloseClicked()
        {
            ScreenManager.Instance.Back();
        }

        // ── Helpers ──────────────────────────────────────────────────────────────

        private FeedbackContext BuildContext()
        {
            int currentLevel = 0;
            int currentWorld = 0;
            int playerLevel  = 1;

            if (ServiceLocator.TryGet<ISaveService>(out var save))
            {
                var data = save.Current;
                currentLevel = data.currentLevel;
                currentWorld = data.currentWorld;
                playerLevel  = data.kingLevel;
            }

            return new FeedbackContext
            {
                currentLevel = currentLevel,
                currentWorld = currentWorld,
                playerLevel  = playerLevel,
                appVersion   = Application.version,
                environment  = FirebaseEnvironmentConfig.Environment
            };
        }

        private void RefreshCategoryHighlight()
        {
            // Visually mark the selected category button.
            // KSButton does not expose a selected-state API; use interactable as
            // a temporary highlight signal until a dedicated selected style is added.
            var categories = System.Enum.GetValues(typeof(FeedbackCategory));
            for (int i = 0; i < _categoryButtons.Length && i < categories.Length; i++)
            {
                if (_categoryButtons[i] == null) continue;
                var btn = _categoryButtons[i].GetComponent<UnityEngine.UI.Button>();
                if (btn != null)
                    btn.interactable = ((FeedbackCategory)i != _selectedCategory);
            }
        }

        private static bool ContainsPiiHint(string message)
        {
            if (string.IsNullOrEmpty(message)) return false;
            string lower = message.ToLowerInvariant();
            foreach (string pattern in PiiPatterns)
            {
                if (lower.Contains(pattern))
                    return true;
            }
            return false;
        }
    }
}
