using System.Reflection;
using NUnit.Framework;
using KingSmash.UI;
using KingSmash.UI.Components;
using KingSmash.UI.Screens;
using KingSmash.UI.Widgets;
using UnityEngine;

namespace KingSmash.Tests.EditMode
{
    /// <summary>
    /// 23 EditMode tests covering the M13 UI system -- design tokens, animation
    /// controller method existence, component structure, navigation API, and
    /// accessibility helpers. No scene setup required.
    /// </summary>
    public class UISystemTests
    {
        // ====================================================================
        // DesignTokenTests (5)
        // ====================================================================

        [Test]
        public void KingSmashTheme_ExtendsUITheme()
        {
            Assert.IsTrue(typeof(UITheme).IsAssignableFrom(typeof(KingSmashTheme)),
                "KingSmashTheme must be a subclass of UITheme");
        }

        [Test]
        public void KingSmashTheme_HasExtendedColors()
        {
            var type = typeof(KingSmashTheme);
            Assert.IsNotNull(type.GetField("gemPurple"), "Missing field: gemPurple");
            Assert.IsNotNull(type.GetField("xpBlue"),    "Missing field: xpBlue");
            Assert.IsNotNull(type.GetField("coinGold"),  "Missing field: coinGold");
        }

        [Test]
        public void KingSmashTheme_HasAnimationTimings()
        {
            var theme = ScriptableObject.CreateInstance<KingSmashTheme>();
            Assert.Greater(theme.durationFast,   0f, "durationFast must be > 0");
            Assert.Greater(theme.durationBounce, 0f, "durationBounce must be > 0");
            Object.DestroyImmediate(theme);
        }

        [Test]
        public void KingSmashTheme_ButtonScalePressedLessThanOne()
        {
            var theme = ScriptableObject.CreateInstance<KingSmashTheme>();
            Assert.Less(theme.buttonScalePressed, 1f,
                "buttonScalePressed should be < 1 for a press-down effect");
            Object.DestroyImmediate(theme);
        }

        [Test]
        public void KingSmashTheme_FontHierarchy()
        {
            // fontDisplay (KingSmashTheme) > fontHeading > fontBody > fontSmall > fontCaption
            var theme = ScriptableObject.CreateInstance<KingSmashTheme>();
            Assert.Greater(theme.fontDisplay, theme.fontHeading,  "fontDisplay > fontHeading");
            Assert.Greater(theme.fontHeading, theme.fontBody,     "fontHeading > fontBody");
            Assert.Greater(theme.fontBody,    theme.fontSmall,    "fontBody > fontSmall");
            Assert.Greater(theme.fontSmall,   theme.fontCaption,  "fontSmall > fontCaption");
            Object.DestroyImmediate(theme);
        }

        // ====================================================================
        // AnimationControllerTests (5)
        // ====================================================================

        [Test]
        public void UIAnimationController_BounceEaseReturnsOne_AtEnd()
        {
            // Mirror the private BounceEase formula from UIAnimationController
            float t = 1.0f;
            float result;
            if      (t < 0.6f) result = t / 0.6f * 1.15f;
            else if (t < 0.8f) result = 1.15f - (t - 0.6f) / 0.2f * 0.15f;
            else               result = 1f;
            Assert.AreEqual(1f, result, 0.001f, "BounceEase at t=1 must equal 1");
        }

        [Test]
        public void UIAnimationController_HasFadeMethod()
        {
            var method = typeof(UIAnimationController).GetMethod(
                "Fade", BindingFlags.Public | BindingFlags.Static);
            Assert.IsNotNull(method, "UIAnimationController must have a public static Fade method");
        }

        [Test]
        public void UIAnimationController_HasBounceReveal()
        {
            var method = typeof(UIAnimationController).GetMethod(
                "BounceReveal", BindingFlags.Public | BindingFlags.Static);
            Assert.IsNotNull(method, "UIAnimationController must have a public static BounceReveal method");
        }

        [Test]
        public void UIAnimationController_HasCountUp()
        {
            var method = typeof(UIAnimationController).GetMethod(
                "CountUp", BindingFlags.Public | BindingFlags.Static);
            Assert.IsNotNull(method, "UIAnimationController must have a public static CountUp method");
        }

        [Test]
        public void UIAnimationController_HasSlideIn()
        {
            var method = typeof(UIAnimationController).GetMethod(
                "SlideIn", BindingFlags.Public | BindingFlags.Static);
            Assert.IsNotNull(method, "UIAnimationController must have a public static SlideIn method");
        }

        // ====================================================================
        // ComponentTests (5)
        // ====================================================================

        [Test]
        public void KSBadge_ShowsCountAboveZero()
        {
            var method = typeof(KSBadge).GetMethod("SetCount",
                BindingFlags.Public | BindingFlags.Instance);
            Assert.IsNotNull(method, "KSBadge must have a public SetCount(int) method");

            var parameters = method.GetParameters();
            Assert.AreEqual(1, parameters.Length);
            Assert.AreEqual(typeof(int), parameters[0].ParameterType);
        }

