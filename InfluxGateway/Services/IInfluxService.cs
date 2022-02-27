namespace InfluxGateway.Services;

public interface IInfluxService
{
    Task<IList<InfluxResult>> GetInfluxValuesAsync(Func<string, bool> keySelector = null);
}
