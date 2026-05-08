using System.Text.Json.Serialization;

namespace BillScanner.Models
{
  public record WebhookContext(
    [property: JsonPropertyName("custom")] WebhookCustomFields CustomFields
  );

  public record WebhookCustomFields(
    [property: JsonPropertyName("user_id")] string UserId,
    [property: JsonPropertyName("is_invoice")] string? IsInvoice
  );

  public class CloudinaryNotification
  {
    [JsonPropertyName("notification_type")]
    public string? NotificationType { get; set; }

    [JsonPropertyName("public_id")] public string? PublicId { get; set; }

    [JsonPropertyName("timestamp")] public string? Timestamp { get; set; }

    [JsonPropertyName("secure_url")] public string? SecureUrl { get; set; }

    [JsonPropertyName("context")] public WebhookContext? Context { get; set; }

    [JsonPropertyName("signature_key")] public string? SignatureKey { get; set; }
  }
}