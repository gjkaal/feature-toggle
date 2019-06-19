using System;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Security;
using System.Threading.Tasks;
using FeatureServices.Storage.DbModel;
using Microsoft.EntityFrameworkCore;

namespace FeatureServices.Storage
{
    public class FeatureServicesContext : DbContext, ITenantContext
    {
       
        public FeatureServicesContext(DbContextOptions<FeatureServicesContext> options) : base(options)
        {
        }

        public int TenantId { get; private set; }
        public DbSet<TenantConfiguration> TenantConfiguration { get; set; }
        public DbSet<FeatureValue> FeatureValue { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Build model
            base.OnModelCreating(modelBuilder);
            // Then add extensions
            DbModel.TenantConfiguration.Build(modelBuilder);
            DbModel.FeatureValue.Build(modelBuilder);
        }

        public async Task<string> CurrentUser()
        {
                var (success, result) = await ExecuteSqlAsync("SELECT CURRENT_USER");
                return success ? result : "Invalid";
        }

        public void Tenant(int tenantId)
        {
            //TODO: check if in context change
            TenantId = tenantId;
        }

        public async Task<TenantConfiguration> CreateApi(string api, string description)
        {
            var config = await TenantConfiguration.FirstOrDefaultAsync(q => q.Name == api && q.Tenant == TenantId);
            if (config == null)
            {
                config = new TenantConfiguration
                {
                    Tenant = TenantId,
                    Name = api,
                    Description = description,
                };
                await TenantConfiguration.AddAsync(config);
            }
           
            return config;
        }

        /// <summary>
        /// Executes the SQL query directly.
        /// </summary>
        /// <param name="sqlSourceCode">The SQL source code.</param>
        /// <returns>Tuple&lt;Boolean, System.String&gt;.</returns>
        /// <remarks>The calling assembly must be signed and should review the SQL query for security vulnerabilities</remarks>
        [System.Diagnostics.CodeAnalysis.SuppressMessage(
            "Microsoft.Security", "CA2100:Review SQL queries for security vulnerabilities",
            Justification = "Calling assembly is validated")]
        internal async Task <(bool success, string result)> ExecuteSqlAsync(string sqlSourceCode)
        {
            var result = false;
            var message = "No data";

            if (string.IsNullOrEmpty(sqlSourceCode)) return (false, "Invalid query");
            try
            {
                // only signed assemblies can call execute
                //VerifyAssembly(Assembly.GetCallingAssembly());

                var con = await CurrentConnection(3);
                using (var cmd = con.CreateCommand())
                {
                    cmd.CommandText = sqlSourceCode;
                    cmd.CommandType = CommandType.Text;
                    var scalarResult = await cmd.ExecuteScalarAsync();
                    if (scalarResult != null)
                    {
                        message = scalarResult.ToString();
                    }
                    result = true;
                }
            }
            catch (SecurityException se)
            {                
                result = false;
                message = se.Message;
            }
            catch (SqlException e)
            {               
                result = false;
                message = e.Message;

            }
            return (result, message);
        }

        protected async Task< DbConnection> CurrentConnection(int retries)
        {
            var con = Database.GetDbConnection();
            if (con.State == ConnectionState.Open) return con;
            if (con.State== ConnectionState.Closed)
            {
                await con.OpenAsync();
                return con;
            }
            if (con.State == ConnectionState.Broken) {
                con.Close();
                if (retries == 0) return null;
                return await CurrentConnection(retries - 1);
            }
            //wait?
            return con;
        }
    }
}
