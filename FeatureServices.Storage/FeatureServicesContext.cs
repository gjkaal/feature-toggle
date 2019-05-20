using Microsoft.EntityFrameworkCore;
using FeatureServices.Storage.DbModel;

namespace FeatureServices.Storage
{
    public class ServicesContext : DbContext
    {
    }

    public class SqlLightServicesContext : ServicesContext
    {
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite("Data Source=FeatureServices.db");
        }
    }


    public class SqlServicesContext : DbContext
    {
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer("");
        }
    }
}
