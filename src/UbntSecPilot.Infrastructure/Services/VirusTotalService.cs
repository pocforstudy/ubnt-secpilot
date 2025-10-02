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
    public class VirusTotalService : IVirusTotalService
    {
        private readonly HttpClient _http;
        private readonly ILogger<VirusTotalService> _logger;
        private readonly string _baseUrl;
        private readonly string? _apiKey;

        public VirusTotalService(HttpClient http, IConfiguration config, ILogger<VirusTotalService> logger)
        {
            _http = http ?? throw new ArgumentNullException(nameof(http));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _baseUrl = config["VirusTotal:BaseUrl"] ?? "https://www.virustotal.com/api/v3";
            _apiKey = config["VirusTotal:ApiKey"];
        }

        public async Task<Dictionary<string, object>> AnalyzeIndicatorAsync(string indicator)
        {
            if (string.IsNullOrWhiteSpace(indicator))
                throw new ArgumentException("indicator required", nameof(indicator));

            using var req = new HttpRequestMessage(HttpMethod.Get, new Uri(new Uri(_baseUrl), $"/search?query={Uri.EscapeDataString(indicator)}"));
            AttachAuth(req);
            var resp = await _http.SendAsync(req).ConfigureAwait(false);
            resp.EnsureSuccessStatusCode();
            var data = await resp.Content.ReadFromJsonAsync<Dictionary<string, object>>().ConfigureAwait(false);
            return data ?? new Dictionary<string, object>();
        }

        public async Task<Dictionary<string, object>> GetQuotaStatusAsync()
        {
            using var req = new HttpRequestMessage(HttpMethod.Get, new Uri(new Uri(_baseUrl), "/users/self/limits"));
            AttachAuth(req);
            var resp = await _http.SendAsync(req).ConfigureAwait(false);
            resp.EnsureSuccessStatusCode();
            var data = await resp.Content.ReadFromJsonAsync<Dictionary<string, object>>().ConfigureAwait(false);
            return data ?? new Dictionary<string, object>();
        }

        private void AttachAuth(HttpRequestMessage req)
        {
            if (string.IsNullOrWhiteSpace(_apiKey)) return;
            if (!req.Headers.Contains("x-apikey"))
            {
                req.Headers.Add("x-apikey", _apiKey);
            }
        }
    }
}
