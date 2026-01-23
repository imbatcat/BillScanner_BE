using MediatR;

namespace Business.Handlers.Authentication.Logout
{
    public record LogoutCommand : IRequest<Unit>
    {
        public string UserId { get; init; } = null!;
    }
}