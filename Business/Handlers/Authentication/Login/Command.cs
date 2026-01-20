using Business.Handlers.Authentication.Login.Dto;
using MediatR;

namespace Business.Handlers.Authentication.Login
{
    public record LoginCommand : IRequest<LoginResponse>
    {
        public string Email { get; init; } = null!;
        public string Password { get; init; } = null!;
    }
}