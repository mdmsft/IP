using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using System.Linq;
using System.Net.Sockets;
using System;
using System.Threading.Tasks;
using IP.Services;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using IP.Models;
using Microsoft.Extensions.Logging;

namespace IP
{
    public class Functions
    {
        private readonly IDnsManagementService managementService;
        private readonly IDnsPayloadService payloadService;
        private readonly IAddressService addressService;
        private readonly ILogger<Functions> logger;

        public Functions(IDnsManagementService managementService, IDnsPayloadService payloadService, IAddressService addressService, ILogger<Functions> logger)
        {
            this.managementService = managementService;
            this.payloadService = payloadService;
            this.addressService = addressService;
            this.logger = logger;
        }

        [FunctionName(nameof(Dns))]
        public async Task<IActionResult> Dns(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "dns")] HttpRequest req,
            [DurableClient] IDurableClient client)
        {
            logger.LogInformation("Headers:{0}{1}", Environment.NewLine, string.Join(Environment.NewLine, req.Headers.Select(header => $"{header.Key}: {header.Value}")));
            logger.LogInformation("Query:{0}{1}", Environment.NewLine, string.Join(Environment.NewLine, req.Query.Select(query => $"{query.Key}={query.Value}")));

            var input = new DnsParameters(req.Headers, req.Query);
            string instanceId = await client.StartNewAsync(nameof(Orchestrator), input);
            var payload = client.CreateHttpManagementPayload(instanceId);
            return new JsonResult(payload);
        }

        [FunctionName(nameof(Orchestrator))]
        public async Task Orchestrator([OrchestrationTrigger] IDurableOrchestrationContext context)
        {
            var parameters = context.GetInput<DnsParameters>();
            var payload = await context.CallActivityAsync<DnsPayload>(nameof(Converter), parameters);
            await context.CallActivityAsync(nameof(Manager), payload);
        }

        [FunctionName(nameof(Converter))]
        public async Task<DnsPayload> Converter([ActivityTrigger] DnsParameters parameters)
        {
            return await payloadService.ConvertAsync(parameters);
        }

        [FunctionName(nameof(Manager))]
        public async Task Manager([ActivityTrigger] DnsPayload payload)
        {
            await managementService.ProcessAsync(payload);
        }

        [FunctionName(nameof(V4))]
        public IActionResult V4(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "4")] HttpRequest req)
        {
            var ip = addressService.GetAddress(req.HttpContext, AddressFamily.InterNetwork);
            if (ip is null)
            {
                return new NoContentResult();
            }

            return new OkObjectResult(ip);
        }

        [FunctionName(nameof(V6))]
        public IActionResult V6(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "6")] HttpRequest req)
        {
            var ip = addressService.GetAddress(req.HttpContext, AddressFamily.InterNetworkV6);
            if (ip is null)
            {
                return new NoContentResult();
            }

            return new OkObjectResult(ip);
        }
    }
}
