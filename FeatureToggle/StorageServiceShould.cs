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

        [Fact]
        public async Task Integration_AdminUserCanModifyGlobalValue()
        {
            var user = new List<Claim>
            {
                new Claim(ClaimTypes.Name, "TestUser"),
                new Claim(ClaimTypes.Role, "ToggleAdministrator"),
                new Claim(ClaimTypes.Role, myApplication)
            };

            const string parameterName = "globalStringValue";
            var storage = new SqlFeatureStorage(FeatureStorageLogger.Object, _dbContextFactory);
            var service = new FeatureService(FeatureServiceLogger.Object, storage);

            // Act
            await service.SaveGlobal(user, validApiKey, myApplication, "userStringValue", "Global User value");
            await service.SaveGlobal(user, validApiKey, myApplication, parameterName, "Global string value");
            var setValue = await service.SaveGlobal(user, validApiKey, myApplication, parameterName, "New string value");

            Assert.Equal("Global string value", setValue);
            
        }

        [Fact]
        public async Task Integration_UserCanSetUserValue()
        {
            const string userName = "TestUser";
            var user = new List<Claim>
            {
                new Claim(ClaimTypes.Name, userName),
                new Claim(ClaimTypes.Role, myApplication)
            };

            const string parameterName = "userStringValue";
            var storage = new SqlFeatureStorage(FeatureStorageLogger.Object, _dbContextFactory);
            var service = new FeatureService(FeatureServiceLogger.Object, storage);

            // Act
            await service.Remove(user, validApiKey, myApplication, parameterName);
            var setValue = await service.Save(user, validApiKey, myApplication, parameterName, "New user value");

            // should return global value, not the new value
            Assert.Equal("Global User value", setValue);
           
        }

        [Fact]
        public async Task Integration_UserCanReadUserValue()
        {
            const string userName = "TestUser";
            var user = new List<Claim>
            {
                new Claim(ClaimTypes.Name, userName),
                new Claim(ClaimTypes.Role, myApplication)
            };

            const string parameterName = "savedStringValue";
            var storage = new SqlFeatureStorage(FeatureStorageLogger.Object, _dbContextFactory);
            var service = new FeatureService(FeatureServiceLogger.Object, storage);

            var saveValue = await service.Save(user, validApiKey, myApplication, parameterName, "User string value");
            // Act
            var setValue = await service.Current<string>(user, validApiKey, myApplication, parameterName);

            Assert.Equal("User string value", setValue);
           
        }

    }
}
