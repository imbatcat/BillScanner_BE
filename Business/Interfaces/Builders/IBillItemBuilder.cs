using Business.Handlers.Bills.CreateBill.Dto;
using Domain.Entities;

namespace Business.Interfaces.Builders;

public interface IBillItemBuilder : IBuilder<BillItem>
{
    IBillItemBuilder WithName(string? name);
    IBillItemBuilder WithDescription(string? description);
    IBillItemBuilder WithQuantity(decimal? quantity);
    IBillItemBuilder WithUnitPrice(decimal? unitPrice);
    IBillItemBuilder WithTotalPrice(decimal? totalPrice);
    IBillItemBuilder FromDto(BillItemDto dto);
    IBillItemBuilder FromExtractedResult(BillItemExtractionResult result);
    BillItemExtractionResult GetExtractionResult();
}
