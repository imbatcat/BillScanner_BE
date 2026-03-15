using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Efcore.Configurations
{
    public class BankConfiguration : IEntityTypeConfiguration<Bank>
    {
        public void Configure(EntityTypeBuilder<Bank> builder)
        {
            builder.ToTable("banks");

            builder.Property(e => e.Code)
                .IsRequired()
                .HasMaxLength(20);

            builder.Property(e => e.Name)
                .IsRequired()
                .HasMaxLength(255);

            builder.Property(e => e.ShortName)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(e => e.Bin)
                .HasMaxLength(10);

            builder.Property(e => e.LogoUrl)
                .HasMaxLength(500);

            builder.HasIndex(e => e.Code)
                .IsUnique();

            builder.HasIndex(e => e.Bin)
                .IsUnique();
        }
    }
}
