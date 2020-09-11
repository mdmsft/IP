using IP.Settings;
using Microsoft.Azure.Management.Dns;
using Microsoft.Azure.Management.Dns.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Rest.Azure;
using System.Threading.Tasks;

namespace IP.Services
{
    public sealed class DnsRecordSetService : IDnsRecordSetService
    {
        private readonly IDnsManagementClient client;
        private readonly string resourceGroupName;

        public DnsRecordSetService(IDnsManagementClient client, IConfiguration configuration)
        {
            this.client = client;
            resourceGroupName = configuration.GetValue<string>(ConfigurationSettings.ResourceGroup);
        }

        public async Task CreateOrUpdateRecordSetAsync(string zoneName, string recordSetName, RecordType recordType, RecordSet recordSet)
        {
            await client.RecordSets.CreateOrUpdateAsync(resourceGroupName, zoneName, recordSetName, recordType, recordSet);
        }

        public async Task<RecordSet> GetRecordSetAsync(string zoneName, string recordSetName, RecordType recordType)
        {
            try
            {
                return await client.RecordSets.GetAsync(resourceGroupName, zoneName, recordSetName, recordType);
            }
            catch (CloudException exception) when (exception.Body.Code == "NotFound")
            {
                return default;
            }
        }
    }
}
