using System;
using System.Threading.Tasks;

namespace KingSmash.Ads
{
    public interface IInterstitialAdService
    {
        bool CanShowInterstitial(int completedLevelsCount);
        Task ShowInterstitialAsync(int completedLevelsCount, Action onDone);
        void Preload();
    }
}
