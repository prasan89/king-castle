using UnityEngine;
namespace KingSmash.UI
{
    [CreateAssetMenu(fileName = "UITheme", menuName = "KingSmash/UI/UITheme")]
    public class UITheme : ScriptableObject
    {
        [Header("Colors")]
        public Color primaryButton   = new Color(0.18f, 0.72f, 0.25f); // green
        public Color secondaryButton = new Color(0.20f, 0.45f, 0.85f); // blue
        public Color dangerButton    = new Color(0.85f, 0.22f, 0.22f); // red
        public Color goldAccent      = new Color(1.00f, 0.82f, 0.09f); // gold
        public Color panelBackground = new Color(0.10f, 0.18f, 0.35f, 0.95f); // dark navy
        public Color textPrimary     = Color.white;
        public Color textSecondary   = new Color(0.85f, 0.85f, 0.85f);
        public Color disabledColor   = new Color(0.5f, 0.5f, 0.5f);
        public Color lockedColor     = new Color(0.35f, 0.35f, 0.45f);
        public Color starFilled      = new Color(1.00f, 0.82f, 0.09f);
        public Color starEmpty       = new Color(0.4f, 0.4f, 0.4f);

        [Header("Spacing")]
        public float spacingXS  =  4f;
        public float spacingS   =  8f;
        public float spacingM   = 16f;
        public float spacingL   = 24f;
        public float spacingXL  = 32f;

        [Header("Corner Radius")]
        public float radiusS  =  8f;
        public float radiusM  = 16f;
        public float radiusL  = 24f;
        public float radiusXL = 32f;

        [Header("Button Sizes")]
        public Vector2 primaryButtonSize   = new Vector2(320f, 80f);
        public Vector2 secondaryButtonSize = new Vector2(200f, 64f);
        public Vector2 iconButtonSize      = new Vector2(64f, 64f);

        [Header("Font Sizes")]
        public float fontTitle    = 48f;
        public float fontHeading  = 32f;
        public float fontBody     = 24f;
        public float fontSmall    = 18f;
        public float fontTiny     = 14f;

        [Header("Animation")]
        public float durationFast   = 0.15f;
        public float durationNormal = 0.25f;
        public float duractionSlow  = 0.40f;
        public float buttonScalePressed  = 0.92f;
        public float panelSlideDistance = 60f;

        [Header("Icons — assign in Inspector")]
        public Sprite iconCoin;
        public Sprite iconGem;
        public Sprite iconStar;
        public Sprite iconStarEmpty;
        public Sprite iconLock;
        public Sprite iconSettings;
        public Sprite iconHome;
        public Sprite iconPlay;
        public Sprite iconPause;
        public Sprite iconCheck;
        public Sprite iconBack;
    }
}
