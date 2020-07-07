using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using System.Net;
using Xunit;

namespace IP.Tests
{
    public partial class FunctionsTests
    {
        [Fact]
        public void WhenV4WithoutForwardedForHeaderAndRemoteAddressReturnsNoContent()
        {
            var context = new DefaultHttpContext();
            context.Request.Headers.Clear();
            context.Request.HttpContext.Connection.RemoteIpAddress = default;

            Assert.IsType<NoContentResult>(Functions.V4(context.Request));
        }

        [Fact]
        public void WhenV4WithoutForwardedForHeaderAndV6RemoteAddressReturnsNoContent()
        {
            var context = new DefaultHttpContext();
            context.Request.Headers.Clear();
            context.Request.HttpContext.Connection.RemoteIpAddress = IPAddress.IPv6Loopback;

            Assert.IsType<NoContentResult>(Functions.V4(context.Request));
        }

        [Fact]
        public void WhenV4WithoutForwardedForHeaderReturnsRemoteAddress()
        {
            var context = new DefaultHttpContext();
            context.Request.Headers.Clear();
            context.Request.HttpContext.Connection.RemoteIpAddress = IPAddress.Loopback;

            var okObjectResult = Assert.IsType<OkObjectResult>(Functions.V4(context.Request));
            var value = Assert.IsType<string>(okObjectResult.Value);
            Assert.Equal(IPAddress.Loopback, IPAddress.Parse(value));
        }

        [Fact]
        public void WhenV4WithEmptyForwardedForHeaderReturnsRemoteAddress()
        {
            var context = new DefaultHttpContext();
            context.Request.Headers.Clear();
            context.Request.Headers.Add("X-Forwarded-For", string.Empty);
            context.Request.HttpContext.Connection.RemoteIpAddress = IPAddress.Loopback;

            var okObjectResult = Assert.IsType<OkObjectResult>(Functions.V4(context.Request));
            var value = Assert.IsType<string>(okObjectResult.Value);
            Assert.Equal(IPAddress.Loopback, IPAddress.Parse(value));
        }

        [Fact]
        public void WhenV4WithCommaSeparatedEmptyForwardedForHeaderReturnsRemoteAddress()
        {
            var context = new DefaultHttpContext();
            context.Request.Headers.Clear();
            context.Request.Headers.Add("X-Forwarded-For", ",");
            context.Request.HttpContext.Connection.RemoteIpAddress = IPAddress.Loopback;

            var okObjectResult = Assert.IsType<OkObjectResult>(Functions.V4(context.Request));
            var value = Assert.IsType<string>(okObjectResult.Value);
            Assert.Equal(IPAddress.Loopback, IPAddress.Parse(value));
        }

        [Fact]
        public void WhenV4WithInvalidForwardedForHeaderReturnsRemoteAddress()
        {
            var context = new DefaultHttpContext();
            context.Request.Headers.Clear();
            context.Request.Headers.Add("X-Forwarded-For", "a.b.c.d");
            context.Request.HttpContext.Connection.RemoteIpAddress = IPAddress.Loopback;

            var okObjectResult = Assert.IsType<OkObjectResult>(Functions.V4(context.Request));
            var value = Assert.IsType<string>(okObjectResult.Value);
            Assert.Equal(IPAddress.Loopback, IPAddress.Parse(value));
        }

        [Fact]
        public void WhenV4WithMultipleInvalidForwardedForHeaderReturnsRemoteAddress()
        {
            var context = new DefaultHttpContext();
            context.Request.Headers.Clear();
            context.Request.Headers.Add("X-Forwarded-For", "a.b.c.d, e.f.g.h");
            context.Request.HttpContext.Connection.RemoteIpAddress = IPAddress.Loopback;

            var okObjectResult = Assert.IsType<OkObjectResult>(Functions.V4(context.Request));
            var value = Assert.IsType<string>(okObjectResult.Value);
            Assert.Equal(IPAddress.Loopback, IPAddress.Parse(value));
        }

        [Fact]
        public void WhenV4WithValidAndInvalidForwardedForHeaderReturnsValidOne()
        {
            var context = new DefaultHttpContext();
            context.Request.Headers.Clear();
            context.Request.Headers.Add("X-Forwarded-For", "a.b.c.d, 1.2.3.4");
            context.Request.HttpContext.Connection.RemoteIpAddress = IPAddress.Loopback;

            var okObjectResult = Assert.IsType<OkObjectResult>(Functions.V4(context.Request));
            var value = Assert.IsType<string>(okObjectResult.Value);
            Assert.Equal("1.2.3.4", value);
        }

        [Fact]
        public void WhenV4WithTwoValidForwardedForHeaderReturnsFirstOne()
        {
            var context = new DefaultHttpContext();
            context.Request.Headers.Clear();
            context.Request.Headers.Add("X-Forwarded-For", "1.2.3.4, 5.6.7.8");
            context.Request.HttpContext.Connection.RemoteIpAddress = IPAddress.Loopback;

            var okObjectResult = Assert.IsType<OkObjectResult>(Functions.V4(context.Request));
            var value = Assert.IsType<string>(okObjectResult.Value);
            Assert.Equal("1.2.3.4", value);
        }

        [Fact]
        public void WhenV4WithOneValidForwardedForHeaderReturnsRemoteAddress()
        {
            var context = new DefaultHttpContext();
            context.Request.Headers.Clear();
            context.Request.Headers.Add("X-Forwarded-For", "1.2.3.4");
            context.Request.HttpContext.Connection.RemoteIpAddress = IPAddress.Loopback;

            var okObjectResult = Assert.IsType<OkObjectResult>(Functions.V4(context.Request));
            var value = Assert.IsType<string>(okObjectResult.Value);
            Assert.Equal("1.2.3.4", value);
        }

        [Fact]
        public void WhenV4WithV6ForwardedForHeaderReturnsRemoteAddress()
        {
            var context = new DefaultHttpContext();
            context.Request.Headers.Clear();
            context.Request.Headers.Add("X-Forwarded-For", IPAddress.IPv6Loopback.ToString());
            context.Request.HttpContext.Connection.RemoteIpAddress = IPAddress.Loopback;

            var okObjectResult = Assert.IsType<OkObjectResult>(Functions.V4(context.Request));
            var value = Assert.IsType<string>(okObjectResult.Value);
            Assert.Equal(IPAddress.Loopback, IPAddress.Parse(value));
        }

        [Fact]
        public void WhenV4WithV4AndV6ForwardedForHeaderReturnsV4()
        {
            var context = new DefaultHttpContext();
            context.Request.Headers.Clear();
            context.Request.Headers.Add("X-Forwarded-For", new StringValues(new[] { IPAddress.IPv6Loopback.ToString(), "1.2.3.4" }));
            context.Request.HttpContext.Connection.RemoteIpAddress = IPAddress.Loopback;

            var okObjectResult = Assert.IsType<OkObjectResult>(Functions.V4(context.Request));
            var value = Assert.IsType<string>(okObjectResult.Value);
            Assert.Equal("1.2.3.4", value);
        }
    }
}
