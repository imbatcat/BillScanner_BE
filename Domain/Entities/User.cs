namespace Domain.Entities
{
    internal class User : BaseEntity
    {
        public string Email { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public string PreferredLanguage { get; set; } = "vi";
        public DateTime UpdatedAt { get; set; }

        public ICollection<Bill> Bills { get; set; } = new List<Bill>();
    }
}