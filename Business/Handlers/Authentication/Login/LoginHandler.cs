using Business.Handlers.Authentication.Login.Dto;
using Business.Handlers.Authentication.Login.Spec;
using Business.Interfaces.Repositories;
using Business.Interfaces.Services;
using Domain.Entities;
using MediatR;
using Microsoft.Extensions.Logging;
using System.Security.Cryptography;
using System.Text;

namespace Business.Handlers.Authentication.Login
{
    public class LoginHandler(
        IUnitOfWork _unitOfWork,
        IUserTokenService _tokenService,
        ILogger<LoginHandler> _logger) : IRequestHandler<LoginCommand, LoginResponse>
    {
        public async Task<LoginResponse> Handle(LoginCommand request, CancellationToken cancellationToken)
        {
            var emailSpec = new UserByEmailSpecification(request.Email);
            var user = await _unitOfWork.Repository<User>()
                .GetBySpecificationAsync(emailSpec);

            if (user == null)
            {
                _logger.LogWarning("Login failed: User not found - {Email}", request.Email);
                throw new ArgumentException("Invalid email or password");
            }

            if (!VerifyPassword(request.Password, user.Password))
            {
                _logger.LogWarning("Login failed: Invalid password - {Email}", request.Email);
                throw new ArgumentException("Invalid email or password");
            }

            _logger.LogInformation("User logged in successfully: {Email}", user.Email);

            var roles = new List<string> { "User" };
            var accessToken = _tokenService.CreateAccessToken(user, roles);
            var refreshToken = _tokenService.CreateRefreshToken(user);
            var idToken = _tokenService.CreateIdToken(user, roles);

            return new LoginResponse
            {
                UserId = user.Id,
                Email = user.Email,
                DisplayName = user.DisplayName,
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                IdToken = idToken
            };
        }

        private static string HashPassword(string password)
        {
            using var sha256 = SHA256.Create();
            var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            return Convert.ToBase64String(bytes);
        }

        private static bool VerifyPassword(string password, string hash)
        {
            var passwordHash = HashPassword(password);
            return passwordHash == hash;
        }
    }
}