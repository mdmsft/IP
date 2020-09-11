using FakeItEasy;
using IP.Models;
using IP.Services;
using Microsoft.Azure.Management.Dns.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Security;
using System.Threading.Tasks;
using Xunit;

namespace IP.Tests
{
    public class DnsManagementServiceTests
    {
        private readonly IDnsManagementService sut;

        private readonly IDnsRecordSetService dnsRecordSetService;

        private readonly ILogger<DnsManagementService> logger;

        public DnsManagementServiceTests()
        {
            dnsRecordSetService = A.Fake<IDnsRecordSetService>();
            logger = A.Fake<ILogger<DnsManagementService>>();
            sut = new DnsManagementService(dnsRecordSetService, logger);
        }

        [Fact]
        public async Task WhenV4AndRecordSetDoesNotExistThenCreatesRecordSetWithMetadata()
        {
            var update = new DnsPayload { V4 = "1.2.3.4", Client = "foo", Secret = "bar", Domain = "www.foo.bar" };
            A.CallTo(() => dnsRecordSetService.GetRecordSetAsync("foo.bar", "www", RecordType.A)).Returns((RecordSet)default);

            Expression<Func<RecordSet, bool>> recordSetPredicate = recordSet =>
                recordSet.Metadata.ContainsKey("Owner") &&
                recordSet.Metadata["Owner"] == "foo" &&
                recordSet.ARecords.Single().Ipv4Address == "1.2.3.4";

            await sut.ProcessAsync(update);

            A.CallTo(() => dnsRecordSetService.CreateOrUpdateRecordSetAsync("foo.bar", "www", RecordType.A, A<RecordSet>.That.Matches(recordSetPredicate))).MustHaveHappenedOnceExactly();
        }

        [Fact]
        public async Task WhenV4AndRecordSetExistsAndWithoutMetadataThenThrowsSecurityException()
        {
            var update = new DnsPayload { V4 = "1.2.3.4", Client = "foo", Secret = "bar", Domain = "www.foo.bar" };
            A.CallTo(() => dnsRecordSetService.GetRecordSetAsync("foo.bar", "www", RecordType.A)).Returns(new RecordSet(metadata: new Dictionary<string, string>()));

            await Assert.ThrowsAsync<SecurityException>(() => sut.ProcessAsync(update));
        }

        [Fact]
        public async Task WhenV4AndRecordSetExistsAndWithMismatchMetadataThenThrowsSecurityException()
        {
            var update = new DnsPayload { V4 = "1.2.3.4", Client = "foo", Secret = "bar", Domain = "www.foo.bar" };
            A.CallTo(() => dnsRecordSetService.GetRecordSetAsync("foo.bar", "www", RecordType.A)).Returns(new RecordSet(metadata: new Dictionary<string, string> { { "Owner", "baz" } }));

            await Assert.ThrowsAsync<SecurityException>(() => sut.ProcessAsync(update));
        }

        [Fact]
        public async Task WhenV4AndRecordSetExistsAndMetadataMatchThenUpdatesRecordSet()
        {
            var update = new DnsPayload { V4 = "1.2.3.4", Client = "foo", Secret = "bar", Domain = "www.foo.bar" };
            A.CallTo(() => dnsRecordSetService.GetRecordSetAsync("foo.bar", "www", RecordType.A)).Returns(new RecordSet(metadata: new Dictionary<string, string> { { "Owner", "foo" } }, aRecords: new List<ARecord> { new ARecord("5.6.7.8") }));

            Expression<Func<RecordSet, bool>> recordSetPredicate = recordSet =>
                recordSet.Metadata.ContainsKey("Owner") &&
                recordSet.Metadata["Owner"] == "foo" &&
                recordSet.ARecords.Single().Ipv4Address == "1.2.3.4";

            await sut.ProcessAsync(update);

            A.CallTo(() => dnsRecordSetService.CreateOrUpdateRecordSetAsync("foo.bar", "www", RecordType.A, A<RecordSet>.That.Matches(recordSetPredicate))).MustHaveHappenedOnceExactly();
        }

        [Fact]
        public async Task WhenV6AndRecordSetDoesNotExistThenCreatesRecordSetWithMetadata()
        {
            var update = new DnsPayload { V6 = "::1", Client = "foo", Secret = "bar", Domain = "www.foo.bar" };
            A.CallTo(() => dnsRecordSetService.GetRecordSetAsync("foo.bar", "www", RecordType.AAAA)).Returns((RecordSet)default);

            Expression<Func<RecordSet, bool>> recordSetPredicate = recordSet =>
                recordSet.Metadata.ContainsKey("Owner") &&
                recordSet.Metadata["Owner"] == "foo" &&
                recordSet.AaaaRecords.Single().Ipv6Address == "::1";

            await sut.ProcessAsync(update);

            A.CallTo(() => dnsRecordSetService.CreateOrUpdateRecordSetAsync("foo.bar", "www", RecordType.AAAA, A<RecordSet>.That.Matches(recordSetPredicate))).MustHaveHappenedOnceExactly();
        }

        [Fact]
        public async Task WhenV6AndRecordSetExistsAndWithoutMetadataThenThrowsSecurityException()
        {
            var update = new DnsPayload { V6 = "::1", Client = "foo", Secret = "bar", Domain = "www.foo.bar" };
            A.CallTo(() => dnsRecordSetService.GetRecordSetAsync("foo.bar", "www", RecordType.AAAA)).Returns(new RecordSet(metadata: new Dictionary<string, string>()));

            await Assert.ThrowsAsync<SecurityException>(() => sut.ProcessAsync(update));
        }

        [Fact]
        public async Task WhenV6AndRecordSetExistsAndWithMismatchMetadataThenThrowsSecurityException()
        {
            var update = new DnsPayload { V6 = "::1", Client = "foo", Secret = "bar", Domain = "www.foo.bar" };
            A.CallTo(() => dnsRecordSetService.GetRecordSetAsync("foo.bar", "www", RecordType.AAAA)).Returns(new RecordSet(metadata: new Dictionary<string, string> { { "Owner", "baz" } }));

            await Assert.ThrowsAsync<SecurityException>(() => sut.ProcessAsync(update));
        }

        [Fact]
        public async Task WhenV6AndRecordSetExistsAndMetadataMatchThenUpdatesRecordSet()
        {
            var update = new DnsPayload { V6 = "::1", Client = "foo", Secret = "bar", Domain = "www.foo.bar" };
            A.CallTo(() => dnsRecordSetService.GetRecordSetAsync("foo.bar", "www", RecordType.AAAA)).Returns(new RecordSet(metadata: new Dictionary<string, string> { { "Owner", "foo" } }, aaaaRecords: new List<AaaaRecord> { new AaaaRecord("::2") }));

            Expression<Func<RecordSet, bool>> recordSetPredicate = recordSet =>
                recordSet.Metadata.ContainsKey("Owner") &&
                recordSet.Metadata["Owner"] == "foo" &&
                recordSet.AaaaRecords.Single().Ipv6Address == "::1";

            await sut.ProcessAsync(update);

            A.CallTo(() => dnsRecordSetService.CreateOrUpdateRecordSetAsync("foo.bar", "www", RecordType.AAAA, A<RecordSet>.That.Matches(recordSetPredicate))).MustHaveHappenedOnceExactly();
        }
    }
}
