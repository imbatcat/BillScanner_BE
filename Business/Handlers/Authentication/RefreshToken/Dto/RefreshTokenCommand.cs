using MediatR;
using System.ComponentModel.DataAnnotations;

namespace Business.Handlers.Authentication.RefreshToken.Dto
{
    public class RefreshTokenCommand : IRequest<RefreshTokenResponse>
    {
        [Required]
        public string RefreshToken { get; set; } = string.Empty;
    }
}
