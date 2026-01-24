using Business.Handlers.Authentication.Login;
using Business.Handlers.Authentication.Login.Spec;
using Business.Interfaces.Repositories;
using Business.Interfaces.Services;
using Domain.Entities;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit.Abstractions;

namespace Test.Unit.Business.Handlers
{
    public class LoginHandlerTest
    {
        private readonly Mock<IGenericRepository<User>> _userRepositoryMock;

        private readonly Mock<IUnitOfWork> _unitOfWorkMock;

        private readonly Mock<IUserTokenService> _tokenServiceMock;

        private readonly Mock<ILogger<LoginHandler>> _loggerMock;

        private readonly LoginHandler _handler;

        public LoginHandlerTest(ITestOutputHelper output)
        {
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _loggerMock = new Mock<ILogger<LoginHandler>>();

            _loggerMock.Setup(x => x.Log(
                    It.IsAny<LogLevel>(),
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<Exception>(),
                    (Func<It.IsAnyType, Exception?, string>)It.IsAny<object>()))
                .Callback(new InvocationAction(invocation =>
                {
                    var logLevel = (LogLevel)invocation.Arguments[0];
                    var state = invocation.Arguments[2];
                    var exception = (Exception?)invocation.Arguments[3];
                    var formatter = invocation.Arguments[4];

                    var invokeMethod = formatter.GetType().GetMethod("Invoke");
                    var logMessage = (string?)invokeMethod?.Invoke(formatter, new[] { state, exception });

                    output.WriteLine($"[{logLevel}] {logMessage}");
                }));

            _userRepositoryMock = new Mock<IGenericRepository<User>>();
            _tokenServiceMock = new Mock<IUserTokenService>();
            _handler = new LoginHandler(
                _unitOfWorkMock.Object,
                _tokenServiceMock.Object,
                _loggerMock.Object);
        }

        [Fact]
        public async Task LoginHandler_SuccessfulLogin_LoginSuccess()
        {
            // Arrange
            var query = new LoginCommand
            {
                Email = "testemail@password",
                Password = "password"
            };

            var user = new User
            {
                Id = Guid.NewGuid(),
                Email = "test@example.com",
                Password = HashPassword("password"),
                DisplayName = "Test User"
            };

            SetupSuccessfulLogin(user);

            // Act
            await _handler.Handle(query, CancellationToken.None);
        }

        [Fact]
        public async Task LoginHandler_FailedLoginUserNotFound_LoginFailed()
        {
            // Arrange
            var query = new LoginCommand
            {
                Email = "testemail@password",
                Password = "password"
            };

            SetupNullUserByEmail();

            // Act & Assert
            var exception =
                await Assert.ThrowsAsync<ArgumentException>(async () =>
                    await _handler.Handle(query, CancellationToken.None));

            Assert.NotNull(exception);
        }

        [Fact]
        public async Task LoginHandler_FailedLoginInvalidPassword_LoginFailed()
        {
            // Arrange
            var query = new LoginCommand
            {
                Email = "testemail@password",
                Password = "password"
            };

            var user = new User
            {
                Id = Guid.NewGuid(),
                Email = "testemail@password",
                Password = HashPassword("differentpassword"),
            };

            SetupInvalidPassword(user);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(async () =>
                await _handler.Handle(query, CancellationToken.None));

            Assert.NotNull(exception);
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Invalid password")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        private void SetupInvalidPassword(User user)
        {
            _userRepositoryMock.Setup(x => x.GetBySpecificationAsync(It.IsAny<UserByEmailSpecification>(), true))
                .ReturnsAsync(user);
            _unitOfWorkMock.Setup(x => x.Repository<User>()).Returns(_userRepositoryMock.Object);
        }

        private void SetupNullUserByEmail()
        {
            _userRepositoryMock.Setup(x => x.GetBySpecificationAsync(It.IsAny<UserByEmailSpecification>(), true))
                .ReturnsAsync((User?)null);
            _unitOfWorkMock.Setup(x => x.Repository<User>()).Returns(_userRepositoryMock.Object);
        }

        private void SetupSuccessfulLogin(User user)
        {
            _userRepositoryMock.Setup(x => x.GetBySpecificationAsync(It.IsAny<UserByEmailSpecification>(), true))
                .ReturnsAsync(user);
            _unitOfWorkMock.Setup(x => x.Repository<User>()).Returns(_userRepositoryMock.Object);

            _tokenServiceMock.Setup(x => x.CreateAccessToken(It.IsAny<User>(), It.IsAny<List<string>>()))
                .Returns("access-token");
            _tokenServiceMock.Setup(x => x.CreateRefreshToken(It.IsAny<User>()))
                .Returns("refresh-token");
            _tokenServiceMock.Setup(x => x.CreateIdToken(It.IsAny<User>(), It.IsAny<List<string>>()))
                .Returns("id-token");
        }

        private static string HashPassword(string password)
        {
            using var sha256 = System.Security.Cryptography.SHA256.Create();
            var bytes = sha256.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
            return Convert.ToBase64String(bytes);
        }
    }
}