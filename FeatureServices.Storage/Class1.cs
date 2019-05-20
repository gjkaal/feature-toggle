using Microsoft.EntityFrameworkCore;
using System;

namespace FeatureServices.Storage
{
    public class FeatureServicesContext : DbContext
    {
        public DbSet<Tenant> Configuration { get; set; }
        public DbSet<FeatureValue> FeatureValue { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite("Data Source=FeatureServices.db");
        }
    }
    
}
