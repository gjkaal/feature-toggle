using Microsoft.EntityFrameworkCore;
using FeatureServices.Storage.DbModel;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore.Design;

namespace FeatureServices.Storage
{
    public class ServicesContext : DbContext
    {
        public DbSet<TenantConfiguration> TenantConfiguration { get; set; }
        public DbSet<FeatureValue> FeatureValue { get; set; }
    }

    public class StorageFactory : IDesignTimeDbContextFactory<SqlServicesContext>
    {
        readonly IConfiguration _configuration;
        readonly ILogger<SqlServicesContext> _logger;
        public StorageFactory(IConfiguration configuration, ILogger<SqlServicesContext> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public SqlServicesContext CreateDbContext(string[] args)
        {
            _logger.LogInformation("Creating new db context");
            var optionsBuilder = new DbContextOptionsBuilder<SqlServicesContext>();
            optionsBuilder.UseSqlServer("Data Source=blog.db");
            return new SqlServicesContext(optionsBuilder);

        }

        

    }

    public class SqlServicesContext : ServicesContext
    {
        private readonly string _connectionString;
        private readonly DbContextOptionsBuilder _optionsBuilder;

        public SqlServicesContext(DbContextOptionsBuilder optionsBuilder)
        {
            _optionsBuilder = optionsBuilder;
            //  _connectionString = connectionString;
        }

        //protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        //{
        //    optionsBuilder.UseSqlServer();
        //}
    }
}
