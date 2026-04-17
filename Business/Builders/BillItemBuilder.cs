using Business.Handlers.Bills.CreateBill.Dto;
using Business.Interfaces.Builders;
using Domain.Entities;
using JetBrains.Annotations;

namespace Business.Builders;

[UsedImplicitly]
public class BillItemBuilder : IBillItemBuilder
{
    private BillItem _item = new();

    public IBillItemBuilder WithName(string? name)
    {
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
        _item.Quantity = quantity ?? _item.Quantity;
        return this;
    }

    public IBillItemBuilder WithUnitPrice(decimal? unitPrice)
    {
        _item.UnitPrice = unitPrice ?? _item.UnitPrice;
        return this;
    }

    public IBillItemBuilder WithTotalPrice(decimal? totalPrice)
    {
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

    public BillItem Build() => _item;
}
