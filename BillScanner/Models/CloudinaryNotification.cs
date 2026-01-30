using System.Text.Json.Serialization;

namespace BillScanner.Models
{
  public class CloudinaryNotification
  {
    [JsonPropertyName("notification_type")]
    public string NotificationType { get; set; }

    [JsonPropertyName("public_id")] public string PublicId { get; set; }

    [JsonPropertyName("timestamp")] public string Timestamp { get; set; }

    [JsonPropertyName("secure_url")] public string SecureUrl { get; set; }
    [JsonPropertyName("signature_key")] public string SignatureKey { get; set; }
  }
}