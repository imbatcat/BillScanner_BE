using System.Text.Json.Serialization;

namespace Business.Handlers.Authentication.Login.Dto
{
    public record LoginResponse
    {
        [JsonPropertyName("user")] public LoginUserResponse User { get; init; } = null!;
        public string AccessToken { get; init; } = null!;
        public string RefreshToken { get; init; } = null!;
        public string IdToken { get; init; } = null!;
    }

    public record LoginUserResponse
    {
        public Guid Id { get; init; }
        public string Email { get; init; } = null!;
        public string? DisplayName { get; init; }
    }
}