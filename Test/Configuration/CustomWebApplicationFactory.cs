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
using Testcontainers.PostgreSql;
using Xunit.Abstractions;

namespace Test.Configuration
{
    public class CustomWebApplicationFactory : WebApplicationFactory<IApiMarker>, IAsyncLifetime
    {
        private readonly PostgreSqlContainer _dbContainer = new PostgreSqlBuilder("postgres:18.1-alpine3.23")
            .WithDatabase("billscanner_test")
            .WithUsername("test")
            .WithPassword("test")
            .Build();

        public HttpClient HttpClient { get; private set; } = null!;

        private Respawner _respawner = null!;

        // private readonly IMessageSink _messageSink = messageSink;
        public ITestOutputHelper? Output { get; set; } = null!;

        private ILogger<SqlExceptionHandlingInterceptor>? _logger;

        private BillScannerDbContext? _dbContext;

        private DbConnection _dbConnection = null!;

        public async Task InitializeAsync()
        {
            await _dbContainer.StartAsync();

            HttpClient = CreateClient();

            // Use NullLogger during initialization phase
            _logger = NullLogger<SqlExceptionHandlingInterceptor>.Instance;

            // Create interceptor
            var interceptor = new SqlExceptionHandlingInterceptor(_logger);

            // Configure DbContext with PostgreSQL and interceptor
            var options = new DbContextOptionsBuilder<BillScannerDbContext>()
                .UseNpgsql(_dbContainer.GetConnectionString())
                .AddInterceptors(interceptor)
                .EnableSensitiveDataLogging()
                .EnableDetailedErrors()
                .Options;

            _dbContext = new BillScannerDbContext(options);

            // Create database schema
            await _dbContext.Database.EnsureCreatedAsync();

            _dbConnection = new NpgsqlConnection(_dbContainer.GetConnectionString());
            await _dbConnection.OpenAsync();
            await InitializeRespawnerAsync();
        }

        private async Task InitializeRespawnerAsync()
        {
            _respawner = await Respawner.CreateAsync(_dbConnection, new RespawnerOptions
            {
                SchemasToInclude = ["public"],
                DbAdapter = DbAdapter.Postgres
            });
        }

        public new async Task DisposeAsync()
        {
            await _dbConnection.DisposeAsync();
            await _dbContainer.DisposeAsync();
        }

        public async Task ResetDatabaseAsync()
        {
            await _respawner.ResetAsync(_dbConnection);
        }

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseSetting("ConnectionStrings:BillScannerDb", _dbContainer.GetConnectionString());

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