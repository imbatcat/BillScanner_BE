using Business.Handlers.Bills.CreateBill.Dto;
using Business.Interfaces.Builders;
using Domain.Entities;
using JetBrains.Annotations;

namespace Business.Builders;

[UsedImplicitly]
public class BillBuilder(IBuilderFactory builderFactory) : IBillBuilder
{
    private Bill _bill = new();
    private BillExtractionResult _extractionResult = new();
    private readonly List<BillItemExtractionResult> _itemExtractionResults = [];

    public IBillBuilder WithMerchant(string? name)
    {
        if (name != null && name != _extractionResult.ExtractedMerchantName)
            _extractionResult.IsMerchantNameCorrect = false;
        _bill.MerchantName = name ?? _bill.MerchantName;
        return this;
    }

    public IBillBuilder WithDate(DateOnly date)
    {
        if (date != default && date != _extractionResult.ExtractedBillDate) _extractionResult.IsBillDateCorrect = false;
        _bill.BillDate = date != default ? date : _bill.BillDate;
        return this;
    }

    public IBillBuilder WithTime(TimeSpan? time)
    {
        if (time != null && time != _extractionResult.ExtractedBillTime) _extractionResult.IsBillTimeCorrect = false;
        _bill.BillTime = time ?? _bill.BillTime;
        return this;
    }

    public IBillBuilder WithUserId(Guid userId)
    {
        _bill.UserId = userId;
        return this;
    }

    public IBillBuilder WithImgUrl(string url)
    {
        _bill.ImgUrl = url;
        return this;
    }

    public IBillBuilder WithTotal(decimal? total)
    {
        if (total != null && total != _extractionResult.ExtractedTransactionAmount)
            _extractionResult.IsTransactionAmountCorrect = false;
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

    public IBillBuilder WithItems(List<BillItemDto> items)
    {
        if (items is not { Count: > 0 }) return this;

        // for user edits, we might need a more sophisticated way to match items
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
            .WithDate(dto.BillDate)
            .WithTime(dto.BillTime)
            .WithTotal(dto.Total)
            .WithSubTotal(dto.SubTotal)
            .WithTax(dto.Tax)
            .WithItems(dto.Items);
    }

    public IBillBuilder FromProcessResult(Handlers.Images.ProcessImage.Dto.ImageProcessing.ImageProcessResult result)
    {
        _extractionResult = new BillExtractionResult
        {
            ExtractedMerchantName = result.Vendor.Name.Value,
            MerchantNameConfidence = result.Vendor.Name.Confidence,
            IsMerchantNameCorrect = true,

            ExtractedBillDate = result.BillDate.Value,
            BillDateConfidence = result.BillDate.Confidence,
            IsBillDateCorrect = true,

            ExtractedBillTime = result.BillTime.Value,
            BillTimeConfidence = result.BillTime.Confidence,
            IsBillTimeCorrect = true,

            ExtractedTransactionAmount = result.Total.Value,
            TransactionAmountConfidence = result.Total.Confidence,
            IsTransactionAmountCorrect = true,

            ExtractedCurrency = result.Currency.Value,
            CurrencyConfidence = result.Currency.Confidence,
            IsCurrencyCorrect = true
        };

        _bill.MerchantName = _extractionResult.ExtractedMerchantName;
        _bill.BillDate = _extractionResult.ExtractedBillDate ?? DateOnly.FromDateTime(DateTime.UtcNow);
        _bill.BillTime = _extractionResult.ExtractedBillTime;
        _bill.SubTotal = result.SubTotal.Value;
        _bill.Tax = result.Tax.Value;
        _bill.Total = _extractionResult.ExtractedTransactionAmount;

        _bill.BillItems = result.Items.Select(i =>
        {
            var itemExtraction = new BillItemExtractionResult
            {
                ExtractedItemName = i.ItemName.Value,
                ItemNameConfidence = i.ItemName.Confidence,
                ExtractedQuantity = i.Quantity.Value,
                QuantityConfidence = i.Quantity.Confidence,
                ExtractedUnitPrice = i.UnitPrice.Value,
                UnitPriceConfidence = i.UnitPrice.Confidence,
                ExtractedTotalPrice = i.TotalPrice.Value,
                TotalPriceConfidence = i.TotalPrice.Confidence
            };

            var itemBuilder = builderFactory.Builder<IBillItemBuilder>()
                .FromExtractedResult(itemExtraction);

            var item = itemBuilder.Build();
            _itemExtractionResults.Add(itemBuilder.GetExtractionResult());
            return item;
        }).ToList();

        return this;
    }

    public Bill Build()
    {
        _extractionResult.BillItemExtractionResults = _itemExtractionResults;
        return _bill;
    }

    public BillExtractionResult GetExtractionResult() => _extractionResult;
}
