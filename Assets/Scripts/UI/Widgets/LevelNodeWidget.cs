using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using KingSmash.Core;
using KingSmash.Services;

namespace KingSmash.UI.Widgets
{
    public class LevelNodeWidget : MonoBehaviour
    {
        // ── Existing fields (preserved) ────────────────────────────────────────

        [SerializeField] private Button              _button;
        [SerializeField] private TextMeshProUGUI     _levelLabel;
        [SerializeField] private List<GameObject>    _starIcons;
        [SerializeField] private GameObject          _lockIcon;
        [SerializeField] private Image               _nodeBackground;

        // ── M13 Polish fields ──────────────────────────────────────────────

        [Header("M13 Polish")]
        [SerializeField] private KingSmashTheme  _themeConfig;
        [SerializeField] private GameObject      _currentLevelGlow;
        [SerializeField] private TextMeshProUGUI _starsCountLabel;
        [SerializeField] private GameObject      _completedCheck;

        private Coroutine _glowPulseCoroutine;

        // ── Public API ──────────────────────────────────────────────────

        /// <summary>
        /// Original 4-argument overload — kept for backwards compatibility.
        /// </summary>
        public void Setup(int levelNumber, int stars, bool unlocked, Action onTap)
            => Setup(levelNumber, stars, unlocked, onTap, isCurrent: false);

        /// <summary>
        /// M13 overload with isCurrent flag.
        /// </summary>
        public void Setup(int levelNumber, int stars, bool unlocked, Action onTap, bool isCurrent)
        {
            // Label
            if (_levelLabel != null) _levelLabel.text = levelNumber.ToString();

            // Star icons (original)
            for (int i = 0; i < _starIcons.Count; i++)
                if (_starIcons[i] != null) _starIcons[i].SetActive(i < stars);

            // Star count label (M13)
            if (_starsCountLabel != null)
                _starsCountLabel.text = stars > 0 ? $"{stars}/3" : "";

            // Lock state
            if (_lockIcon != null) _lockIcon.SetActive(!unlocked);

            // Completed check
            if (_completedCheck != null) _completedCheck.SetActive(stars > 0);

            // Button wiring
            if (_button != null)
            {
                _button.onClick.RemoveAllListeners();
                _button.interactable = unlocked;
                if (unlocked) _button.onClick.AddListener(() => onTap?.Invoke());
            }

            // Background color using theme
            ApplyNodeColor(stars, unlocked, isCurrent);

            // Alpha dim for locked
            var cg = GetComponent<CanvasGroup>();
            if (cg != null) cg.alpha = unlocked ? 1f : 0.5f;

            // Current level highlight
            if (_currentLevelGlow != null) _currentLevelGlow.SetActive(isCurrent);

            if (isCurrent)
            {
                transform.localScale = Vector3.one * 1.1f;
                if (gameObject.activeInHierarchy)
                {
                    if (_glowPulseCoroutine != null) StopCoroutine(_glowPulseCoroutine);
                    _glowPulseCoroutine = StartCoroutine(GlowPulse());
                }
            }
            else
            {
                transform.localScale = Vector3.one;
                if (_glowPulseCoroutine != null) { StopCoroutine(_glowPulseCoroutine); _glowPulseCoroutine = null; }
            }
        }

        private void ApplyNodeColor(int stars, bool unlocked, bool isCurrent)
        {
            if (_nodeBackground == null) return;

            if (!unlocked)
            {
                _nodeBackground.color = _themeConfig != null
                    ? _themeConfig.lockedColor
                    : new Color(0.35f, 0.35f, 0.45f);
            }
            else if (stars > 0)
            {
                _nodeBackground.color = _themeConfig != null
                    ? _themeConfig.success
                    : new Color(0.18f, 0.72f, 0.25f);
            }
            else
            {
                _nodeBackground.color = _themeConfig != null
                    ? _themeConfig.secondaryButton
                    : new Color(0.20f, 0.45f, 0.85f);
            }
        }

        // ── Idle glow pulse for current level ──────────────────────────────

        private IEnumerator GlowPulse()
        {
            if (_currentLevelGlow == null) yield break;

            var glowCg = _currentLevelGlow.GetComponent<CanvasGroup>();
            if (glowCg == null) glowCg = _currentLevelGlow.AddComponent<CanvasGroup>();

            while (true)
            {
                // Fade in
                float e = 0f;
                float dur = 0.7f;
                while (e < dur)
                {
                    e += Time.deltaTime;
                    glowCg.alpha = Mathf.Lerp(0.3f, 1f, e / dur);
                    yield return null;
                }
                // Fade out
                e = 0f;
                while (e < dur)
                {
                    e += Time.deltaTime;
                    glowCg.alpha = Mathf.Lerp(1f, 0.3f, e / dur);
                    yield return null;
                }
            }
        }

        private void OnDisable()
        {
            if (_glowPulseCoroutine != null) { StopCoroutine(_glowPulseCoroutine); _glowPulseCoroutine = null; }
        }
    }
}
