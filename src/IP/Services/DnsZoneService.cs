using IP.Settings;
using Microsoft.Azure.Management.Dns;
using Microsoft.Azure.Management.Dns.Models;
using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;

namespace IP.Services
{
    public sealed class DnsZoneService : IDnsZoneService
    {
        private readonly IDnsManagementClient client;
        private readonly IConfiguration configuration;

        public DnsZoneService(IDnsManagementClient client, IConfiguration configuration)
        {
            this.client = client;
            this.configuration = configuration;
        }

        public async Task<Zone> GetZoneAsync(string zoneName)
        {
            var resourceGroupName = configuration.GetValue<string>(ConfigurationSettings.ResourceGroup);
            return await client.Zones.GetAsync(resourceGroupName, zoneName);
        }
    }
}
