using Business.Handlers.Bills.CreateBill.Dto;
using Business.Interfaces.Builders;
using Domain.Entities;
using JetBrains.Annotations;

namespace Business.Builders;

[UsedImplicitly]
public class BillItemBuilder : IBillItemBuilder
{
    private BillItem _item = new();
    private BillItemExtractionResult _extractionResult = new();

    public IBillItemBuilder WithName(string? name)
    {
        if (name != null && name != _extractionResult.ExtractedItemName)
            _extractionResult.IsItemNameCorrect = false;
        _item.ItemName = name ?? _item.ItemName;
        return this;
    }

    public IBillItemBuilder WithDescription(string? description)
    {
        _item.Description = description ?? _item.Description;
        return this;
    }

    public IBillItemBuilder WithQuantity(decimal? quantity)
    {
        if (quantity != null && quantity != _extractionResult.ExtractedQuantity)
            _extractionResult.IsQuantityCorrect = false;
        _item.Quantity = quantity ?? _item.Quantity;
        return this;
    }

    public IBillItemBuilder WithUnitPrice(decimal? unitPrice)
    {
        if (unitPrice != null && unitPrice != _extractionResult.ExtractedUnitPrice)
            _extractionResult.IsUnitPriceCorrect = false;
        _item.UnitPrice = unitPrice ?? _item.UnitPrice;
        return this;
    }

    public IBillItemBuilder WithTotalPrice(decimal? totalPrice)
    {
        if (totalPrice != null && totalPrice != _extractionResult.ExtractedTotalPrice)
            _extractionResult.IsTotalPriceCorrect = false;
        _item.TotalPrice = totalPrice ?? _item.TotalPrice;
        return this;
    }

    public IBillItemBuilder FromDto(BillItemDto dto)
    {
        return this
            .WithName(dto.ItemName)
            .WithQuantity(dto.Quantity)
            .WithUnitPrice(dto.UnitPrice)
            .WithTotalPrice(dto.TotalPrice);
    }

    public IBillItemBuilder FromExtractedResult(BillItemExtractionResult result)
    {
        _extractionResult = result;
        _item.ItemName = result.ExtractedItemName ?? "Unknown";
        _item.Quantity = result.ExtractedQuantity ?? 1;
        _item.UnitPrice = result.ExtractedUnitPrice ?? 0;
        _item.TotalPrice = result.ExtractedTotalPrice ?? 0;

        // Initialize flags to true initially
        _extractionResult.IsItemNameCorrect = true;
        _extractionResult.IsQuantityCorrect = true;
        _extractionResult.IsUnitPriceCorrect = true;
        _extractionResult.IsTotalPriceCorrect = true;

        return this;
    }

    public BillItem Build() => _item;
    public BillItemExtractionResult GetExtractionResult() => _extractionResult;
}
