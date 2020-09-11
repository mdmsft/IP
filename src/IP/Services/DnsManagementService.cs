using IP.Models;
using Microsoft.Azure.Management.Dns.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Security;
using System.Threading.Tasks;

namespace IP.Services
{
    public sealed class DnsManagementService : IDnsManagementService
    {
        private readonly ILogger<DnsManagementService> logger;
        private readonly IDnsRecordSetService recordSetService;

        private const string OwnerMetadataKey = "Owner";

        public DnsManagementService(IDnsRecordSetService recordSetService, ILogger<DnsManagementService> logger)
        {
            this.logger = logger;
            this.recordSetService = recordSetService;
        }

        public async Task ProcessAsync(DnsPayload update)
        {
            var tasks = new List<Task>();

            if (!string.IsNullOrEmpty(update.V4))
            {
                tasks.Add(CreateOrUpdateRecordSet(update.V4, update.RecordSetName, update.ZoneName, update.Client, RecordType.A));
            }

            if (!string.IsNullOrEmpty(update.V6))
            {
                tasks.Add(CreateOrUpdateRecordSet(update.V6, update.RecordSetName, update.ZoneName, update.Client, RecordType.AAAA));
            }

            await Task.WhenAll(tasks);
        }

        private async Task CreateOrUpdateRecordSet(string ip, string recordSetName, string zoneName, string userName, RecordType recordType)
        {
            logger.LogInformation("Loading {0} record set for {1}...", recordType, recordSetName);
            var recordSet = await recordSetService.GetRecordSetAsync(zoneName, recordSetName, recordType);

            if (recordSet is null)
            {
                logger.LogInformation("Record set does not exist, creating...");
                recordSet = new RecordSet
                {
                    Metadata = new Dictionary<string, string> { { OwnerMetadataKey, userName } },
                    TTL = 3600
                };
                switch (recordType)
                {
                    case RecordType.A:
                        recordSet.ARecords = new List<ARecord> { new ARecord(ip) };
                        break;
                    case RecordType.AAAA:
                        recordSet.AaaaRecords = new List<AaaaRecord> { new AaaaRecord(ip) };
                        break;
                }
            }
            else
            {
                logger.LogInformation("Record set exists, checking metadata...");
                if (recordSet.Metadata.TryGetValue(OwnerMetadataKey, out var owner) && owner.Equals(userName, StringComparison.Ordinal))
                {
                    logger.LogInformation("Metadata OK, updating record set...");
                    switch (recordType)
                    {
                        case RecordType.A:
                            recordSet.ARecords.Clear();
                            recordSet.ARecords.Add(new ARecord(ip));
                            break;
                        case RecordType.AAAA:
                            recordSet.AaaaRecords.Clear();
                            recordSet.AaaaRecords.Add(new AaaaRecord(ip));
                            break;
                    }
                }
                else
                {
                    logger.LogError("Metadata mismatch! '{0}' is not authorized for A '{1}' owned by '{2}'. Aborting...", userName, recordSetName, owner);
                    throw new SecurityException($"Client '{userName}' is not authorized for A record set '{recordSetName}'");
                }
            }

            await recordSetService.CreateOrUpdateRecordSetAsync(zoneName, recordSetName, recordType, recordSet);
        }
    }
}
