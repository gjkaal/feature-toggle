using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

namespace FeatureServices
{
    public interface IFeatureService
    {
        bool Initialized { get; }

        Task<T> Current<T>(List<Claim> user, string apiKey, string applicationName, string parameterName, T defaultvalue);
        Task<T> Current<T>(List<Claim> user, string apiKey, string applicationName, string parameterName);

        Task<bool> Initialize(string applicationName, string apiKey);

        void Reset();

        Task<T> Save<T>(List<Claim> user, string apiKey, string applicationName, string parameterName, T newValue);
        Task<T> SaveGlobal<T>(List<Claim> user, string apiKey, string applicationName, string parameterName, T newValue);

        Task<bool> Remove(List<Claim> user, string apiKey, string applicationName, string parameterName);
        Task<bool> RemoveGlobal(List<Claim> user, string apiKey, string applicationName, string parameterName);

        Task<bool> ResetGlobal(List<Claim> user, string apiKey, string applicationName, string parameterName);
        Task<bool> SetGlobal(List<Claim> user, string apiKey, string applicationName, string parameterName);
        Task<bool> ToggleGlobal(List<Claim> user, string apiKey, string applicationName, string parameterName);
    }
}
