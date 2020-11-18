using FluentValidation;
using IP.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Security;
using System.Text;
using System.Threading.Tasks;

namespace IP.Services
{
    public class DnsPayloadService : IDnsPayloadService
    {
        private const string domainParameter = "fqdn";

        private const string dualStackParameter = "ds";

        private const string ipv4Parameter = "ipv4";

        private const string ipv6Parameter = "ipv6";

        private const string authorizationHeader = "Authorization";

        private readonly IValidator<DnsPayload> validator;

        private readonly ILogger<DnsPayloadService> logger;

        public DnsPayloadService(IValidator<DnsPayload> validator, ILoggerFactory factory)
        {
            this.validator = validator;
            logger = factory.CreateLogger<DnsPayloadService>();
        }

        public async Task<DnsPayload> ConvertAsync(DnsParameters request)
        {
            request.Query.TryGetValue(domainParameter, out var domain);
            request.Query.TryGetValue(dualStackParameter, out var dualStack);
            request.Query.TryGetValue(ipv4Parameter, out var v4);
            request.Query.TryGetValue(ipv6Parameter, out var v6);

            logger.LogInformation("Converting DNS parameters: IPv4 ({0}), IPv6 ({1}), Domain ({2}), DualStack ({3})", v4, v6, domain, dualStack);

            request.Headers.TryGetValue(authorizationHeader, out var authorizationHeaderValue);

            if (string.IsNullOrEmpty(authorizationHeaderValue))
            {
                throw new UnauthorizedAccessException("Authorization either not provided or violates security requirements");
            }

            if (!authorizationHeaderValue.StartsWith("Basic"))
            {
                logger.LogError("Unsupported authorization scheme: {0}", authorizationHeaderValue);
                throw new NotSupportedException("Only basic authorization supported");
            }

            string[] authorization;

            try
            {
                authorization = Encoding.ASCII.GetString(Convert.FromBase64String(authorizationHeaderValue[6..])).Split(':');
            }
            catch (FormatException e)
            {
                logger.LogError(e, "Base64 decoding error: {0}", authorizationHeaderValue);
                throw new SecurityException("Authorization header is not base64 encoded");
            }

            if (authorization.Length != 2)
            {
                logger.LogError("Malformed authorization header: {0}", authorizationHeaderValue);
                throw new SecurityException("Authorization header is malformed");
            }

            var client = authorization[0];
            var secret = authorization[1];

            var update = new DnsPayload { V4 = v4, V6 = v6, Domain = domain, Client = client, Secret = secret, IsDualStack = dualStack?.Equals("1") == true };

            await validator.ValidateAndThrowAsync(update);

            return update;
        }
    }
}
