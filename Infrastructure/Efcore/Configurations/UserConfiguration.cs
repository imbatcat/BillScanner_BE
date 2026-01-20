using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Efcore.Configurations
{
    public class UserConfiguration : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> builder)
        {
            builder.ToTable("users");

            builder.Property(e => e.Email)
                .IsRequired()
                .HasMaxLength(255);

            builder.HasIndex(e => e.Email)
                .IsUnique();

            builder.Property(e => e.DisplayName)
                .HasMaxLength(100);

            builder.Property(e => e.Password)
                .IsRequired()
                .HasMaxLength(255);

            builder.Property(e => e.Role)
                .IsRequired()
                .HasConversion(
                    v => v.ToString(),
                    v => Enum.Parse<UserRole>(v))
                .HasMaxLength(20);

            builder.HasMany(e => e.Bills)
                .WithOne(e => e.User)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}