using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FeatureServices
{
    public interface IFeatureStorage
    {
        Task<ICollection<ApiKey>> GetApiKeys();

        Task<string> GetFeatureValue(string apiKey, string applicationName, string parameterName);

        Task<FeatureConfig> GetStartupConfig(string apiKey, string applicationName);

        Task SetFeatureValue(string apiKey, string applicationName, string name, string value, string internalType);
        Task RemoveFeatureValue(string apiKey, string key, string name);
    }
}
