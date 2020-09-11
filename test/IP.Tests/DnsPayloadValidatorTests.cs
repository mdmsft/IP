using FakeItEasy;
using IP.Models;
using IP.Services;
using IP.Validators;
using Microsoft.Azure.Management.Dns.Models;
using Microsoft.Extensions.Configuration;
using System.Net;
using System.Threading.Tasks;
using Xunit;

namespace IP.Tests
{
    public class DnsPayloadValidatorTests
    {
        private readonly DnsPayloadValidator sut;
        private readonly IDnsZoneService zoneService;
        private readonly IConfiguration configuration;

        public DnsPayloadValidatorTests()
        {
            zoneService = A.Fake<IDnsZoneService>();
            configuration = A.Fake<IConfiguration>();
            sut = new DnsPayloadValidator(zoneService, configuration);
        }

        [Fact]
        public void WhenV4IsNullAndV6IsNullThenInvalid()
        {
            var input = new DnsPayload { Client = "client", Secret = "secret", Domain = "domain" };

            var result = sut.Validate(input);

            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => (e.PropertyName == nameof(DnsPayload.V4)) && (e.ErrorCode == "NotNullValidator"));
        }

        [Fact]
        public void WhenV4IsEmptyAndV6IsNullThenInvalid()
        {
            var input = new DnsPayload { Client = "client", Secret = "secret", Domain = "domain", V4 = string.Empty };

            var result = sut.Validate(input);

            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => (e.PropertyName == nameof(DnsPayload.V6)) && (e.ErrorCode == "NotEmptyValidator"));
        }

        [Fact]
        public void WhenV4IsInvalidIpThenInvalid()
        {
            var input = new DnsPayload { Client = "client", Secret = "secret", Domain = "domain", V4 = "v4" };

            var result = sut.Validate(input);

            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => (e.PropertyName == nameof(DnsPayload.V4)) && (e.ErrorCode == "PredicateValidator"));
        }

        [Fact]
        public void WhenV6IsInvalidIpThenInvalid()
        {
            var input = new DnsPayload { Client = "client", Secret = "secret", Domain = "domain", V6 = "v6" };

            var result = sut.Validate(input);

            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => (e.PropertyName == nameof(DnsPayload.V6)) && (e.ErrorCode == "PredicateValidator"));
        }

        [Fact]
        public void WhenDomainIsNullThenInvalid()
        {
            var input = new DnsPayload { Client = "client", Secret = "secret", V4 = IPAddress.Loopback.ToString() };

            var result = sut.Validate(input);

            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => (e.PropertyName == nameof(DnsPayload.Domain)) && (e.ErrorCode == "NotNullValidator"));
        }

        [Fact]
        public void WhenDomainIsEmptyThenInvalid()
        {
            var input = new DnsPayload { Client = "client", Secret = "secret", Domain = string.Empty, V4 = IPAddress.Loopback.ToString() };

            var result = sut.Validate(input);

            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => (e.PropertyName == nameof(DnsPayload.Domain)) && (e.ErrorCode == "NotEmptyValidator"));
        }

        [Fact]
        public void WhenClientIsNullThenInvalid()
        {
            var input = new DnsPayload { Domain = "domain", Secret = "secret", V4 = IPAddress.Loopback.ToString() };

            var result = sut.Validate(input);

            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => (e.PropertyName == nameof(DnsPayload.Client)) && (e.ErrorCode == "NotNullValidator"));
        }

        [Fact]
        public void WhenClientIsEmptyThenInvalid()
        {
            var input = new DnsPayload { Client = string.Empty, Secret = "secret", Domain = "domain", V4 = IPAddress.Loopback.ToString() };

            var result = sut.Validate(input);

            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => (e.PropertyName == nameof(DnsPayload.Client)) && (e.ErrorCode == "NotEmptyValidator"));
        }

        [Fact]
        public void WhenClientDoesNotMatchConfigurationThenInvalid()
        {
            var configurationSection = A.Fake<IConfigurationSection>();
            A.CallTo(() => configurationSection.Value).Returns("other client");
            A.CallTo(() => configuration.GetSection(nameof(DnsPayload.Client))).Returns(configurationSection);
            var input = new DnsPayload { Client = "client", Secret = "secret", Domain = "domain", V4 = IPAddress.Loopback.ToString() };

            var result = sut.Validate(input);

            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => (e.PropertyName == nameof(DnsPayload.Client)) && (e.ErrorCode == "PredicateValidator"));
        }

        [Fact]
        public void WhenSecretIsNullThenInvalid()
        {
            var input = new DnsPayload { Domain = "domain", Client = "client", V4 = IPAddress.Loopback.ToString() };

            var result = sut.Validate(input);

            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => (e.PropertyName == nameof(DnsPayload.Secret)) && (e.ErrorCode == "NotNullValidator"));
        }

        [Fact]
        public void WhenSecretIsEmptyThenInvalid()
        {
            var input = new DnsPayload { Client = "client", Secret = string.Empty, Domain = "domain", V4 = IPAddress.Loopback.ToString() };

            var result = sut.Validate(input);

            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => (e.PropertyName == nameof(DnsPayload.Secret)) && (e.ErrorCode == "NotEmptyValidator"));
        }

        [Fact]
        public void WhenSecretDoesNotMatchConfigurationThenInvalid()
        {
            var clientConfigurationSection = A.Fake<IConfigurationSection>();
            A.CallTo(() => clientConfigurationSection.Value).Returns("client");
            A.CallTo(() => configuration.GetSection(nameof(DnsPayload.Client))).Returns(clientConfigurationSection);
            var secretConfigurationSection = A.Fake<IConfigurationSection>();
            A.CallTo(() => secretConfigurationSection.Value).Returns("other secret");
            A.CallTo(() => configuration.GetSection(nameof(DnsPayload.Secret))).Returns(secretConfigurationSection);
            var input = new DnsPayload { Client = "client", Secret = "secret", Domain = "domain", V4 = IPAddress.Loopback.ToString() };

            var result = sut.Validate(input);

            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => (e.PropertyName == nameof(DnsPayload.Secret)) && (e.ErrorCode == "PredicateValidator"));
        }

        [Fact]
        public async Task WhenDnsZoneNotFoundThenInvalid()
        {
            var input = new DnsPayload { Client = "client", Secret = "secret", Domain = "domain.local", V4 = IPAddress.Loopback.ToString(), V6 = IPAddress.IPv6Loopback.ToString() };
            A.CallTo(() => zoneService.GetZoneAsync("local")).Returns((Zone)default);

            var result = await sut.ValidateAsync(input);

            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => (e.PropertyName == nameof(DnsPayload.Domain)) && (e.ErrorCode == "AsyncPredicateValidator"));
        }

        [Fact]
        public void WhenAllValuesAreCorrectThenValid()
        {
            var clientConfigurationSection = A.Fake<IConfigurationSection>();
            A.CallTo(() => clientConfigurationSection.Value).Returns("client");
            A.CallTo(() => configuration.GetSection(nameof(DnsPayload.Client))).Returns(clientConfigurationSection);
            var secretConfigurationSection = A.Fake<IConfigurationSection>();
            A.CallTo(() => secretConfigurationSection.Value).Returns("secret");
            A.CallTo(() => configuration.GetSection(nameof(DnsPayload.Secret))).Returns(secretConfigurationSection);
            A.CallTo(() => zoneService.GetZoneAsync(A<string>._)).Returns(new Zone());
            var input = new DnsPayload { Client = "client", Secret = "secret", Domain = "domain", V4 = IPAddress.Loopback.ToString(), V6 = IPAddress.IPv6Loopback.ToString() };

            var result = sut.Validate(input);

            Assert.True(result.IsValid);
        }
    }
}
