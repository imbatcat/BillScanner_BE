namespace Domain.Entities
{
    public class Category : BaseEntity
    {
      public string Name { get; set; } = null!;

      public ICollection<Item> Items { get; set; } = [];
    }
}