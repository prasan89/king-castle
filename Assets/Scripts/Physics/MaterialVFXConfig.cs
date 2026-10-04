using UnityEngine;

namespace KingSmash.Physics
{
    [CreateAssetMenu(fileName = "MaterialVFXConfig",
                     menuName  = "KingSmash/Physics/MaterialVFXConfig")]
    public class MaterialVFXConfig : ScriptableObject
    {
        [Header("Identity")]
        public StructureMaterialType materialType;

        [Header("Color Tints")]
        public Color intactTint      = Color.white;
        public Color damagedTint     = new Color(0.8f, 0.6f, 0.4f);
        public Color destroyedFlash  = Color.white;

        [Header("Sprites")]
        public Sprite[] intactSprites;
        public Sprite[] damagedSprites;

        [Header("Debris")]
        public float debrisSpinSpeed = 180f;
    }
}