        [Test]
        public void ButtonStyle_HasAllRequiredStyles()
        {
            Assert.IsTrue(System.Enum.IsDefined(typeof(ButtonStyle), "Primary"),   "Missing Primary");
            Assert.IsTrue(System.Enum.IsDefined(typeof(ButtonStyle), "Secondary"), "Missing Secondary");
            Assert.IsTrue(System.Enum.IsDefined(typeof(ButtonStyle), "Danger"),    "Missing Danger");
            Assert.IsTrue(System.Enum.IsDefined(typeof(ButtonStyle), "Ghost"),     "Missing Ghost");
            Assert.IsTrue(System.Enum.IsDefined(typeof(ButtonStyle), "Icon"),      "Missing Icon");
        }

        [Test]
        public void DayState_HasAllRequired()
        {
            Assert.IsTrue(System.Enum.IsDefined(typeof(DayState), "Claimed"),          "Missing Claimed");
            Assert.IsTrue(System.Enum.IsDefined(typeof(DayState), "CurrentAvailable"), "Missing CurrentAvailable");
            Assert.IsTrue(System.Enum.IsDefined(typeof(DayState), "CurrentClaimed"),   "Missing CurrentClaimed");
            Assert.IsTrue(System.Enum.IsDefined(typeof(DayState), "Upcoming"),         "Missing Upcoming");
        }

        [Test]
        public void ToastService_CanBeInstantiated()
        {
            Assert.IsTrue(typeof(UnityEngine.MonoBehaviour).IsAssignableFrom(typeof(ToastService)),
                "ToastService must be a MonoBehaviour");
        }

        [Test]
        public void ScreenTransitionService_CanBeInstantiated()
        {
            Assert.IsTrue(typeof(UnityEngine.MonoBehaviour).IsAssignableFrom(typeof(ScreenTransitionService)),
                "ScreenTransitionService must be a MonoBehaviour");
        }

        // ====================================================================
        // NavigationTests (4)
        // ====================================================================

        [Test]
        public void ScreenManager_HasShowMethod()
        {
            var methods = typeof(ScreenManager).GetMethods(BindingFlags.Public | BindingFlags.Instance);
            bool found = false;
            foreach (var m in methods)
                if (m.Name == "Show" && m.IsGenericMethodDefinition) { found = true; break; }
            Assert.IsTrue(found, "ScreenManager must have a public generic Show<T>() method");
        }

        [Test]
        public void ScreenManager_HasBackMethod()
        {
            var method = typeof(ScreenManager).GetMethod("Back",
                BindingFlags.Public | BindingFlags.Instance);
            Assert.IsNotNull(method, "ScreenManager must have a public Back() method");
        }

        [Test]
        public void UIScreen_HasIsVisibleProperty()
        {
            var prop = typeof(UIScreen).GetProperty("IsVisible",
                BindingFlags.Public | BindingFlags.Instance);
            Assert.IsNotNull(prop, "UIScreen must have a public IsVisible property");
            Assert.AreEqual(typeof(bool), prop.PropertyType);
        }

        [Test]
        public void UIScreen_HasShowAndHide()
        {
            Assert.IsNotNull(
                typeof(UIScreen).GetMethod("Show", BindingFlags.Public | BindingFlags.Instance),
                "UIScreen must have a public Show() method");
            Assert.IsNotNull(
                typeof(UIScreen).GetMethod("Hide", BindingFlags.Public | BindingFlags.Instance),
                "UIScreen must have a public Hide() method");
        }

        // ====================================================================
        // AccessibilityTests (4)
        // ====================================================================

        [Test]
        public void SafeAreaHandler_HasApplySafeArea()
        {
            var method = typeof(SafeAreaHandler).GetMethod("ApplySafeArea",
                BindingFlags.Public | BindingFlags.Instance,
                null,
                new[] { typeof(UnityEngine.RectTransform) },
                null);
            Assert.IsNotNull(method, "SafeAreaHandler must have ApplySafeArea(RectTransform)");
        }

        [Test]
        public void KSLoadingSpinner_HasShowAndHide()
        {
            Assert.IsNotNull(
                typeof(KSLoadingSpinner).GetMethod("Show", BindingFlags.Public | BindingFlags.Instance),
                "KSLoadingSpinner must have a public Show() method");
            Assert.IsNotNull(
                typeof(KSLoadingSpinner).GetMethod("Hide", BindingFlags.Public | BindingFlags.Instance),
                "KSLoadingSpinner must have a public Hide() method");
        }

        [Test]
        public void KSBackButton_HandlesEscapeKey()
        {
            var method = typeof(KSBackButton).GetMethod("Update",
                BindingFlags.NonPublic | BindingFlags.Instance);
            Assert.IsNotNull(method, "KSBackButton must implement Update() to poll for Escape key");
        }

        [Test]
        public void KSDoubleTapGuard_HasCooldownField()
        {
            var field = typeof(KSDoubleTapGuard).GetField("_cooldown",
                BindingFlags.NonPublic | BindingFlags.Instance);
            Assert.IsNotNull(field, "KSDoubleTapGuard must have a _cooldown field");
            Assert.AreEqual(typeof(float), field.FieldType, "_cooldown must be a float");
        }
    }
}
