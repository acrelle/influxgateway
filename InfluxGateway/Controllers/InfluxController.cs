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
    private readonly InfluxConnectionSettings _influxConnectionSettings;
    private readonly ILogger<InfluxController> _logger;
    public IInfluxDatabase _influxDatabase;

    public InfluxController(
        IOptions<InfluxConnectionSettings> influxConnectionSettings,
        ILogger<InfluxController> logger,
        IInfluxDatabase influxDatabase)
    {
        _influxConnectionSettings = influxConnectionSettings.Value;
        _logger = logger;
        _influxDatabase = influxDatabase;
    }

    /// <summary>
    /// Performs the lookups through the InfluxDB.Net.Core.
    /// </summary>
    /// <param name="entityIdList"></param>
    /// <param name="selector"></param>
    /// <returns></returns>
    private async Task<IList<(string, string)>> GetInfluxValuesAsync(Func<KeyValuePair<string, string>, bool> selector = null)
    {
        // A list of friendly names mapped to the entity_id in influx, e.g. ...
        //
        //"sensors": {
        //    "Living Room Temperature": "climate_1_temperature",
        //    "Landing Temperature": "climate_3_temperature",
        //    "Office Temperature": "climate_4_temperature",
        //    "Bedroom Temperature": "climate_5_temperature"
        //    }
        var sensorList = _influxConnectionSettings.Sensors.Where(x => selector is null || selector(x));

        // Generate an individual Influx HTTP API query against each entity (rather than perform multiple queries in a single call).
        var resultList = new List<(string Sensor, Task<string>)>();
        foreach (var sensorItem in sensorList)
        {
            _logger.LogDebug($"Querying {sensorItem.Key} using influx entity_id {sensorItem.Value}.");
            resultList.Add(new(sensorItem.Key, _influxDatabase.GetFirstResultForInfluxQuery(sensorItem.Value)));
        }

        // Wait to complete.
        await Task.WhenAll(resultList.Select(y => y.Item2).ToList());
        _logger.LogDebug($"{resultList.Count} queries complete.");

        // Parse the results of each query ready to return to the caller.
        var results = new List<(string Sensor, string Result)>();
        foreach (var item in resultList)
        {
            results.Add(new(item.Sensor, item.Item2.Result));
        }

        return results;
    }


    // GET api/influx
    [HttpGet]
    public async Task<IActionResult> Get()
    {
        _logger.LogInformation("Client is requesting all values.");
        var results = await GetInfluxValuesAsync();
        return Ok(results); ;
    }


    // GET api/influx/sensor+name
    [HttpGet("{id}")]
    public async Task<IActionResult> Get(string id)
    {
        _logger.LogInformation($"Client is requesting value of: {id}.");

        var resultList = await GetInfluxValuesAsync(x => x.Key.Equals(id, StringComparison.InvariantCultureIgnoreCase));

        return Ok(resultList.FirstOrDefault());
    }
}
