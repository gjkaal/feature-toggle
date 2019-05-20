using System.Collections.Generic;
using System.Security.Claims;

namespace FeatureServices
{
    public interface IFeatureService
    {
        bool Initialized { get; }

        T Current<T>(List<Claim> user, string apiKey, string applicationName, string parameterName, T defaultvalue);
        T Current<T>(List<Claim> user, string apiKey, string applicationName, string parameterName);

        bool Initialize(string applicationName, string apiKey);

        void Reset();

        T Save<T>(List<Claim> user, string apiKey, string applicationName, string parameterName, T newValue);
        T SaveGlobal<T>(List<Claim> user, string apiKey, string applicationName, string parameterName, T newValue);

        bool ResetGlobal(List<Claim> user, string apiKey, string applicationName, string parameterName);
        bool SetGlobal(List<Claim> user, string apiKey, string applicationName, string parameterName);
        bool ToggleGlobal(List<Claim> user, string apiKey, string applicationName, string parameterName);
    }
}
