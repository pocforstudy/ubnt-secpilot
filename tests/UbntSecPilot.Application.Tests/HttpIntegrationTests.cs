using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using UbntSecPilot.Infrastructure.Services;
using Xunit;

namespace UbntSecPilot.Application.Tests
{
    public class HttpIntegrationTests
    {
        private sealed class MockHandler : HttpMessageHandler
        {
            private readonly Func<HttpRequestMessage, HttpResponseMessage> _responder;
            public MockHandler(Func<HttpRequestMessage, HttpResponseMessage> responder) => _responder = responder;
            protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
                => Task.FromResult(_responder(request));
        }

        [Fact]
        public async Task VirusTotalService_Search_Returns_Data()
        {
            var handler = new MockHandler(req =>
            {
                Assert.Contains("/search", req.RequestUri!.AbsoluteUri);
                var json = "{ \"data\": { \"type\": \"search\" } }";
                return new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(json, System.Text.Encoding.UTF8, "application/json")
                };
            });
            var http = new HttpClient(handler) { BaseAddress = new Uri("https://www.virustotal.com/") };
            var cfg = new ConfigurationBuilder().AddInMemoryCollection(new[]
            {
                new System.Collections.Generic.KeyValuePair<string,string>("VirusTotal:BaseUrl", "https://www.virustotal.com/api/v3"),
                new System.Collections.Generic.KeyValuePair<string,string>("VirusTotal:ApiKey", "test-key")
            }).Build();
            var svc = new VirusTotalService(http, cfg, NullLogger<VirusTotalService>.Instance);

            var resp = await svc.AnalyzeIndicatorAsync("1.2.3.4");
            Assert.NotNull(resp);
            Assert.True(resp.Count > 0);
        }

        [Fact]
        public async Task UdmProService_List_Rules_Returns_Array()
        {
            var handler = new MockHandler(req =>
            {
                Assert.Contains("/api/firewall/rules", req.RequestUri!.AbsoluteUri);
                var json = "[{ \"id\": \"r1\", \"action\": \"accept\" }]";
                return new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(json, System.Text.Encoding.UTF8, "application/json")
                };
            });
            var http = new HttpClient(handler) { BaseAddress = new Uri("http://udm-pro.local/") };
            var cfg = new ConfigurationBuilder().AddInMemoryCollection(new[]
            {
                new System.Collections.Generic.KeyValuePair<string,string>("UdmPro:BaseUrl", "http://udm-pro.local"),
                new System.Collections.Generic.KeyValuePair<string,string>("UdmPro:ApiKey", "test-key")
            }).Build();
            var svc = new UdmProService(http, cfg, NullLogger<UdmProService>.Instance);

            var rules = await svc.GetFirewallRulesAsync();
            Assert.NotNull(rules);
            Assert.Single(rules);
            Assert.True(rules[0].ContainsKey("id"));
        }
    }
}
