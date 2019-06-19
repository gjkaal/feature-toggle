using System;
using System.IO;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;

namespace FeatureToggle.Web
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateWebHostBuilder(args).Build().Run();
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration(JsonSettings)
                .UseStartup<Startup>()
            ;

        private static void JsonSettings(WebHostBuilderContext builder, IConfigurationBuilder config)
        {
            var currentFolder = Directory.GetCurrentDirectory();
            config.SetBasePath(currentFolder);
            config.AddJsonFile(currentFolder + "\\Settings.json", true);

            var userProfile = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            config.SetBasePath(userProfile);
            config.AddJsonFile("UserSecrets.json", true);
        }

        
    }
}
