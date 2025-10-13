using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Orleans;
using Orleans.Configuration;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace UbntSecPilot.Agents.Orleans
{
    public static class OrleansAgentsModule
    {
        // Host the Orleans Silo on the HostBuilder (WebApi or Worker)
        public static IHostBuilder UseOrleansAgents(this IHostBuilder hostBuilder, string serviceId = "UbntSecPilot")
        {
            return hostBuilder.UseOrleans(siloBuilder =>
            {
                siloBuilder
                    .UseLocalhostClustering()
                    .Configure<ClusterOptions>(options =>
                    {
                        options.ClusterId = "dev";
                        options.ServiceId = serviceId;
                    })
                    .ConfigureServices(services =>
                    {
                        // Configure OpenTelemetry for Orleans
                        services.AddOpenTelemetry()
                            .WithTracing(tracing =>
                            {
                                tracing
                                    .SetResourceBuilder(ResourceBuilder.CreateDefault()
                                        .AddService("UbntSecPilot.Agents.Orleans", serviceVersion: "1.0.0"))
                                    .AddOtlpExporter(options =>
                                    {
                                        options.Endpoint = new Uri("http://localhost:18888");
                                        options.Protocol = OpenTelemetry.Exporter.OtlpExportProtocol.Grpc;
                                    });
                            })
                            .WithMetrics(metrics =>
                            {
                                metrics
                                    .SetResourceBuilder(ResourceBuilder.CreateDefault()
                                        .AddService("UbntSecPilot.Agents.Orleans", serviceVersion: "1.0.0"))
                                    .AddRuntimeInstrumentation()
                                    .AddOtlpExporter(options =>
                                    {
                                        options.Endpoint = new Uri("http://localhost:18888");
                                        options.Protocol = OpenTelemetry.Exporter.OtlpExportProtocol.Grpc;
                                    });
                            });
                    });
            });
        }

        // Register an Orleans client for calling grains from DI
        public static IServiceCollection AddOrleansAgentsClient(this IServiceCollection services, string serviceId = "UbntSecPilot")
        {
            services.AddOrleansClient(clientBuilder =>
            {
                clientBuilder.UseLocalhostClustering();
                clientBuilder.Configure<ClusterOptions>(options =>
                {
                    options.ClusterId = "dev";
                    options.ServiceId = serviceId;
                });
            });

            return services;
        }
    }
}
