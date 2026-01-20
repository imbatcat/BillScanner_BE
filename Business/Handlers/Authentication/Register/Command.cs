using Business.Handlers.Authentication.Register.Dto;
using MediatR;

namespace Business.Handlers.Authentication.Register
{
    public record RegisterCommand : IRequest<RegisterResponse>
    {
        public string Email { get; init; } = null!;
        public string Password { get; init; } = null!;
        public string? DisplayName { get; init; }
    }
}