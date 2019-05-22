using FeatureServices.Storage;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Claims;
using Xunit;

namespace FeatureServices
{
    public class StorageServiceShould
    {
        private static readonly ILogger<FeatureServicesContext> logger = new Mock<ILogger<FeatureServicesContext>>().Object;
        private static readonly IConfiguration configuration;
        private readonly FeatureServicesContextFactory _dbContextFactory = new FeatureServicesContextFactory();

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
            configuration = config.Build();
        }

        [Fact]
        public void Initialize()
        {
            var connectionString = configuration.GetConnectionString("FeatureServiceDb");
            using (var dbContext = _dbContextFactory.CreateDbContext(new[] { connectionString }))
            {
                dbContext.TenantConfiguration.Add(new Storage.DbModel.TenantConfiguration { });
                dbContext.SaveChanges();
            }

        }
    }

    public class FeatureServiceShould
    {
        private Mock<IFeatureStorage> featureStorageMoq = new Mock<IFeatureStorage>();
        private Mock<ILogger<FeatureService>> loggerMoq = new Mock<ILogger<FeatureService>>();
        private string validApiKey = "7AAF2182-1BA6-4909-B587-6578BB08D6B6";
        private string myApplication = "myApplication";
        private readonly IFeatureService service;

        public FeatureServiceShould()
        {
            featureStorageMoq.Setup(q => q.GetApiKeys())
                .Returns(
                    new List<ApiKey> {
                        new ApiKey { Id= validApiKey , TenantName="TestTenant"}
                    });

            featureStorageMoq.Setup(q => q.GetStartupConfig(It.Is<string>(m => m != myApplication)))
                .Returns(default(FeatureConfig));

            featureStorageMoq.Setup(q => q.GetStartupConfig(It.Is<string>(m => m == myApplication)))
                .Returns(new FeatureConfig
                {
                    ApplicationName = myApplication,
                    Initialized = DateTime.UtcNow
                });

            // initialize
            service = new FeatureService(featureStorageMoq.Object, loggerMoq.Object);
            service.Initialize(validApiKey, myApplication);
        }

        [Fact]
        public void CanBeResetToStartup()
        {
            service.Reset();
            featureStorageMoq.Invocations.Clear();
            var initResult = service.Initialize(validApiKey, myApplication);
            featureStorageMoq.Verify(mock => mock.GetApiKeys(), Times.Once());
            featureStorageMoq.Verify(mock => mock.GetStartupConfig(It.Is<string>(q => q == myApplication)), Times.Once());

            Assert.True(service.Initialized);

            service.Reset();

            Assert.NotNull(service);
            Assert.False(service.Initialized);
        }

        [Fact]
        public void StartWithValidApiKey()
        {
            service.Reset();
            var initResult = service.Initialize(validApiKey, myApplication);
            featureStorageMoq.Verify(mock => mock.GetApiKeys(), Times.Once());
            featureStorageMoq.Verify(mock => mock.GetStartupConfig(It.Is<string>(q => q == myApplication)), Times.Once());

            Assert.NotNull(service);
            Assert.True(initResult);
        }

        [Fact]
        public void AcceptReinitializationApiKey()
        {
            service.Reset();
            featureStorageMoq.Invocations.Clear();
            var initResult = service.Initialize(validApiKey, myApplication);
            var reInitResult = service.Initialize(validApiKey, myApplication);
            featureStorageMoq.Verify(mock => mock.GetApiKeys(), Times.Once());
            featureStorageMoq.Verify(mock => mock.GetStartupConfig(It.Is<string>(q => q == myApplication)), Times.Once());
            Assert.True(initResult);
            Assert.True(reInitResult);
        }

        [Fact]
        public void RefuseInvalidApiKey()
        {
            service.Reset();
            var initResult = service.Initialize("invalidApiKey", myApplication);
            Assert.NotNull(service);
            Assert.False(initResult);
        }

        [Fact]
        public void AdminUserCanModifyGlobalToggles()
        {
            var user = new List<Claim>
            {
                new Claim(ClaimTypes.Name, "TestUser"),
                new Claim(ClaimTypes.Role, "ToggleAdministrator"),
                new Claim(ClaimTypes.Role, myApplication)
            };

            var setValue = service.SetGlobal(user, validApiKey, myApplication, "myToggle");
            var resetValue = service.ResetGlobal(user, validApiKey, myApplication, "myToggle");

            Assert.True(setValue);
            Assert.True(resetValue);
        }

        [Fact]
        public void AdminUserCanModifyGlobalValue()
        {
            var user = new List<Claim>
            {
                new Claim(ClaimTypes.Name, "TestUser"),
                new Claim(ClaimTypes.Role, "ToggleAdministrator"),
                new Claim(ClaimTypes.Role, myApplication)
            };

            const string parameterName = "savedStringValue";
            featureStorageMoq.Setup(q => q.GetFeatureValue(
                It.Is<string>(v => v == validApiKey),
                It.Is<string>(m => m == myApplication),
                It.Is<string>(m => m == parameterName)))
                .Returns("Global string value");

            // Act
            var setValue = service.SaveGlobal(user, validApiKey, myApplication, parameterName, "New string value");

            Assert.Equal("Global string value", setValue);

            featureStorageMoq.Verify(mock => mock.GetFeatureValue(
                It.Is<string>(v => v == validApiKey),
                It.Is<string>(v => v == myApplication),
                It.Is<string>(v => v == parameterName)
            ), Times.AtMostOnce());

            featureStorageMoq.Verify(mock => mock.SetFeatureValue(
                It.Is<string>(v => v == validApiKey),
                It.Is<string>(v => v == myApplication),
                It.Is<string>(v => v == parameterName),
                It.Is<string>(v => v == "New string value"),
                It.Is<string>(v => v == typeof(string).ToString())
                ), Times.AtMostOnce());
        }

        [Fact]
        public void UserCanSetUserValue()
        {
            const string userName = "TestUser";
            var user = new List<Claim>
            {
                new Claim(ClaimTypes.Name, userName),
                new Claim(ClaimTypes.Role, myApplication)
            };

            const string parameterName = "savedStringValue";
            featureStorageMoq.Setup(q => q.GetFeatureValue(
                It.Is<string>(v => v == validApiKey),
                It.Is<string>(m => m == myApplication),
                It.Is<string>(m => m == parameterName)))
                .Returns("Global string value");

            // Act
            var setValue = service.Save(user, validApiKey, myApplication, parameterName, "New user value");

            Assert.Equal("Global string value", setValue);

            featureStorageMoq.Verify(mock => mock.GetFeatureValue(
                It.Is<string>(v => v == validApiKey),
                It.Is<string>(v => v == myApplication + '-' + userName),
                It.Is<string>(v => v == parameterName)
            ), Times.AtMostOnce());

            featureStorageMoq.Verify(mock => mock.GetFeatureValue(
                It.Is<string>(v => v == validApiKey),
                It.Is<string>(v => v == myApplication),
                It.Is<string>(v => v == parameterName)
            ), Times.AtMostOnce());

            featureStorageMoq.Verify(mock => mock.SetFeatureValue(
                It.Is<string>(v => v == validApiKey),
                It.Is<string>(v => v == myApplication),
                It.Is<string>(v => v == parameterName),
                It.Is<string>(v => v == "New user value"),
                It.Is<string>(v => v == typeof(string).ToString())
                ), Times.Never());

            featureStorageMoq.Verify(mock => mock.SetFeatureValue(
                It.Is<string>(v => v == validApiKey),
                It.Is<string>(v => v == myApplication + '-' + userName),
                It.Is<string>(v => v == parameterName),
                It.Is<string>(v => v == "New user value"),
                It.Is<string>(v => v == typeof(string).ToString())
                ), Times.Once());
        }

        [Fact]
        public void UserCanReadUserValue()
        {
            const string userName = "TestUser";
            var user = new List<Claim>
            {
                new Claim(ClaimTypes.Name, userName),
                new Claim(ClaimTypes.Role, myApplication)
            };

            const string parameterName = "savedStringValue";
            featureStorageMoq.Setup(q => q.GetFeatureValue(
                It.Is<string>(v => v == validApiKey),
                It.Is<string>(m => m == myApplication),
                It.Is<string>(m => m == parameterName)))
                .Returns("Global string value");

            featureStorageMoq.Setup(q => q.GetFeatureValue(
                It.Is<string>(v => v == validApiKey),
                It.Is<string>(m => m == myApplication + '-' + userName),
                It.Is<string>(m => m == parameterName)))
                .Returns("User string value");

            // Act
            var setValue = service.Current<string>(user, validApiKey, myApplication, parameterName);

            Assert.Equal("User string value", setValue);

            featureStorageMoq.Verify(mock => mock.GetFeatureValue(
                It.Is<string>(v => v == validApiKey),
                It.Is<string>(v => v == myApplication + '-' + userName),
                It.Is<string>(v => v == parameterName)
            ), Times.Once());

            featureStorageMoq.Verify(mock => mock.GetFeatureValue(
                It.Is<string>(v => v == validApiKey),
                It.Is<string>(v => v == myApplication),
                It.Is<string>(v => v == parameterName)
            ), Times.Never());
        }

        [Fact]
        public void ThrowsExceptionIfNotAuthorizedToModify()
        {
            var user = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, "TestUser"),
                    new Claim(ClaimTypes.Role, myApplication)
                };

            const string parameterName = "savedStringValue";
            featureStorageMoq.Setup(q => q.GetFeatureValue(
                It.Is<string>(v => v == validApiKey),
                It.Is<string>(m => m == myApplication),
                It.Is<string>(m => m == parameterName)))
                .Returns("Global string value");

            Assert.Throws<UnauthorizedAccessException>(() =>
            {
                var value = service.SaveGlobal(user, validApiKey, myApplication, parameterName, "New string value");
            });

            featureStorageMoq.Verify(mock => mock.GetFeatureValue(
                It.Is<string>(v => v == validApiKey),
                It.Is<string>(v => v == myApplication),
                It.Is<string>(v => v == parameterName)
                ), Times.Never());

            featureStorageMoq.Verify(mock => mock.SetFeatureValue(
                It.Is<string>(v => v == validApiKey),
                It.Is<string>(v => v == myApplication),
                It.Is<string>(v => v == parameterName),
                It.Is<string>(v => v == "New string value"),
                It.Is<string>(v => v == typeof(string).ToString())
                ), Times.Never());
        }

        [Fact]
        public void ThrowsExceptionIfNotAuthorizedToApplication()
        {
            Assert.Throws<UnauthorizedAccessException>(() =>
            {
                var user = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, "TestUser"),
                    new Claim(ClaimTypes.Role, "ToggleAdministrator"),
                };
                var value = service.Current(user, validApiKey, myApplication, "myToggle", true);
            });
        }

        [Fact]
        public void ReturnsToggleValuesWithDefaultBool()
        {
            var user = new List<Claim>
            {
                new Claim(ClaimTypes.Name, "TestUser"),
                new Claim(ClaimTypes.Role, myApplication)
            };
            var value = service.Current(user, validApiKey, myApplication, "myToggle", true);
            Assert.True(value);
        }

        [Fact]
        public void ReturnsToggleValuesWithDefaultString()
        {
            var user = new List<Claim>
            {
                new Claim(ClaimTypes.Name, "TestUser"),
                new Claim(ClaimTypes.Role, myApplication)
            };
            var value = service.Current(user, validApiKey, myApplication, "myToggle", "default");
            Assert.Equal("default", value);
        }

        [Fact]
        public void ReturnsToggleValuesWithDefaultDecimal()
        {
            var user = new List<Claim>
            {
                new Claim(ClaimTypes.Name, "TestUser"),
                new Claim(ClaimTypes.Role, myApplication)
            };
            var value = service.Current(user, validApiKey, myApplication, "myToggle", 3.14m);
            Assert.Equal(3.14m, value);
        }

        [Fact]
        public void ReturnsToggleValuesBool()
        {
            featureStorageMoq.Setup(q => q.GetFeatureValue(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.Is<string>(m => m == "savedBoolValue")))
                .Returns("true");

            var user = new List<Claim>
            {
                new Claim(ClaimTypes.Name, "TestUser"),
                new Claim(ClaimTypes.Role, myApplication)
            };
            var value = service.Current<bool>(user, validApiKey, myApplication, "savedBoolValue");
            Assert.True(value);
        }

        [Fact]
        public void ReturnsToggleValuesString()
        {
            featureStorageMoq.Setup(q => q.GetFeatureValue(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.Is<string>(m => m == "savedStringValue")))
                .Returns("A string value");

            var user = new List<Claim>
            {
                new Claim(ClaimTypes.Name, "TestUser"),
                new Claim(ClaimTypes.Role, myApplication)
            };
            var value = service.Current<string>(user, validApiKey, myApplication, "savedStringValue");
            Assert.Equal("A string value", value);
        }

        [Fact]
        public void ReturnsUserValues()
        {
            featureStorageMoq.Setup(q => q.GetFeatureValue(
                It.IsAny<string>(),
                It.Is<string>(m => m == "myApplication"),
                It.Is<string>(m => m == "savedStringValue")))
                .Returns("Global string value");

            featureStorageMoq.Setup(q => q.GetFeatureValue(
                It.IsAny<string>(),
                It.Is<string>(m => m == "myApplication-TestUser"),
                It.Is<string>(m => m == "savedStringValue")))
                .Returns("Testuser string value");

            var user = new List<Claim>
            {
                new Claim(ClaimTypes.Name, "TestUser"),
                new Claim(ClaimTypes.Role, myApplication)
            };
            var value = service.Current<string>(user, validApiKey, myApplication, "savedStringValue");
            Assert.Equal("Testuser string value", value);
        }

        [Fact]
        public void ReturnsOnlyUserValues()
        {
            featureStorageMoq.Setup(q => q.GetFeatureValue(
                It.IsAny<string>(),
                It.Is<string>(m => m == "myApplication"),
                It.Is<string>(m => m == "savedStringValue")))
                .Returns("Global string value");

            featureStorageMoq.Setup(q => q.GetFeatureValue(
                It.IsAny<string>(),
                It.Is<string>(m => m == "myApplication-OtherUser"),
                It.Is<string>(m => m == "savedStringValue")))
                .Returns("Testuser string value");

            var user = new List<Claim>
            {
                new Claim(ClaimTypes.Name, "TestUser"),
                new Claim(ClaimTypes.Role, myApplication)
            };
            var value = service.Current<string>(user, validApiKey, myApplication, "savedStringValue");
            Assert.Equal("Global string value", value);
        }

        [Fact]
        public void ReturnsToggleValuesDefaultForTypeIfNotMatched()
        {
            featureStorageMoq.Setup(q => q.GetFeatureValue(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.Is<string>(m => m == "savedStringValue")))
                .Returns("A string value");

            var user = new List<Claim>
            {
                new Claim(ClaimTypes.Name, "TestUser"),
                new Claim(ClaimTypes.Role, myApplication)
            };
            var value = service.Current<int>(user, validApiKey, myApplication, "savedStringValue");
            Assert.Equal(0, value);
        }
    }
}
