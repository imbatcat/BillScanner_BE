using System.Data.Common;
using BillScanner;
using Infrastructure.Efcore.Interceptors;
using Infrastructure.Efcore.Persistence;
using Meziantou.Extensions.Logging.Xunit;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Npgsql;
using Respawn;
using StackExchange.Redis;
using Testcontainers.PostgreSql;
using Testcontainers.Redis;
using Xunit.Abstractions;

namespace Test.Configuration
{
    public class CustomWebApplicationFactory : WebApplicationFactory<IApiMarker>, IAsyncLifetime
    {
        private readonly PostgreSqlContainer dbContainer = new PostgreSqlBuilder("postgres:18.1-alpine3.23")
            .WithDatabase("billscanner_test")
            .WithUsername("test")
            .WithPassword("test")
            .Build();

        private readonly RedisContainer redisContainer = new RedisBuilder("redis:7.4.2-alpine")
            .Build();

        public HttpClient HttpClient { get; private set; } = null!;

        public IConnectionMultiplexer RedisConnection { get; private set; } = null!;

        private Respawner respawner = null!;

        // private readonly IMessageSink _messageSink = messageSink;
        public ITestOutputHelper? Output { get; set; } = null!;

        private ILogger<SqlExceptionHandlingInterceptor>? logger;

        private BillScannerDbContext? dbContext;

        private DbConnection dbConnection = null!;

        public async Task InitializeAsync()
        {
            await dbContainer.StartAsync();
            await redisContainer.StartAsync();

            HttpClient = CreateClient();

            // Initialize Redis connection
            RedisConnection = await ConnectionMultiplexer.ConnectAsync(redisContainer.GetConnectionString());

            // Use NullLogger during initialization phase
            logger = NullLogger<SqlExceptionHandlingInterceptor>.Instance;

            // Create interceptor
            var interceptor = new SqlExceptionHandlingInterceptor(logger);

            // Configure DbContext with PostgreSQL and interceptor
            var options = new DbContextOptionsBuilder<BillScannerDbContext>()
                .UseNpgsql(dbContainer.GetConnectionString())
                .AddInterceptors(interceptor)
                .EnableSensitiveDataLogging()
                .EnableDetailedErrors()
                .Options;

            dbContext = new BillScannerDbContext(options);

            // Create database schema
            await dbContext.Database.EnsureCreatedAsync();

            dbConnection = new NpgsqlConnection(dbContainer.GetConnectionString());
            await dbConnection.OpenAsync();
            await InitializeRespawnerAsync();
        }

        private async Task InitializeRespawnerAsync()
        {
            respawner = await Respawner.CreateAsync(dbConnection, new RespawnerOptions
            {
                SchemasToInclude = ["public"],
                DbAdapter = DbAdapter.Postgres
            });
        }

        public new async Task DisposeAsync()
        {
            await dbConnection.DisposeAsync();
            RedisConnection?.Dispose();
            await dbContainer.DisposeAsync();
            await redisContainer.DisposeAsync();
        }

        public async Task ResetDatabaseAsync()
        {
            await respawner.ResetAsync(dbConnection);
        }

        public async Task ResetRedisAsync()
        {
            var server = RedisConnection.GetServer(RedisConnection.GetEndPoints().First());
            await server.FlushAllDatabasesAsync();
        }

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseSetting("ConnectionStrings:BillScannerDb", dbContainer.GetConnectionString());
            builder.UseSetting("ConnectionStrings:Redis", redisContainer.GetConnectionString());

            builder.ConfigureLogging(logging =>
            {
                logging.ClearProviders();
                logging.SetMinimumLevel(LogLevel.Debug);
                if (Output != null)
                {
                    logging.AddProvider(new XUnitLoggerProvider(Output));
                }
            });
        }

        public async Task ExecuteDbContextAsync(Func<BillScannerDbContext, Task> func)
        {
            using var scope = Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<BillScannerDbContext>();
            await func(dbContext);
        }
    }
}