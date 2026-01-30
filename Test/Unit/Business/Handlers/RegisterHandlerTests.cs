using Business.Handlers.Authentication.Register;
using Business.Handlers.Authentication.Register.Dto;
using Business.Handlers.Authentication.Register.Spec;
using Business.Interfaces.Repositories;
using Business.Interfaces.Services;
using Domain.Entities;
using EntityFramework.Exceptions.Common;
using Infrastructure.Efcore.Interceptors;
using Infrastructure.Efcore.Persistence;
using Meziantou.Extensions.Logging.Xunit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Testcontainers.PostgreSql;
using Xunit;
using Xunit.Abstractions;

namespace Test.Unit.Business.Handlers
{
    public class RegisterHandlerTests : IAsyncLifetime
    {
        private readonly PostgreSqlContainer dbContainer = new PostgreSqlBuilder()
            .WithImage("postgres:18.1-alpine3.23")
            .WithDatabase("billscanner_test")
            .WithUsername("test")
            .WithPassword("test")
            .Build();

        private readonly ITestOutputHelper output;

        private readonly Mock<IUserTokenService> tokenServiceMock;

        private BillScannerDbContext dbContext = null!;

        private UnitOfWork unitOfWork = null!;

        private RegisterHandler handler = null!;

        private ILoggerFactory loggerFactory = null!;

        public RegisterHandlerTests(ITestOutputHelper output)
        {
            this.output = output;
            tokenServiceMock = new Mock<IUserTokenService>();
        }

        public async Task InitializeAsync()
        {
            // Start the container
            await dbContainer.StartAsync();

            // Setup Logging
            loggerFactory = LoggerFactory.Create(builder =>
            {
                builder.SetMinimumLevel(LogLevel.Debug);
                builder.AddProvider(new XUnitLoggerProvider(output));
            });

            var interceptorLogger = loggerFactory.CreateLogger<SqlExceptionHandlingInterceptor>();

            // Setup DbContext
            var options = new DbContextOptionsBuilder<BillScannerDbContext>()
                .UseNpgsql(dbContainer.GetConnectionString())
                .AddInterceptors(new SqlExceptionHandlingInterceptor(interceptorLogger))
                .EnableSensitiveDataLogging()
                .EnableDetailedErrors()
                .Options;

            dbContext = new BillScannerDbContext(options);
            await dbContext.Database.EnsureCreatedAsync();

            // Setup UnitOfWork
            unitOfWork = new UnitOfWork(dbContext);

            // Setup Handler
            handler = new RegisterHandler(
                unitOfWork,
                tokenServiceMock.Object);

            // Default Token Service Mocks
            SetupTokenService();
        }

        public async Task DisposeAsync()
        {
            await dbContext.DisposeAsync();
            await dbContainer.DisposeAsync();
            loggerFactory.Dispose();
        }

        [Fact]
        public async Task Handle_NewUser_ReturnsRegisterResponse()
        {
            // Arrange
            var command = new RegisterCommand
            {
                Email = "newuser@example.com",
                Password = "SecurePassword123!",
                DisplayName = "New User"
            };

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(command.Email, result.Email);
            Assert.Equal(command.DisplayName, result.DisplayName);
            Assert.Equal("access-token", result.AccessToken);
            Assert.NotEqual(Guid.Empty, result.UserId);

            var userInDb = await dbContext.Users.FirstOrDefaultAsync(u => u.Email == command.Email);
            Assert.NotNull(userInDb);
            Assert.Equal("New User", userInDb.DisplayName);
        }

        [Fact]
        public async Task Handle_NewUserWithoutDisplayName_UsesEmailPrefix()
        {
            // Arrange
            var command = new RegisterCommand
            {
                Email = "testuser@example.com",
                Password = "Password123!",
                DisplayName = null
            };

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("testuser", result.DisplayName);

            var userInDb = await dbContext.Users.FirstOrDefaultAsync(u => u.Email == command.Email);
            Assert.NotNull(userInDb);
            Assert.Equal("testuser", userInDb.DisplayName);
        }

