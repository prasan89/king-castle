using System;
using System.Collections.Generic;
using UnityEngine;

namespace KingSmash.Shop
{
    [Serializable]
    public class PowerUpReward
    {
        public string powerUpTypeId;
        public int count;
    }

    [Serializable]
    public class ShopProduct
    {
        public string productId;
        public string displayName;
        public string description;
        public ShopProductType shopProductType;
        public PurchaseProductType purchaseProductType;
        public long coinsGranted;
        public int gemsGranted;
        public List<PowerUpReward> powerUpRewards = new();
        public bool isFeatured;
        public int sortOrder;
        public string bonusLabel;
        public Sprite icon;
        public string fallbackPrice;

        // PascalCase properties for UI access
        public string ProductId      => productId;
        public string DisplayName    => displayName;
        public long   CoinsGranted   => coinsGranted;
        public int    GemsGranted    => gemsGranted;
        public string BonusLabel     => bonusLabel;
        public Sprite Icon           => icon;
        public string FallbackPrice  => fallbackPrice;
        public bool   IsFeatured     => isFeatured;
        public bool   IsNonConsumable => purchaseProductType == PurchaseProductType.NonConsumable;
    }
}
