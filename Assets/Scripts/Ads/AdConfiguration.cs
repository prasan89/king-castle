using System.Collections.Generic;
using KingSmash.PowerUps;
using UnityEngine;

namespace KingSmash.Ads
{
    [CreateAssetMenu(fileName = "AdConfiguration", menuName = "KingSmash/Config/AdConfiguration")]
    public class AdConfiguration : ScriptableObject
    {
        [Header("Feature Flags")]
        public bool rewardedAdsEnabled = true;
        public bool interstitialAdsEnabled = true;
        public bool doubleRewardEnabled = true;
        public bool extraAttemptEnabled = true;
        public bool freePowerUpEnabled = true;

        [Header("Interstitial Policy")]
        public int interstitialMinLevels = 3;
        public int maxInterstitialsPerSession = 3;

        [Header("Reward Configuration")]
        public float doubleRewardMultiplier = 2f;
        public PowerUpType freePowerUpType = PowerUpType.Bomb;

        [Header("Test Device IDs")]
        public List<string> testDeviceIds = new();

        [Header("Dev Ad Unit IDs (Google Official Test IDs)")]
        [SerializeField] private string devAppId = "ca-app-pub-3940256099942544~3347511713";
        [SerializeField] private string devRewardedAdUnitId = "ca-app-pub-3940256099942544/5224354917";
        [SerializeField] private string devInterstitialAdUnitId = "ca-app-pub-3940256099942544/1033173712";

        [Header("Staging Ad Unit IDs")]
        [SerializeField] private string stagingAppId = "STAGING_APP_ID";
        [SerializeField] private string stagingRewardedAdUnitId = "STAGING_REWARDED_ID";
        [SerializeField] private string stagingInterstitialAdUnitId = "STAGING_INTERSTITIAL_ID";

        [Header("Production Ad Unit IDs (Set in Inspector — never commit values)")]
        [SerializeField] private string productionAppId = "";
        [SerializeField] private string productionRewardedAdUnitId = "";
        [SerializeField] private string productionInterstitialAdUnitId = "";

        public string GetAppId()
        {
#if KING_SMASH_DEV
            return devAppId;
#elif KING_SMASH_STAGING
            return stagingAppId;
#else
            return productionAppId;
#endif
        }

        public string GetRewardedAdUnitId()
        {
#if KING_SMASH_DEV
            return devRewardedAdUnitId;
#elif KING_SMASH_STAGING
            return stagingRewardedAdUnitId;
#else
            return productionRewardedAdUnitId;
#endif
        }

        public string GetInterstitialAdUnitId()
        {
#if KING_SMASH_DEV
            return devInterstitialAdUnitId;
#elif KING_SMASH_STAGING
            return stagingInterstitialAdUnitId;
#else
            return productionInterstitialAdUnitId;
#endif
        }
    }
}
