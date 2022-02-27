namespace InfluxGateway.Services;

public class InfluxService : IInfluxService
{
    private readonly InfluxDb _influxDb;
    private readonly ILogger<InfluxController> _logger;
    private readonly InfluxConnectionSettings _influxConnectionSettings;

    public InfluxService(ILogger<InfluxController> logger, IOptions<InfluxConnectionSettings> influxConnectionSettings)
    {
        _logger = logger;
        _influxConnectionSettings = influxConnectionSettings.Value;
        _influxDb = GetInfluxConnection();
    }

    /// <summary>
    /// Performs the lookups through the InfluxDB.Net.Core.
    /// </summary>
    public async Task<IList<InfluxResult>> GetInfluxValuesAsync(Func<string, bool> keySelector = default)
    {
        // A list of friendly names mapped to the entity_id in influx, e.g. ...
        //
        //"sensors": {
        //    "Living Room Temperature": "climate_1_temperature",
        //    "Landing Temperature": "climate_3_temperature",
        //    "Office Temperature": "climate_4_temperature",
        //    "Bedroom Temperature": "climate_5_temperature"
        //    }
        var sensorList = _influxConnectionSettings.Sensors.Where(x => keySelector is null || keySelector(x.Key));

        // Generate an individual Influx HTTP API query against each entity (rather than perform multiple queries in a single call).
        var resultList = new List<(string Sensor, Task<string>)>();
        foreach (var sensorItem in sensorList)
        {
            _logger.LogDebug($"Querying {sensorItem.Key} using influx entity_id {sensorItem.Value}.");
            resultList.Add(new(sensorItem.Key, GetFirstResultForInfluxQuery(sensorItem.Value)));
        }

        // Wait to complete.
        await Task.WhenAll(resultList.Select(y => y.Item2).ToList());
        _logger.LogDebug($"{resultList.Count} queries complete.");

        // Parse the results of each query ready to return to the caller.
        var results = new List<InfluxResult>();
        foreach (var item in resultList)
        {
            results.Add(new(item.Sensor, item.Item2.Result));
        }

        return results;
    }

    /// <summary>
    /// TODO: Sanitise user input to prevent command injection.
    /// </summary>
    /// <param name="sensorName"></param>
    /// <returns></returns>
    private string BuildQuery(string sensorName)
    {
        return _influxConnectionSettings.InfluxQuery.Replace("%s", sensorName);
    }

    private async Task<string> GetFirstResultForInfluxQuery(string query)
    {
        var x = await _influxDb.QueryAsync(_influxConnectionSettings.InfluxDatabaseConnection, BuildQuery(query));
        return x.FirstOrDefault().Values[0][1].ToString();
    }

    /// <summary>
    /// Construct a Influx Client
    /// </summary>
    private InfluxDb GetInfluxConnection()
    {
        _logger.LogDebug($"New connection to be created against {_influxConnectionSettings.InfluxUrl}");
        return new InfluxDb(_influxConnectionSettings.InfluxUrl, _influxConnectionSettings.InfluxUsername, _influxConnectionSettings.InfluxPassword);
    }
}
