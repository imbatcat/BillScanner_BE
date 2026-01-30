using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Efcore.Configurations
{
    public class BillItemExtractionResultConfiguration : IEntityTypeConfiguration<BillItemExtractionResult>
    {
        public void Configure(EntityTypeBuilder<BillItemExtractionResult> builder)
        {
            builder.ToTable("bill_item_extraction_results");

            builder.Property(e => e.BillExtractionResultId)
                .IsRequired();

            builder.Property(e => e.ExtractedItemName)
                .HasMaxLength(255);

            builder.Property(e => e.ItemNameConfidence)
                .HasPrecision(5, 4);

            builder.Property(e => e.ExtractedQuantity)
                .HasPrecision(18, 2);

            builder.Property(e => e.QuantityConfidence)
                .HasPrecision(5, 4);

            builder.Property(e => e.ExtractedUnitPrice)
                .HasPrecision(18, 2);

            builder.Property(e => e.UnitPriceConfidence)
                .HasPrecision(5, 4);

            builder.Property(e => e.ExtractedTotalPrice)
                .HasPrecision(18, 2);

            builder.Property(e => e.TotalPriceConfidence)
                .HasPrecision(5, 4);

            builder.HasOne(e => e.BillExtractionResult)
                .WithMany(e => e.BillItemExtractionResults)
                .HasForeignKey(e => e.BillExtractionResultId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
