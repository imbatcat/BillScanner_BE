using System.Security.Claims;
using System.Text;
using Business.Interfaces.Services;
using Domain.Entities;
using Business.Interfaces.Repositories;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using Infrastructure.MarkerInterfaces;
using JetBrains.Annotations;

namespace Infrastructure.Services
{
    [UsedImplicitly]
    public class UserTokenService(
        IConfiguration configuration,
        IUnitOfWork unitOfWork
    ) : IUserTokenService, IScopedService
    {
        public string CreateAccessToken(User user, List<string> roles)
        {
            var jwtSettings = configuration.GetSection("JwtAccessTokenSettings");
            string secretKey = jwtSettings["Secret"]!;
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));

            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var tokenDescriptior = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(
                [
                    new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                    new Claim(ClaimTypes.Email, user.Email),
                    new Claim(ClaimTypes.Name, user.DisplayName ?? ""),
                    new Claim(ClaimTypes.Role, string.Join(",", roles)),
                ]),
                Expires =
                    DateTime.Now.AddMinutes(configuration.GetValue<int>("JwtAccessTokenSettings:ExpirationInMinutes")),
                SigningCredentials = credentials,
                Audience = jwtSettings["Audience"],
                Issuer = jwtSettings["Issuer"]!,
            };

            var handler = new JsonWebTokenHandler();

            string token = handler.CreateToken(tokenDescriptior);

            return token;
        }

        public string CreateIdToken(User user, List<string> roles)
        {
            var jwtSettings = configuration.GetSection("JwtIDTokenSettings");
            string secretKey = jwtSettings["Secret"]!;
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));

            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var audiences = jwtSettings.GetSection("Audience").Get<string[]>() ?? [];

            var claimsList = new List<Claim>()
            {
                new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new(JwtRegisteredClaimNames.Email, user.Email),
                new(JwtRegisteredClaimNames.Name, user.DisplayName ?? ""),
                new("role", string.Join(",", roles)),
                new("aud", audiences[0]),
            };

            var tokenDescriptior = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claimsList),
                Expires = DateTime.Now.AddMinutes(configuration.GetValue<int>("JwtIDTokenSettings:ExpirationInMinutes")),
                SigningCredentials = credentials,
                Issuer = jwtSettings["Issuer"]!,
            };

            var handler = new JsonWebTokenHandler();

            string token = handler.CreateToken(tokenDescriptior);

            return token;
        }

        public string CreateRefreshToken(User user)
        {
            var jwtSettings = configuration.GetSection("JwtRefreshTokenSettings");

            string secretKey = jwtSettings["Secret"]!;
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));

            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Claims = new Dictionary<string, object>
                {
                    { JwtRegisteredClaimNames.Sub, user.Id.ToString() }
                },
                Expires = DateTime.Now.AddMinutes(
                    configuration.GetValue<int>("JwtRefreshTokenSettings:ExpirationInMinutes")),
                SigningCredentials = credentials,
                Issuer = jwtSettings["Issuer"]!,
            };

            var handler = new JsonWebTokenHandler();

            string token = handler.CreateToken(tokenDescriptor);

            return token;
        }

        public async Task RevokeUserTokenAsync(string userId)
        {
            if (Guid.TryParse(userId, out var guidUserId))
            {
                var user = await unitOfWork.Repository<User>().GetByIdAsync(guidUserId);
                if (user == null)
                {
                    throw new KeyNotFoundException($"User with ID {userId} not found");
                }

                //user.RefreshToken = null;
                unitOfWork.Repository<User>().Update(user);
                await unitOfWork.CommitAsync();
            }
        }

        public async Task<string?> ValidateRefreshTokenAsync(string refreshToken)
        {
            try
            {
                var jwtSettings = configuration.GetSection("JwtRefreshTokenSettings");
                string secretKey = jwtSettings["Secret"]!;

                var handler = new JsonWebTokenHandler();
                var result = await handler.ValidateTokenAsync(refreshToken, new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
                    ValidateIssuer = true,
                    ValidIssuer = jwtSettings["Issuer"],
                    ValidateAudience = false,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero
                });

                if (!result.IsValid)
                {
                    return null;
                }

                result.Claims.TryGetValue(JwtRegisteredClaimNames.Sub, out var userIdObj);
                var userId = userIdObj?.ToString();

                if (!Guid.TryParse(userId, out _))
                {
                    return null;
                }

                return userId;
            }
            catch
            {
                return null;
            }
        }
    }
}