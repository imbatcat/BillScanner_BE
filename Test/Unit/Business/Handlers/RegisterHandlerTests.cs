using Business.Handlers.Authentication.Register;
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
        private readonly PostgreSqlContainer _dbContainer = new PostgreSqlBuilder()
            .WithImage("postgres:18.1-alpine3.23")
            .WithDatabase("billscanner_test")
            .WithUsername("test")
            .WithPassword("test")
            .Build();

        private readonly ITestOutputHelper _output;
        private readonly Mock<IUserTokenService> _tokenServiceMock;

        private BillScannerDbContext _dbContext = null!;
        private UnitOfWork _unitOfWork = null!;
        private RegisterHandler _handler = null!;
        private ILoggerFactory _loggerFactory = null!;

        public RegisterHandlerTests(ITestOutputHelper output)
        {
            _output = output;
            _tokenServiceMock = new Mock<IUserTokenService>();
        }

        public async Task InitializeAsync()
        {
            // Start the container
            await _dbContainer.StartAsync();

            // Setup Logging
            _loggerFactory = LoggerFactory.Create(builder =>
            {
                builder.SetMinimumLevel(LogLevel.Debug);
                builder.AddProvider(new XUnitLoggerProvider(_output));
            });

            var interceptorLogger = _loggerFactory.CreateLogger<SqlExceptionHandlingInterceptor>();

            // Setup DbContext
            var options = new DbContextOptionsBuilder<BillScannerDbContext>()
                .UseNpgsql(_dbContainer.GetConnectionString())
                .AddInterceptors(new SqlExceptionHandlingInterceptor(interceptorLogger))
                .EnableSensitiveDataLogging()
                .EnableDetailedErrors()
                .Options;

            _dbContext = new BillScannerDbContext(options);
            await _dbContext.Database.EnsureCreatedAsync();

            // Setup UnitOfWork
            _unitOfWork = new UnitOfWork(_dbContext);

            // Setup Handler
            _handler = new RegisterHandler(
                _unitOfWork,
                _tokenServiceMock.Object);

            // Default Token Service Mocks
            SetupTokenService();
        }

        public async Task DisposeAsync()
        {
            await _dbContext.DisposeAsync();
            await _dbContainer.DisposeAsync();
            _loggerFactory.Dispose();
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
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(command.Email, result.Email);
            Assert.Equal(command.DisplayName, result.DisplayName);
            Assert.Equal("access-token", result.AccessToken);
            Assert.NotEqual(Guid.Empty, result.UserId);

            var userInDb = await _dbContext.Users.FirstOrDefaultAsync(u => u.Email == command.Email);
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
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("testuser", result.DisplayName);

            var userInDb = await _dbContext.Users.FirstOrDefaultAsync(u => u.Email == command.Email);
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

            _dbContext.Users.Add(existingUser);
            await _dbContext.SaveChangesAsync();

            var command = new RegisterCommand
            {
                Email = "existing@example.com",
                Password = "Password123!"
            };

            // Act & Assert
            var exception =
                await Assert.ThrowsAsync<UniqueConstraintException>(() =>
                    _handler.Handle(command, CancellationToken.None));

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
            await _handler.Handle(command, CancellationToken.None);

            // Assert
            var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Email == command.Email);
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
            await _handler.Handle(command, CancellationToken.None);

            // Assert
            var count = await _dbContext.Users.CountAsync(u => u.Email == command.Email);
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
            await _handler.Handle(command, CancellationToken.None);

            // Assert
            var user = await _dbContext.Users.FirstAsync(u => u.Email == command.Email);
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
            await _handler.Handle(command, CancellationToken.None);

            // Assert
            _tokenServiceMock.Verify(
                t => t.CreateAccessToken(
                    It.IsAny<User>(),
                    It.Is<List<string>>(roles => roles.Contains("User"))),
                Times.Once);

            _tokenServiceMock.Verify(
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
            await _handler.Handle(command, CancellationToken.None);

            // Assert
            _tokenServiceMock.Verify(
                t => t.CreateAccessToken(It.IsAny<User>(), It.IsAny<List<string>>()),
                Times.Once);

            _tokenServiceMock.Verify(
                t => t.CreateRefreshToken(It.IsAny<User>()),
                Times.Once);

            _tokenServiceMock.Verify(
                t => t.CreateIdToken(It.IsAny<User>(), It.IsAny<List<string>>()),
                Times.Once);
        }

        private void SetupTokenService()
        {
            _tokenServiceMock
                .Setup(t => t.CreateAccessToken(It.IsAny<User>(), It.IsAny<List<string>>()))
                .Returns("access-token");

            _tokenServiceMock
                .Setup(t => t.CreateRefreshToken(It.IsAny<User>()))
                .Returns("refresh-token");

            _tokenServiceMock
                .Setup(t => t.CreateIdToken(It.IsAny<User>(), It.IsAny<List<string>>()))
                .Returns("id-token");
        }
    }
}