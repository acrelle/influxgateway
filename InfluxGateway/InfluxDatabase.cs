﻿namespace InfluxGateway;

public class InfluxDatabase : IInfluxDatabase
{
    private readonly InfluxDb _influxDb;
    private readonly ILogger<InfluxController> _logger;
    private readonly InfluxConnectionSettings _influxConnectionSettings;

    public InfluxDatabase(ILogger<InfluxController> logger, IOptions<InfluxConnectionSettings> influxConnectionSettings)
    {
        _logger = logger;
        _influxConnectionSettings = influxConnectionSettings.Value;
        _influxDb = GetInfluxConnection();
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

    public async Task<string> GetFirstResultForInfluxQuery(string query)
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
