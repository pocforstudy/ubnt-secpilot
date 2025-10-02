using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using UbntSecPilot.Application.Services;

namespace UbntSecPilot.Infrastructure.Services
{
    public class UdmProService : IUdmProService
    {
        private readonly HttpClient _http;
        private readonly ILogger<UdmProService> _logger;
        private readonly string _baseUrl;
        private readonly string? _apiKey;

        public UdmProService(HttpClient http, IConfiguration config, ILogger<UdmProService> logger)
        {
            _http = http ?? throw new ArgumentNullException(nameof(http));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _baseUrl = config["UdmPro:BaseUrl"] ?? "http://localhost:8081";
            _apiKey = config["UdmPro:ApiKey"];
        }

        public async Task<List<Dictionary<string, object>>> GetFirewallRulesAsync()
        {
            using var req = new HttpRequestMessage(HttpMethod.Get, new Uri(new Uri(_baseUrl), "/api/firewall/rules"));
            AttachAuth(req);
            var resp = await _http.SendAsync(req).ConfigureAwait(false);
            resp.EnsureSuccessStatusCode();
            var data = await resp.Content.ReadFromJsonAsync<List<Dictionary<string, object>>>().ConfigureAwait(false);
            return data ?? new List<Dictionary<string, object>>();
        }

        public async Task<Dictionary<string, object>> CreateFirewallRuleAsync(Dictionary<string, object> ruleData)
        {
            using var req = new HttpRequestMessage(HttpMethod.Post, new Uri(new Uri(_baseUrl), "/api/firewall/rules"));
            AttachAuth(req);
            req.Content = JsonContent.Create(ruleData);
            var resp = await _http.SendAsync(req).ConfigureAwait(false);
            resp.EnsureSuccessStatusCode();
            return (await resp.Content.ReadFromJsonAsync<Dictionary<string, object>>().ConfigureAwait(false))!
                   ?? new Dictionary<string, object>();
        }

        public async Task<Dictionary<string, object>> UpdateFirewallRuleAsync(string ruleId, Dictionary<string, object> updates)
        {
            using var req = new HttpRequestMessage(HttpMethod.Put, new Uri(new Uri(_baseUrl), $"/api/firewall/rules/{ruleId}"));
            AttachAuth(req);
            req.Content = JsonContent.Create(updates);
            var resp = await _http.SendAsync(req).ConfigureAwait(false);
            resp.EnsureSuccessStatusCode();
            return (await resp.Content.ReadFromJsonAsync<Dictionary<string, object>>().ConfigureAwait(false))!
                   ?? new Dictionary<string, object>();
        }

        public async Task DeleteFirewallRuleAsync(string ruleId)
        {
            using var req = new HttpRequestMessage(HttpMethod.Delete, new Uri(new Uri(_baseUrl), $"/api/firewall/rules/{ruleId}"));
            AttachAuth(req);
            var resp = await _http.SendAsync(req).ConfigureAwait(false);
            resp.EnsureSuccessStatusCode();
        }

        private void AttachAuth(HttpRequestMessage req)
        {
            if (!string.IsNullOrWhiteSpace(_apiKey))
            {
                req.Headers.Add("X-API-Key", _apiKey);
            }
        }
    }
}
