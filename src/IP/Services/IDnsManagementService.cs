using IP.Models;
using System.Threading.Tasks;

namespace IP.Services
{
    public interface IDnsManagementService
    {
        Task ProcessAsync(DnsPayload update);
    }
}
