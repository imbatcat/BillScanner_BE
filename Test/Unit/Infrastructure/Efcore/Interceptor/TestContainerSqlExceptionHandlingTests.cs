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
        private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder()
            .WithImage("postgres:15-alpine")
            .WithDatabase("billscanner_test")
            .WithUsername("test")
            .WithPassword("test")
            .Build();
        private BillScannerDbContext? _dbContext;

        private ILogger<SqlExceptionHandlingInterceptor>? _logger;

        public async Task InitializeAsync()
        {
            await _postgres.StartAsync();

            // Create logger that outputs to test console
            _logger = XUnitLogger.CreateLogger<SqlExceptionHandlingInterceptor>(outputHelper);

            // Create interceptor
            var interceptor = new SqlExceptionHandlingInterceptor(_logger);

            // Configure DbContext with PostgreSQL and interceptor
            var options = new DbContextOptionsBuilder<BillScannerDbContext>()
                .UseNpgsql(_postgres.GetConnectionString())
                .AddInterceptors(interceptor)
                .EnableSensitiveDataLogging()
                .EnableDetailedErrors()
                .Options;

            _dbContext = new BillScannerDbContext(options);

            // Create database schema
            await _dbContext.Database.EnsureCreatedAsync();
        }

        public async Task DisposeAsync()
        {
            if (_dbContext != null)
            {
                await _dbContext.DisposeAsync();
            }
            await _postgres.DisposeAsync();
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

            _dbContext!.Set<User>().Add(user1);
            await _dbContext.SaveChangesAsync();

            // Create second user with same email (violates unique constraint)
            var user2 = new User
            {
                Email = "duplicate@example.com",
                Password = "password456",
                DisplayName = "User 2"
            };

            _dbContext.Set<User>().Add(user2);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<UniqueConstraintException>(
                () => _dbContext.SaveChangesAsync());

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

            _dbContext!.Set<User>().Add(user1);
            _dbContext.SaveChanges();

            // Create second user with same email (violates unique constraint)
            var user2 = new User
            {
                Email = "sync.duplicate@example.com",
                Password = "password456",
                DisplayName = "User 2"
            };

            _dbContext.Set<User>().Add(user2);

            // Act & Assert
            var exception = Assert.Throws<UniqueConstraintException>(
                () => _dbContext.SaveChanges());

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

            _dbContext!.Set<User>().Add(user);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<CannotInsertNullException>(
                () => _dbContext.SaveChangesAsync());

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

            _dbContext!.Set<User>().Add(user);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<MaxLengthExceededException>(
                () => _dbContext.SaveChangesAsync());

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

            _dbContext!.Set<User>().Add(validUser);
            await _dbContext.SaveChangesAsync();

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
                    .UseNpgsql(_postgres.GetConnectionString())
                    .AddInterceptors(new SqlExceptionHandlingInterceptor(_logger!))
                    .Options;

                await using var context = new BillScannerDbContext(options);

                context.Set<User>().Add(user);

                // Act & Assert
                var exception = await Assert.ThrowsAnyAsync<Exception>(
                    () => context.SaveChangesAsync());

                Assert.IsType(expectedExceptionType, exception);
                outputHelper.WriteLine($"Violation {violations.IndexOf((user, expectedExceptionType)) + 1}: {exception.GetType().Name}");
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

            _dbContext!.Set<User>().Add(user);

            // Act
            await _dbContext.SaveChangesAsync();

            // Assert - verify user was saved
            var savedUser = await _dbContext.Set<User>()
                .FirstOrDefaultAsync(u => u.Email == "valid.user@example.com");

            Assert.NotNull(savedUser);
            Assert.Equal("Valid User", savedUser.DisplayName);
            Assert.NotEqual(Guid.Empty, savedUser.Id);

            outputHelper.WriteLine($"User saved successfully with ID: {savedUser.Id}");
        }
    }
}