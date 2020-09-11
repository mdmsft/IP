using FluentValidation;
using IP.Models;
using IP.Services;
using IP.Settings;
using IP.Validators;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Azure.Management.Dns;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Rest.Azure.Authentication;
using System;

[assembly: FunctionsStartup(typeof(IP.Startup))]

namespace IP
{
    public class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            builder.Services.AddSingleton<IDnsManagementService, DnsManagementService>();
            builder.Services.AddSingleton<IDnsPayloadService, DnsPayloadService>();
            builder.Services.AddSingleton<IDnsZoneService, DnsZoneService>();
            builder.Services.AddSingleton<IDnsRecordSetService, DnsRecordSetService>();
            builder.Services.AddSingleton<IValidator<DnsPayload>, DnsPayloadValidator>();
            builder.Services.AddSingleton(GetDnsManagementClient);
        }

        private static IDnsManagementClient GetDnsManagementClient(IServiceProvider provider)
        {
            var configuration = provider.GetRequiredService<IConfiguration>();

            var subscription = configuration.GetValue<string>(ConfigurationSettings.Subscription);
            var tenant = configuration.GetValue<string>(ConfigurationSettings.Tenant);
            var client = configuration.GetValue<string>(ConfigurationSettings.Client);
            var secret = configuration.GetValue<string>(ConfigurationSettings.Secret);

            var credentials = ApplicationTokenProvider.LoginSilentAsync(tenant, client, secret).GetAwaiter().GetResult();

            return new DnsManagementClient(credentials) { SubscriptionId = subscription };
        }
    }
}
