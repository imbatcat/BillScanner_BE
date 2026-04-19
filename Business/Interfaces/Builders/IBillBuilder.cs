using Business.Handlers.Bills.CreateBill.Dto;
using Domain.Entities;

namespace Business.Interfaces.Builders;

public interface IBillBuilder : IBuilder<Bill>
{
    IBillBuilder FromProcessResult(Handlers.Images.ProcessImage.Dto.ImageProcessing.ImageProcessResult result);
    IBillBuilder WithUserEdits(UserEditsDto dto);
    IBillBuilder WithMerchant(string? name);
    IBillBuilder WithMerchantBank(string? bank);
    IBillBuilder WithMerchantBankNumber(string? number);
    IBillBuilder WithDate(DateOnly date);
    IBillBuilder WithTime(TimeSpan? time);
    IBillBuilder WithUserId(Guid userId);
    IBillBuilder WithImgUrl(string? url);
    IBillBuilder WithTotal(decimal? total);
    IBillBuilder WithSubTotal(decimal? subTotal);
    IBillBuilder WithTax(decimal? tax);
    IBillBuilder WithCurrency(string? currency);
    IBillBuilder WithExtractionMethod(ExtractionMethod method);
    IBillBuilder WithItems(List<BillItemDto> items);
}
