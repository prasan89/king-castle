namespace KingSmash.Shop
{
    public interface IEntitlementService
    {
        bool HasEntitlement(string entitlementId);
        void GrantEntitlement(string entitlementId);
        void RevokeEntitlement(string entitlementId);
    }
}
