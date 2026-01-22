namespace Domain.Entities;

public class ItemPriceHistory : BaseEntity
{
    public Guid ItemId { get; set; }
    public Item Item { get; set; } = null!;
    public decimal Price { get; set; }
    public DateTime Date { get; set; }
}