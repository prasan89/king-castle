using System;
using System.Linq;
using NUnit.Framework;
using KingSmash.Audio;
using KingSmash.VFX;
using KingSmash.Camera;
using KingSmash.Characters;
using KingSmash.Gameplay;

namespace KingSmash.Tests.EditMode
{
    [TestFixture]
    public class AudioCategoryTests
    {
        [Test]
        public void SoundId_AllValuesUnique()
        {
            var values   = (SoundId[])Enum.GetValues(typeof(SoundId));
            var distinct = values.Distinct().ToArray();
            Assert.AreEqual(values.Length, distinct.Length,
                "SoundId enum contains duplicate integer values.");
        }

        [Test]
        public void AudioCategory_HasExpectedValues()
        {
            Assert.IsTrue(Enum.IsDefined(typeof(AudioCategory), AudioCategory.Music),   "Missing AudioCategory.Music");
            Assert.IsTrue(Enum.IsDefined(typeof(AudioCategory), AudioCategory.SFX),     "Missing AudioCategory.SFX");
            Assert.IsTrue(Enum.IsDefined(typeof(AudioCategory), AudioCategory.UI),      "Missing AudioCategory.UI");
            Assert.IsTrue(Enum.IsDefined(typeof(AudioCategory), AudioCategory.Ambient), "Missing AudioCategory.Ambient");
        }

        [Test]
        public void SoundId_HasRequiredUIValues()
        {
            Assert.IsTrue(Enum.IsDefined(typeof(SoundId), SoundId.ButtonClick),   "Missing SoundId.ButtonClick");
            Assert.IsTrue(Enum.IsDefined(typeof(SoundId), SoundId.ButtonConfirm), "Missing SoundId.ButtonConfirm");
            Assert.IsTrue(Enum.IsDefined(typeof(SoundId), SoundId.Back),          "Missing SoundId.Back");
            Assert.IsTrue(Enum.IsDefined(typeof(SoundId), SoundId.Reward),        "Missing SoundId.Reward");
        }

        [Test]
        public void SoundId_HasRequiredGameplayValues()
        {
            Assert.IsTrue(Enum.IsDefined(typeof(SoundId), SoundId.KingLaunch),      "Missing SoundId.KingLaunch");
            Assert.IsTrue(Enum.IsDefined(typeof(SoundId), SoundId.KingImpactHeavy), "Missing SoundId.KingImpactHeavy");
            Assert.IsTrue(Enum.IsDefined(typeof(SoundId), SoundId.WoodBreak),       "Missing SoundId.WoodBreak");
            Assert.IsTrue(Enum.IsDefined(typeof(SoundId), SoundId.LevelComplete),   "Missing SoundId.LevelComplete");
        }
    }

    [TestFixture]
    public class VFXIdTests
    {
        [Test]
        public void VFXId_AllValuesUnique()
        {
            var values   = (VFXId[])Enum.GetValues(typeof(VFXId));
            var distinct = values.Distinct().ToArray();
            Assert.AreEqual(values.Length, distinct.Length,
                "VFXId enum contains duplicate integer values.");
        }

        [Test]
        public void VFXId_HasDestructionVFX()
        {
            Assert.IsTrue(Enum.IsDefined(typeof(VFXId), VFXId.WoodBreak),   "Missing VFXId.WoodBreak");
            Assert.IsTrue(Enum.IsDefined(typeof(VFXId), VFXId.StoneBreak),  "Missing VFXId.StoneBreak");
            Assert.IsTrue(Enum.IsDefined(typeof(VFXId), VFXId.MetalImpact), "Missing VFXId.MetalImpact");
            Assert.IsTrue(Enum.IsDefined(typeof(VFXId), VFXId.IceBreak),    "Missing VFXId.IceBreak");
        }

        [Test]
        public void VFXId_HasPowerUpVFX()
        {
            Assert.IsTrue(Enum.IsDefined(typeof(VFXId), VFXId.BombExplosion),   "Missing VFXId.BombExplosion");
            Assert.IsTrue(Enum.IsDefined(typeof(VFXId), VFXId.FireImpact),      "Missing VFXId.FireImpact");
            Assert.IsTrue(Enum.IsDefined(typeof(VFXId), VFXId.IceImpact),       "Missing VFXId.IceImpact");
            Assert.IsTrue(Enum.IsDefined(typeof(VFXId), VFXId.LightningImpact), "Missing VFXId.LightningImpact");
            Assert.IsTrue(Enum.IsDefined(typeof(VFXId), VFXId.MegaKingAura),    "Missing VFXId.MegaKingAura");
        }

