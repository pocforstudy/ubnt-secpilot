using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Orleans;
using Orleans.Configuration;

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
