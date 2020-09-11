using FakeItEasy;
using FluentValidation;
using IP.Services;
using IP.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Internal;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using System;
using System.Collections.Generic;
using System.Security;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace IP.Tests
{
    public class DnsPayloadServiceTests
    {
        private readonly IDnsPayloadService sut;

        public DnsPayloadServiceTests()
        {
            sut = new DnsPayloadService(A.Fake<IValidator<DnsPayload>>(), A.Fake<ILoggerFactory>());
        }

        [Fact]
        public async Task WhenNoAuthorizationHeaderProvidedThenThrowsUnauthorizedAccessException()
        {
            var request = new DnsParameters(new HeaderDictionary(), new QueryCollection());

            await Assert.ThrowsAsync<UnauthorizedAccessException>(() => sut.ConvertAsync(request));
        }

        [Fact]
        public async Task WhenMoreThanOneAuthorizationHeaderProvidedThenThrowsNotSupportedException()
        {
            var headers = new HeaderDictionary(new Dictionary<string, StringValues> { { "Authorization", new StringValues(new[] { "one", "two" }) } });
            var query = new QueryCollection();
            var request = new DnsParameters(headers, query);

            await Assert.ThrowsAsync<NotSupportedException>(() => sut.ConvertAsync(request));
        }

        [Fact]
        public async Task WhenAuthorizationSchemeIsNotBasicThenThrowsNotSupportedException()
        {
            var headers = new HeaderDictionary(new Dictionary<string, StringValues> { { "Authorization", new StringValues("Digest") } });
            var query = new QueryCollection();
            var request = new DnsParameters(headers, query);

            await Assert.ThrowsAsync<NotSupportedException>(() => sut.ConvertAsync(request));
        }

        [Fact]
        public async Task WhenAuthorizationHeaderIsNotBase64ThenThrowsSecurityException()
        {
            var headers = new HeaderDictionary(new Dictionary<string, StringValues> { { "Authorization", new StringValues("Basic foobar") } });
            var query = new QueryCollection();
            var request = new DnsParameters(headers, query);

            await Assert.ThrowsAsync<SecurityException>(() => sut.ConvertAsync(request));
        }

        [Fact]
        public async Task WhenAuthorizationHeaderIsMalformedThenThrowsSecurityException()
        {
            var headers = new HeaderDictionary(new Dictionary<string, StringValues> { { "Authorization", new StringValues($"Basic {Convert.ToBase64String(Encoding.ASCII.GetBytes("foo:bar:baz"))}") } });
            var query = new QueryCollection();
            var request = new DnsParameters(headers, query);

            await Assert.ThrowsAsync<SecurityException>(() => sut.ConvertAsync(request));
        }

        [Fact]
        public async Task WhenAllParametersAreCorrectThenConverts()
        {
            var headers = new HeaderDictionary(new Dictionary<string, StringValues> { { "Authorization", new StringValues($"Basic {Convert.ToBase64String(Encoding.ASCII.GetBytes("foo:bar"))}") } });
            var query = new QueryCollection(new Dictionary<string, StringValues> { { "ipv4", new StringValues("1.2.3.4") }, { "ipv6", new StringValues("::1") }, { "fqdn", new StringValues("www.foo.bar") }, { "ds", new StringValues("0") } });
            var request = new DnsParameters(headers, query);

            var update = await sut.ConvertAsync(request);

            Assert.Equal("1.2.3.4", update.V4);
            Assert.Equal("::1", update.V6);
            Assert.Equal("www.foo.bar", update.Domain);
            Assert.Equal("foo", update.Client);
            Assert.Equal("bar", update.Secret);
        }
    }
}
