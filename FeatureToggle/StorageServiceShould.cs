using FeatureServices.Storage;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.IO;
using System.Threading.Tasks;
using Xunit;

namespace FeatureServices
{
    public class StorageServiceShould
    {
        private static readonly IConfiguration _configuration;
        private static readonly DbContextFactory _dbContextFactory;

        private Mock<ILogger<FeatureService>> FeatureServiceLogger = new Mock<ILogger<FeatureService>>();
        private Mock<ILogger<SqlFeatureStorage>> FeatureStorageLogger = new Mock<ILogger<SqlFeatureStorage>>();

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
        public async Task Initialize()
        {
            using (var dbContext = _dbContextFactory.CreateDbContext<FeatureServicesContext>("FeatureServiceTest"))
            {
                await dbContext.CreateApi("MFRGCY3BMRQWE4TB", "Nice 2 Experience");
                dbContext.SaveChanges();
            }
        }

        [Fact]
        public async Task Integration_InitService()
        {
            // initialize
            var storage = new SqlFeatureStorage(FeatureStorageLogger.Object, _dbContextFactory);
            var service = new FeatureService(FeatureServiceLogger.Object, storage);
            var initialized = await service.Initialize("MFRGCY3BMRQWE4TB", "StorageServiceShould");
            Assert.True(initialized);
        }


        [Fact]
        public async Task Integration_InitServiceWithNewApplication()
        {
            // initialize
            var storage = new SqlFeatureStorage(FeatureStorageLogger.Object, _dbContextFactory);
            var service = new FeatureService(FeatureServiceLogger.Object, storage);
            var initialized = await service.Initialize("MFRGCY3BMRQWE4TB", "OtherApplication");
            Assert.True(initialized);
        }

        [Fact]
        public async Task Integration_InitServiceWithInvalidApiKey()
        {
            // initialize
            var storage = new SqlFeatureStorage(FeatureStorageLogger.Object, _dbContextFactory);
            var service = new FeatureService(FeatureServiceLogger.Object, storage);
            var initialized = await service.Initialize("MFRGCY3BMRQWEAAA", "OtherApplication");
            Assert.False(initialized);
        }

    }
}
