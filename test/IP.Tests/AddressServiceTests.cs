using IP.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using System.Net;
using System.Net.Sockets;
using Xunit;

namespace IP.Tests
{
    public class AddressServiceTests
    {
        private readonly IAddressService sut = new AddressService();

        [Fact]
        public void WhenV4WithoutForwardedForHeaderAndRemoteAddressReturnsNoContent()
        {
            var context = new DefaultHttpContext();
            context.Request.Headers.Clear();
            context.Connection.RemoteIpAddress = default;

            Assert.Equal(default, sut.GetAddress(context, AddressFamily.InterNetwork));
        }

        [Fact]
        public void WhenV4WithoutForwardedForHeaderAndV6RemoteAddressReturnsNoContent()
        {
            var context = new DefaultHttpContext();
            context.Request.Headers.Clear();
            context.Connection.RemoteIpAddress = IPAddress.IPv6Loopback;

            Assert.Equal(default, sut.GetAddress(context, AddressFamily.InterNetwork));
        }

        [Fact]
        public void WhenV4WithoutForwardedForHeaderReturnsRemoteAddress()
        {
            var context = new DefaultHttpContext();
            context.Request.Headers.Clear();
            context.Connection.RemoteIpAddress = IPAddress.Loopback;

            Assert.Equal(IPAddress.Loopback, IPAddress.Parse(sut.GetAddress(context, AddressFamily.InterNetwork)));
        }

        [Fact]
        public void WhenV4WithEmptyForwardedForHeaderReturnsRemoteAddress()
        {
            var context = new DefaultHttpContext();
            context.Request.Headers.Clear();
            context.Request.Headers.Add("X-Forwarded-For", string.Empty);
            context.Connection.RemoteIpAddress = IPAddress.Loopback;

            Assert.Equal(IPAddress.Loopback, IPAddress.Parse(sut.GetAddress(context, AddressFamily.InterNetwork)));
        }

        [Fact]
        public void WhenV4WithCommaSeparatedEmptyForwardedForHeaderReturnsRemoteAddress()
        {
            var context = new DefaultHttpContext();
            context.Request.Headers.Clear();
            context.Request.Headers.Add("X-Forwarded-For", ",");
            context.Connection.RemoteIpAddress = IPAddress.Loopback;

            Assert.Equal(IPAddress.Loopback, IPAddress.Parse(sut.GetAddress(context, AddressFamily.InterNetwork)));
        }

        [Fact]
        public void WhenV4WithInvalidForwardedForHeaderReturnsRemoteAddress()
        {
            var context = new DefaultHttpContext();
            context.Request.Headers.Clear();
            context.Request.Headers.Add("X-Forwarded-For", "a.b.c.d");
            context.Connection.RemoteIpAddress = IPAddress.Loopback;

            Assert.Equal(IPAddress.Loopback, IPAddress.Parse(sut.GetAddress(context, AddressFamily.InterNetwork)));
        }

        [Fact]
        public void WhenV4WithMultipleInvalidForwardedForHeaderReturnsRemoteAddress()
        {
            var context = new DefaultHttpContext();
            context.Request.Headers.Clear();
            context.Request.Headers.Add("X-Forwarded-For", "a.b.c.d, e.f.g.h");
            context.Connection.RemoteIpAddress = IPAddress.Loopback;

            Assert.Equal(IPAddress.Loopback, IPAddress.Parse(sut.GetAddress(context, AddressFamily.InterNetwork)));
        }

        [Fact]
        public void WhenV4WithValidAndInvalidForwardedForHeaderReturnsValidOne()
        {
            var context = new DefaultHttpContext();
            context.Request.Headers.Clear();
            context.Request.Headers.Add("X-Forwarded-For", "a.b.c.d, 1.2.3.4");
            context.Connection.RemoteIpAddress = IPAddress.Loopback;

            Assert.Equal("1.2.3.4", sut.GetAddress(context, AddressFamily.InterNetwork));
        }

        [Fact]
        public void WhenV4WithTwoValidForwardedForHeaderReturnsFirstOne()
        {
            var context = new DefaultHttpContext();
            context.Request.Headers.Clear();
            context.Request.Headers.Add("X-Forwarded-For", "1.2.3.4, 5.6.7.8");
            context.Connection.RemoteIpAddress = IPAddress.Loopback;

            Assert.Equal("1.2.3.4", sut.GetAddress(context, AddressFamily.InterNetwork));
        }

        [Fact]
        public void WhenV4WithOneValidForwardedForHeaderReturnsRemoteAddress()
        {
            var context = new DefaultHttpContext();
            context.Request.Headers.Clear();
            context.Request.Headers.Add("X-Forwarded-For", "1.2.3.4");
            context.Connection.RemoteIpAddress = IPAddress.Loopback;

            Assert.Equal("1.2.3.4", sut.GetAddress(context, AddressFamily.InterNetwork));
        }

