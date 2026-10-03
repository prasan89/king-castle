using System;
using UnityEngine;
namespace KingSmash.UI
{
    public abstract class UIScreen : MonoBehaviour
    {
        [SerializeField] protected UITheme _theme;
        [SerializeField] private CanvasGroup _canvasGroup;

        public bool IsVisible { get; private set; }

        public static event Action<UIScreen> OnScreenShown;
        public static event Action<UIScreen> OnScreenHidden;

        protected virtual void Awake()
        {
            if (_canvasGroup == null) _canvasGroup = GetComponent<CanvasGroup>();
            if (_canvasGroup == null) _canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }

        public virtual void Show()
        {
            gameObject.SetActive(true);
            IsVisible = true;
            if (_canvasGroup != null) { _canvasGroup.alpha = 1f; _canvasGroup.interactable = true; _canvasGroup.blocksRaycasts = true; }
            OnScreenShown?.Invoke(this);
            OnShow();
        }

        public virtual void Hide()
        {
            IsVisible = false;
            if (_canvasGroup != null) { _canvasGroup.alpha = 0f; _canvasGroup.interactable = false; _canvasGroup.blocksRaycasts = false; }
            gameObject.SetActive(false);
            OnScreenHidden?.Invoke(this);
            OnHide();
        }

        protected virtual void OnShow() { }
        protected virtual void OnHide() { }
    }
}
