using System.Collections.Concurrent;
// using UbntSecPilot.Application.Services; // not used
using UbntSecPilot.WebApi.Hosted;
using UbntSecPilot.Agents.Orleans;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using System.Text;
using System.IdentityModel.Tokens.Jwt;
using MediatR;
using UbntSecPilot.Application.Handlers;
using UbntSecPilot.Application.Queries;
using UbntSecPilot.Infrastructure.Data;
var builder = WebApplication.CreateBuilder(args);

// Temporarily disable Orleans silo hosting to avoid client/server conflict
builder.Host.UseOrleansAgents("UbntSecPilot");

// Add services to the container
builder.Services.AddControllers();

// Add MediatR for CQRS
builder.Services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssembly(typeof(AnalyzeNetworkEventHandler).Assembly);
});

// Add Swagger/OpenAPI with enhanced configuration
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    // Basic API information
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "UBNT SecPilot API",
        Version = "v1.0.0",
        Description = "Enterprise Security Threat Analysis and Monitoring API",
        Contact = new OpenApiContact
        {
            Name = "UBNT Security Team",
            Email = "security@ubnt.com",
            Url = new Uri("https://ubnt-security.com")
        },
        License = new OpenApiLicense
        {
            Name = "MIT License",
            Url = new Uri("https://opensource.org/licenses/MIT")
        },
        TermsOfService = new Uri("https://ubnt-security.com/terms")
    });

    // JWT Bearer Authentication
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = @"JWT Authorization header using the Bearer scheme.
                        Enter 'Bearer' [space] and then your token in the text input below.
                        Example: 'Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...'",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        BearerFormat = "JWT"
    });

    // API Key Authentication (for external integrations)
    c.AddSecurityDefinition("ApiKey", new OpenApiSecurityScheme
    {
        Description = "API Key Authentication",
        Name = "X-API-Key",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "ApiKey"
    });

    // Security Requirements
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });

    // Enable annotations for better documentation
    c.EnableAnnotations();

    // Group endpoints by tags
    c.TagActionsBy(api => new[] { api.GroupName ?? api.ActionDescriptor.RouteValues["controller"] });
    c.DocInclusionPredicate((name, api) => true);

    // Basic defaults only; advanced filters removed for now
});

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader());

    options.AddPolicy("AllowSpecific", policy =>
        policy.WithOrigins("http://localhost:5000", "https://localhost:5001")
              .AllowAnyMethod()
              .AllowAnyHeader());
});

// Temporarily simplified authentication (JWT disabled for debugging)
// builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
//     .AddJwtBearer(options =>
//     {
//         options.TokenValidationParameters = new TokenValidationParameters
//         {
//             ValidateIssuer = true,
//             ValidateAudience = true,
//             ValidateLifetime = true,
//             ValidateIssuerSigningKey = true,
//             ValidIssuer = builder.Configuration["Jwt:Issuer"],
//             ValidAudience = builder.Configuration["Jwt:Audience"],
//             IssuerSigningKey = new SymmetricSecurityKey(
//                 Encoding.UTF8.GetBytes(builder.Configuration["Jwt:SecretKey"] ?? throw new ArgumentException("JWT SecretKey not configured")))
//         };
//     });

// Add Authorization
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));
    options.AddPolicy("UserOnly", policy => policy.RequireRole("User", "Admin"));
});

// Add security services (temporarily disabled until integrations are wired)
// builder.Services.AddScoped<IUdmProService, UdmProService>();
// builder.Services.AddScoped<IVirusTotalService, VirusTotalService>();


// Temporarily disable MongoDB repositories for debugging
// builder.Services.AddMongoRepositories(builder.Configuration);

// Domain services used by MediatR handlers
builder.Services.AddScoped<UbntSecPilot.Domain.Services.IThreatAnalysisService, UbntSecPilot.Domain.Services.ThreatAnalysisService>();
builder.Services.AddScoped<UbntSecPilot.Domain.Services.IThreadAnalysisService, UbntSecPilot.Domain.Services.ThreadAnalysisService>();

// Orleans client registered via AddOrleansAgentsClient above; no service registration needed here

// Register HttpClient-based integrations (temporarily disabled for debugging)
// builder.Services.AddHttpClient<IUdmProService, UdmProService>();
// builder.Services.AddHttpClient<IVirusTotalService, VirusTotalService>();

// Application services (temporarily disabled)
// builder.Services.AddScoped<PreAnalysisService>();
// builder.Services.AddScoped<EventCollectionService>();

// Hosted background services (UDM poller temporarily disabled for debugging)
// builder.Services.AddHostedService<UdmPollerService>();

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "UBNT SecPilot API v1.0.0");
        c.RoutePrefix = "swagger";
        c.DocumentTitle = "UBNT SecPilot API Documentation";

        // Enable deep linking
        c.DocExpansion(Swashbuckle.AspNetCore.SwaggerUI.DocExpansion.None);
        c.DefaultModelsExpandDepth(-1);
        c.DefaultModelExpandDepth(2);

        // Add custom CSS for better styling
        c.InjectStylesheet("/swagger-ui/custom.css");

        // Enable OAuth2
        c.OAuthClientId("swagger-ui");
        c.OAuthClientSecret("swagger-ui-secret");

        // Enable syntax highlighting
        c.EnableDeepLinking();
        c.EnableFilter();
        c.ShowExtensions();
        c.EnableValidator();
    });
}
else
{
    // In production, only show Swagger on specific conditions
    if (builder.Configuration.GetValue<bool>("EnableSwaggerInProduction"))
    {
        app.UseSwagger();
        app.UseSwaggerUI(c =>
        {
            c.SwaggerEndpoint("/swagger/v1/swagger.json", "UBNT SecPilot API v1.0.0");
            c.RoutePrefix = "swagger";
        });
    }
}

