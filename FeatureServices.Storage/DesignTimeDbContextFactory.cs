using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using System;
using System.IO;

namespace FeatureServices.Storage
{
    public class DesignTimeDbContextFactory<T> : IDesignTimeDbContextFactory<T> where T : DbContext
    {
        private readonly IConfiguration _configuration;
        public DesignTimeDbContextFactory()
        {
            var config = new ConfigurationBuilder();
            var currentFolder = Directory.GetCurrentDirectory();
            config.AddJsonFile(currentFolder + "\\Settings.json", true);

            var userProfile = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            config.SetBasePath(userProfile);
            config.AddJsonFile("UserSecrets.json", true);
            _configuration = config.Build();
        }

        public T CreateDbContext(string[] args)
        {
            var typeName = typeof(T).Name;
            DbContextOptionsBuilder<T> builder = new DbContextOptionsBuilder<T>();
            var connectionString = _configuration.GetConnectionString(typeName);

            if (string.IsNullOrEmpty(connectionString))
            {
                throw new ArgumentOutOfRangeException("connectionString", $"Connectionstring not found : [{typeName}]");
            }

            builder.UseSqlServer(connectionString);

            T dbContext = (T)Activator.CreateInstance(typeof(T), builder.Options);
            return dbContext;
        }
    }


    
}
