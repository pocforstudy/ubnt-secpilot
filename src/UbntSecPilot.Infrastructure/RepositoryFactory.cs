using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using MongoDB.Driver;
using StackExchange.Redis;
using UbntSecPilot.Domain.Repositories;
using Microsoft.Extensions.DependencyInjection;
using UbntSecPilot.Infrastructure.Data.InMemory;
using UbntSecPilot.Infrastructure.Data.Redis;
using UbntSecPilot.Infrastructure.Data.SqlServer;

namespace UbntSecPilot.Infrastructure.Data
{
    /// <summary>
    /// Factory for creating repository instances based on configuration
    /// </summary>
    public class RepositoryFactory
    {
        private readonly IConfiguration _configuration;
        private readonly IServiceProvider _serviceProvider;

        public RepositoryFactory(IConfiguration configuration, IServiceProvider serviceProvider)
        {
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        }

        /// <summary>
        /// Creates an event repository based on configuration
        /// </summary>
        public INetworkEventRepository CreateEventRepository()
        {
            var repositoryType = _configuration.GetValue<string>("Repository:EventRepository") ?? "MongoDB";

            return repositoryType.ToLower() switch
            {
                "mongodb" => CreateMongoEventRepository(),
                "sqlserver" => CreateSqlServerEventRepository(),
                // Only MongoDB and SQL Server are supported for events currently
                _ => throw new ArgumentException($"Unknown repository type: {repositoryType}")
            };
        }

        /// <summary>
        /// Creates a finding repository based on configuration
        /// </summary>
        public IThreatFindingRepository CreateFindingRepository()
        {
            var repositoryType = _configuration.GetValue<string>("Repository:FindingRepository") ?? "MongoDB";

            return repositoryType.ToLower() switch
            {
                "mongodb" => CreateMongoFindingRepository(),
                "sqlserver" => CreateSqlServerFindingRepository(),
                "redis" => CreateRedisFindingRepository(),
                "inmemory" => CreateInMemoryFindingRepository(),
                _ => throw new ArgumentException($"Unknown repository type: {repositoryType}")
            };
        }

        /// <summary>
        /// Creates a decision repository based on configuration
        /// </summary>
        public IAgentDecisionRepository CreateDecisionRepository()
        {
            var repositoryType = _configuration.GetValue<string>("Repository:DecisionRepository") ?? "MongoDB";

            return repositoryType.ToLower() switch
            {
                "mongodb" => CreateMongoDecisionRepository(),
                "sqlserver" => CreateSqlServerDecisionRepository(),
                // Only MongoDB and SQL Server are supported for decisions currently
                _ => throw new ArgumentException($"Unknown repository type: {repositoryType}")
            };
        }

        /// <summary>
        /// Creates a thread analysis repository based on configuration
        /// </summary>
        public IThreadAnalysisRepository CreateThreadAnalysisRepository()
        {
            var repositoryType = _configuration.GetValue<string>("Repository:ThreadAnalysisRepository") ?? "MongoDB";

            return repositoryType.ToLower() switch
            {
                "mongodb" => CreateMongoThreadAnalysisRepository(),
                "sqlserver" => CreateSqlServerThreadAnalysisRepository(),
                // Only MongoDB, SQL Server and InMemory are supported for thread analysis currently
                "inmemory" => CreateInMemoryThreadAnalysisRepository(),
                _ => throw new ArgumentException($"Unknown repository type: {repositoryType}")
            };
        }

        // MongoDB repositories
        private INetworkEventRepository CreateMongoEventRepository()
        {
            var mongoClient = _serviceProvider.GetService(typeof(IMongoClient)) as IMongoClient;
            var database = mongoClient.GetDatabase(_configuration.GetValue<string>("MongoDB:DatabaseName"));
            return new UbntSecPilot.Infrastructure.Repositories.MongoEventRepository(database);
        }

        private IThreatFindingRepository CreateMongoFindingRepository()
        {
            var mongoClient = _serviceProvider.GetService(typeof(IMongoClient)) as IMongoClient;
            var database = mongoClient.GetDatabase(_configuration.GetValue<string>("MongoDB:DatabaseName"));
            return new UbntSecPilot.Infrastructure.Repositories.MongoFindingRepository(database);
        }

        private IAgentDecisionRepository CreateMongoDecisionRepository()
        {
            var mongoClient = _serviceProvider.GetService(typeof(IMongoClient)) as IMongoClient;
            var database = mongoClient.GetDatabase(_configuration.GetValue<string>("MongoDB:DatabaseName"));
            return new UbntSecPilot.Infrastructure.Repositories.MongoDecisionRepository(database);
        }

        private IThreadAnalysisRepository CreateMongoThreadAnalysisRepository()
        {
            var mongoClient = _serviceProvider.GetService(typeof(IMongoClient)) as IMongoClient;
            var database = mongoClient.GetDatabase(_configuration.GetValue<string>("MongoDB:DatabaseName"));
            return new UbntSecPilot.Infrastructure.Repositories.MongoThreadAnalysisRepository(database);
        }

        // SQL Server repositories
        private INetworkEventRepository CreateSqlServerEventRepository()
        {
            var context = _serviceProvider.GetService(typeof(SecurityDbContext)) as SecurityDbContext;
            return new SqlServerEventRepository(context);
        }

