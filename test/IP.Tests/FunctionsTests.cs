using FakeItEasy;
using IP.Models;
using IP.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using System.Net.Sockets;
using System.Threading.Tasks;
using Xunit;

namespace IP.Tests
{
    public class FunctionsTests
    {
        private readonly IDnsManagementService managementService;
        private readonly IDnsPayloadService payloadService;
        private readonly IAddressService addressService;
        private readonly IHttpContextAccessor httpContextAccessor;
        private readonly IDurableOrchestrationContext context;
        private readonly IDurableClient client;

        private readonly Functions sut;

        public FunctionsTests()
        {
            addressService = A.Fake<IAddressService>();
            payloadService = A.Fake<IDnsPayloadService>();
            managementService = A.Fake<IDnsManagementService>();

            client = A.Fake<IDurableClient>();
            context = A.Fake<IDurableOrchestrationContext>();
            httpContextAccessor = A.Fake<IHttpContextAccessor>();

            sut = new Functions(managementService, payloadService, addressService, A.Fake<ILogger<Functions>>());
        }

        [Fact]
        public void WhenV4AndNoAddresseturnsNoContent()
        {
            A.CallTo(() => addressService.GetAddress(A<HttpContext>._, AddressFamily.InterNetwork)).Returns(null);

            Assert.IsType<NoContentResult>(sut.V4(httpContextAccessor.HttpContext.Request));
        }

        [Fact]
        public void WhenV4AndAddresseturnsOkResult()
        {
            A.CallTo(() => addressService.GetAddress(A<HttpContext>._, AddressFamily.InterNetwork)).Returns("1.2.3.4");

            var okObjectResult = Assert.IsType<OkObjectResult>(sut.V4(httpContextAccessor.HttpContext.Request));
            Assert.Equal("1.2.3.4", okObjectResult.Value);
        }

        [Fact]
        public void WhenV6AndNoAddresseturnsNoContent()
        {
            A.CallTo(() => addressService.GetAddress(A<HttpContext>._, AddressFamily.InterNetworkV6)).Returns(null);

            Assert.IsType<NoContentResult>(sut.V6(httpContextAccessor.HttpContext.Request));
        }

        [Fact]
        public void WhenV6AndAddresseturnsOkResult()
        {
            A.CallTo(() => addressService.GetAddress(A<HttpContext>._, AddressFamily.InterNetworkV6)).Returns("2a02::");

            var okObjectResult = Assert.IsType<OkObjectResult>(sut.V6(httpContextAccessor.HttpContext.Request));
            Assert.Equal("2a02::", okObjectResult.Value);
        }

        [Fact]
        public async Task WhenManagerCallsManagementService()
        {
            var payload = new DnsPayload();

            await sut.Manager(payload);

            A.CallTo(() => managementService.ProcessAsync(A<DnsPayload>.That.IsSameAs(payload))).MustHaveHappenedOnceExactly();
        }

        [Fact]
        public async Task WhenConverterCallsPayloadService()
        {
            var parameters = new DnsParameters();
            A.CallTo(() => payloadService.ConvertAsync(parameters)).Returns(new DnsPayload { V4 = "v4", V6 = "v6" });

            var payload = await sut.Converter(parameters);

            Assert.Equal("v4", payload.V4);
            Assert.Equal("v6", payload.V6);
        }

        [Fact]
        public async Task WhenOrchestratorCallsConverterThenManager()
        {
            var parameters = new DnsParameters();
            var payload = new DnsPayload();

            A.CallTo(() => context.GetInput<DnsParameters>()).Returns(parameters);
            A.CallTo(() => context.CallActivityAsync<DnsPayload>("Converter", parameters)).Returns(payload);

            await sut.Orchestrator(context);

            A.CallTo(() => context.CallActivityAsync("Manager", A<DnsPayload>.That.IsSameAs(payload))).MustHaveHappenedOnceExactly();
        }

        [Fact]
        public async Task WhenDnsStartsNewOrchestratorThenCreatesPayloadAndReturnsJson()
        {
            var payload = new DnsPayload();

            A.CallTo(() => client.StartNewAsync("Orchestrator", payload)).Returns("instance");
            A.CallTo(() => client.CreateHttpManagementPayload("instance")).Returns(new HttpManagementPayload());

            var result = await sut.Dns(httpContextAccessor.HttpContext.Request, client);

            var jsonResult = Assert.IsType<JsonResult>(result);
            Assert.IsAssignableFrom<HttpManagementPayload>(jsonResult.Value);
        }
    }
}
