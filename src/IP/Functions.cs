using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System;

namespace IP
{
    public static class Functions
    {
        [FunctionName(nameof(V4))]
        public static IActionResult V4(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "4")] HttpRequest req)
        {
            var ip = GetAddress(req, AddressFamily.InterNetwork);
            if (ip is null)
            {
                return new NoContentResult();
            }

            return new OkObjectResult(ip);
        }

        [FunctionName(nameof(V6))]
        public static IActionResult V6(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "6")] HttpRequest req)
        {
            var ip = GetAddress(req, AddressFamily.InterNetworkV6);
            if (ip is null)
            {
                return new NoContentResult();
            }

            return new OkObjectResult(ip);
        }

        private static string GetAddress(HttpRequest req, AddressFamily family)
        {
            var ips = ParseHeaders(req.Headers);
            var ip = ips?.FirstOrDefault(address => address.AddressFamily == family) ?? req.HttpContext.Connection.RemoteIpAddress;
            if (ip?.AddressFamily == family)
            {
                return ip.ToString();
            }

            return default;
        }

        private static IEnumerable<IPAddress> ParseHeaders(IHeaderDictionary headers)
        {
            const string headerName = "X-Forwarded-For";
            var ipRegex = new Regex("\\d+\\.\\d+\\.\\d+\\.\\d+");

            if (headers.TryGetValue(headerName, out var header))
            {
                foreach (var value in header)
                {
                    foreach (var address in value.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(v => v.Trim()))
                    {
                        if (ipRegex.IsMatch(address) && IPAddress.TryParse(ipRegex.Match(address).Value, out var ip))
                        {
                            yield return ip;
                        }

                        if (IPAddress.TryParse(address, out ip))
                        {
                            yield return ip;
                        }
                    }
                }
            }
        }
    }
}
