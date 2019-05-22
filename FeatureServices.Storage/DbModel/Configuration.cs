using Microsoft.EntityFrameworkCore;
using System.Text;

namespace FeatureServices.Storage.DbModel
{

    public class FeatureValue : DbRecord
    {
        public virtual int TenantConfigurationId { get; set; }
        public virtual TenantConfiguration TenantConfiguration { get; set; }

        public static ModelBuilder Build(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<FeatureValue>().HasIndex(b => b.Name);
            return modelBuilder;
        }
    }
}
