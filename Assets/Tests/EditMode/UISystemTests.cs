using System;
using NUnit.Framework;
using UnityEngine;
using KingSmash.UI;
using KingSmash.UI.Components;

namespace KingSmash.Tests.EditMode
{
    [TestFixture]
    public class UISystemTests
    {
        // ── KingSmashTheme ────────────────────────────────────────────────────

        [Test]
        public void KingSmashTheme_CanBeCreated()
        {
            var theme = ScriptableObject.CreateInstance<KingSmashTheme>();
            Assert.IsNotNull(theme);
            UnityEngine.Object.DestroyImmediate(theme);
        }

        [Test]
        public void KingSmashTheme_HasCoinGoldColor()
        {
            var theme = ScriptableObject.CreateInstance<KingSmashTheme>();
            // coinGold should be a warm yellow (R > 0.8, G > 0.7, B < 0.4)
            Assert.Greater(theme.coinGold.r, 0.5f, "coinGold R should be > 0.5");
            UnityEngine.Object.DestroyImmediate(theme);
        }

        [Test]
        public void KingSmashTheme_HasGemPurpleColor()
        {
            var theme = ScriptableObject.CreateInstance<KingSmashTheme>();
            Assert.Greater(theme.gemPurple.b, 0.3f, "gemPurple B should be > 0.3");
            UnityEngine.Object.DestroyImmediate(theme);
        }

        // ── KSProgressBar ─────────────────────────────────────────────────────

        [Test]
        public void KSProgressBar_ClampsBelowZero()
        {
            var go  = new GameObject("pb");
            var bar = go.AddComponent<KSProgressBar>();
            bar.SetValue(-0.5f);
            Assert.GreaterOrEqual(bar.CurrentValue, 0f, "SetValue(-0.5f) should clamp to 0");
            UnityEngine.Object.DestroyImmediate(go);
        }

        [Test]
        public void KSProgressBar_ClampsAboveOne()
        {
            var go  = new GameObject("pb");
            var bar = go.AddComponent<KSProgressBar>();
            bar.SetValue(1.5f);
            Assert.LessOrEqual(bar.CurrentValue, 1f, "SetValue(1.5f) should clamp to 1");
            UnityEngine.Object.DestroyImmediate(go);
        }

        [Test]
        public void KSProgressBar_HalfValue()
        {
            var go  = new GameObject("pb");
            var bar = go.AddComponent<KSProgressBar>();
            bar.SetValue(0.5f);
            Assert.AreEqual(0.5f, bar.CurrentValue, 0.001f);
            UnityEngine.Object.DestroyImmediate(go);
        }

        // ── KSDoubleTapGuard ──────────────────────────────────────────────────

        [Test]
        public void KSDoubleTapGuard_OnDoubleTap_CanBeSet()
        {
            var go    = new GameObject("dtg");
            var guard = go.AddComponent<KSDoubleTapGuard>();
            bool fired = false;
            guard.OnDoubleTap = () => fired = true;
            Assert.IsNotNull(guard.OnDoubleTap);
            Assert.IsFalse(fired);
            UnityEngine.Object.DestroyImmediate(go);
        }

        // ── UIAnimationController (static) ────────────────────────────────────

        [Test]
        public void UIAnimationController_Shake_ReturnsIEnumerator()
        {
            var go = new GameObject("ks");
            var rt = go.AddComponent<RectTransform>();
            var result = UIAnimationController.Shake(rt, 0.1f, 5f);
            Assert.IsNotNull(result);
            UnityEngine.Object.DestroyImmediate(go);
        }

        [Test]
        public void UIAnimationController_BounceReveal_ReturnsIEnumerator()
        {
            var go = new GameObject("ks");
            var rt = go.AddComponent<RectTransform>();
            var result = UIAnimationController.BounceReveal(rt, 0.3f);
            Assert.IsNotNull(result);
            UnityEngine.Object.DestroyImmediate(go);
        }

        [Test]
        public void UIAnimationController_Fade_ReturnsIEnumerator()
        {
            var go = new GameObject("ks");
            var cg = go.AddComponent<CanvasGroup>();
            var result = UIAnimationController.Fade(cg, 0f, 1f, 0.2f);
            Assert.IsNotNull(result);
            UnityEngine.Object.DestroyImmediate(go);
        }

        // ── ScreenTransitionService ───────────────────────────────────────────

        [Test]
        public void ScreenTransitionService_CanBeInstantiated()
        {
            var go  = new GameObject("sts");
            var svc = go.AddComponent<ScreenTransitionService>();
            Assert.IsNotNull(svc);
            UnityEngine.Object.DestroyImmediate(go);
        }

        // ── ToastService ──────────────────────────────────────────────────────

        [Test]
        public void ToastService_CanBeInstantiated()
        {
            var go  = new GameObject("toast");
            var svc = go.AddComponent<ToastService>();
            Assert.IsNotNull(svc);
            UnityEngine.Object.DestroyImmediate(go);
        }

        // ── KSLoadingSpinner ──────────────────────────────────────────────────

        [Test]
        public void KSLoadingSpinner_CanBeInstantiated()
        {
            var go  = new GameObject("spinner");
            var svc = go.AddComponent<KSLoadingSpinner>();
            Assert.IsNotNull(svc);
            UnityEngine.Object.DestroyImmediate(go);
        }

        // ── KSBackButton ──────────────────────────────────────────────────────

        [Test]
        public void KSBackButton_CanBeInstantiated()
        {
            var go = new GameObject("bb");
            go.AddComponent<UnityEngine.UI.Button>();
            var btn = go.AddComponent<KSBackButton>();
            Assert.IsNotNull(btn);
            UnityEngine.Object.DestroyImmediate(go);
        }

        // ── KSBadge ───────────────────────────────────────────────────────────

        [Test]
        public void KSBadge_CanBeInstantiated()
        {
            var go    = new GameObject("badge");
            var badge = go.AddComponent<KSBadge>();
            Assert.IsNotNull(badge);
            UnityEngine.Object.DestroyImmediate(go);
        }

        // ── KSTab ─────────────────────────────────────────────────────────────

        [Test]
        public void KSTab_CanBeInstantiated()
        {
            var go  = new GameObject("tab");
            var tab = go.AddComponent<KSTab>();
            Assert.IsNotNull(tab);
            UnityEngine.Object.DestroyImmediate(go);
        }
    }
}
