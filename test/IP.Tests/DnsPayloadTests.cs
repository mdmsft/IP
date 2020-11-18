using IP.Models;
using Xunit;

namespace IP.Tests
{
    public class DnsPayloadTests
    {
        private readonly DnsPayload sut = new DnsPayload { Domain = "www.foo.bar" };

        [Fact]
        public void WhenGetZoneNameThenReturnsZoneNameFromDomain()
        {
            Assert.Equal("foo.bar", sut.ZoneName);
        }

        [Fact]
        public void WhenGetRecordSetNameThenReturnsRecordSetNameFromDomain()
        {
            Assert.Equal("www", sut.RecordSetName);
        }
    }
}
