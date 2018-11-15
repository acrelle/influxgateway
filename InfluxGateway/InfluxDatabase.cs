using InfluxDB.Net;
using InfluxGateway.Controllers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace InfluxGateway
{
    public class InfluxDatabase : IInfluxDatabase
    {

        private InfluxDb InfluxDb { get; }

        // The following four settings should be defined as envinronmental variables at runtime 
        // or optionally the SecretManager in Development.
        private string InfluxUrl => Configuration["influx_url"];
        private string InfluxUsername => Configuration["influx_username"];
        private string InfluxPassword => Configuration["influx_password"];
        private string InfluxDatabaseConnection => Configuration["influx_database"];
        // This is stored in the appsettings.json.
        private string InfluxQuery => Configuration["influxgateway:query"];


        private IConfiguration Configuration { get; }
        private ILogger<InfluxController> Logger { get; }

        public InfluxDatabase(IConfiguration configuration, ILogger<InfluxController> logger)
        {
            Configuration = configuration;
            Logger = logger;

            // Error checking.
            if (string.IsNullOrEmpty(InfluxUsername))
                Logger.LogWarning("No username has been supplied - ensure the environmental variables have been set if necessary (inspect the dockerfile).");
            if (!Uri.TryCreate(InfluxUrl, UriKind.Absolute, out var result))
                Logger.LogError("The Influx URL is invalid: {url}", InfluxUrl);

            InfluxDb = GetInfluxConnection();

        }

        /// <summary>
        /// TODO: Sanitise user input to prevent command injection.
        /// </summary>
        /// <param name="sensorName"></param>
        /// <returns></returns>
        private string BuildQuery(string sensorName)
        {
            return InfluxQuery.Replace("%s", sensorName);
        }

        public async Task<string> GetFirstResultForInfluxQuery(string query)
        {
            var x = await InfluxDb.QueryAsync(InfluxDatabaseConnection, BuildQuery(query));
            return x.FirstOrDefault().Values[0][1].ToString();
        }

        /// <summary>
        /// Construct a Influx Client
        /// </summary>
        /// <returns></returns>
        private InfluxDb GetInfluxConnection()
        {
            Logger.LogDebug("New connection to be created against {url}", InfluxUrl);
            return new InfluxDb(InfluxUrl, InfluxUsername, InfluxPassword);
        }

    }
}