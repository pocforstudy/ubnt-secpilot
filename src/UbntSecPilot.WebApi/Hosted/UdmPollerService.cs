using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using UbntSecPilot.Application.Services;

namespace UbntSecPilot.WebApi.Hosted
{
    /// <summary>
    /// Polls UDM Pro periodically and converts results into NetworkEvent entries via EventCollectionService.
    /// Configure interval via UdmPoller:IntervalSeconds (default 60).
    /// </summary>
    public sealed class UdmPollerService : BackgroundService
    {
        private readonly ILogger<UdmPollerService> _logger;
        private readonly IServiceProvider _services;
        private readonly IConfiguration _config;

        public UdmPollerService(ILogger<UdmPollerService> logger, IServiceProvider services, IConfiguration config)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _services = services ?? throw new ArgumentNullException(nameof(services));
            _config = config ?? throw new ArgumentNullException(nameof(config));
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var interval = Math.Max(10, _config.GetValue<int?>("UdmPoller:IntervalSeconds") ?? 60);
            _logger.LogInformation("UdmPollerService started with interval {Interval}s", interval);

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using var scope = _services.CreateScope();
                    var udm = scope.ServiceProvider.GetRequiredService<IUdmProService>();
                    var collector = scope.ServiceProvider.GetRequiredService<EventCollectionService>();

                    var rules = await udm.GetFirewallRulesAsync().ConfigureAwait(false);
                    var ts = DateTime.UtcNow;
                    int idx = 0;
                    foreach (var rule in rules)
                    {
                        var payload = new Dictionary<string, object>(rule);
                        var eventId = $"udm-rule-{ts:yyyyMMddHHmmss}-{idx++}";
                        await collector.CollectAsync(eventId, "udm-pro", payload, ts).ConfigureAwait(false);
                    }
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "UdmPollerService iteration failed");
                }

                try
                {
                    await Task.Delay(TimeSpan.FromSeconds(interval), stoppingToken).ConfigureAwait(false);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
            }

            _logger.LogInformation("UdmPollerService stopping");
        }
    }
}
