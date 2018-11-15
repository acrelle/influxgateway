using InfluxDB.Net;
using InfluxGateway.Controllers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Linq;
using System.Threading.Tasks;

namespace InfluxGateway
{
    public class InfluxDatabase : IInfluxDatabase
    {

        private InfluxDb InfluxDb { get; }
        private IConfiguration Configuration { get; }
        private ILogger<InfluxController> Logger { get; }
        private IInfluxConnectionSettings InfluxConnectionSettings { get; }

        public InfluxDatabase(IConfiguration configuration, ILogger<InfluxController> logger, IInfluxConnectionSettings influxConnectionSettings)
        {
            Configuration = configuration;
            Logger = logger;
            InfluxConnectionSettings = influxConnectionSettings;
            InfluxDb = GetInfluxConnection();

        }

        /// <summary>
        /// TODO: Sanitise user input to prevent command injection.
        /// </summary>
        /// <param name="sensorName"></param>
        /// <returns></returns>
        private string BuildQuery(string sensorName)
        {
            return InfluxConnectionSettings.InfluxQuery.Replace("%s", sensorName);
        }

        public async Task<string> GetFirstResultForInfluxQuery(string query)
        {
            var x = await InfluxDb.QueryAsync(InfluxConnectionSettings.InfluxDatabaseConnection, BuildQuery(query));
            return x.FirstOrDefault().Values[0][1].ToString();
        }

        /// <summary>
        /// Construct a Influx Client
        /// </summary>
        /// <returns></returns>
        private InfluxDb GetInfluxConnection()
        {
            Logger.LogDebug("New connection to be created against {url}", InfluxConnectionSettings.InfluxUrl);
            return new InfluxDb(InfluxConnectionSettings.InfluxUrl, InfluxConnectionSettings.InfluxUsername, InfluxConnectionSettings.InfluxPassword);
        }

    }
}