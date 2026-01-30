namespace Domain.Entities
{
    public class BillItem : BaseEntity
    {
        public Guid BillId { get; set; }

        public Guid ItemId { get; set; }

        public string ItemName { get; set; } = null!;

        public string? Description { get; set; }

        public decimal Quantity { get; set; } = 1;

        public decimal UnitPrice { get; set; }

        public decimal TotalPrice { get; set; }

        public Bill Bill { get; set; } = null!;
        public Item Item { get; set; }
    }
}