namespace InfluxGateway.Models;

public class InfluxConnectionSettings
{
    // The following four settings should be defined as environmental variables at runtime 
    // or optionally the SecretManager in Development.
    public string InfluxUrl { get; set; }
    public string InfluxUsername { get; set; }
    public string InfluxPassword { get; set; }
    public string InfluxDatabaseConnection { get; set; }
    public string InfluxQuery { get; set; }
    public Dictionary<string, string> Sensors { get; set; }

    public bool Validate()
    {
        if (string.IsNullOrEmpty(InfluxUsername))
        {
            Console.WriteLine("No username has been supplied - ensure the environmental variables have been set if necessary (inspect the dockerfile).");
            return false;
        }
        if (!Uri.TryCreate(InfluxUrl, UriKind.Absolute, out var _))
        {
            Console.WriteLine($"The Influx URL is invalid: {InfluxUrl}");
            return false;
        }
        return true;
    }
}

