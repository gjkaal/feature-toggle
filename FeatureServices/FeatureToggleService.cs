using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace FeatureServices
{
    public class FeatureService : IFeatureService
    {

        private const string Administrator = "ToggleAdministrator";

        private static readonly object _initializedLock = new object();

        private static ConcurrentDictionary<string, FeatureConfig> _configurations = new ConcurrentDictionary<string, FeatureConfig>();

        private static bool _initialized = false;

        private static ConcurrentDictionary<string, ApiKey> _validKeys = new ConcurrentDictionary<string, ApiKey>();

        private readonly IFeatureStorage _storage;
        private readonly ILogger<FeatureService> _logger;


        public bool Initialized => _initialized;

        public FeatureService(IFeatureStorage storage, ILogger<FeatureService> logger)
        {
            _storage = storage;
            _logger = logger;
        }

        public async Task<T> Current<T>(List<Claim> user, string apiKey, string applicationName, string parameterName)
        {
            return await Current(user, apiKey, applicationName, parameterName, default(T));
        }

        public async Task< T> Current<T>(List<Claim> user, string apiKey, string applicationName, string parameterName, T defaultvalue)
        {
            ValidateApiKey(apiKey);
            user.HasClaim(ClaimTypes.Name);
            user.IsInRole(applicationName);
            try
            {
                var username = user.UserName();
                bool found;
                T value;
                (found, value) = await Feature(apiKey, $"{applicationName}-{username}", parameterName, defaultvalue);
                if (found) return value;
                (found, value) = await Feature(apiKey, applicationName, parameterName, defaultvalue);
                return found ? value : defaultvalue;
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"Current for {applicationName} / {parameterName} failed");
                return defaultvalue;
            }
        }

        public async Task<bool> Initialize(string apiKey, string applicationName)
        {
            if (!_initialized)
            {
                var validKeys = await _storage.GetApiKeys();
                for (var i = 0; i < validKeys.Count; i++)
                {
                    var key = validKeys.ElementAt(i);
                    _validKeys.TryAdd(key.Id, key);
                }
                _initialized = true;
            }

            if (_validKeys.ContainsKey(apiKey))
            {
                if (_configurations.ContainsKey(applicationName))
                {
                    return true;
                }
                var config = await _storage.GetStartupConfig(applicationName);
                if (config != null)
                {
                    return _configurations.TryAdd(applicationName, config);
                }
                return false;
            }
            else
            {
                return false;
            }
        }

        public void Reset()
        {
            lock (_initializedLock)
            {
                _initialized = false;
                _validKeys.Clear();
                _configurations.Clear();
            }
        }

        public async Task<bool> ResetGlobal(List<Claim> user, string apiKey, string applicationName, string parameterName)
        {
            var (success, previousValue) = await ChangeValue(user, true, apiKey, applicationName, parameterName, false);
            return success;
        }

        public async Task<T> Save<T>(List<Claim> user, string apiKey, string applicationName, string parameterName, T newValue)
        {
            var (success, oldValue) = await ChangeValue(user, false, apiKey, applicationName, parameterName, newValue);
            return oldValue;
        }

        public async Task<T> SaveGlobal<T>(List<Claim> user, string apiKey, string applicationName, string parameterName, T newValue)
        {
            var (success, oldValue) = await ChangeValue(user, true, apiKey, applicationName, parameterName, newValue);
            return oldValue;
        }

        public async Task<bool> SetGlobal(List<Claim> user, string apiKey, string applicationName, string parameterName)
        {
            var (success, previousValue) = await ChangeValue(user, true, apiKey, applicationName, parameterName, true);
            return success;
        }

        public async Task<bool> ToggleGlobal(List<Claim> user, string apiKey, string applicationName, string parameterName)
        {
            var currentValue =  await Current(user, apiKey, applicationName, parameterName, false);
           await  ChangeValue(user, true, apiKey, applicationName, parameterName, !currentValue);
            return currentValue;
        }



        private async Task<(bool success, T previousValue)> ChangeValue<T>(List<Claim> user, bool global, string apiKey, string applicationName, string parameterName, T newValue)
        {
            ValidateApiKey(apiKey);
            user.HasClaim(ClaimTypes.Name);
            if (global) user.IsInRole(Administrator);
            user.IsInRole(applicationName);
            T oldValue = default(T);
            try
            {
                var userName = user.UserName();
                var key = global ? applicationName : $"{applicationName}-{userName}";
                oldValue = await Current<T>(user, apiKey, applicationName, parameterName);
                var updateItem = new FeatureToggle<T>
                {
                    ApiKey = apiKey,
                    Key = key,
                    Name = parameterName,
                    Value = newValue
                };

                SaveItem(updateItem, userName);
                return (true, oldValue);
            }
            catch (Exception)
            {
                return (false, oldValue);
            }
        }

        private async Task<(bool found, T value)> Feature<T>(string apiKey, string applicationName, string parameterName, T defaultvalue)
        {
            var found = false;
            T itemValue;
            string value = await _storage.GetFeatureValue(apiKey, applicationName, parameterName);
            if (string.IsNullOrEmpty(value))
            {
                itemValue = defaultvalue;
            }
            else
            {
                found = true;
                itemValue = (T)Convert.ChangeType(value, typeof(T));
            }
            return (found, itemValue);
        }

        private async Task<FeatureToggle<T>> LoadItem<T>(string apiKey, string applicationName, string parameterName, T defaultValue)
        {
            var (found, itemValue) = await Feature<T>(apiKey, applicationName, parameterName, defaultValue);
            return new FeatureToggle<T>
            {
                ApiKey = apiKey,
                Key = applicationName,
                Name = parameterName,
                Value = itemValue
            };
        }

        private void SaveItem<T>(FeatureToggle<T> item, string userName)
        {
            _storage.SetFeatureValue(item.ApiKey, item.Key, item.Name, item.Value.ToString(), item.Value.GetType().ToString());
        }
        private void ValidateApiKey(string apiKey)
        {
            if (!_validKeys.ContainsKey(apiKey))
            {
                _logger.LogCritical($"Invalid api key used: {apiKey}");
                throw new UnauthorizedAccessException($"Invalid API Key: {apiKey}");
            }
        }

    }
}
