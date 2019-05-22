using FeatureServices.Storage.DbModel;
using Microsoft.EntityFrameworkCore;

namespace FeatureServices.Storage
{
    public class FeatureServicesContext : DbContext
    {
        public FeatureServicesContext(DbContextOptions<FeatureServicesContext> options) : base(options)
        {
        }

        public DbSet<TenantConfiguration> TenantConfiguration { get; set; }
        public DbSet<FeatureValue> FeatureValue { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Build model
            base.OnModelCreating(modelBuilder);
            // Then add extensions
            DbModel.TenantConfiguration.Build(modelBuilder);
            DbModel.FeatureValue.Build(modelBuilder);
        }
    }


    
}
