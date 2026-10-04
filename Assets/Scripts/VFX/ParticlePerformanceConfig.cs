using UnityEngine;

namespace KingSmash.VFX
{
    [CreateAssetMenu(fileName = "ParticlePerformanceConfig",
                     menuName  = "KingSmash/VFX/ParticlePerformanceConfig")]
    public class ParticlePerformanceConfig : ScriptableObject
    {
        public enum QualityTier { Low, Medium, High }

        [Header("Low Tier")]
        [SerializeField] private int  _lowMaxSFXParticles     = 20;
        [SerializeField] private int  _lowMaxAmbientParticles = 10;
        [SerializeField] private bool _lowEnableScreenFlash   = true;
        [SerializeField] private bool _lowEnableHitStop       = false;

        [Header("Medium Tier")]
        [SerializeField] private int  _medMaxSFXParticles     = 40;
        [SerializeField] private int  _medMaxAmbientParticles = 25;
        [SerializeField] private bool _medEnableScreenFlash   = true;
        [SerializeField] private bool _medEnableHitStop       = true;

        [Header("High Tier")]
        [SerializeField] private int  _highMaxSFXParticles     = 80;
        [SerializeField] private int  _highMaxAmbientParticles = 50;
        [SerializeField] private bool _highEnableScreenFlash   = true;
        [SerializeField] private bool _highEnableHitStop       = true;

        public int  MaxSFXParticles    (QualityTier tier) => tier switch
        {
            QualityTier.Low    => _lowMaxSFXParticles,
            QualityTier.Medium => _medMaxSFXParticles,
            _                  => _highMaxSFXParticles
        };

        public int  MaxAmbientParticles(QualityTier tier) => tier switch
        {
            QualityTier.Low    => _lowMaxAmbientParticles,
            QualityTier.Medium => _medMaxAmbientParticles,
            _                  => _highMaxAmbientParticles
        };

        public bool EnableScreenFlash  (QualityTier tier) => tier switch
        {
            QualityTier.Low    => _lowEnableScreenFlash,
            QualityTier.Medium => _medEnableScreenFlash,
            _                  => _highEnableScreenFlash
        };

        public bool EnableHitStop      (QualityTier tier) => tier switch
        {
            QualityTier.Low    => _lowEnableHitStop,
            QualityTier.Medium => _medEnableHitStop,
            _                  => _highEnableHitStop
        };

        public static QualityTier DetectTier()
        {
            int vramMB   = SystemInfo.graphicsMemorySize;
            int cpuCores = SystemInfo.processorCount;

            if (vramMB < 1024 || cpuCores < 4)
                return QualityTier.Low;

            if (vramMB < 2048 || cpuCores < 6)
                return QualityTier.Medium;

            return QualityTier.High;
        }
    }
}
