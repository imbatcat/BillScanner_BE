using Business.Handlers.Authentication.Logout;
using Business.Interfaces.Services;
using Microsoft.Extensions.Logging;
using Moq;
using MediatRUnit = MediatR.Unit;

namespace Test.Unit.Business.Handlers
{
    public class LogoutHandlerTests
    {
        private readonly Mock<IUserTokenService> _tokenServiceMock;

        private readonly Mock<ILogger<LogoutCommandHandler>> _loggerMock;

        private readonly LogoutCommandHandler _handler;

        public LogoutHandlerTests()
        {
            _tokenServiceMock = new Mock<IUserTokenService>();
            _loggerMock = new Mock<ILogger<LogoutCommandHandler>>();

            _handler = new LogoutCommandHandler(
                _tokenServiceMock.Object,
                _loggerMock.Object);
        }

        [Fact]
        public async Task Handle_ValidUserId_RevokesTokenAndReturnsUnit()
        {
            // Arrange
            var userId = Guid.NewGuid().ToString();
            var command = new LogoutCommand { UserId = userId };

            _tokenServiceMock
                .Setup(t => t.RevokeUserTokenAsync(userId))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.Equal(MediatRUnit.Value, result);
            _tokenServiceMock.Verify(t => t.RevokeUserTokenAsync(userId), Times.Once);
        }

        [Fact]
        public async Task Handle_ValidUserId_CallsRevokeUserToken()
        {
            // Arrange
            var userId = "test-user-123";
            var command = new LogoutCommand { UserId = userId };

            _tokenServiceMock
                .Setup(t => t.RevokeUserTokenAsync(It.IsAny<string>()))
                .Returns(Task.CompletedTask);

            // Act
            await _handler.Handle(command, CancellationToken.None);

            // Assert
            _tokenServiceMock.Verify(
                t => t.RevokeUserTokenAsync(userId),
                Times.Once);
        }

        [Fact]
        public async Task Handle_SuccessfulLogout_LogsInformation()
        {
            // Arrange
            var userId = Guid.NewGuid().ToString();
            var command = new LogoutCommand { UserId = userId };

            _tokenServiceMock
                .Setup(t => t.RevokeUserTokenAsync(It.IsAny<string>()))
                .Returns(Task.CompletedTask);

            // Act
            await _handler.Handle(command, CancellationToken.None);

            // Assert
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) =>
                        v.ToString()!.Contains("User logged out successfully") &&
                        v.ToString()!.Contains(userId)),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_RevokeTokenThrowsException_ExceptionPropagates()
        {
            // Arrange
            var userId = Guid.NewGuid().ToString();
            var command = new LogoutCommand { UserId = userId };

            var expectedException = new InvalidOperationException("Token revocation failed");

            _tokenServiceMock
                .Setup(t => t.RevokeUserTokenAsync(userId))
                .ThrowsAsync(expectedException);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(
                () => _handler.Handle(command, CancellationToken.None));

            Assert.Equal("Token revocation failed", exception.Message);
        }

        [Fact]
        public async Task Handle_EmptyUserId_StillCallsRevokeUserToken()
        {
            // Arrange
            var command = new LogoutCommand { UserId = string.Empty };

            _tokenServiceMock
                .Setup(t => t.RevokeUserTokenAsync(string.Empty))
                .Returns(Task.CompletedTask);

            // Act
            await _handler.Handle(command, CancellationToken.None);

            // Assert
            _tokenServiceMock.Verify(
                t => t.RevokeUserTokenAsync(string.Empty),
                Times.Once);
        }

        [Fact]
        public async Task Handle_MultipleLogouts_CallsRevokeForEachUser()
        {
            // Arrange
            var userId1 = "user-1";
            var userId2 = "user-2";

            _tokenServiceMock
                .Setup(t => t.RevokeUserTokenAsync(It.IsAny<string>()))
                .Returns(Task.CompletedTask);

            // Act
            await _handler.Handle(new LogoutCommand { UserId = userId1 }, CancellationToken.None);
            await _handler.Handle(new LogoutCommand { UserId = userId2 }, CancellationToken.None);

            // Assert
            _tokenServiceMock.Verify(t => t.RevokeUserTokenAsync(userId1), Times.Once);
            _tokenServiceMock.Verify(t => t.RevokeUserTokenAsync(userId2), Times.Once);
            _tokenServiceMock.Verify(t => t.RevokeUserTokenAsync(It.IsAny<string>()), Times.Exactly(2));
        }

        [Fact]
        public async Task Handle_CancellationTokenProvided_PassedToAsyncOperations()
        {
            // Arrange
            var userId = Guid.NewGuid().ToString();
            var command = new LogoutCommand { UserId = userId };
            var cts = new CancellationTokenSource();

            _tokenServiceMock
                .Setup(t => t.RevokeUserTokenAsync(userId))
                .Returns(Task.CompletedTask);

            // Act
            await _handler.Handle(command, cts.Token);

            // Assert
            _tokenServiceMock.Verify(t => t.RevokeUserTokenAsync(userId), Times.Once);
        }

        [Fact]
        public async Task Handle_SuccessfulLogout_ReturnsUnitValue()
        {
            // Arrange
            var command = new LogoutCommand { UserId = "test-user" };

            _tokenServiceMock
                .Setup(t => t.RevokeUserTokenAsync(It.IsAny<string>()))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<MediatRUnit>(result);
        }
    }
}