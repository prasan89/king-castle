using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using KingSmash.UI.Components;

namespace KingSmash.UI
{
    public class ToastService : MonoBehaviour
    {
        public static ToastService Instance { get; private set; }

        [SerializeField] private KSToast        _toastPrefab;
        [SerializeField] private Transform      _toastContainer;
        [SerializeField] private KingSmashTheme _theme;

        private const int PoolSize = 4;
        private readonly Queue<KSToast> _pool      = new Queue<KSToast>();
        private readonly List<KSToast>  _allToasts = new List<KSToast>();

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;

            for (int i = 0; i < PoolSize; i++)
            {
                KSToast t = Instantiate(_toastPrefab, _toastContainer);
                t.gameObject.SetActive(false);
                _pool.Enqueue(t);
                _allToasts.Add(t);
            }
        }

        // ── Static convenience API ──────────────────────────────────────────────

        public static void ShowCoin(long amount)
        {
            if (Instance == null) return;
            Sprite icon = Instance._theme != null ? Instance._theme.iconCoin : null;
            Color  col  = Instance._theme != null ? Instance._theme.coinGold  : new Color(1f, 0.82f, 0.09f);
            Instance.Show("+" + amount + " Coins", icon, col);
        }

        public static void ShowGem(int amount)
        {
            if (Instance == null) return;
            Sprite icon = Instance._theme != null ? Instance._theme.iconGem   : null;
            Color  col  = Instance._theme != null ? Instance._theme.gemPurple : new Color(0.62f, 0.25f, 0.92f);
            Instance.Show("+" + amount + " Gems", icon, col);
        }

        public static void ShowMessage(string msg, Sprite icon = null)
        {
            if (Instance == null) return;
            Color col = Instance._theme != null ? Instance._theme.panelBackground : new Color(0.1f, 0.18f, 0.35f, 0.95f);
            Instance.Show(msg, icon, col);
        }

        public static void ShowSuccess(string msg)
        {
            if (Instance == null) return;
            Color col = Instance._theme != null ? Instance._theme.success : new Color(0.18f, 0.72f, 0.25f);
            Instance.Show(msg, null, col);
        }

        public static void ShowError(string msg)
        {
            if (Instance == null) return;
            Color col = Instance._theme != null ? Instance._theme.error : new Color(0.85f, 0.22f, 0.22f);
            Instance.Show(msg, null, col);
        }

        // ── Core show ──────────────────────────────────────────────────────────

        public void Show(string message, Sprite icon, Color bgColor, float duration = 2.5f)
        {
            KSToast toast = GetFromPool();
            if (toast == null) return;

            toast.Show(message, icon, bgColor, duration);
            StartCoroutine(ReturnToPool(toast, duration + 0.5f));
        }

        private IEnumerator ReturnToPool(KSToast toast, float delay)
        {
            yield return new WaitForSecondsRealtime(delay);
            if (toast != null)
            {
                toast.gameObject.SetActive(false);
                _pool.Enqueue(toast);
            }
        }

        private KSToast GetFromPool()
        {
            if (_pool.Count > 0) return _pool.Dequeue();

            // All busy — steal the first active one
            for (int i = 0; i < _allToasts.Count; i++)
            {
                if (_allToasts[i] != null && _allToasts[i].gameObject.activeSelf)
                {
                    _allToasts[i].gameObject.SetActive(false);
                    return _allToasts[i];
                }
            }
            return null;
        }
    }
}
