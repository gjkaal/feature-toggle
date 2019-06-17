using System;
using System.Threading.Tasks;
using FeatureServices.Storage.DbModel;
using Microsoft.EntityFrameworkCore;

namespace FeatureServices.Storage
{
    public class FeatureServicesContext : DbContext, ITenantContext
    {
       
        public FeatureServicesContext(DbContextOptions<FeatureServicesContext> options) : base(options)
        {
        }

        public int TenantId { get; private set; }
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

        public void Tenant(int tenantId)
        {
            //TODO: check if in context change
            TenantId = tenantId;
        }

        public async Task<TenantConfiguration> CreateApi(string api, string description)
        {
            var config = await TenantConfiguration.FirstOrDefaultAsync(q => q.Name == api && q.Tenant == TenantId);
            if (config == null)
            {
                config = new TenantConfiguration
                {
                    Tenant = TenantId,
                    Name = api,
                    Description = description,
                };
                await TenantConfiguration.AddAsync(config);
            }
           
            return config;
        }
    }
}