        [Test]
        public void VFXId_HasWorldAtmosphereVFX()
        {
            Assert.IsTrue(Enum.IsDefined(typeof(VFXId), VFXId.World1Leaves), "Missing VFXId.World1Leaves");
            Assert.IsTrue(Enum.IsDefined(typeof(VFXId), VFXId.World2Dust),   "Missing VFXId.World2Dust");
            Assert.IsTrue(Enum.IsDefined(typeof(VFXId), VFXId.World3Snow),   "Missing VFXId.World3Snow");
            Assert.IsTrue(Enum.IsDefined(typeof(VFXId), VFXId.World4Embers), "Missing VFXId.World4Embers");
            Assert.IsTrue(Enum.IsDefined(typeof(VFXId), VFXId.World5Sparks), "Missing VFXId.World5Sparks");
        }
    }

    [TestFixture]
    public class CameraShakeTests
    {
        [Test]
        public void CameraShakeProfile_CanBeCreated()
        {
            var profile = UnityEngine.ScriptableObject.CreateInstance<CameraShakeProfile>();
            Assert.IsNotNull(profile);
            UnityEngine.Object.DestroyImmediate(profile);
        }

        [Test]
        public void CameraShakeProfile_DurationPositive()
        {
            var profile = UnityEngine.ScriptableObject.CreateInstance<CameraShakeProfile>();
            profile.duration = 0.3f;
            Assert.Greater(profile.duration, 0f);
            UnityEngine.Object.DestroyImmediate(profile);
        }

        [Test]
        public void CameraShakeProfile_MagnitudeInRange()
        {
            var profile = UnityEngine.ScriptableObject.CreateInstance<CameraShakeProfile>();
            Assert.GreaterOrEqual(profile.magnitude, 0.01f,
                "Default magnitude should be at least 0.01");
            Assert.LessOrEqual(profile.magnitude, 1.0f,
                "Default magnitude should be at most 1.0");
            UnityEngine.Object.DestroyImmediate(profile);
        }

        [Test]
        public void HitStopDuration_ExtremelyShort()
        {
            var config = UnityEngine.ScriptableObject.CreateInstance<HitStopConfig>();
            Assert.Less(config.light,  0.25f, "light preset must be < 0.25 s");
            Assert.Less(config.medium, 0.25f, "medium preset must be < 0.25 s");
            Assert.Less(config.heavy,  0.25f, "heavy preset must be < 0.25 s");
            Assert.Less(config.boss,   0.25f, "boss preset must be < 0.25 s");
            UnityEngine.Object.DestroyImmediate(config);
        }
    }

    [TestFixture]
    public class KingStateTests
    {
        [Test]
        public void KingState_HasIdleState()
        {
            Assert.IsTrue(Enum.IsDefined(typeof(KingState), KingState.Idle));
        }

        [Test]
        public void KingState_HasAllFlightStates()
        {
            Assert.IsTrue(Enum.IsDefined(typeof(KingState), KingState.Flying), "Missing KingState.Flying");
            var names = Enum.GetNames(typeof(KingState));
            Assert.Contains("FastFlight",  names, "Missing KingState.FastFlight");
            Assert.Contains("FirstImpact", names, "Missing KingState.FirstImpact");
            Assert.Contains("Rebound",     names, "Missing KingState.Rebound");
        }

        [Test]
        public void KingState_HasPresentationStates()
        {
            var names = Enum.GetNames(typeof(KingState));
            Assert.Contains("PowerUpActive", names, "Missing KingState.PowerUpActive");
            Assert.Contains("QueenRescue",   names, "Missing KingState.QueenRescue");
            Assert.Contains("LevelComplete", names, "Missing KingState.LevelComplete");
        }

        [Test]
        public void KingState_ValuesNotReordered()
        {
            Assert.AreEqual(0, (int)KingState.Idle,            "KingState.Idle must be 0");
            Assert.AreEqual(1, (int)KingState.PreparingLaunch, "KingState.PreparingLaunch must be 1");
            Assert.AreEqual(2, (int)KingState.Launched,        "KingState.Launched must be 2");
            Assert.AreEqual(3, (int)KingState.Flying,          "KingState.Flying must be 3");
            Assert.AreEqual(4, (int)KingState.Impact,          "KingState.Impact must be 4");
            Assert.AreEqual(5, (int)KingState.Stunned,         "KingState.Stunned must be 5");
            Assert.AreEqual(6, (int)KingState.Defeated,        "KingState.Defeated must be 6");
            Assert.AreEqual(7, (int)KingState.Victory,         "KingState.Victory must be 7");
        }
    }

