using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using InfluxDB.Net;
using InfluxDB.Net.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace InfluxGateway.Controllers
{
    /// <summary>
    /// A simple access layer for influx data.
    /// It is recommended that Authentication / SSL is applied to protect your data.
    /// I handle this through a reverse proxy outside of .NET Core.
    /// 
    /// Anthony Relle - 2018.
    /// </summary>
    [Route("api/[controller]")]
    public class InfluxController : Controller
    {
        private IConfiguration Configuration { get; }
        private ILogger Logger { get; }

        // The following four settings should be defined as envinronmental variables at runtime 
        // or optionally the SecretManager in Development.
        private string InfluxUrl => Configuration["influx_url"];
        private string InfluxUsername => Configuration["influx_username"];
        private string InfluxPassword => Configuration["influx_password"];
        private string InfluxDatabase => Configuration["influx_database"];

        // This is stored in the appsettings.json.
        private string InfluxQuery => Configuration["influxgateway:query"];

        public InfluxController(IConfiguration configuration, ILogger<InfluxController> logger)
        {
            Configuration = configuration;
            Logger = logger;

            // Error checking.
            if (String.IsNullOrEmpty(InfluxUsername))
                Logger.LogWarning("No username has been supplied - ensure the environmental variables have been set if necessary (inspect the dockerfile).");
            if (!Uri.TryCreate(InfluxUrl, UriKind.Absolute, out var result))
                Logger.LogError("The Influx URL is invalid: {url}", InfluxUrl);

        }

        /// <summary>
        /// TODO: Sanitise user input to prevent command injection.
        /// </summary>
        /// <param name="sensorName"></param>
        /// <returns></returns>
        private String BuildQuery(String sensorName)
        {
            return InfluxQuery.Replace("%s", sensorName);
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

        /// <summary>
        /// Performs the lookups through the InfluxDB.Net.Core.
        /// </summary>
        /// <param name="entityIdList"></param>
        /// <param name="selector"></param>
        /// <returns></returns>
        private async Task<List<KeyValuePair<String, String>>> GetInfluxValuesAsync(Func<IConfigurationSection, bool> selector = null)
        {
            var _client = GetInfluxConnection();

            // A list of friendly names mapped to the entity_id in influx, e.g. ...
            //
            //"sensors": {
            //    "Living Room Temperature": "climate_1_temperature",
            //    "Landing Temperature": "climate_3_temperature",
            //    "Office Temperature": "climate_4_temperature",
            //    "Bedroom Temperature": "climate_5_temperature"
            //    }
            var sensorList = Configuration.GetSection("influxgateway:sensors").GetChildren();

            // Allow a filter
            if (selector != null)
                sensorList = sensorList.Where(selector);

            // Generate an individual Influx HTTP API query against each eneity (rather than perform multiple queries in a single call).
            var resultList = new List<Tuple<String, String, Task<List<Serie>>>>();
            foreach (var sensorItem in sensorList)
            {
                Logger.LogDebug("Querying {key} using influx entity_id {value}.", sensorItem.Key, sensorItem.Value);
                resultList.Add(Tuple.Create(sensorItem.Key, sensorItem.Value, _client.QueryAsync(InfluxDatabase, BuildQuery(sensorItem.Value))));
            }

            // Wait to complete.
            await Task.WhenAll(resultList.Select(y => y.Item3).ToList());
            Logger.LogDebug("{count} queries complete.", resultList.Count);

            // Parse the results of each query ready to return to the caller.

            var results = new List<KeyValuePair<string, string>>();
            foreach (var item in resultList)
            {
                var x = item.Item3.Result.FirstOrDefault().Values[0];
                results.Add(new KeyValuePair<string, string>(item.Item1, x[1].ToString()));
            }

            return results;
        }


        // GET api/influx
        [HttpGet]
        public async Task<IEnumerable<KeyValuePair<String,String>>> Get()
        {
            Logger.LogInformation("Client is requesting all values.");

            var resultList =  await GetInfluxValuesAsync();

            return resultList;
        }


        // GET api/influx/sensor+name
        [HttpGet("{id}")]
        public async Task<KeyValuePair<String, String>> Get(String id)
        {
            Logger.LogInformation("Client is requesting value of: {val}.", id);

            var resultList = await GetInfluxValuesAsync(x => x.Key.Equals(id, StringComparison.InvariantCultureIgnoreCase));

            return resultList.FirstOrDefault();
        }

        // POST api/influx
        [HttpPost]
        public void Post([FromBody]string value)
        {
            // Not implemented
        }

        // PUT api/influx/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody]string value)
        {
            // Not implemented
        }

        // DELETE api/influx/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
            // Not implemented
        }
    }
}
