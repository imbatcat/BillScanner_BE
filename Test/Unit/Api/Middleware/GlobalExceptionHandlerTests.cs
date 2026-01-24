using BillScanner.Controllers.Base;
using BillScanner.Middleware;
using EntityFramework.Exceptions.Common;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Moq;
using System.Text.Json;

namespace Test.Unit.Api.Middleware
{
    public class GlobalExceptionHandlerTests : IDisposable
    {
        private readonly Mock<ILogger<GlobalExceptionHandler>> _loggerMock;

        private readonly GlobalExceptionHandler _handler;

        private readonly DefaultHttpContext _httpContext;

        private readonly MemoryStream _responseBody;

        public GlobalExceptionHandlerTests()
        {
            _loggerMock = new Mock<ILogger<GlobalExceptionHandler>>();
            _handler = new GlobalExceptionHandler(_loggerMock.Object);

            // Setup HttpContext with mocked services
            _httpContext = new DefaultHttpContext();
            _responseBody = new MemoryStream();
            _httpContext.Response.Body = _responseBody;

            // Mock IHostEnvironment for Development/Production checks
            var hostEnvironmentMock = new Mock<IHostEnvironment>();
            hostEnvironmentMock.Setup(x => x.EnvironmentName).Returns("Development");

            var serviceProviderMock = new Mock<IServiceProvider>();
            serviceProviderMock
                .Setup(x => x.GetService(typeof(IHostEnvironment)))
                .Returns(hostEnvironmentMock.Object);

            _httpContext.RequestServices = serviceProviderMock.Object;
        }

        #region Status Code Mapping Tests

        [Theory]
        [InlineData(typeof(UnauthorizedAccessException), 401)]
        [InlineData(typeof(InvalidOperationException), 400)]
        [InlineData(typeof(ArgumentException), 400)]
        [InlineData(typeof(KeyNotFoundException), 404)]
        [InlineData(typeof(TaskCanceledException), 408)]
        [InlineData(typeof(OperationCanceledException), 408)]
        public async Task TryHandleAsync_KnownExceptionTypes_ReturnsCorrectStatusCode(
            Type exceptionType,
            int expectedStatusCode)
        {
            // Arrange
            var exception = (Exception)Activator.CreateInstance(exceptionType, "Test message")!;

            // Act
            var result = await _handler.TryHandleAsync(
                _httpContext,
                exception,
                CancellationToken.None);

            // Assert
            Assert.True(result, "Handler should return true (exception handled)");
            Assert.Equal(expectedStatusCode, _httpContext.Response.StatusCode);
            Assert.Contains("application/json", _httpContext.Response.ContentType);
        }

        [Theory]
        [InlineData(typeof(UniqueConstraintException), 409)]
        [InlineData(typeof(ReferenceConstraintException), 409)]
        [InlineData(typeof(CannotInsertNullException), 400)]
        [InlineData(typeof(MaxLengthExceededException), 400)]
        [InlineData(typeof(NumericOverflowException), 400)]
        public async Task TryHandleAsync_DatabaseExceptions_ReturnsCorrectStatusCode(
            Type exceptionType,
            int expectedStatusCode)
        {
            // Arrange
            var exception = (Exception)Activator.CreateInstance(exceptionType, "DB error")!;

            // Act
            var result = await _handler.TryHandleAsync(
                _httpContext,
                exception,
                CancellationToken.None);

            // Assert
            Assert.True(result);
            Assert.Equal(expectedStatusCode, _httpContext.Response.StatusCode);
        }

        #endregion Status Code Mapping Tests

        #region Logging Tests

        [Fact]
        public async Task TryHandleAsync_AnyException_LogsError()
        {
            // Arrange
            var exception = new Exception("Test error");

            // Act
            await _handler.TryHandleAsync(_httpContext, exception, CancellationToken.None);

            // Assert - Verify initial LogError was called
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("An exception occurred")),
                    exception,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public async Task TryHandleAsync_UnauthorizedException_LogsWarning()
        {
            // Arrange
            var exception = new UnauthorizedAccessException("Unauthorized");

            // Act
            await _handler.TryHandleAsync(_httpContext, exception, CancellationToken.None);

            // Assert - Verify LogWarning was called
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Unauthorized access")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public async Task TryHandleAsync_UniqueConstraintException_LogsWarning()
        {
            // Arrange
            var exception = new UniqueConstraintException("Duplicate");

            // Act
            await _handler.TryHandleAsync(_httpContext, exception, CancellationToken.None);

            // Assert
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Unique constraint violation")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public async Task TryHandleAsync_UnhandledException_LogsErrorTwice()
        {
            // Arrange
            var exception = new Exception("Unhandled error");

            // Act
            await _handler.TryHandleAsync(_httpContext, exception, CancellationToken.None);

            // Assert - Verify both initial and final LogError were called
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Exactly(2));
        }

        #endregion Logging Tests

        public void Dispose()
        {
            _responseBody?.Dispose();
        }
    }
}