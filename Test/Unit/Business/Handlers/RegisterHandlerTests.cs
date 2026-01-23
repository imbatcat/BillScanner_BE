using Business.Handlers.Authentication.Register;
using Business.Handlers.Authentication.Register.Spec;
using Business.Interfaces.Repositories;
using Business.Interfaces.Services;
using Domain.Entities;
using Microsoft.Extensions.Logging;
using Moq;

namespace Test.Unit.Business.Handlers
{
    public class RegisterHandlerTests
    {
        private readonly Mock<IGenericRepository<User>> _userRepositoryMock;

        private readonly Mock<IUnitOfWork> _unitOfWorkMock;

        private readonly Mock<IUserTokenService> _tokenServiceMock;

        private readonly Mock<ILogger<CommandHandler>> _loggerMock;

        private readonly CommandHandler _handler;

        public RegisterHandlerTests()
        {
            _userRepositoryMock = new Mock<IGenericRepository<User>>();
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _tokenServiceMock = new Mock<IUserTokenService>();
            _loggerMock = new Mock<ILogger<CommandHandler>>();

            _handler = new CommandHandler(
                _unitOfWorkMock.Object,
                _tokenServiceMock.Object);
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

            // User doesn't exist
            _userRepositoryMock
                .Setup(r => r.GetBySpecificationAsync(It.IsAny<UserByEmailSpecification>(), true))
                .ReturnsAsync((User?)null);

            _unitOfWorkMock
                .Setup(u => u.Repository<User>())
                .Returns(_userRepositoryMock.Object);

            // Setup successful insert
            _userRepositoryMock
                .Setup(r => r.Insert(It.IsAny<User>()))
                .Returns<User>(u => u);

            _unitOfWorkMock
                .Setup(u => u.CommitAsync())
                .ReturnsAsync(1);

            // Setup token service
            SetupTokenService();

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(command.Email, result.Email);
            Assert.Equal(command.DisplayName, result.DisplayName);
            Assert.Equal("access-token", result.AccessToken);
            Assert.Equal("refresh-token", result.RefreshToken);
            Assert.Equal("id-token", result.IdToken);
            Assert.NotEqual(Guid.Empty, result.UserId);
        }

        [Fact]
        public async Task Handle_NewUserWithoutDisplayName_UsesEmailPrefix()
        {
            // Arrange
            var command = new RegisterCommand
            {
                Email = "testuser@example.com",
                Password = "Password123!",
                DisplayName = null // No display name provided
            };

            SetupSuccessfulRegistration();

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.NotNull(result.DisplayName);
            // Verify the user was inserted with email prefix as display name
            _userRepositoryMock.Verify(
                r => r.Insert(It.Is<User>(u => u.DisplayName == "testuser")),
                Times.Once);
        }

        [Fact]
        public async Task Handle_DuplicateEmail_ThrowsInvalidOperationException()
        {
            // Arrange
            var command = new RegisterCommand
            {
                Email = "existing@example.com",
                Password = "Password123!"
            };

            var existingUser = new User
            {
                Id = Guid.NewGuid(),
                Email = "existing@example.com",
                Password = "hashed-password"
            };

            // User already exists
            _userRepositoryMock
                .Setup(r => r.GetBySpecificationAsync(It.IsAny<UserByEmailSpecification>(), true))
                .ReturnsAsync(existingUser);

            _unitOfWorkMock
                .Setup(u => u.Repository<User>())
                .Returns(_userRepositoryMock.Object);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(
                () => _handler.Handle(command, CancellationToken.None));

            Assert.Equal("User with this email already exists", exception.Message);

            // Verify Insert was never called
            _userRepositoryMock.Verify(r => r.Insert(It.IsAny<User>()), Times.Never);
            _unitOfWorkMock.Verify(u => u.CommitAsync(), Times.Never);
        }

        [Fact]
        public async Task Handle_SuccessfulRegistration_LogsInformation()
        {
            // Arrange
            var command = new RegisterCommand
            {
                Email = "test@example.com",
                Password = "Password123!",
                DisplayName = "Test User"
            };

            SetupSuccessfulRegistration();

            // Act
            await _handler.Handle(command, CancellationToken.None);

            // Assert - Verify logging
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("User registered successfully")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_SuccessfulRegistration_CallsCommitAsync()
        {
            // Arrange
            var command = new RegisterCommand
            {
                Email = "test@example.com",
                Password = "Password123!",
                DisplayName = "Test User"
            };

            SetupSuccessfulRegistration();

            // Act
            await _handler.Handle(command, CancellationToken.None);

            // Assert
            _unitOfWorkMock.Verify(u => u.CommitAsync(), Times.Once);
        }

        [Fact]
        public async Task Handle_SuccessfulRegistration_InsertsUserWithHashedPassword()
        {
            // Arrange
            var command = new RegisterCommand
            {
                Email = "test@example.com",
                Password = "PlainTextPassword",
                DisplayName = "Test User"
            };

            SetupSuccessfulRegistration();

            // Act
            await _handler.Handle(command, CancellationToken.None);

            // Assert - Verify password was hashed (not stored as plain text)
            _userRepositoryMock.Verify(
                r => r.Insert(It.Is<User>(u =>
                    u.Email == command.Email &&
                    u.Password != "PlainTextPassword" &&
                    !string.IsNullOrEmpty(u.Password))),
                Times.Once);
        }

        [Fact]
        public async Task Handle_SuccessfulRegistration_CallsTokenServiceWithUserRole()
        {
            // Arrange
            var command = new RegisterCommand
            {
                Email = "test@example.com",
                Password = "Password123!",
                DisplayName = "Test User"
            };

            SetupSuccessfulRegistration();

            // Act
            await _handler.Handle(command, CancellationToken.None);

            // Assert - Verify token service was called with "User" role
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
                Email = "test@example.com",
                Password = "Password123!",
                DisplayName = "Test User"
            };

            SetupSuccessfulRegistration();

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

        #region Helper Methods

        private void SetupSuccessfulRegistration()
        {
            // User doesn't exist
            _userRepositoryMock
                .Setup(r => r.GetBySpecificationAsync(It.IsAny<UserByEmailSpecification>(), true))
                .ReturnsAsync((User?)null);

            _unitOfWorkMock
                .Setup(u => u.Repository<User>())
                .Returns(_userRepositoryMock.Object);

            _userRepositoryMock
                .Setup(r => r.Insert(It.IsAny<User>()))
                .Returns<User>(u => u);

            _unitOfWorkMock
                .Setup(u => u.CommitAsync())
                .ReturnsAsync(1);

            SetupTokenService();
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

        #endregion Helper Methods
    }
}