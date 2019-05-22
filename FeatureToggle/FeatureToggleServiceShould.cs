using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Xunit;

namespace FeatureServices
{

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
                .ReturnsAsync(
                    new List<ApiKey> {
                        new ApiKey { Id= validApiKey , TenantName="TestTenant"}
                    });

            featureStorageMoq.Setup(q => q.GetStartupConfig(It.Is<string>(m => m != myApplication)))
                .ReturnsAsync (default(FeatureConfig));

            featureStorageMoq.Setup(q => q.GetStartupConfig(It.Is<string>(m => m == myApplication)))
                .ReturnsAsync(new FeatureConfig
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
        public async Task StartWithValidApiKey()
        {
            service.Reset();
            var initResult = await service.Initialize(validApiKey, myApplication);
            featureStorageMoq.Verify(mock => mock.GetApiKeys(), Times.Once());
            featureStorageMoq.Verify(mock => mock.GetStartupConfig(It.Is<string>(q => q == myApplication)), Times.Once());

            Assert.NotNull(service);
            Assert.True(initResult);
        }

        [Fact]
        public async Task AcceptReinitializationApiKey()
        {
            service.Reset();
            featureStorageMoq.Invocations.Clear();
            var initResult = await service.Initialize(validApiKey, myApplication);
            var reInitResult = await service.Initialize(validApiKey, myApplication);
            featureStorageMoq.Verify(mock => mock.GetApiKeys(), Times.Once());
            featureStorageMoq.Verify(mock => mock.GetStartupConfig(It.Is<string>(q => q == myApplication)), Times.Once());
            Assert.True(initResult);
            Assert.True(reInitResult);
        }

        [Fact]
        public async Task RefuseInvalidApiKey()
        {
            service.Reset();
            var initResult = await service.Initialize("invalidApiKey", myApplication);
            Assert.NotNull(service);
            Assert.False(initResult);
        }

        [Fact]
        public async Task AdminUserCanModifyGlobalToggles()
        {
            var user = new List<Claim>
            {
                new Claim(ClaimTypes.Name, "TestUser"),
                new Claim(ClaimTypes.Role, "ToggleAdministrator"),
                new Claim(ClaimTypes.Role, myApplication)
            };

            var setValue = await service.SetGlobal(user, validApiKey, myApplication, "myToggle");
            var resetValue = await service.ResetGlobal(user, validApiKey, myApplication, "myToggle");

            Assert.True(setValue);
            Assert.True(resetValue);
        }

        [Fact]
        public async Task AdminUserCanModifyGlobalValue()
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
                .ReturnsAsync("Global string value");

            // Act
            var setValue = await service.SaveGlobal(user, validApiKey, myApplication, parameterName, "New string value");

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
        public async Task UserCanSetUserValue()
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
                .ReturnsAsync("Global string value");

            // Act
            var setValue = await service.Save(user, validApiKey, myApplication, parameterName, "New user value");

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
        public async Task UserCanReadUserValue()
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
                .ReturnsAsync("Global string value");

            featureStorageMoq.Setup(q => q.GetFeatureValue(
                It.Is<string>(v => v == validApiKey),
                It.Is<string>(m => m == myApplication + '-' + userName),
                It.Is<string>(m => m == parameterName)))
                .ReturnsAsync("User string value");

            // Act
            var setValue = await service.Current<string>(user, validApiKey, myApplication, parameterName);

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
        public async Task ThrowsExceptionIfNotAuthorizedToModify()
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
                .ReturnsAsync("Global string value");

            await Assert.ThrowsAsync<UnauthorizedAccessException>(async () =>
            {
                var value = await service.SaveGlobal(user, validApiKey, myApplication, parameterName, "New string value");
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
        public async Task ThrowsExceptionIfNotAuthorizedToApplication()
        {
            await Assert.ThrowsAsync<UnauthorizedAccessException>(async () =>
             {
                 var user = new List<Claim>
                 {
                    new Claim(ClaimTypes.Name, "TestUser"),
                    new Claim(ClaimTypes.Role, "ToggleAdministrator"),
                 };
                 var value = await service.Current(user, validApiKey, myApplication, "myToggle", true);
                 Assert.True(value);
             });
        }

        [Fact]
        public async Task ReturnsToggleValuesWithDefaultBool()
        {
            var user = new List<Claim>
            {
                new Claim(ClaimTypes.Name, "TestUser"),
                new Claim(ClaimTypes.Role, myApplication)
            };
            var value = await service.Current(user, validApiKey, myApplication, "myToggle", true);
            Assert.True(value);
        }

        [Fact]
        public async Task ReturnsToggleValuesWithDefaultString()
        {
            var user = new List<Claim>
            {
                new Claim(ClaimTypes.Name, "TestUser"),
                new Claim(ClaimTypes.Role, myApplication)
            };
            var value = await service.Current(user, validApiKey, myApplication, "myToggle", "default");
            Assert.Equal("default", value);
        }

        [Fact]
        public async Task ReturnsToggleValuesWithDefaultDecimal()
        {
            var user = new List<Claim>
            {
                new Claim(ClaimTypes.Name, "TestUser"),
                new Claim(ClaimTypes.Role, myApplication)
            };
            var value = await service.Current(user, validApiKey, myApplication, "myToggle", 3.14m);
            Assert.Equal(3.14m, value);
        }

        [Fact]
        public async Task ReturnsToggleValuesBool()
        {
            featureStorageMoq.Setup(q => q.GetFeatureValue(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.Is<string>(m => m == "savedBoolValue")))
                .ReturnsAsync("true");

            var user = new List<Claim>
            {
                new Claim(ClaimTypes.Name, "TestUser"),
                new Claim(ClaimTypes.Role, myApplication)
            };
            var value = await service.Current<bool>(user, validApiKey, myApplication, "savedBoolValue");
            Assert.True(value);
        }

        [Fact]
        public async Task ReturnsToggleValuesString()
        {
            featureStorageMoq.Setup(q => q.GetFeatureValue(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.Is<string>(m => m == "savedStringValue")))
                .ReturnsAsync("A string value");

            var user = new List<Claim>
            {
                new Claim(ClaimTypes.Name, "TestUser"),
                new Claim(ClaimTypes.Role, myApplication)
            };
            var value = await service.Current<string>(user, validApiKey, myApplication, "savedStringValue");
            Assert.Equal("A string value", value);
        }

        [Fact]
        public async Task ReturnsUserValues()
        {
            featureStorageMoq.Setup(q => q.GetFeatureValue(
                It.IsAny<string>(),
                It.Is<string>(m => m == "myApplication"),
                It.Is<string>(m => m == "savedStringValue")))
                .ReturnsAsync("Global string value");

            featureStorageMoq.Setup(q => q.GetFeatureValue(
                It.IsAny<string>(),
                It.Is<string>(m => m == "myApplication-TestUser"),
                It.Is<string>(m => m == "savedStringValue")))
                .ReturnsAsync("Testuser string value");

            var user = new List<Claim>
            {
                new Claim(ClaimTypes.Name, "TestUser"),
                new Claim(ClaimTypes.Role, myApplication)
            };
            var value = await service.Current<string>(user, validApiKey, myApplication, "savedStringValue");
            Assert.Equal("Testuser string value", value);
        }

        [Fact]
        public async Task ReturnsOnlyUserValues()
        {
            featureStorageMoq.Setup(q => q.GetFeatureValue(
                It.IsAny<string>(),
                It.Is<string>(m => m == "myApplication"),
                It.Is<string>(m => m == "savedStringValue")))
                .ReturnsAsync("Global string value");

            featureStorageMoq.Setup(q => q.GetFeatureValue(
                It.IsAny<string>(),
                It.Is<string>(m => m == "myApplication-OtherUser"),
                It.Is<string>(m => m == "savedStringValue")))
                .ReturnsAsync("Testuser string value");

            var user = new List<Claim>
            {
                new Claim(ClaimTypes.Name, "TestUser"),
                new Claim(ClaimTypes.Role, myApplication)
            };
            var value = await service.Current<string>(user, validApiKey, myApplication, "savedStringValue");
            Assert.Equal("Global string value", value);
        }

        [Fact]
        public async Task ReturnsToggleValuesDefaultForTypeIfNotMatched()
        {
            featureStorageMoq.Setup(q => q.GetFeatureValue(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.Is<string>(m => m == "savedStringValue")))
                .ReturnsAsync("A string value");

            var user = new List<Claim>
            {
                new Claim(ClaimTypes.Name, "TestUser"),
                new Claim(ClaimTypes.Role, myApplication)
            };
            var value = await service.Current<int>(user, validApiKey, myApplication, "savedStringValue");
            Assert.Equal(0, value);
        }
    }
}
