namespace Domain.Entities
{
    public class Bank : BaseEntity
    {
        public string Code { get; set; } = null!;

        public string Name { get; set; } = null!;

        public string ShortName { get; set; } = null!;

        public string? Bin { get; set; }

        public string? LogoUrl { get; set; }
    }
}
