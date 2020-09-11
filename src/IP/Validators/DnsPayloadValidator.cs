using FluentValidation;
using IP.Models;
using IP.Services;
using Microsoft.Extensions.Configuration;
using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace IP.Validators
{
    public sealed class DnsPayloadValidator : AbstractValidator<DnsPayload>
    {
        private readonly IDnsZoneService zoneService;
        private readonly IConfiguration configuration;

        public DnsPayloadValidator(IDnsZoneService zoneService, IConfiguration configuration)
        {
            this.zoneService = zoneService;
            this.configuration = configuration;
            RuleFor(x => x.V4)
                .NotEmpty()
                .NotNull()
                .When(x => string.IsNullOrEmpty(x.V6?.Trim()));

            RuleFor(x => x.V6)
                .NotEmpty()
                .NotNull()
                .When(x => string.IsNullOrEmpty(x.V4?.Trim()));

            RuleFor(x => x.V4)
                .Must(x => IPAddress.TryParse(x, out var _))
                .When(x => !string.IsNullOrEmpty(x.V4?.Trim()));

            RuleFor(x => x.V6)
                .Must(x => IPAddress.TryParse(x, out var _))
                .When(x => !string.IsNullOrEmpty(x.V6?.Trim()));

            RuleFor(x => x.Client)
                .NotNull()
                .NotEmpty();

            RuleFor(x => x.Client)
                .Must(CheckClient)
                .When(x => !string.IsNullOrEmpty(x?.Client));

            RuleFor(x => x.Secret)
                .NotNull()
                .NotEmpty();

            RuleFor(x => x.Secret)
                .Must(CheckSecret)
                .When(x => !string.IsNullOrEmpty(x?.Secret));

            RuleFor(x => x.Domain)
                .NotNull()
                .NotEmpty();

            RuleFor(x => x.Domain)
                .MustAsync(CheckDnsZoneAsync)
                .When(x => !string.IsNullOrEmpty(x?.Domain));
        }

        private async Task<bool> CheckDnsZoneAsync(DnsPayload instance, string domain, CancellationToken token = default)
        {
            var zone = await zoneService.GetZoneAsync(instance.ZoneName);
            return zone != null;
        }

        private bool CheckClient(DnsPayload instance, string client) =>
            client.Equals(configuration.GetValue<string>(nameof(DnsPayload.Client)), StringComparison.Ordinal);

        private bool CheckSecret(DnsPayload instance, string secret) =>
            secret.Equals(configuration.GetValue<string>(nameof(DnsPayload.Secret)), StringComparison.Ordinal);
    }
}
