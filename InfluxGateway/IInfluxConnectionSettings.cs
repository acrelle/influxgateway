namespace InfluxGateway
{
    public interface IInfluxConnectionSettings
    {
        string InfluxUrl { get; }
        string InfluxUsername { get; }
        string InfluxPassword { get; }
        string InfluxDatabaseConnection { get; }
        string InfluxQuery { get; }
    }
}
