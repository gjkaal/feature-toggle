using System.Collections.Generic;

namespace FeatureServices
{
    public interface IFeatureStorage
    {
        ICollection<ApiKey> GetApiKeys();

        string GetFeatureValue(string apiKey, string applicationName, string parameterName);

        FeatureConfig GetStartupConfig(string applicationName);

        void SetFeatureValue(string apiKey, string applicationName, string name, string value, string internalTypev);
    }
}
