namespace Domain.Entities
{
    public class User : BaseEntity
    {
        public string? DisplayName { get; set; }

        public string Email { get; set; } = null!;

        public string Password { get; set; } = null!;
        public UserRole Role { get; set; } 

        public ICollection<Bill> Bills { get; set; } = [];
    }
    
    public enum UserRole {
        User,
        Admin
    }
}