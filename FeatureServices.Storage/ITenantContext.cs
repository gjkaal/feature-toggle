namespace FeatureServices.Storage
{
    public interface ITenantContext
    {
        void Tenant(int tenantId);
    }
}