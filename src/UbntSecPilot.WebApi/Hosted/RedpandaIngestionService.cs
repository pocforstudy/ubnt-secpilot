using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using UbntSecPilot.Application.Services;
using UbntSecPilot.Domain.Models;
using UbntSecPilot.Infrastructure.Services;

namespace UbntSecPilot.WebApi.Hosted
{
    /// <summary>
    /// Background service that consumes Redpanda/Kafka messages and persists them as NetworkEvent entries.
    /// Expects messages to be JSON with fields: eventId (string), source (string), payload (object), occurredAt (ISO string).
    /// </summary>
    public sealed class RedpandaIngestionService : BackgroundService
    {
        private readonly ILogger<RedpandaIngestionService> _logger;
        private readonly IServiceProvider _services;
        private readonly IStreamService _stream; // already registered RedpandaStreamService

        public RedpandaIngestionService(
            ILogger<RedpandaIngestionService> logger,
            IServiceProvider services,
            IStreamService stream)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _services = services ?? throw new ArgumentNullException(nameof(services));
            _stream = stream ?? throw new ArgumentNullException(nameof(stream));
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("RedpandaIngestionService started");

            await foreach (var message in _stream.ConsumeAsync(stoppingToken))
            {
                try
                {
                    using var scope = _services.CreateScope();
                    var collector = scope.ServiceProvider.GetRequiredService<EventCollectionService>();

                    // Parse message
                    var doc = JsonDocument.Parse(message);
                    var root = doc.RootElement;
                    var eventId = root.TryGetProperty("eventId", out var evIdEl) ? evIdEl.GetString() ?? Guid.NewGuid().ToString("N") : Guid.NewGuid().ToString("N");
                    var source = root.TryGetProperty("source", out var srcEl) ? srcEl.GetString() ?? "redpanda" : "redpanda";
                    var occurredAt = DateTime.UtcNow;
                    if (root.TryGetProperty("occurredAt", out var tsEl) && tsEl.ValueKind == JsonValueKind.String && DateTime.TryParse(tsEl.GetString(), out var parsed))
                        occurredAt = parsed.ToUniversalTime();

                    // Build payload as a plain dictionary
                    var payload = new Dictionary<string, object>();
                    if (root.TryGetProperty("payload", out var payloadEl))
                    {
                        foreach (var prop in payloadEl.EnumerateObject())
                        {
                            payload[prop.Name] = prop.Value.ToString();
                        }
                    }

                    await collector.CollectAsync(eventId, source, payload, occurredAt).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to ingest message from Redpanda");
                }
            }

            _logger.LogInformation("RedpandaIngestionService stopping");
        }
    }
}

