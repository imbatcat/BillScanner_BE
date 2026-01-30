namespace Domain.Entities
{
    public class Item : BaseEntity
    {
        public Guid CategoryId { get; set; }
        public string Name { get; set; } = null!;
        public decimal CurrentPrice { get; set; }
        public Category Category { get; set; } = null!;
        public List<ItemPriceHistory> ItemPriceHistories { get; set; } = [];
    }
}