using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Efcore.Configurations
{
    public class ItemPriceHistoryConfiguration : IEntityTypeConfiguration<ItemPriceHistory>
    {
        public void Configure(EntityTypeBuilder<ItemPriceHistory> builder)
        {
            builder.ToTable("item_price_histories");

            builder.Property(e => e.Price)
                .HasPrecision(15, 2)
                .IsRequired();

            builder.Property(e => e.Date)
                .IsRequired();
        }
    }
}