        [Fact]
        public void WhenV4WithV6ForwardedForHeaderReturnsRemoteAddress()
        {
            var context = new DefaultHttpContext();
            context.Request.Headers.Clear();
            context.Request.Headers.Add("X-Forwarded-For", IPAddress.IPv6Loopback.ToString());
            context.Connection.RemoteIpAddress = IPAddress.Loopback;

            Assert.Equal(IPAddress.Loopback, IPAddress.Parse(sut.GetAddress(context, AddressFamily.InterNetwork)));
        }

        [Fact]
        public void WhenV4WithV4AndV6ForwardedForHeaderReturnsV4()
        {
            var context = new DefaultHttpContext();
            context.Request.Headers.Clear();
            context.Request.Headers.Add("X-Forwarded-For", new StringValues(new[] { IPAddress.IPv6Loopback.ToString(), "1.2.3.4" }));
            context.Connection.RemoteIpAddress = IPAddress.Loopback;

            Assert.Equal("1.2.3.4", sut.GetAddress(context, AddressFamily.InterNetwork));
        }

        [Fact]
        public void WhenV4WithV4AndPortsReturnsV4()
        {
            var context = new DefaultHttpContext();
            context.Request.Headers.Clear();
            context.Request.Headers.Add("X-Forwarded-For", new StringValues(new[] { "a.b.c.d:80", "1.2.3.4:443" }));
            context.Connection.RemoteIpAddress = IPAddress.Loopback;

            Assert.Equal("1.2.3.4", sut.GetAddress(context, AddressFamily.InterNetwork));
        }

        [Fact]
        public void WhenV6WithoutForwardedForHeaderAndRemoteAddressReturnsNoContent()
        {
            var context = new DefaultHttpContext();
            context.Request.Headers.Clear();
            context.Connection.RemoteIpAddress = default;

            Assert.Equal(default, sut.GetAddress(context, AddressFamily.InterNetworkV6));
        }

        [Fact]
        public void WhenV6WithoutForwardedForHeaderAndV4RemoteAddressReturnsNoContent()
        {
            var context = new DefaultHttpContext();
            context.Request.Headers.Clear();
            context.Connection.RemoteIpAddress = IPAddress.Loopback;

            Assert.Equal(default, sut.GetAddress(context, AddressFamily.InterNetworkV6));
        }

        [Fact]
        public void WhenV6WithoutForwardedForHeaderReturnsRemoteAddress()
        {
            var context = new DefaultHttpContext();
            context.Request.Headers.Clear();
            context.Connection.RemoteIpAddress = IPAddress.IPv6Loopback;

            Assert.Equal(IPAddress.IPv6Loopback, IPAddress.Parse(sut.GetAddress(context, AddressFamily.InterNetworkV6)));
        }

        [Fact]
        public void WhenV6WithEmptyForwardedForHeaderReturnsRemoteAddress()
        {
            var context = new DefaultHttpContext();
            context.Request.Headers.Clear();
            context.Request.Headers.Add("X-Forwarded-For", string.Empty);
            context.Connection.RemoteIpAddress = IPAddress.IPv6Loopback;

            Assert.Equal(IPAddress.IPv6Loopback, IPAddress.Parse(sut.GetAddress(context, AddressFamily.InterNetworkV6)));
        }

        [Fact]
        public void WhenV6WithCommaSeparatedEmptyForwardedForHeaderReturnsRemoteAddress()
        {
            var context = new DefaultHttpContext();
            context.Request.Headers.Clear();
            context.Request.Headers.Add("X-Forwarded-For", ",");
            context.Connection.RemoteIpAddress = IPAddress.IPv6Loopback;

            Assert.Equal(IPAddress.IPv6Loopback, IPAddress.Parse(sut.GetAddress(context, AddressFamily.InterNetworkV6)));
        }

        [Fact]
        public void WhenV6WithInvalidForwardedForHeaderReturnsRemoteAddress()
        {
            var context = new DefaultHttpContext();
            context.Request.Headers.Clear();
            context.Request.Headers.Add("X-Forwarded-For", "a.b.c.d");
            context.Connection.RemoteIpAddress = IPAddress.IPv6Loopback;

            Assert.Equal(IPAddress.IPv6Loopback, IPAddress.Parse(sut.GetAddress(context, AddressFamily.InterNetworkV6)));
        }

        [Fact]
        public void WhenV6WithMultipleInvalidForwardedForHeaderReturnsRemoteAddress()
        {
            var context = new DefaultHttpContext();
            context.Request.Headers.Clear();
            context.Request.Headers.Add("X-Forwarded-For", "a.b.c.d, e.f.g.h");
            context.Connection.RemoteIpAddress = IPAddress.IPv6Loopback;

            Assert.Equal(IPAddress.IPv6Loopback, IPAddress.Parse(sut.GetAddress(context, AddressFamily.InterNetworkV6)));
        }
    }
}
