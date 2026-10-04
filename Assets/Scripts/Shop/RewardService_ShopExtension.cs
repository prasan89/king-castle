using KingSmash.Core;
using KingSmash.Economy;

namespace KingSmash.Economy
{
    public static class RewardServiceShopExtensions
    {
        public static void GrantShopCoins(this RewardService rs, long coins, string productId)
        {
            if (!ServiceLocator.TryGet<CurrencyService>(out var currency))
            {
                GameLogger.Error("RewardServiceShopExtensions", "CurrencyService not registered.");
                return;
            }
            currency.TryAdd(coins, "shop_purchase_" + productId, out _);
        }
    }
}
