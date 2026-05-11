using Domain.Entities;

namespace Business.Handlers.Bills.GetBillDetails.Dto;

public record BillDto(
    Guid Id,
    string? MerchantName,
    string? MerchantBankName,
    string? MerchantBankNumber,
    DateOnly BillDate,
    TimeSpan? BillTime,
    decimal? Total,
    decimal? SubTotal,
    decimal? Tax,
    decimal Discount,
    string Currency,
    ExtractionMethod ExtractionMethod,
    List<BillItemDto> Items
)
{
    public static BillDto From(Bill bill) => new(
        bill.Id,
        bill.MerchantName,
        bill.MerchantBankName,
        bill.MerchantBankNumber,
        bill.BillDate,
        bill.BillTime,
        bill.Total,
        bill.SubTotal,
        bill.Tax,
        bill.Discount,
        bill.Currency,
        bill.ExtractionMethod,
        bill.BillItems.Select(i => new BillItemDto(i.ItemName, i.Quantity, i.UnitPrice, i.TotalPrice)).ToList()
    );
}
