namespace BillScanner.Models
{
    public record LoginModel
    {
        public string Email { get; init; } = null!;
        public string Password { get; init; } = null!;
    }
}