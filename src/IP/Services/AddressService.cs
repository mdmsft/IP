using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;

namespace IP.Services
{
    public sealed class AddressService : IAddressService
    {
        public string GetAddress(HttpContext context, AddressFamily family)
        {
            var address = FindAddressInHeaders(context.Request.Headers) ?? context.Connection.RemoteIpAddress;
            return address?.AddressFamily == family ? address.ToString() : default;
        }

        private static IPAddress FindAddressInHeaders(IHeaderDictionary headers)
        {
            const string headerName = "X-Forwarded-For";

            if (!headers.TryGetValue(headerName, out var headerValue) || headerValue == StringValues.Empty)
            {
                return default;
            }
            
            return headerValue
                .SelectMany(value => value.Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(ip => ip.Split(':')[0].Trim()))
                .Where(ip => IPAddress.TryParse(ip, out var _))
                .Select(IPAddress.Parse)
                .FirstOrDefault();
        }
    }
}
