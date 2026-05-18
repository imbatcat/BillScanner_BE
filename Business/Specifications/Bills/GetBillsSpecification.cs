using System.Linq.Expressions;
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
            Action<Expression<Func<Bill, object>>> orderBy = descending ? AddOrderByDesc : AddOrderBy;
            Action<Expression<Func<Bill, object>>> thenBy = descending ? AddThenByDesc : AddThenBy;
            var sortFlag = true;

            switch (p.SortBy.ToLower())
            {
                case "total":
                    orderBy(b => b.Total!);
                    break;
                case "merchantname":
                    orderBy(b => b.MerchantName!);
                    break;
                default:
                    sortFlag = false;
                    break;
            }

            if (sortFlag)
            {
                thenBy(b => b.BillDate);
            }
            else
            {
                orderBy(b => b.BillDate);
            }
            thenBy(b => b.BillTime!);

            if (applyPaging)
                AddPaging((p.Page - 1) * p.Size, p.Size);
        }
    }
}
