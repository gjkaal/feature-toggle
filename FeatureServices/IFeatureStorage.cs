using FeatureServices.Storage;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FeatureServices
{
    public interface IFeatureStorage
    {
        Task<ICollection<ApiKey>> GetApiKeys();

        Task<string> GetFeatureValue(string apiKey, string applicationName, string parameterName);

        Task<FeatureConfig> GetStartupConfig(string applicationName);

        Task SetFeatureValue(string apiKey, string applicationName, string name, string value, string internalType);
    }

    public class SqlFeatureStorage : IFeatureStorage
    {
        const string applicationName = "FeatireServices";
        private readonly IDbContextFactory _dbContextFactory;
        private readonly ILogger<SqlFeatureStorage> _logger;
        public SqlFeatureStorage(ILogger<SqlFeatureStorage> logger,  IDbContextFactory dbContextFactory)
        {
            _logger = logger;
            _dbContextFactory = dbContextFactory;
        }

        private FeatureServicesContext GetDb()
        {
            return _dbContextFactory.CreateDbContext<FeatureServicesContext>(applicationName);
        }

        public async Task<ICollection<ApiKey>> GetApiKeys()
        {
            var result = new List<ApiKey>();
            using (var db = GetDb())
            {
                var query = db.TenantConfiguration
                    .Where(q => !q.IsDeleted)
                    .Select(q => new ApiKey
                    {
                        Id = q.Name,
                        TenantName = q.Description
                    }).ToAsyncEnumerable();
                var e = query.GetEnumerator();
                while(await e.MoveNext())
                {
                    result.Add(e.Current);
                }                
            }
            return result;
        }

        public Task<string> GetFeatureValue(string apiKey, string applicationName, string parameterName)
        {
            throw new System.NotImplementedException();
        }

        public Task<FeatureConfig> GetStartupConfig(string applicationName)
        {
            throw new System.NotImplementedException();
        }

        public Task SetFeatureValue(string apiKey, string applicationName, string name, string value, string internalType)
        {
            throw new System.NotImplementedException();
        }
    }
}
