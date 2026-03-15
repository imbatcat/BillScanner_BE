using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Efcore.Configurations
{
    public class BillItemConfiguration : IEntityTypeConfiguration<BillItem>
    {
        public void Configure(EntityTypeBuilder<BillItem> builder)
        {
            builder.ToTable("bill_items");

            builder.Property(e => e.BillId)
                .IsRequired();

            builder.Property(e => e.ItemName)
                .HasMaxLength(255)
                .IsRequired();

            builder.Property(e => e.Description)
                .HasColumnType("text");

            builder.Property(e => e.Quantity)
                .HasPrecision(10, 2)
                .HasDefaultValue(1);

            builder.Property(e => e.UnitPrice)
                .HasPrecision(15, 2)
                .IsRequired();

            builder.Property(e => e.TotalPrice)
                .HasPrecision(15, 2)
                .IsRequired();

        }
    }
}