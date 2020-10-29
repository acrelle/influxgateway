using System.Threading.Tasks;

namespace InfluxGateway
{
    public interface IInfluxDatabase
    {

        Task<string> GetFirstResultForInfluxQuery(string query);

    }
}
