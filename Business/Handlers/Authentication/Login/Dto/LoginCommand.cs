using MediatR;

namespace Business.Handlers.Authentication.Login.Dto
{
    public record LoginCommand : IRequest<LoginResponse>
    {
        public string Email { get; init; } = null!;
        public string Password { get; init; } = null!;
    }
}