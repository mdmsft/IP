using IP.Models;
using System.Threading.Tasks;

namespace IP.Services
{
    public interface IDnsPayloadService
    {
        Task<DnsPayload> ConvertAsync(DnsParameters request);
    }
}
