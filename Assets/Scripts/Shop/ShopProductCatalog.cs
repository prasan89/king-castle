using System.Collections.Generic;
using UnityEngine;

namespace KingSmash.Shop
{
    [CreateAssetMenu(fileName = "ShopProductCatalog", menuName = "KingSmash/Config/ShopProductCatalog")]
    public class ShopProductCatalog : ScriptableObject
    {
        [SerializeField]
        private List<ShopProduct> _products = new();

        private void Reset() => InitializeDefaults();

        public void InitializeDefaults()
        {
            _products = new List<ShopProduct>
            {
                new ShopProduct
                {
                    productId = "coins_1000", displayName = "1,000 Coins",
                    description = "Get 1,000 coins",
                    shopProductType = ShopProductType.CoinPack,
                    purchaseProductType = PurchaseProductType.Consumable,
                    coinsGranted = 1000, sortOrder = 0
                },
                new ShopProduct
                {
                    productId = "coins_5000", displayName = "5,000 Coins",
                    description = "Get 5,000 coins",
                    shopProductType = ShopProductType.CoinPack,
                    purchaseProductType = PurchaseProductType.Consumable,
                    coinsGranted = 5000, bonusLabel = "+10% Bonus", sortOrder = 1
                },
                new ShopProduct
                {
                    productId = "coins_15000", displayName = "15,000 Coins",
                    description = "Get 15,000 coins",
                    shopProductType = ShopProductType.CoinPack,
                    purchaseProductType = PurchaseProductType.Consumable,
                    coinsGranted = 15000, bonusLabel = "+25% Bonus", isFeatured = true, sortOrder = 2
                },
                new ShopProduct
                {
                    productId = "coins_50000", displayName = "50,000 Coins",
                    description = "Get 50,000 coins",
                    shopProductType = ShopProductType.CoinPack,
                    purchaseProductType = PurchaseProductType.Consumable,
                    coinsGranted = 50000, bonusLabel = "+50% Bonus", sortOrder = 3
                },
                new ShopProduct
                {
                    productId = "gems_small", displayName = "Small Gem Pack",
                    description = "Get 10 gems",
                    shopProductType = ShopProductType.GemPack,
                    purchaseProductType = PurchaseProductType.Consumable,
                    gemsGranted = 10, sortOrder = 10
                },
                new ShopProduct
                {
                    productId = "gems_medium", displayName = "Medium Gem Pack",
                    description = "Get 50 gems",
                    shopProductType = ShopProductType.GemPack,
                    purchaseProductType = PurchaseProductType.Consumable,
                    gemsGranted = 50, bonusLabel = "+10%", sortOrder = 11
                },
                new ShopProduct
                {
                    productId = "gems_large", displayName = "Large Gem Pack",
                    description = "Get 100 gems",
                    shopProductType = ShopProductType.GemPack,
                    purchaseProductType = PurchaseProductType.Consumable,
                    gemsGranted = 100, bonusLabel = "+25%", isFeatured = true, sortOrder = 12
                },
                new ShopProduct
                {
                    productId = "gems_mega", displayName = "Mega Gem Pack",
                    description = "Get 500 gems",
                    shopProductType = ShopProductType.GemPack,
                    purchaseProductType = PurchaseProductType.Consumable,
                    gemsGranted = 500, bonusLabel = "+50%", sortOrder = 13
                },
                new ShopProduct
                {
                    productId = "remove_ads", displayName = "Remove Ads",
                    description = "Remove all ads permanently",
                    shopProductType = ShopProductType.RemoveAds,
                    purchaseProductType = PurchaseProductType.NonConsumable,
                    sortOrder = 20
                },
                new ShopProduct
                {
                    productId = "starter_bundle", displayName = "Starter Bundle",
                    description = "Coins, gems, and power-ups to get you started",
                    shopProductType = ShopProductType.StarterBundle,
                    purchaseProductType = PurchaseProductType.Consumable,
                    coinsGranted = 5000, gemsGranted = 50,
                    powerUpRewards = new List<PowerUpReward>
                    {
                        new PowerUpReward { powerUpTypeId = "powerup_bomb",  count = 3 },
                        new PowerUpReward { powerUpTypeId = "powerup_fire",  count = 3 },
                    },
                    isFeatured = true, sortOrder = 21
                },
                new ShopProduct
                {
                    productId = "king_bundle", displayName = "King Bundle",
                    description = "Premium coins, gems, and rare power-ups",
                    shopProductType = ShopProductType.KingBundle,
                    purchaseProductType = PurchaseProductType.Consumable,
                    coinsGranted = 15000, gemsGranted = 100,
                    powerUpRewards = new List<PowerUpReward>
                    {
                        new PowerUpReward { powerUpTypeId = "powerup_megaking",  count = 2 },
                        new PowerUpReward { powerUpTypeId = "powerup_lightning", count = 2 },
                    },
                    sortOrder = 22
                },
            };
        }

        public ShopProduct GetProduct(string productId)
        {
            foreach (var p in _products)
                if (p.productId == productId) return p;
            return null;
        }

        public List<ShopProduct> GetByType(ShopProductType type)
        {
            var result = new List<ShopProduct>();
            foreach (var p in _products)
                if (p.shopProductType == type) result.Add(p);
            return result;
        }

        public List<ShopProduct> GetAll() => new List<ShopProduct>(_products);

        public List<ShopProduct> CoinProducts    => GetByType(ShopProductType.CoinPack);
        public List<ShopProduct> GemProducts     => GetByType(ShopProductType.GemPack);
        public List<ShopProduct> SpecialProducts
        {
            get
            {
                var result = new List<ShopProduct>();
                foreach (var p in _products)
                    if (p.shopProductType == ShopProductType.RemoveAds
                     || p.shopProductType == ShopProductType.StarterBundle
                     || p.shopProductType == ShopProductType.KingBundle)
                        result.Add(p);
                return result;
            }
        }

        public List<string> GetConsumableIds()
        {
            var ids = new List<string>();
            foreach (var p in _products)
                if (p.purchaseProductType == PurchaseProductType.Consumable)
                    ids.Add(p.productId);
            return ids;
        }

        public List<string> GetNonConsumableIds()
        {
            var ids = new List<string>();
            foreach (var p in _products)
                if (p.purchaseProductType == PurchaseProductType.NonConsumable)
                    ids.Add(p.productId);
            return ids;
        }
    }
}
