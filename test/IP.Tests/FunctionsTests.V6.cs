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
        public void WhenV6WithoutForwardedForHeaderAndRemoteAddressReturnsNoContent()
        {
            var context = new DefaultHttpContext();
            context.Request.Headers.Clear();
            context.Request.HttpContext.Connection.RemoteIpAddress = default;

            Assert.IsType<NoContentResult>(Functions.V6(context.Request));
        }

        [Fact]
        public void WhenV6WithoutForwardedForHeaderAndV4RemoteAddressReturnsNoContent()
        {
            var context = new DefaultHttpContext();
            context.Request.Headers.Clear();
            context.Request.HttpContext.Connection.RemoteIpAddress = IPAddress.Loopback;

            Assert.IsType<NoContentResult>(Functions.V6(context.Request));
        }

        [Fact]
        public void WhenV6WithoutForwardedForHeaderReturnsRemoteAddress()
        {
            var context = new DefaultHttpContext();
            context.Request.Headers.Clear();
            context.Request.HttpContext.Connection.RemoteIpAddress = IPAddress.IPv6Loopback;

            var okObjectResult = Assert.IsType<OkObjectResult>(Functions.V6(context.Request));
            var value = Assert.IsType<string>(okObjectResult.Value);
            Assert.Equal(IPAddress.IPv6Loopback, IPAddress.Parse(value));
        }

        [Fact]
        public void WhenV6WithEmptyForwardedForHeaderReturnsRemoteAddress()
        {
            var context = new DefaultHttpContext();
            context.Request.Headers.Clear();
            context.Request.Headers.Add("X-Forwarded-For", string.Empty);
            context.Request.HttpContext.Connection.RemoteIpAddress = IPAddress.IPv6Loopback;

            var okObjectResult = Assert.IsType<OkObjectResult>(Functions.V6(context.Request));
            var value = Assert.IsType<string>(okObjectResult.Value);
            Assert.Equal(IPAddress.IPv6Loopback, IPAddress.Parse(value));
        }

        [Fact]
        public void WhenV6WithCommaSeparatedEmptyForwardedForHeaderReturnsRemoteAddress()
        {
            var context = new DefaultHttpContext();
            context.Request.Headers.Clear();
            context.Request.Headers.Add("X-Forwarded-For", ",");
            context.Request.HttpContext.Connection.RemoteIpAddress = IPAddress.IPv6Loopback;

            var okObjectResult = Assert.IsType<OkObjectResult>(Functions.V6(context.Request));
            var value = Assert.IsType<string>(okObjectResult.Value);
            Assert.Equal(IPAddress.IPv6Loopback, IPAddress.Parse(value));
        }

        [Fact]
        public void WhenV6WithInvalidForwardedForHeaderReturnsRemoteAddress()
        {
            var context = new DefaultHttpContext();
            context.Request.Headers.Clear();
            context.Request.Headers.Add("X-Forwarded-For", "a.b.c.d");
            context.Request.HttpContext.Connection.RemoteIpAddress = IPAddress.IPv6Loopback;

            var okObjectResult = Assert.IsType<OkObjectResult>(Functions.V6(context.Request));
            var value = Assert.IsType<string>(okObjectResult.Value);
            Assert.Equal(IPAddress.IPv6Loopback, IPAddress.Parse(value));
        }

        [Fact]
        public void WhenV6WithMultipleInvalidForwardedForHeaderReturnsRemoteAddress()
        {
            var context = new DefaultHttpContext();
            context.Request.Headers.Clear();
            context.Request.Headers.Add("X-Forwarded-For", "a.b.c.d, e.f.g.h");
            context.Request.HttpContext.Connection.RemoteIpAddress = IPAddress.IPv6Loopback;

            var okObjectResult = Assert.IsType<OkObjectResult>(Functions.V6(context.Request));
            var value = Assert.IsType<string>(okObjectResult.Value);
            Assert.Equal(IPAddress.IPv6Loopback, IPAddress.Parse(value));
        }

        [Fact]
        public void WhenV6WithValidAndInvalidForwardedForHeaderReturnsValidOne()
        {
            var context = new DefaultHttpContext();
            context.Request.Headers.Clear();
            context.Request.Headers.Add("X-Forwarded-For", $"a.b.c.d, {IPAddress.IPv6Loopback}");
            context.Request.HttpContext.Connection.RemoteIpAddress = IPAddress.IPv6None;

            var okObjectResult = Assert.IsType<OkObjectResult>(Functions.V6(context.Request));
            var value = Assert.IsType<string>(okObjectResult.Value);
            Assert.Equal(IPAddress.IPv6Loopback, IPAddress.Parse(value));
        }

        [Fact]
        public void WhenV6WithTwoValidForwardedForHeaderReturnsFirstOne()
        {
            var context = new DefaultHttpContext();
            context.Request.Headers.Clear();
            context.Request.Headers.Add("X-Forwarded-For", $"{IPAddress.IPv6Any}, {IPAddress.IPv6None}");
            context.Request.HttpContext.Connection.RemoteIpAddress = IPAddress.IPv6Loopback;

            var okObjectResult = Assert.IsType<OkObjectResult>(Functions.V6(context.Request));
            var value = Assert.IsType<string>(okObjectResult.Value);
            Assert.Equal(IPAddress.IPv6None, IPAddress.Parse(value));
        }

        [Fact]
        public void WhenV6WithOneValidForwardedForHeaderReturnsRemoteAddress()
        {
            var context = new DefaultHttpContext();
            context.Request.Headers.Clear();
            context.Request.Headers.Add("X-Forwarded-For", IPAddress.IPv6Any.ToString());
            context.Request.HttpContext.Connection.RemoteIpAddress = IPAddress.IPv6Loopback;

            var okObjectResult = Assert.IsType<OkObjectResult>(Functions.V6(context.Request));
            var value = Assert.IsType<string>(okObjectResult.Value);
            Assert.Equal(IPAddress.IPv6Any, IPAddress.Parse(value));
        }

        [Fact]
        public void WhenV6WithV4ForwardedForHeaderReturnsRemoteAddress()
        {
            var context = new DefaultHttpContext();
            context.Request.Headers.Clear();
            context.Request.Headers.Add("X-Forwarded-For", IPAddress.Loopback.ToString());
            context.Request.HttpContext.Connection.RemoteIpAddress = IPAddress.IPv6Loopback;

            var okObjectResult = Assert.IsType<OkObjectResult>(Functions.V6(context.Request));
            var value = Assert.IsType<string>(okObjectResult.Value);
            Assert.Equal(IPAddress.IPv6Loopback, IPAddress.Parse(value));
        }

        [Fact]
        public void WhenV6WithV6AndV4ForwardedForHeaderReturnsV4()
        {
            var context = new DefaultHttpContext();
            context.Request.Headers.Clear();
            context.Request.Headers.Add("X-Forwarded-For", new StringValues(new[] { IPAddress.IPv6Any.ToString(), "1.2.3.4" }));
            context.Request.HttpContext.Connection.RemoteIpAddress = IPAddress.IPv6Loopback;

            var okObjectResult = Assert.IsType<OkObjectResult>(Functions.V6(context.Request));
            var value = Assert.IsType<string>(okObjectResult.Value);
            Assert.Equal(IPAddress.IPv6Any, IPAddress.Parse(value));
        }
    }
}
