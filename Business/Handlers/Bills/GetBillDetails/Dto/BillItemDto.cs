namespace Business.Handlers.Bills.GetBillDetails.Dto;

public record BillItemDto(
    string ItemName,
    decimal Quantity,
    decimal UnitPrice,
    decimal TotalPrice
);
