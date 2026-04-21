using Business.Handlers.Authentication.RefreshToken.Dto;
using Business.Interfaces.Repositories;
using Business.Interfaces.Services;
using Domain.Entities;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Business.Handlers.Authentication.RefreshToken
{
    public class RefreshTokenHandler(
        IUnitOfWork unitOfWork,
        IUserTokenService tokenService,
        ILogger<RefreshTokenHandler> logger) : IRequestHandler<RefreshTokenCommand, RefreshTokenResponse>
    {
        public async Task<RefreshTokenResponse> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
        {
            var userId = await tokenService.ValidateRefreshTokenAsync(request.RefreshToken);

            if (userId == null || !Guid.TryParse(userId, out var guidUserId))
            {
                logger.LogWarning("Refresh token validation failed");
                throw new UnauthorizedAccessException("Invalid or expired refresh token");
            }

            var user = await unitOfWork.Repository<User>().GetByIdAsync(guidUserId);

            if (user == null)
            {
                logger.LogWarning("Refresh token rejected: user {UserId} not found", userId);
                throw new UnauthorizedAccessException("Invalid or expired refresh token");
            }

            var roles = new List<string> { "User" };
            var newAccessToken = tokenService.CreateAccessToken(user, roles);
            var newRefreshToken = tokenService.CreateRefreshToken(user);

            logger.LogInformation("Tokens rotated for user {UserId}", userId);

            return new RefreshTokenResponse
            {
                AccessToken = newAccessToken,
                RefreshToken = newRefreshToken
            };
        }
    }
}
