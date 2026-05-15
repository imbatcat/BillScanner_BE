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

        public bool IsProcessed { get; set; } = true;
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public decimal? MinTotal { get; set; }
        public decimal? MaxTotal { get; set; }
    }
}
