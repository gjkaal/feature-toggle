using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using System;
using System.IO;

namespace FeatureServices.Storage
{
    public class DesignTimeDbContextFactory<T> : IDesignTimeDbContextFactory<T> where T : DbContext
    {
        private DbContextFactory _dbContextFactory;
        public DesignTimeDbContextFactory()
        {
            var config = new ConfigurationBuilder();
            var currentFolder = Directory.GetCurrentDirectory();
            config.AddJsonFile(currentFolder + "\\Settings.json", true);

            var userProfile = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            config.SetBasePath(userProfile);
            config.AddJsonFile("UserSecrets.json", true);

            _dbContextFactory = new DbContextFactory(config.Build());
        }

        public T CreateDbContext(string[] args)
        {
            return _dbContextFactory.CreateDbContext<T>("DesignTime");
        }
    }

}
