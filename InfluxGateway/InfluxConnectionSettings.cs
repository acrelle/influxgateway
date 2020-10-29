using System;
using InfluxGateway.Controllers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace InfluxGateway
{
    internal class InfluxConnectionSettings : IInfluxConnectionSettings
    {

        // The following four settings should be defined as envinronmental variables at runtime 
        // or optionally the SecretManager in Development.
        public string InfluxUrl => Configuration["influx_url"];
        public string InfluxUsername => Configuration["influx_username"];
        public string InfluxPassword => Configuration["influx_password"];
        public string InfluxDatabaseConnection => Configuration["influx_database"];
        // This is stored in the appsettings.json.
        public string InfluxQuery => Configuration["influxgateway:query"];

        public IConfiguration Configuration { get; }
        public ILogger<InfluxController> Logger { get; }

        public InfluxConnectionSettings(IConfiguration configuration, ILogger<InfluxController> logger)
        {
            Configuration = configuration;
            Logger = logger;

            // Error checking.
            if (string.IsNullOrEmpty(InfluxUsername))
                Logger.LogWarning("No username has been supplied - ensure the environmental variables have been set if necessary (inspect the dockerfile).");
            if (!Uri.TryCreate(InfluxUrl, UriKind.Absolute, out var result))
                Logger.LogError("The Influx URL is invalid: {url}", InfluxUrl);
        }
    }
}
