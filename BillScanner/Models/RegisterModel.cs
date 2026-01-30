namespace BillScanner.Models
{
    public record RegisterModel
    {
        public string Email { get; init; } = null!;
        public string Password { get; init; } = null!;
        public string? DisplayName { get; init; }
    }
}