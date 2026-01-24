using Business.Handlers.Authentication.Register.Dto;
using Business.Handlers.Authentication.Register.Spec;
using Business.Interfaces.Repositories;
using Business.Interfaces.Services;
using Domain.Entities;
using MediatR;
using System.Security.Cryptography;
using System.Text;

namespace Business.Handlers.Authentication.Register
{
    public class RegisterHandler(
        IUnitOfWork _unitOfWork,
        IUserTokenService _tokenService
    ) : IRequestHandler<RegisterCommand, RegisterResponse>
    {
        public async Task<RegisterResponse> Handle(RegisterCommand request, CancellationToken cancellationToken)
        {
            var passwordHash = HashPassword(request.Password);

            var user = new User
            {
                Email = request.Email,
                Password = passwordHash,
                DisplayName = request.DisplayName ?? request.Email.Split('@')[0]
            };

            _unitOfWork.Repository<User>().Insert(user);
            await _unitOfWork.CommitAsync();

            GenerateTokens(user, out var accessToken, out var refreshToken, out var idToken);

            return new RegisterResponse
            {
                UserId = user.Id,
                Email = user.Email,
                DisplayName = user.DisplayName,
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                IdToken = idToken
            };
        }

        private void GenerateTokens(User user, out string accessToken, out string refreshToken, out string idToken)
        {
            var roles = new List<string> { "User" };
            accessToken = _tokenService.CreateAccessToken(user, roles);
            refreshToken = _tokenService.CreateRefreshToken(user);
            idToken = _tokenService.CreateIdToken(user, roles);
        }

        private static string HashPassword(string password)
        {
            using var sha256 = SHA256.Create();
            var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            return Convert.ToBase64String(bytes);
        }
    }
}