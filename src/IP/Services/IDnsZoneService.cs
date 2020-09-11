using Microsoft.Azure.Management.Dns.Models;
using System.Threading.Tasks;

namespace IP.Services
{
    public interface IDnsZoneService
    {
        Task<Zone> GetZoneAsync(string zoneName);
    }
}
