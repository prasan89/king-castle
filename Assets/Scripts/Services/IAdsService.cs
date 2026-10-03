using System;
using System.Threading.Tasks;

namespace KingSmash.Services
{
    public enum AdResult { Completed, Skipped, Failed }

    public interface IAdsService
    {
        Task InitializeAsync();
        bool IsRewardedAdReady { get; }
        bool IsInterstitialReady { get; }
        Task<AdResult> ShowRewardedAdAsync(string placement);
        Task ShowInterstitialAsync(string placement);
        void LoadRewardedAd();
        void LoadInterstitial();
    }
}
