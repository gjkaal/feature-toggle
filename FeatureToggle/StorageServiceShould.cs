using FeatureServices.Storage;
using Microsoft.Extensions.Configuration;
using System;
using System.IO;
using Xunit;

namespace FeatureServices
{
    public class StorageServiceShould
    {
        private static readonly IConfiguration _configuration;
        private static readonly DbContextFactory _dbContextFactory;

        static StorageServiceShould()
        {
            var config = new ConfigurationBuilder();
            var currentFolder = Directory.GetCurrentDirectory();
            config.SetBasePath(currentFolder);
            config.AddJsonFile(currentFolder + "\\Settings.json", true);

            // Call additional providers here as needed.
            // Call AddCommandLine last to allow arguments to override other configuration.

            var userProfile = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            config.SetBasePath(userProfile);
            config.AddJsonFile("UserSecrets.json", true);
            _configuration = config.Build();

            _dbContextFactory = new DbContextFactory(_configuration);
        }

        [Fact]
        public void Initialize()
        {
            using (var dbContext = _dbContextFactory.CreateDbContext<FeatureServicesContext>("FeatureServiceTest"))
            {
                dbContext.TenantConfiguration.Add(new Storage.DbModel.TenantConfiguration { });
                dbContext.SaveChanges();
            }

        }
    }
}
