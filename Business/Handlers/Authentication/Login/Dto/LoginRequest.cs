namespace Business.Handlers.Authentication.Login.Dto
{
    public record LoginRequest
    {
        public string Email { get; init; } = null!;
        public string Password { get; init; } = null!;
    }
}
