using Business.Handlers.Authentication.Logout.Dto;
using Business.Interfaces.Services;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Business.Handlers.Authentication.Logout
{
    public class LogoutHandler(
        IUserTokenService tokenService,
        ILogger<LogoutHandler> logger) : IRequestHandler<LogoutCommand, Unit>
    {
        public async Task<Unit> Handle(LogoutCommand request, CancellationToken cancellationToken)
        {
            // Revoke refresh token
            await tokenService.RevokeUserTokenAsync(request.UserId);

            logger.LogInformation("User logged out successfully: {UserId}", request.UserId);

            return Unit.Value;
        }
    }
}