using System.Threading.Tasks;

namespace KingSmash.Ads
{
    public interface IRewardedAdService
    {
        AdAvailability CheckAvailability(AdPlacement placement);
        Task<AdRewardResult> ShowRewardedAdAsync(AdPlacement placement);
        void Preload();
    }
}
