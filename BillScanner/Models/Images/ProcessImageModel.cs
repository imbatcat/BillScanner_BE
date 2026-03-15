namespace BillScanner.Models.Images;

public record ProcessImageModel
{
    public string Url { get; init; } = null!;
    public bool IsInvoice { get; init; }
}
