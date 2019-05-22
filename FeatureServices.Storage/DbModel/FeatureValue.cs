
using Microsoft.EntityFrameworkCore;

namespace FeatureServices.Storage.DbModel
{

    public class FeatureValue : DbRecord
    {
        public string ApplicationName { get; set; }
        public string Value { get; set; }
        public string InternalType { get; set; }

        public virtual int TenantConfigurationId { get; set; }
        public virtual TenantConfiguration TenantConfiguration { get; set; }

        public static ModelBuilder Build(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<FeatureValue>().HasIndex(b => b.ApplicationName);
            modelBuilder.Entity<FeatureValue>().HasIndex(b => b.Name);
            return modelBuilder;
        }
    }
}
