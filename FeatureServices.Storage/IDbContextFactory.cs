using Microsoft.EntityFrameworkCore;

namespace FeatureServices.Storage
{
    public interface IDbContextFactory
    {
        T CreateDbContext<T>(string applicationName) where T : DbContext;
    }
}