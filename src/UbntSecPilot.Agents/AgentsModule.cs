using Microsoft.Extensions.DependencyInjection;

namespace UbntSecPilot.Agents
{
    public static class AgentsModule
    {
        public static IServiceCollection AddAgents(this IServiceCollection services)
        {
            // Register agents
            services.AddScoped<IAgent, ThreatEnrichmentAgent>();
            return services;
        }
    }
}
