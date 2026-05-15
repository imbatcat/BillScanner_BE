using Business.Handlers.Bills.GetBills.Dto;
using Domain.Entities;

namespace Business.Specifications.Bills
{
    public class GetBillsSpecification : BaseSpecification<Bill>
    {
        public GetBillsSpecification(Guid userId, BillParams p, bool applyPaging = true)
            : base(b =>
                b.UserId == userId
                && (string.IsNullOrWhiteSpace(p.SearchTerm) || (b.MerchantName != null &&
                                                                b.MerchantName.ToLower()
                                                                    .Contains(p.SearchTerm.ToLower())))
                && (!p.FromDate.HasValue || b.BillDate >= DateOnly.FromDateTime(p.FromDate.Value))
                && (!p.ToDate.HasValue || b.BillDate <= DateOnly.FromDateTime(p.ToDate.Value))
                && (!p.MinTotal.HasValue || b.Total >= p.MinTotal.Value)
                && (!p.MaxTotal.HasValue || b.Total <= p.MaxTotal.Value))
        {
            var descending = p.SortOrder.Equals("desc", StringComparison.OrdinalIgnoreCase);

            switch (p.SortBy.ToLower())
            {
                case "total":
                    if (descending) AddOrderByDesc(b => b.Total!);
                    else AddOrderBy(b => b.Total!);
                    break;
                case "merchantname":
                    if (descending) AddOrderByDesc(b => b.MerchantName!);
                    else AddOrderBy(b => b.MerchantName!);
                    break;
                default: // "billdate"
                    if (descending) AddOrderByDesc(b => b.BillDate);
                    else AddOrderBy(b => b.BillDate);
                    break;
            }

            if (applyPaging)
                AddPaging((p.Page - 1) * p.Size, p.Size);
        }
    }
}
