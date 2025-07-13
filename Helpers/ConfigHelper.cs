using System.IO;
using Microsoft.Extensions.Configuration;

namespace McpSQLServerApp.Helpers
{
    public static class ConfigHelper
    {
        private static IConfigurationRoot? _config;
        private static string? _connectionString;

        static ConfigHelper()
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
            _config = builder.Build();
            _connectionString = _config.GetConnectionString("MSSQLConnection");
        }

        public static string GetDefaultConnectionString()
        {
            return _connectionString ?? "Server=IRON;Database=master;Trusted_Connection=true;MultipleActiveResultSets=true;TrustServerCertificate=true;Encrypt=false;";
        }
    }
}
