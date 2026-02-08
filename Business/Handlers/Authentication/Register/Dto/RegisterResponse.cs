using System.Text.Json.Serialization;

namespace Business.Handlers.Authentication.Register.Dto
{
    public record RegisterResponse
    {
        [JsonPropertyName("user")] public RegisteredUserResponse User { get; init; } = null!;
        public string AccessToken { get; init; } = null!;
        public string RefreshToken { get; init; } = null!;
        public string IdToken { get; init; } = null!;
    }

    public record RegisteredUserResponse
    {
        public Guid Id { get; init; }
        public string Email { get; init; } = null!;
        public string? DisplayName { get; init; }
    }
}