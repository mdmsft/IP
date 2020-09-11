using Microsoft.Azure.Management.Dns.Models;
using System.Threading.Tasks;

namespace IP.Services
{
    public interface IDnsRecordSetService
    {
        Task<RecordSet> GetRecordSetAsync(string zoneName, string recordSetName, RecordType recordType);

        Task CreateOrUpdateRecordSetAsync(string zoneName, string recordSetName, RecordType recordType, RecordSet recordSet);
    }
}
