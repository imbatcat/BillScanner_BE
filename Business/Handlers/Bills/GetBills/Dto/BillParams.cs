using Business.Specifications;
using Domain.Entities;

namespace Business.Handlers.Bills.GetBills.Dto
{
    public class BillParams : BaseParams
    {
        public BillParams()
        {
            SortBy = "billdate";
        }

        public BillStatus? Status { get; set; }
    }
}
