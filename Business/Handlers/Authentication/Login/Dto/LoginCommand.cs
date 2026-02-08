using System.ComponentModel.DataAnnotations;
using MediatR;

namespace Business.Handlers.Authentication.Login.Dto
{
    public record LoginCommand : IRequest<LoginResponse>
    {
        [Required, EmailAddress] public string Email { get; init; } = null!;

        [Required] public string Password { get; init; } = null!;
    }
}