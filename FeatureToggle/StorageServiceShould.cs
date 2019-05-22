using FeatureServices.Storage;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Claims;
using System.Threading.Tasks;
using Xunit;

namespace FeatureServices
{
    public class StorageServiceShould
    {
        public const string myApplication = "StorageServiceShould";
        public const string validApiKey = "MFRGCY3BMRQWE4TB";

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
            var initialized = await service.Initialize(validApiKey, myApplication);
            Assert.True(initialized);
        }


        [Fact]
        public async Task Integration_InitServiceWithNewApplication()
        {
            // initialize
            var storage = new SqlFeatureStorage(FeatureStorageLogger.Object, _dbContextFactory);
            var service = new FeatureService(FeatureServiceLogger.Object, storage);
            var initialized = await service.Initialize(validApiKey, "OtherApplication");
            Assert.True(initialized);
        }

        [Fact]
        public async Task Integration_InitServiceWithInvalidApiKey()
        {
            // initialize
            var storage = new SqlFeatureStorage(FeatureStorageLogger.Object, _dbContextFactory);
            var service = new FeatureService(FeatureServiceLogger.Object, storage);
            var initialized = await service.Initialize(validApiKey, myApplication);
            Assert.True(initialized);
            service.Reset();
            var initResult = await service.Initialize("invalidApiKey", myApplication);
            Assert.NotNull(service);
            Assert.False(initResult);
        }


        [Fact]
        public async Task Integration_AdminUserCanModifyGlobalToggles()
        {
            var user = new List<Claim>
            {
                new Claim(ClaimTypes.Name, "TestUser"),
                new Claim(ClaimTypes.Role, "ToggleAdministrator"),
                new Claim(ClaimTypes.Role, myApplication)
            };
            var storage = new SqlFeatureStorage(FeatureStorageLogger.Object, _dbContextFactory);
            var service = new FeatureService(FeatureServiceLogger.Object, storage);

            var setValue = await service.SetGlobal(user, validApiKey, myApplication, "myToggle");
            var resetValue = await service.ResetGlobal(user, validApiKey, myApplication, "myToggle");

            Assert.True(setValue);
            Assert.True(resetValue);
        }

    }
}
