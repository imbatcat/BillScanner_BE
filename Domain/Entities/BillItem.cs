namespace Domain.Entities
{
    internal class BillItem : BaseEntity
    {
        public Guid BillId { get; set; }
        public string ItemName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal Quantity { get; set; } = 1;
        public decimal UnitPrice { get; set; }
        public decimal TotalPrice { get; set; }
        public int ItemConfidence { get; set; }

        public Bill Bill { get; set; } = null!;
    }
}