app.UseHttpsRedirection();
app.UseCors("AllowAll");

// Add security middleware
// Security middleware removed for now

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Health endpoint
app.MapGet("/health", () => Results.Ok(new { status = "ok", timestamp = DateTime.UtcNow }))
    .WithName("HealthCheck")
    .WithOpenApi()
    .WithTags("Health");

// API endpoints (simplified for debugging - MediatR/CQRS endpoints disabled)
// Orleans endpoints for agent management
app.MapGet("/api/orleans/agents", async (IClusterClient clusterClient) =>
{
    var orchestrator = clusterClient.GetGrain<IAgentOrchestrator>("default");
    var agents = await orchestrator.GetAvailableAgentsAsync();
    return Results.Ok(agents);
})
.RequireAuthorization("AdminOnly")
.WithName("GetAvailableAgents")
.WithTags("Orleans", "Agents")
.WithOpenApi();

app.MapGet("/api/orleans/agents/{agentName}/status", async (IClusterClient clusterClient, string agentName) =>
{
    var orchestrator = clusterClient.GetGrain<IAgentOrchestrator>("default");
    var status = await orchestrator.GetAgentStatusAsync(agentName);
    return Results.Ok(status);
})
.RequireAuthorization("AdminOnly")
.WithName("GetAgentStatus")
.WithTags("Orleans", "Agents")
.WithOpenApi();

app.MapPost("/api/orleans/agents/{agentName}/run", async (IClusterClient clusterClient, string agentName) =>
{
    var orchestrator = clusterClient.GetGrain<IAgentOrchestrator>("default");
    var result = await orchestrator.RunAgentAsync(agentName);
    return Results.Ok(result);
})
.RequireAuthorization("AdminOnly")
.WithName("RunAgent")
.WithTags("Orleans", "Agents")
.WithOpenApi();

// Thread analysis endpoint (temporarily disabled - missing types)
// app.MapPost("/api/analysis/thread", async (IMediator mediator, List<ThreadMessageDto> messages) => ...);

// API Version endpoint
app.MapGet("/api/version", () => Results.Ok(new
{
    version = "1.0.0",
    name = "UBNT SecPilot API",
    description = "Security threat analysis and monitoring API",
    timestamp = DateTime.UtcNow
}))
.WithName("GetApiVersion")
.WithTags("System")
.WithOpenApi();

// Metrics endpoint
app.MapGet("/api/metrics", async () =>
{
    var metrics = new
    {
        uptime = DateTime.UtcNow - System.Diagnostics.Process.GetCurrentProcess().StartTime.ToUniversalTime(),
        memory_usage = System.Diagnostics.Process.GetCurrentProcess().WorkingSet64,
        thread_count = System.Diagnostics.Process.GetCurrentProcess().Threads.Count,
        timestamp = DateTime.UtcNow
    };
    return Results.Ok(metrics);
})
.RequireAuthorization("AdminOnly")
.WithName("GetMetrics")
.WithTags("System", "Monitoring")
.WithOpenApi();

// Prometheus-style metrics endpoint
app.MapGet("/metrics", () =>
{
    var lines = new List<string>
    {
        "# HELP ubnt_ingestion_events_total Total number of ingested events",
        "# TYPE ubnt_ingestion_events_total counter",
        $"ubnt_ingestion_events_total {MetricsRecorder.Get("ingestion_events_total")}\n",
        "# HELP ubnt_2pc_prepared_total Total prepared participants in 2PC",
        "# TYPE ubnt_2pc_prepared_total counter",
        $"ubnt_2pc_prepared_total {MetricsRecorder.Get("two_phase_commit_prepared_total")}\n",
        "# HELP ubnt_2pc_committed_total Total committed participants in 2PC",
        "# TYPE ubnt_2pc_committed_total counter",
        $"ubnt_2pc_committed_total {MetricsRecorder.Get("two_phase_commit_committed_total")}\n",
        "# HELP ubnt_2pc_aborted_total Total aborted participants in 2PC",
        "# TYPE ubnt_2pc_aborted_total counter",
        $"ubnt_2pc_aborted_total {MetricsRecorder.Get("two_phase_commit_aborted_total")}\n"
    };
    var payload = string.Join('\n', lines);
    return Results.Text(payload, "text/plain");
})
.WithName("PrometheusMetrics")
.WithTags("System", "Monitoring")
.WithOpenApi();

app.Run();

internal static class MetricsRecorder
{
    private static readonly ConcurrentDictionary<string, long> Counters = new();

    public static void Inc(string name, long value = 1)
    {
        Counters.AddOrUpdate(name, value, (_, old) => old + value);
    }

    public static long Get(string name)
    {
        return Counters.TryGetValue(name, out var v) ? v : 0;
    }
}
