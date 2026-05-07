using Business.Handlers.Bills.GetBills.Dto;
using Business.Interfaces.Repositories;
using Business.Specifications.Bills;
using Domain.Entities;
using MediatR;

namespace Business.Handlers.Bills.GetBills
{
    public class GetBillsHandler(IUnitOfWork unitOfWork)
        : IRequestHandler<GetBillsQuery, GetBillsResponse>
    {
        public async Task<GetBillsResponse> Handle(GetBillsQuery request, CancellationToken cancellationToken)
        {
            var dataSpec  = new GetBillsSpecification(request.UserId, request.Params, applyPaging: true);
            var countSpec = new GetBillsSpecification(request.UserId, request.Params, applyPaging: false);

            var bills = await unitOfWork.Repository<Bill>().GetAllWithSpecificationAsync(dataSpec);
            var total = await unitOfWork.Repository<Bill>().CountAsync(countSpec);

            var dtos = bills.Select(b => new BillDto
            {
                Id               = b.Id,
                BillDate         = b.BillDate,
                BillTime         = b.BillTime,
                MerchantName     = b.MerchantName,
                Currency         = b.Currency,
                Total            = b.Total,
                ImgUrl           = b.ImgUrl,
                ExtractionMethod = b.ExtractionMethod,
                Status           = b.Status,
            }).ToList();

            return new GetBillsResponse(dtos, total);
        }
    }
}
