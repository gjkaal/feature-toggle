using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;

namespace FeatureServices.Storage.DbModel
{
    public class TenantConfiguration : DbRecord
    {
        public virtual List<FeatureValue> FeatureValue { get; set; }

        public static ModelBuilder Build(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<TenantConfiguration>().HasIndex(b => b.Name);
            return modelBuilder;
        }
    }
}
