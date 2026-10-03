using System;
using System.Collections.Generic;
using UnityEngine;
using KingSmash.Core;
namespace KingSmash.UI
{
    public class ScreenManager : MonoBehaviour
    {
        public static ScreenManager Instance { get; private set; }

        [SerializeField] private List<UIScreen> _screens = new();

        private readonly Stack<UIScreen> _history = new();
        private UIScreen _current;

        public static event Action<UIScreen, UIScreen> OnScreenTransition; // prev, next

        private void Awake()
        {
            if (Instance != null) { Destroy(gameObject); return; }
            Instance = this;
        }

        private void Start()
        {
            foreach (var s in _screens) s.Hide();
        }

        public T Show<T>() where T : UIScreen
        {
            var screen = GetScreen<T>();
            if (screen == null)
            {
                GameLogger.Warning("ScreenManager", $"Screen not found: {typeof(T).Name}");
                return null;
            }
            TransitionTo(screen);
            return screen;
        }

        public void ShowByName(string screenName)
        {
            foreach (var s in _screens)
            {
                if (s.GetType().Name == screenName) { TransitionTo(s); return; }
            }
            GameLogger.Warning("ScreenManager", $"Screen not found by name: {screenName}");
        }

        public void Back()
        {
            if (_history.Count <= 1) return;
            _history.Pop();
            var prev = _history.Peek();
            ShowDirect(prev, pushHistory: false);
        }

        private void TransitionTo(UIScreen next, bool pushHistory = true)
        {
            var prev = _current;
            prev?.Hide();
            if (pushHistory && _current != null) _history.Push(_current);
            ShowDirect(next, pushHistory: false);
            OnScreenTransition?.Invoke(prev, next);
        }

        private void ShowDirect(UIScreen screen, bool pushHistory)
        {
            _current = screen;
            if (pushHistory) _history.Push(screen);
            screen.Show();
        }

        public T GetScreen<T>() where T : UIScreen
        {
            foreach (var s in _screens)
                if (s is T typed) return typed;
            return null;
        }

        public void RegisterScreen(UIScreen screen)
        {
            if (!_screens.Contains(screen)) _screens.Add(screen);
        }
    }
}
