using Business.Interfaces.Repositories;
using Business.Interfaces.Services;
using Domain.Entities;
using MediatR;

namespace Business.Handlers.Bills.UpdateBill;

public class UpdateBillHandler(IUnitOfWork unitOfWork, IExchangeRateService exchangeRateService)
    : IRequestHandler<UpdateBillCommand>
{
    public async Task Handle(UpdateBillCommand request, CancellationToken cancellationToken)
    {
        var bill = await unitOfWork.Repository<Bill>().GetByIdAsync(request.Id, asNoTracking: false, includes: ["BillItems"])
            ?? throw new KeyNotFoundException("Bill not found.");

        if (bill.UserId != request.UserId)
            throw new UnauthorizedAccessException("Bill does not belong to current user.");

        var dto = request.UserEdits;

        bill.MerchantName       = dto.MerchantName;
        bill.MerchantBankName   = dto.MerchantBankName;
        bill.MerchantBankNumber = dto.MerchantBankNumber;
        bill.BillDate           = dto.BillDate;
        bill.BillTime           = dto.BillTime;
        bill.SubTotal           = dto.SubTotal;
        bill.Tax                = dto.Tax;
        bill.Total              = dto.Total;
        bill.Discount           = dto.Discount ?? 0;
        bill.Currency           = dto.Currency ?? bill.Currency;

        foreach (var item in bill.BillItems.ToList())
            unitOfWork.Repository<BillItem>().Delete(item);

        bill.BillItems = dto.Items.Select(i => new BillItem
        {
            BillId     = bill.Id,
            ItemName   = i.ItemName,
            Quantity   = i.Quantity,
            UnitPrice  = i.UnitPrice,
            TotalPrice = i.TotalPrice,
        }).ToList();

        if (!bill.Currency.Equals("VND", StringComparison.OrdinalIgnoreCase))
        {
            var currency = bill.Currency;
            bill.Total    = bill.Total    is { } t ? await exchangeRateService.ConvertToVndAsync(t, currency) : null;
            bill.SubTotal = bill.SubTotal is { } s ? await exchangeRateService.ConvertToVndAsync(s, currency) : null;
            bill.Tax      = bill.Tax      is { } x ? await exchangeRateService.ConvertToVndAsync(x, currency) : null;
            bill.Discount = await exchangeRateService.ConvertToVndAsync(bill.Discount, currency);
            foreach (var item in bill.BillItems)
            {
                item.UnitPrice  = await exchangeRateService.ConvertToVndAsync(item.UnitPrice, currency);
                item.TotalPrice = await exchangeRateService.ConvertToVndAsync(item.TotalPrice, currency);
            }
            bill.Currency = "VND";
        }

        unitOfWork.Repository<Bill>().Update(bill);
        await unitOfWork.CommitAsync();
    }
}
