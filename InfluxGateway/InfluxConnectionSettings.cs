namespace InfluxGateway;

internal class InfluxConnectionSettings : IInfluxConnectionSettings
{

    // The following four settings should be defined as environmental variables at runtime 
    // or optionally the SecretManager in Development.
    public string InfluxUrl => _configuration["influx_url"];
    public string InfluxUsername => _configuration["influx_username"];
    public string InfluxPassword => _configuration["influx_password"];
    public string InfluxDatabaseConnection => _configuration["influx_database"];
    // This is stored in the appsettings.json.
    public string InfluxQuery => _configuration["influxgateway:query"];

    public IConfiguration _configuration;
    public ILogger<InfluxController> _logger { get; }

    public InfluxConnectionSettings(IConfiguration configuration, ILogger<InfluxController> logger)
    {
        _configuration = configuration;
        _logger = logger;

        // Error checking.
        if (string.IsNullOrEmpty(InfluxUsername))
        {
            _logger.LogWarning("No username has been supplied - ensure the environmental variables have been set if necessary (inspect the dockerfile).");
        }
        if (!Uri.TryCreate(InfluxUrl, UriKind.Absolute, out var result))
        {
            _logger.LogError("The Influx URL is invalid: {url}", InfluxUrl);
        }
    }
}