    [TestFixture]
    public class WorldThemeTests
    {
        [Test]
        public void WorldId_HasAllFiveWorlds()
        {
            var worldIdType = typeof(KingSmash.VFX.WorldAtmosphereController).GetNestedType("WorldId");
            Assert.IsNotNull(worldIdType, "WorldAtmosphereController.WorldId nested type not found");

            var names = Enum.GetNames(worldIdType);
            Assert.Contains("Forest",      names, "Missing WorldId.Forest");
            Assert.Contains("FinalCastle", names, "Missing WorldId.FinalCastle");

            int forestVal      = (int)Enum.Parse(worldIdType, "Forest");
            int finalCastleVal = (int)Enum.Parse(worldIdType, "FinalCastle");
            Assert.AreEqual(1, forestVal,      "WorldId.Forest must equal 1");
            Assert.AreEqual(5, finalCastleVal, "WorldId.FinalCastle must equal 5");
        }

        [Test]
        public void QueenState_HasWorriedAndCelebrating()
        {
            var names = Enum.GetNames(typeof(QueenState));
            Assert.Contains("Worried",     names, "Missing QueenState.Worried");
            Assert.Contains("Celebrating", names, "Missing QueenState.Celebrating");
        }

        [Test]
        public void QueenState_CapturedAndRescuedPreserved()
        {
            Assert.IsTrue(Enum.IsDefined(typeof(QueenState), QueenState.Captured), "Missing QueenState.Captured");
            Assert.IsTrue(Enum.IsDefined(typeof(QueenState), QueenState.Rescued),  "Missing QueenState.Rescued");
        }

        [Test]
        public void VFXWorldIds_MatchWorldCount()
        {
            var worldVFX = new[]
            {
                VFXId.World1Leaves,
                VFXId.World2Dust,
                VFXId.World3Snow,
                VFXId.World4Embers,
                VFXId.World5Sparks
            };
            Assert.AreEqual(5, worldVFX.Length);
        }
    }

    [TestFixture]
    public class ParticlePerformanceTests
    {
        [Test]
        public void QualityTier_HasLowMediumHigh()
        {
            Assert.IsTrue(Enum.IsDefined(typeof(ParticlePerformanceConfig.QualityTier), ParticlePerformanceConfig.QualityTier.Low),    "Missing QualityTier.Low");
            Assert.IsTrue(Enum.IsDefined(typeof(ParticlePerformanceConfig.QualityTier), ParticlePerformanceConfig.QualityTier.Medium), "Missing QualityTier.Medium");
            Assert.IsTrue(Enum.IsDefined(typeof(ParticlePerformanceConfig.QualityTier), ParticlePerformanceConfig.QualityTier.High),   "Missing QualityTier.High");
        }

        [Test]
        public void ParticlePerformanceConfig_LowTierHasFewerParticles()
        {
            var config  = UnityEngine.ScriptableObject.CreateInstance<ParticlePerformanceConfig>();
            int lowMax  = config.MaxSFXParticles(ParticlePerformanceConfig.QualityTier.Low);
            int highMax = config.MaxSFXParticles(ParticlePerformanceConfig.QualityTier.High);
            Assert.Less(lowMax, highMax,
                $"Low tier max ({lowMax}) should be less than High tier max ({highMax})");
            UnityEngine.Object.DestroyImmediate(config);
        }

        [Test]
        public void ParticlePerformanceConfig_HighTierHasMoreParticles()
        {
            var config  = UnityEngine.ScriptableObject.CreateInstance<ParticlePerformanceConfig>();
            int medMax  = config.MaxSFXParticles(ParticlePerformanceConfig.QualityTier.Medium);
            int highMax = config.MaxSFXParticles(ParticlePerformanceConfig.QualityTier.High);
            Assert.GreaterOrEqual(highMax, medMax,
                $"High tier max ({highMax}) should be >= Medium tier max ({medMax})");
            UnityEngine.Object.DestroyImmediate(config);
        }
    }
}
