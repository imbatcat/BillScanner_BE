using Business.Handlers.Bills.CreateBill.Dto;
using Business.Interfaces.Builders;
using Domain.Entities;
using JetBrains.Annotations;

namespace Business.Builders;

[UsedImplicitly]
public class BillBuilder(IBuilderFactory builderFactory) : IBillBuilder
{
    private Bill _bill = new();

    public IBillBuilder WithMerchant(string? name)
    {
        _bill.MerchantName = name ?? _bill.MerchantName;
        return this;
    }

    public IBillBuilder WithDate(DateOnly date)
    {
        _bill.BillDate = date != default ? date : _bill.BillDate;
        return this;
    }

    public IBillBuilder WithTime(TimeSpan? time)
    {
        _bill.BillTime = time ?? _bill.BillTime;
        return this;
    }

    public IBillBuilder WithUserId(Guid userId)
    {
        _bill.UserId = userId;
        return this;
    }

    public IBillBuilder WithImgUrl(string? url)
    {
        _bill.ImgUrl = url;
        return this;
    }

    public IBillBuilder WithTotal(decimal? total)
    {
        _bill.Total = total ?? _bill.Total;
        return this;
    }

    public IBillBuilder WithSubTotal(decimal? subTotal)
    {
        _bill.SubTotal = subTotal ?? _bill.SubTotal;
        return this;
    }

    public IBillBuilder WithTax(decimal? tax)
    {
        _bill.Tax = tax ?? _bill.Tax;
        return this;
    }

    public IBillBuilder WithDiscount(decimal? discount)
    {
        _bill.Discount = discount ?? _bill.Discount;
        return this;
    }

    public IBillBuilder WithCurrency(string? currency)
    {
        if (currency != null)
            _bill.Currency = currency;
        return this;
    }

    public IBillBuilder WithMerchantBank(string? bank)
    {
        _bill.MerchantBankName = bank ?? _bill.MerchantBankName;
        return this;
    }

    public IBillBuilder WithMerchantBankNumber(string? number)
    {
        _bill.MerchantBankNumber = number ?? _bill.MerchantBankNumber;
        return this;
    }

    public IBillBuilder WithExtractionMethod(ExtractionMethod method)
    {
        _bill.ExtractionMethod = method;
        return this;
    }

    public IBillBuilder WithItems(List<BillItemDto> items)
    {
        if (items is not { Count: > 0 }) return this;

        _bill.BillItems = items.Select(i =>
            builderFactory.Builder<IBillItemBuilder>()
                .FromDto(i)
                .Build()
        ).ToList();
        return this;
    }

    public IBillBuilder WithUserEdits(UserEditsDto dto)
    {
        return this
            .WithMerchant(dto.MerchantName)
            .WithMerchantBank(dto.MerchantBankName)
            .WithMerchantBankNumber(dto.MerchantBankNumber)
            .WithDate(dto.BillDate)
            .WithTime(dto.BillTime)
            .WithTotal(dto.Total)
            .WithSubTotal(dto.SubTotal)
            .WithTax(dto.Tax)
            .WithDiscount(dto.Discount)
            .WithCurrency(dto.Currency)
            .WithItems(dto.Items);
    }

    public IBillBuilder FromProcessResult(Handlers.Images.ProcessImage.Dto.ImageProcessing.ImageProcessResult result)
    {
        _bill.MerchantName = result.Vendor.Name.Value;
        _bill.BillDate = result.BillDate.Value ?? DateOnly.FromDateTime(DateTime.UtcNow);
        _bill.BillTime = result.BillTime.Value;
        _bill.SubTotal = result.SubTotal.Value;
        _bill.Tax = result.Tax.Value;
        _bill.Total = result.Total.Value;
        _bill.Currency = result.Currency.Value ?? _bill.Currency;
        _bill.ExtractionMethod = ExtractionMethod.Ocr;

        _bill.BillItems = result.Items.Select(i =>
            builderFactory.Builder<IBillItemBuilder>()
                .WithName(i.ItemName.Value)
                .WithQuantity(i.Quantity.Value)
                .WithUnitPrice(i.UnitPrice.Value)
                .WithTotalPrice(i.TotalPrice.Value)
                .Build()
        ).ToList();

        return this;
    }

    public Bill Build() => _bill;
}
