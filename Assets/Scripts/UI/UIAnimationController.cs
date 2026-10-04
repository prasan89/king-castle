using System;
using System.Collections;
using UnityEngine;
namespace KingSmash.UI
{
    public static class UIAnimationController
    {
        // Shake a transform horizontally (insufficient funds feedback)
        public static IEnumerator Shake(Transform target, float magnitude = 8f, float duration = 0.3f)
        {
            Vector3 original = target.localPosition;
            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float x = UnityEngine.Random.Range(-1f, 1f) * magnitude * (1f - elapsed / duration);
                target.localPosition = original + new Vector3(x, 0f, 0f);
                yield return null;
            }
            target.localPosition = original;
        }

        // Punch-scale a button on press
        public static IEnumerator ButtonPress(Transform target, float scaleTo = 0.92f, float duration = 0.08f)
        {
            Vector3 original = target.localScale;
            float half = duration * 0.5f;
            yield return LerpScale(target, original, original * scaleTo, half);
            yield return LerpScale(target, original * scaleTo, original, half);
        }

        // Fade a CanvasGroup in/out
        public static IEnumerator Fade(CanvasGroup cg, float from, float to, float duration)
        {
            float elapsed = 0f;
            cg.alpha = from;
            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                cg.alpha = Mathf.Lerp(from, to, elapsed / duration);
                yield return null;
            }
            cg.alpha = to;
        }

        // Slide panel in from bottom
        public static IEnumerator SlideIn(RectTransform rt, float distance = 60f, float duration = 0.25f)
        {
            Vector2 end = rt.anchoredPosition;
            Vector2 start = end + Vector2.down * distance;
            yield return LerpAnchoredPos(rt, start, end, duration);
        }

        // Slide panel out to bottom
        public static IEnumerator SlideOut(RectTransform rt, float distance = 60f, float duration = 0.20f)
        {
            Vector2 start = rt.anchoredPosition;
            Vector2 end = start + Vector2.down * distance;
            yield return LerpAnchoredPos(rt, start, end, duration);
        }

        // Bounce-scale reveal (stars, rewards)
        public static IEnumerator BounceReveal(Transform target, float duration = 0.30f)
        {
            Vector3 original = target.localScale;
            target.localScale = Vector3.zero;
            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = elapsed / duration;
                float scale = BounceEase(t);
                target.localScale = original * scale;
                yield return null;
            }
            target.localScale = original;
        }

        // Count up a number label (coins, score)
        public static IEnumerator CountUp(TMPro.TextMeshProUGUI label, long from, long to, float duration, string prefix = "", string suffix = "")
        {
            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                long val = (long)Mathf.Lerp(from, to, elapsed / duration);
                label.text = prefix + val.ToString("N0") + suffix;
                yield return null;
            }
            label.text = prefix + to.ToString("N0") + suffix;
        }

        private static float BounceEase(float t)
        {
            if (t < 0.6f) return t / 0.6f * 1.15f;
            if (t < 0.8f) return 1.15f - (t - 0.6f) / 0.2f * 0.15f;
            return 1f;
        }

        private static IEnumerator LerpScale(Transform t, Vector3 from, Vector3 to, float dur)
        {
            float e = 0f;
            while (e < dur) { e += Time.unscaledDeltaTime; t.localScale = Vector3.Lerp(from, to, e / dur); yield return null; }
            t.localScale = to;
        }

        private static IEnumerator LerpAnchoredPos(RectTransform rt, Vector2 from, Vector2 to, float dur)
        {
            float e = 0f;
            while (e < dur) { e += Time.unscaledDeltaTime; rt.anchoredPosition = Vector2.Lerp(from, to, e / dur); yield return null; }
            rt.anchoredPosition = to;
        }
    }
}
