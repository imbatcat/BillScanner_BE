using Testcontainers.PostgreSql;
using Microsoft.EntityFrameworkCore;
using Infrastructure.Efcore.Persistence;
using Infrastructure.Efcore.Interceptors;
using Microsoft.Extensions.Logging;
using Xunit.Abstractions;
using Meziantou.Extensions.Logging.Xunit;
using Domain.Entities;
using EntityFramework.Exceptions.Common;

namespace Test.Unit.Infrastructure.Efcore.Interceptor
{
    public sealed class TestContainerSqlExceptionHandlingTests(ITestOutputHelper outputHelper) : IAsyncLifetime
    {
        private readonly PostgreSqlContainer postgres = new PostgreSqlBuilder()
            .WithImage("postgres:15-alpine")
            .WithDatabase("billscanner_test")
            .WithUsername("test")
            .WithPassword("test")
            .Build();

        private BillScannerDbContext? dbContext;

        private ILogger<SqlExceptionHandlingInterceptor>? logger;

        public async Task InitializeAsync()
        {
            await postgres.StartAsync();

            // Create logger that outputs to test console
            logger = XUnitLogger.CreateLogger<SqlExceptionHandlingInterceptor>(outputHelper);

            // Create interceptor
            var interceptor = new SqlExceptionHandlingInterceptor(logger);

            // Configure DbContext with PostgreSQL and interceptor
            var options = new DbContextOptionsBuilder<BillScannerDbContext>()
                .UseNpgsql(postgres.GetConnectionString())
                .AddInterceptors(interceptor)
                .EnableSensitiveDataLogging()
                .EnableDetailedErrors()
                .Options;

            dbContext = new BillScannerDbContext(options);

            // Create database schema
            await dbContext.Database.EnsureCreatedAsync();
        }

        public async Task DisposeAsync()
        {
            if (dbContext != null)
            {
                await dbContext.DisposeAsync();
            }

            await postgres.DisposeAsync();
        }

        [Fact]
        public async Task SaveChangesAsync_WithUniqueConstraintViolation_LogsErrorAndThrowsException()
        {
            // Arrange
            var user1 = new User
            {
                Email = "duplicate@example.com",
                Password = "password123",
                DisplayName = "User 1"
            };

            dbContext!.Set<User>().Add(user1);
            await dbContext.SaveChangesAsync();

            // Create second user with same email (violates unique constraint)
            var user2 = new User
            {
                Email = "duplicate@example.com",
                Password = "password456",
                DisplayName = "User 2"
            };

            dbContext.Set<User>().Add(user2);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<UniqueConstraintException>(() => dbContext.SaveChangesAsync());

            // Verify exception details
            Assert.NotNull(exception);
            Assert.Contains("unique constraint violation", exception.Message.ToLower());

            // Logging will be visible in test output with EventId 1001
            outputHelper.WriteLine($"Exception caught: {exception.GetType().Name}");
        }

        [Fact]
        public async Task SaveChanges_WithUniqueConstraintViolation_LogsErrorAndThrowsException()
        {
            // Arrange
            var user1 = new User
            {
                Email = "sync.duplicate@example.com",
                Password = "password123",
                DisplayName = "User 1"
            };

            dbContext!.Set<User>().Add(user1);
            dbContext.SaveChanges();

            // Create second user with same email (violates unique constraint)
            var user2 = new User
            {
                Email = "sync.duplicate@example.com",
                Password = "password456",
                DisplayName = "User 2"
            };

            dbContext.Set<User>().Add(user2);

            // Act & Assert
            var exception = Assert.Throws<UniqueConstraintException>(() => dbContext.SaveChanges());

            // Verify exception details
            Assert.NotNull(exception);
            Assert.Contains("unique", exception.Message.ToLower());

            outputHelper.WriteLine($"Exception caught: {exception.GetType().Name}");
        }

        [Fact]
        public async Task SaveChangesAsync_WithNotNullViolation_LogsErrorAndThrowsException()
        {
            // Arrange - User entity requires Email to be non-null
            var user = new User
            {
                Email = null!, // Violates NOT NULL constraint
                Password = "password123",
                DisplayName = "Test User"
            };

            dbContext!.Set<User>().Add(user);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<CannotInsertNullException>(() => dbContext.SaveChangesAsync());

            // Verify exception details
            Assert.NotNull(exception);

            outputHelper.WriteLine($"Exception caught: {exception.GetType().Name}");
        }

        [Fact]
        public async Task SaveChangesAsync_WithMaxLengthViolation_LogsErrorAndThrowsException()
        {
            // Arrange - Email has max length constraint
            var longEmail = new string('a', 300) + "@example.com"; // Exceeds max length
            var user = new User
            {
                Email = longEmail,
                Password = "password123",
                DisplayName = "Test User"
            };

            dbContext!.Set<User>().Add(user);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<MaxLengthExceededException>(() => dbContext.SaveChangesAsync());

            // Verify exception details
            Assert.NotNull(exception);

            outputHelper.WriteLine($"Exception caught: {exception.GetType().Name}");
        }

        [Fact]
        public async Task SaveChangesAsync_MultipleViolations_LogsEachError()
        {
            // Arrange - Create a user successfully
            var validUser = new User
            {
                Email = "valid@example.com",
                Password = "password123",
                DisplayName = "Valid User"
            };

            dbContext!.Set<User>().Add(validUser);
            await dbContext.SaveChangesAsync();

            // Test multiple different violations in sequence
            var violations = new List<(User User, Type ExpectedException)>
            {
                (new User { Email = "valid@example.com", Password = "pwd", DisplayName = "Duplicate" },
                    typeof(UniqueConstraintException)),
                (new User { Email = null!, Password = "pwd", DisplayName = "Null Email" },
                    typeof(CannotInsertNullException))
            };

            foreach (var (user, expectedExceptionType) in violations)
            {
                // Create new context for each test to avoid state issues
                var options = new DbContextOptionsBuilder<BillScannerDbContext>()
                    .UseNpgsql(postgres.GetConnectionString())
                    .AddInterceptors(new SqlExceptionHandlingInterceptor(logger!))
                    .Options;

                await using var context = new BillScannerDbContext(options);

                context.Set<User>().Add(user);

                // Act & Assert
                var exception = await Assert.ThrowsAnyAsync<Exception>(() => context.SaveChangesAsync());

                Assert.IsType(expectedExceptionType, exception);
                outputHelper.WriteLine(
                    $"Violation {violations.IndexOf((user, expectedExceptionType)) + 1}: {exception.GetType().Name}");
            }
        }

        [Fact]
        public async Task SaveChangesAsync_ValidData_NoExceptionThrown()
        {
            // Arrange
            var user = new User
            {
                Email = "valid.user@example.com",
                Password = "password123",
                DisplayName = "Valid User"
            };

            dbContext!.Set<User>().Add(user);

            // Act
            await dbContext.SaveChangesAsync();

            // Assert - verify user was saved
            var savedUser = await dbContext.Set<User>()
                .FirstOrDefaultAsync(u => u.Email == "valid.user@example.com");

            Assert.NotNull(savedUser);
            Assert.Equal("Valid User", savedUser.DisplayName);
            Assert.NotEqual(Guid.Empty, savedUser.Id);

            outputHelper.WriteLine($"User saved successfully with ID: {savedUser.Id}");
        }
    }
}