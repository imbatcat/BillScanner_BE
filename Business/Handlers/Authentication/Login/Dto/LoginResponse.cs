namespace Business.Handlers.Authentication.Login.Dto
{
    public record LoginResponse
    {
        public Guid UserId { get; init; }
        public string Email { get; init; } = null!;
        public string? DisplayName { get; init; }
        public string AccessToken { get; init; } = null!;
        public string RefreshToken { get; init; } = null!;
        public string IdToken { get; init; } = null!;
    }
}