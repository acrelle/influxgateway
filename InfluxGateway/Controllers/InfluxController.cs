namespace InfluxGateway.Controllers;

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
    private IConfiguration _configuration;
    private ILogger<InfluxController> _logger;
    public IInfluxDatabase _influxDatabase;

    public InfluxController(IConfiguration configuration, ILogger<InfluxController> logger, IInfluxDatabase influxDatabase)
    {
        _configuration = configuration;
        _logger = logger;
        _influxDatabase = influxDatabase;
    }

    /// <summary>
    /// Performs the lookups through the InfluxDB.Net.Core.
    /// </summary>
    /// <param name="entityIdList"></param>
    /// <param name="selector"></param>
    /// <returns></returns>
    private async Task<List<KeyValuePair<String, String>>> GetInfluxValuesAsync(Func<IConfigurationSection, bool> selector = null)
    {
        // A list of friendly names mapped to the entity_id in influx, e.g. ...
        //
        //"sensors": {
        //    "Living Room Temperature": "climate_1_temperature",
        //    "Landing Temperature": "climate_3_temperature",
        //    "Office Temperature": "climate_4_temperature",
        //    "Bedroom Temperature": "climate_5_temperature"
        //    }
        var sensorList = _configuration.GetSection("influxgateway:sensors").GetChildren();

        // Allow a filter
        if (selector != null)
            sensorList = sensorList.Where(selector);

        // Generate an individual Influx HTTP API query against each entity (rather than perform multiple queries in a single call).
        var resultList = new List<Tuple<string, string, Task<string>>>();
        foreach (var sensorItem in sensorList)
        {
            _logger.LogDebug("Querying {key} using influx entity_id {value}.", sensorItem.Key, sensorItem.Value);
            resultList.Add(Tuple.Create(sensorItem.Key, sensorItem.Value, _influxDatabase.GetFirstResultForInfluxQuery(sensorItem.Value)));
        }

        // Wait to complete.
        await Task.WhenAll(resultList.Select(y => y.Item3).ToList());
        _logger.LogDebug("{count} queries complete.", resultList.Count);

        // Parse the results of each query ready to return to the caller.
        var results = new List<KeyValuePair<string, string>>();
        foreach (var item in resultList)
        {
            results.Add(new KeyValuePair<string, string>(item.Item1, item.Item3.Result));
        }

        return results;
    }


    // GET api/influx
    [HttpGet]
    public async Task<IEnumerable<KeyValuePair<String, String>>> Get()
    {
        _logger.LogInformation("Client is requesting all values.");

        var resultList = await GetInfluxValuesAsync();

        return resultList;
    }


    // GET api/influx/sensor+name
    [HttpGet("{id}")]
    public async Task<KeyValuePair<String, String>> Get(String id)
    {
        _logger.LogInformation("Client is requesting value of: {val}.", id);

        var resultList = await GetInfluxValuesAsync(x => x.Key.Equals(id, StringComparison.InvariantCultureIgnoreCase));

        return resultList.FirstOrDefault();
    }
}
