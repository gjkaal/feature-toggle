using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Text;

namespace FeatureServices.Storage
{
    public class DbContextFactory
    {
        private readonly IConfiguration _configuration;
        public DbContextFactory(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public T CreateDbContext<T>(string applicationName) where T: DbContext
        {
            if (string.IsNullOrEmpty(applicationName))
            {
                throw new ArgumentNullException(nameof(applicationName));
            }
            var typeName = typeof(T).Name;
            DbContextOptionsBuilder<T> builder = new DbContextOptionsBuilder<T>();
            var connectionString = new StringBuilder( _configuration.GetConnectionString(typeName));

            if (connectionString.Length==0)
            {
                throw new ArgumentOutOfRangeException("connectionString", $"Connection string not found : [{typeName}]");
            }
            connectionString.AppendFormat("Application Name={0};", applicationName);

            builder.UseSqlServer(connectionString.ToString());

            T dbContext = (T)Activator.CreateInstance(typeof(T), builder.Options);
            return dbContext;
        }
    }

}
