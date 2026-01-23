namespace Business.Handlers.Authentication.Register.Dto
{
    public record RegisterRequest
    {
        public string Email { get; init; } = null!;
        public string Password { get; init; } = null!;
        public string? DisplayName { get; init; }
    }
}
