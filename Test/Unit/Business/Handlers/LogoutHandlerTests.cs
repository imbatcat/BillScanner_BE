using Business.Handlers.Authentication.Logout;
using Business.Handlers.Authentication.Logout.Dto;
using Business.Interfaces.Services;
using Microsoft.Extensions.Logging;
using Moq;
using MediatRUnit = MediatR.Unit;

namespace Test.Unit.Business.Handlers
{
    public class LogoutHandlerTests
    {
        private readonly Mock<IUserTokenService> tokenServiceMock;

        private readonly Mock<ILogger<LogoutHandler>> loggerMock;

        private readonly LogoutHandler handler;

        public LogoutHandlerTests()
        {
            tokenServiceMock = new Mock<IUserTokenService>();
            loggerMock = new Mock<ILogger<LogoutHandler>>();

            handler = new LogoutHandler(
                tokenServiceMock.Object,
                loggerMock.Object);
        }

        [Fact]
        public async Task Handle_ValidUserId_RevokesTokenAndReturnsUnit()
        {
            // Arrange
            var userId = Guid.NewGuid().ToString();
            var logoutCommand = new LogoutCommand { UserId = userId };

            tokenServiceMock
                .Setup(t => t.RevokeUserTokenAsync(userId))
                .Returns(Task.CompletedTask);

            // Act
            var result = await handler.Handle(logoutCommand, CancellationToken.None);

            // Assert
            Assert.Equal(MediatRUnit.Value, result);
            tokenServiceMock.Verify(t => t.RevokeUserTokenAsync(userId), Times.Once);
        }

        [Fact]
        public async Task Handle_ValidUserId_CallsRevokeUserToken()
        {
            // Arrange
            var userId = "test-user-123";
            var LogoutCommand = new LogoutCommand { UserId = userId };

            tokenServiceMock
                .Setup(t => t.RevokeUserTokenAsync(It.IsAny<string>()))
                .Returns(Task.CompletedTask);

            // Act
            await handler.Handle(LogoutCommand, CancellationToken.None);

            // Assert
            tokenServiceMock.Verify(
                t => t.RevokeUserTokenAsync(userId),
                Times.Once);
        }

        [Fact]
        public async Task Handle_SuccessfulLogout_LogsInformation()
        {
            // Arrange
            var userId = Guid.NewGuid().ToString();
            var LogoutCommand = new LogoutCommand { UserId = userId };

            tokenServiceMock
                .Setup(t => t.RevokeUserTokenAsync(It.IsAny<string>()))
                .Returns(Task.CompletedTask);

            // Act
            await handler.Handle(LogoutCommand, CancellationToken.None);

            // Assert
            loggerMock.Verify(
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
            var LogoutCommand = new LogoutCommand { UserId = userId };

            var expectedException = new InvalidOperationException("Token revocation failed");

            tokenServiceMock
                .Setup(t => t.RevokeUserTokenAsync(userId))
                .ThrowsAsync(expectedException);

            // Act & Assert
            var exception =
                await Assert.ThrowsAsync<InvalidOperationException>(() =>
                    handler.Handle(LogoutCommand, CancellationToken.None));

            Assert.Equal("Token revocation failed", exception.Message);
        }

        [Fact]
        public async Task Handle_EmptyUserId_StillCallsRevokeUserToken()
        {
            // Arrange
            var LogoutCommand = new LogoutCommand { UserId = string.Empty };

            tokenServiceMock
                .Setup(t => t.RevokeUserTokenAsync(string.Empty))
                .Returns(Task.CompletedTask);

            // Act
            await handler.Handle(LogoutCommand, CancellationToken.None);

            // Assert
            tokenServiceMock.Verify(
                t => t.RevokeUserTokenAsync(string.Empty),
                Times.Once);
        }

        [Fact]
        public async Task Handle_MultipleLogouts_CallsRevokeForEachUser()
        {
            // Arrange
            var userId1 = "user-1";
            var userId2 = "user-2";

            tokenServiceMock
                .Setup(t => t.RevokeUserTokenAsync(It.IsAny<string>()))
                .Returns(Task.CompletedTask);

            // Act
            await handler.Handle(new LogoutCommand { UserId = userId1 }, CancellationToken.None);
            await handler.Handle(new LogoutCommand { UserId = userId2 }, CancellationToken.None);

            // Assert
            tokenServiceMock.Verify(t => t.RevokeUserTokenAsync(userId1), Times.Once);
            tokenServiceMock.Verify(t => t.RevokeUserTokenAsync(userId2), Times.Once);
            tokenServiceMock.Verify(t => t.RevokeUserTokenAsync(It.IsAny<string>()), Times.Exactly(2));
        }

        [Fact]
        public async Task Handle_CancellationTokenProvided_PassedToAsyncOperations()
        {
            // Arrange
            var userId = Guid.NewGuid().ToString();
            var LogoutCommand = new LogoutCommand { UserId = userId };
            var cts = new CancellationTokenSource();

            tokenServiceMock
                .Setup(t => t.RevokeUserTokenAsync(userId))
                .Returns(Task.CompletedTask);

            // Act
            await handler.Handle(LogoutCommand, cts.Token);

            // Assert
            tokenServiceMock.Verify(t => t.RevokeUserTokenAsync(userId), Times.Once);
        }

        [Fact]
        public async Task Handle_SuccessfulLogout_ReturnsUnitValue()
        {
            // Arrange
            var LogoutCommand = new LogoutCommand { UserId = "test-user" };

            tokenServiceMock
                .Setup(t => t.RevokeUserTokenAsync(It.IsAny<string>()))
                .Returns(Task.CompletedTask);

            // Act
            var result = await handler.Handle(LogoutCommand, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<MediatRUnit>(result);
        }
    }
}