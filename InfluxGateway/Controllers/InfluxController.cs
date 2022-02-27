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
    private readonly ILogger<InfluxController> _logger;
    public IInfluxService _influxService;

    public InfluxController(ILogger<InfluxController> logger, IInfluxService influxService)
    {
        _logger = logger;
        _influxService = influxService;
    }

    // GET api/influx
    [HttpGet]
    public async Task<IActionResult> Get()
    {
        _logger.LogInformation("Client is requesting all values.");
        var results = await _influxService.GetInfluxValuesAsync();
        return Ok(results);
    }

    // GET api/influx/sensor+name
    [HttpGet("{id}")]
    public async Task<IActionResult> Get(string id)
    {
        _logger.LogInformation($"Client is requesting value of: {id}.");

        var resultList = await _influxService.GetInfluxValuesAsync(x => x.Equals(id, StringComparison.InvariantCultureIgnoreCase));

        return Ok(resultList.FirstOrDefault());
    }
}