        private IThreatFindingRepository CreateSqlServerFindingRepository()
        {
            var context = _serviceProvider.GetService(typeof(SecurityDbContext)) as SecurityDbContext;
            return new SqlServerFindingRepository(context);
        }

        private IAgentDecisionRepository CreateSqlServerDecisionRepository()
        {
            var context = _serviceProvider.GetService(typeof(SecurityDbContext)) as SecurityDbContext;
            return new SqlServerDecisionRepository(context);
        }

        private IThreadAnalysisRepository CreateSqlServerThreadAnalysisRepository()
        {
            var context = _serviceProvider.GetService(typeof(SecurityDbContext)) as SecurityDbContext;
            return new SqlServerThreadAnalysisRepository(context);
        }

        private IThreatFindingRepository CreateRedisFindingRepository()
        {
            var redis = _serviceProvider.GetService(typeof(IConnectionMultiplexer)) as IConnectionMultiplexer;
            return new RedisFindingRepository(redis);
        }

        // In-memory repositories
        private IThreatFindingRepository CreateInMemoryFindingRepository()
        {
            return new InMemoryFindingRepository();
        }

        private IThreadAnalysisRepository CreateInMemoryThreadAnalysisRepository()
        {
            return new InMemoryThreadAnalysisRepository();
        }
    }

    /// <summary>
    /// Configuration extensions for repository setup
    /// </summary>
    public static class RepositoryConfigurationExtensions
    {
        /// <summary>
        /// Adds repository services to the dependency injection container
        /// </summary>
        public static IServiceCollection AddRepositories(this IServiceCollection services, IConfiguration configuration)
        {
            var repositoryFactory = new RepositoryFactory(configuration, services.BuildServiceProvider());

            // Register repositories using factory (use lambda to satisfy DI signature)
            services.AddSingleton<INetworkEventRepository>(sp => repositoryFactory.CreateEventRepository());
            services.AddSingleton<IThreatFindingRepository>(sp => repositoryFactory.CreateFindingRepository());
            services.AddSingleton<IAgentDecisionRepository>(sp => repositoryFactory.CreateDecisionRepository());
            services.AddSingleton<IThreadAnalysisRepository>(sp => repositoryFactory.CreateThreadAnalysisRepository());

            // Register domain services
            services.AddScoped<UbntSecPilot.Domain.Services.ThreatAnalysisService>();
            services.AddScoped<UbntSecPilot.Domain.Services.ThreadAnalysisService>();

            return services;
        }

        /// <summary>
        /// Adds MongoDB repositories specifically
        /// </summary>
        public static IServiceCollection AddMongoRepositories(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddSingleton<IMongoClient>(sp =>
            {
                var connectionString = configuration.GetConnectionString("MongoDB");
                return new MongoClient(connectionString);
            });

            services.AddScoped<IMongoDatabase>(sp =>
            {
                var client = sp.GetRequiredService<IMongoClient>();
                var databaseName = configuration.GetValue<string>("MongoDB:DatabaseName");
                return client.GetDatabase(databaseName);
            });

            // Register MongoDB repositories
            services.AddScoped<INetworkEventRepository, UbntSecPilot.Infrastructure.Repositories.MongoEventRepository>();
            services.AddScoped<IThreatFindingRepository, UbntSecPilot.Infrastructure.Repositories.MongoFindingRepository>();
            services.AddScoped<IAgentDecisionRepository, UbntSecPilot.Infrastructure.Repositories.MongoDecisionRepository>();
            services.AddScoped<IThreadAnalysisRepository, UbntSecPilot.Infrastructure.Repositories.MongoThreadAnalysisRepository>();

            return services;
        }

        /// <summary>
        /// Adds SQL Server repositories specifically
        /// </summary>
        public static IServiceCollection AddSqlServerRepositories(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<SecurityDbContext>(options =>
                options.UseSqlServer(configuration.GetConnectionString("SqlServer")));

            services.AddScoped<INetworkEventRepository, SqlServerEventRepository>();
            services.AddScoped<IThreatFindingRepository, SqlServerFindingRepository>();
            services.AddScoped<IAgentDecisionRepository, SqlServerDecisionRepository>();
            services.AddScoped<IThreadAnalysisRepository, SqlServerThreadAnalysisRepository>();

            return services;
        }

        /// <summary>
        /// Adds Redis repositories specifically
        /// </summary>
        public static IServiceCollection AddRedisRepositories(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddSingleton<IConnectionMultiplexer>(sp =>
                ConnectionMultiplexer.Connect(configuration.GetConnectionString("Redis")));

            // Only Finding repository is backed by Redis at the moment
            services.AddScoped<IThreatFindingRepository, RedisFindingRepository>();

            return services;
        }

        /// <summary>
        /// Adds in-memory repositories specifically (for testing)
        /// </summary>
        public static IServiceCollection AddInMemoryRepositories(this IServiceCollection services)
        {
            services.AddScoped<IThreatFindingRepository, InMemoryFindingRepository>();
            services.AddScoped<IThreadAnalysisRepository, InMemoryThreadAnalysisRepository>();

            return services;
        }
    }
}
