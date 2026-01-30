using MediatR;

namespace Business.Handlers.Authentication.Logout.Dto
{
    public record LogoutCommand : IRequest<Unit>
    {
        public string UserId { get; init; } = null!;
    }
}