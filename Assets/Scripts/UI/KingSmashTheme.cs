using UnityEngine;

namespace KingSmash.UI
{
    [CreateAssetMenu(fileName = "KingSmashTheme", menuName = "KingSmash/UI/KingSmashTheme")]
    public class KingSmashTheme : UITheme
    {
        [Header("Extended Colors")]
        public Color success    = new Color(0.18f, 0.72f, 0.25f);
        public Color warning    = new Color(1.0f,  0.65f, 0.0f);
        public Color error      = new Color(0.85f, 0.22f, 0.22f);
        public Color coinGold   = new Color(1.0f,  0.82f, 0.09f);
        public Color gemPurple  = new Color(0.62f, 0.25f, 0.92f);
        public Color xpBlue     = new Color(0.15f, 0.55f, 0.95f);
        public Color skyTop     = new Color(0.08f, 0.18f, 0.45f);
        public Color skyBottom  = new Color(0.12f, 0.28f, 0.65f);
        public Color overlayDark = new Color(0f, 0f, 0f, 0.7f);

        [Header("Typography")]
        public float fontDisplay  = 64f;
        public float fontButton   = 28f;
        public float fontCaption  = 16f;
        public float fontCurrency = 36f;

        [Header("Shadows")]
        public Color   shadowColor  = new Color(0f, 0f, 0f, 0.4f);
        public Vector2 shadowOffset = new Vector2(2f, -3f);

        [Header("Animation Extended")]
        public float durationBounce          = 0.35f;
        public float durationCountUp         = 0.8f;
        public float durationScreenTransition = 0.22f;
        public AnimationCurve easeOutBack    = new AnimationCurve(
            new Keyframe(0f,   0f),
            new Keyframe(0.7f, 1.1f),
            new Keyframe(1f,   1f)
        );
        public AnimationCurve easeInOut = new AnimationCurve(
            new Keyframe(0f,   0f),
            new Keyframe(0.5f, 0.5f),
            new Keyframe(1f,   1f)
        );

        [Header("Layout")]
        public float safeAreaPadding  = 20f;
        public float bottomNavHeight  = 120f;
        public float topBarHeight     = 100f;
        public float cardCornerRadius = 20f;
        public float modalCornerRadius = 28f;
    }
}
