using Infrastructure.Efcore.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Npgsql;
using Testcontainers.PostgreSql;
using Testcontainers.Redis;

namespace Infrastructure.Extension;

/// <summary>
/// Extension methods for setting up Testcontainers when running under the "Test" profile.
/// Containers are started and connection strings are injected into configuration.
/// </summary>
public static class TestContainersExtension
{
    /// <summary>
    /// Starts PostgreSQL and Redis containers, executes seed SQL script, and overrides connection strings.
    /// Call this when ASPNETCORE_ENVIRONMENT=Test.
    /// </summary>
    public static async Task<TestContainersContext> AddTestContainersAsync(
        this IServiceCollection services,
        IConfigurationManager configuration,
        string? seedScriptPath = null)
    {
        Console.WriteLine("[TestContainers] Starting PostgreSQL container...");
        var dbContainer = new PostgreSqlBuilder("postgres:18.1-alpine3.23")
            .WithDatabase("billscanner_test")
            .WithUsername("test")
            .WithPassword("test")
            .Build();
        await dbContainer.StartAsync();
        Console.WriteLine($"[TestContainers] PostgreSQL started at: {dbContainer.GetConnectionString()}");

        Console.WriteLine("[TestContainers] Starting Redis container...");
        var redisContainer = new RedisBuilder("redis:7.4.2-alpine")
            .Build();
        await redisContainer.StartAsync();
        Console.WriteLine($"[TestContainers] Redis started at: {redisContainer.GetConnectionString()}");

        // Override connection strings in configuration
        configuration["ConnectionStrings:BillScannerDb"] = dbContainer.GetConnectionString();
        configuration["RedisSettings:ConnectionString"] = redisContainer.GetConnectionString();

        // Create context for cleanup (store seed script path for deferred execution)
        var context = new TestContainersContext(dbContainer, redisContainer, seedScriptPath);

        // Register context and services
        services.AddSingleton(context);
        services.AddHostedService<DatabaseInitializerService>();
        services.AddHostedService<ContainerCleanupService>();

        return context;
    }
}

/// <summary>
/// Holds references to running Testcontainers for cleanup.
/// </summary>
public class TestContainersContext(
    PostgreSqlContainer dbContainer,
    RedisContainer redisContainer,
    string? seedScriptPath = null)
{
    public PostgreSqlContainer DbContainer { get; } = dbContainer;
    public RedisContainer RedisContainer { get; } = redisContainer;
    public string? SeedScriptPath { get; } = seedScriptPath;
}

/// <summary>
/// Hosted service that initializes the database schema and runs seed scripts on startup.
/// </summary>
public class DatabaseInitializerService(
    IServiceProvider serviceProvider,
    TestContainersContext context) : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        Console.WriteLine("[TestContainers] Initializing database schema...");

        using var scope = serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<BillScannerDbContext>();

        // Create database schema (applies model to DB)
        await dbContext.Database.EnsureCreatedAsync(cancellationToken);
        Console.WriteLine("[TestContainers] Database schema created.");

        // Execute seed SQL script if provided
        if (!string.IsNullOrEmpty(context.SeedScriptPath) && File.Exists(context.SeedScriptPath))
        {
            Console.WriteLine($"[TestContainers] Executing seed script: {context.SeedScriptPath}");
            var sql = await File.ReadAllTextAsync(context.SeedScriptPath, cancellationToken);
            if (!string.IsNullOrWhiteSpace(sql))
            {
                await dbContext.Database.ExecuteSqlRawAsync(sql, cancellationToken);
            }

            Console.WriteLine("[TestContainers] Seed script executed successfully.");
        }
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}

/// <summary>
/// Hosted service that disposes containers when the application shuts down.
/// </summary>
public class ContainerCleanupService(TestContainersContext context) : IHostedService
{
    public Task StartAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        Console.WriteLine("[TestContainers] Shutting down containers...");
        await context.RedisContainer.DisposeAsync();
        await context.DbContainer.DisposeAsync();
        Console.WriteLine("[TestContainers] Containers disposed.");
    }
}
