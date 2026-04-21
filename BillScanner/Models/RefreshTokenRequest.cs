namespace BillScanner.Models
{
    public record RefreshTokenRequest
    {
        public string RefreshToken { get; set; } = string.Empty;
    }
}
