using System.ComponentModel.DataAnnotations;
using MediatR;

namespace Business.Handlers.Authentication.Register.Dto
{
    public record RegisterCommand : IRequest<RegisterResponse>
    {
        [Required, EmailAddress] public string Email { get; init; } = null!;

        [Required] public string Password { get; init; } = null!;

        public string? DisplayName { get; init; }
    }
}