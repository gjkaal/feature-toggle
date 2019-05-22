using FeatureServices.Storage;
using FeatureServices.Storage.DbModel;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FeatureServices
{
    public interface IFeatureStorage
    {
        Task<ICollection<ApiKey>> GetApiKeys();

        Task<string> GetFeatureValue(string apiKey, string applicationName, string parameterName);

        Task<FeatureConfig> GetStartupConfig(string apiKey, string applicationName);

        Task SetFeatureValue(string apiKey, string applicationName, string name, string value, string internalType);
    }

    public class SqlFeatureStorage : IFeatureStorage
    {
        private const string applicationName = "FeatireServices";
        private readonly IDbContextFactory _dbContextFactory;
        private readonly ILogger<SqlFeatureStorage> _logger;
        public SqlFeatureStorage(ILogger<SqlFeatureStorage> logger, IDbContextFactory dbContextFactory)
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
                while (await e.MoveNext())
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

        public async Task<FeatureConfig> GetStartupConfig(string apiKey, string applicationName)
        {
            FeatureConfig result=null;
            using (var db = GetDb())
            {
                var value = await FindOrCreate(db, apiKey, applicationName, "_Initialized", applicationName, typeof(string).FullName);
                await db.SaveChangesAsync();
                if (value!=null)
                {
                    result= new FeatureConfig
                    {
                        ApplicationName = applicationName,
                        Initialized = value.Created
                    };
                }
                else
                {
                    return null;
                }
            }
            return result;
        }

        private async Task<FeatureValue> FindOrCreate(FeatureServicesContext db, string apiKey, string applicationName, string name, string newValue, string internalType)
        {
            var exists = await db.FeatureValue
                .Where(t => t.TenantConfiguration.Name == apiKey
                    && t.Name == name
                    && t.ApplicationName == applicationName)
                .FirstOrDefaultAsync();
            if (exists == null)
            {
                var api = await db.TenantConfiguration.SingleAsync(q => q.Name == apiKey);
                exists = new FeatureValue
                {
                    TenantConfigurationId = api.Id,
                    ApplicationName = applicationName,
                    Name = name,
                    Value = newValue,
                    InternalType = internalType
                };
                await db.FeatureValue.AddAsync(exists);
            }
            return exists;
        }

        public Task SetFeatureValue(string apiKey, string applicationName, string name, string value, string internalType)
        {
            throw new System.NotImplementedException();
        }
    }
}