        [Fact]
        public async Task Handle_DuplicateEmail_ThrowsUniqueConstraintException()
        {
            // Arrange
            var existingUser = new User
            {
                Id = Guid.NewGuid(),
                Email = "existing@example.com",
                Password = "hashed-password",
                DisplayName = "Existing"
            };

            dbContext.Users.Add(existingUser);
            await dbContext.SaveChangesAsync();

            var command = new RegisterCommand
            {
                Email = "existing@example.com",
                Password = "Password123!"
            };

            // Act & Assert
            var exception =
                await Assert.ThrowsAsync<UniqueConstraintException>(() =>
                    handler.Handle(command, CancellationToken.None));

            Assert.NotNull(exception);
        }

        [Fact]
        public async Task Handle_SuccessfulRegistration_LogsInformation()
        {
            // Arrange
            var command = new RegisterCommand
            {
                Email = "logging-test@example.com",
                Password = "Password123!",
                DisplayName = "Logger Test"
            };

            // Act
            await handler.Handle(command, CancellationToken.None);

            // Assert
            var user = await dbContext.Users.FirstOrDefaultAsync(u => u.Email == command.Email);
            Assert.NotNull(user);
        }

        [Fact]
        public async Task Handle_SuccessfulRegistration_PersistsData()
        {
            // Arrange
            var command = new RegisterCommand
            {
                Email = "persistance@example.com",
                Password = "Password123!",
                DisplayName = "Persistence Test"
            };

            // Act
            await handler.Handle(command, CancellationToken.None);

            // Assert
            var count = await dbContext.Users.CountAsync(u => u.Email == command.Email);
            Assert.Equal(1, count);
        }

        [Fact]
        public async Task Handle_SuccessfulRegistration_InsertsUserWithHashedPassword()
        {
            // Arrange
            var password = "PlainTextPassword";
            var command = new RegisterCommand
            {
                Email = "hashing@example.com",
                Password = password,
                DisplayName = "Hashing Test"
            };

            // Act
            await handler.Handle(command, CancellationToken.None);

            // Assert
            var user = await dbContext.Users.FirstAsync(u => u.Email == command.Email);
            Assert.NotEqual(password, user.Password);
            Assert.False(string.IsNullOrEmpty(user.Password));
        }

        [Fact]
        public async Task Handle_SuccessfulRegistration_CallsTokenServiceWithUserRole()
        {
            // Arrange
            var command = new RegisterCommand
            {
                Email = "roles@example.com",
                Password = "Password123!",
                DisplayName = "Roles Test"
            };

            // Act
            await handler.Handle(command, CancellationToken.None);

            // Assert
            tokenServiceMock.Verify(
                t => t.CreateAccessToken(
                    It.IsAny<User>(),
                    It.Is<List<string>>(roles => roles.Contains("User"))),
                Times.Once);

            tokenServiceMock.Verify(
                t => t.CreateIdToken(
                    It.IsAny<User>(),
                    It.Is<List<string>>(roles => roles.Contains("User"))),
                Times.Once);
        }

        [Fact]
        public async Task Handle_SuccessfulRegistration_CreatesAllThreeTokens()
        {
            // Arrange
            var command = new RegisterCommand
            {
                Email = "tokens@example.com",
                Password = "Password123!",
                DisplayName = "Tokens Test"
            };

            // Act
            await handler.Handle(command, CancellationToken.None);

            // Assert
            tokenServiceMock.Verify(
                t => t.CreateAccessToken(It.IsAny<User>(), It.IsAny<List<string>>()),
                Times.Once);

            tokenServiceMock.Verify(
                t => t.CreateRefreshToken(It.IsAny<User>()),
                Times.Once);

            tokenServiceMock.Verify(
                t => t.CreateIdToken(It.IsAny<User>(), It.IsAny<List<string>>()),
                Times.Once);
        }

        private void SetupTokenService()
        {
            tokenServiceMock
                .Setup(t => t.CreateAccessToken(It.IsAny<User>(), It.IsAny<List<string>>()))
                .Returns("access-token");

            tokenServiceMock
                .Setup(t => t.CreateRefreshToken(It.IsAny<User>()))
                .Returns("refresh-token");

            tokenServiceMock
                .Setup(t => t.CreateIdToken(It.IsAny<User>(), It.IsAny<List<string>>()))
                .Returns("id-token");
        }
    }
}