using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Efcore.Configurations
{
    public class BillExtractionResultConfiguration : IEntityTypeConfiguration<BillExtractionResult>
    {
        public void Configure(EntityTypeBuilder<BillExtractionResult> builder)
        {
            builder.ToTable("bill_extraction_results");

            builder.Property(e => e.BillId)
                .IsRequired();

            builder.Property(e => e.ExtractedMerchantName)
                .HasMaxLength(255);

            builder.Property(e => e.MerchantNameConfidence)
                .HasPrecision(5, 4); // To support 0.0000 to 1.0000 or similar

            builder.Property(e => e.BillDateConfidence)
                .HasPrecision(5, 4);

            builder.Property(e => e.BillTimeConfidence)
                .HasPrecision(5, 4);

            builder.Property(e => e.ExtractedCurrency)
                .HasMaxLength(10);

            builder.Property(e => e.CurrencyConfidence)
                .HasPrecision(5, 4);

            builder.HasOne(e => e.Bill)
                .WithOne()
                .HasForeignKey<BillExtractionResult>(e => e.BillId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
