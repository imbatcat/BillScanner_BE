using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Efcore.Configurations
{
    public class BillConfiguration : IEntityTypeConfiguration<Bill>
    {
        public void Configure(EntityTypeBuilder<Bill> builder)
        {
            builder.ToTable("bills");

            builder.Property(e => e.UserId)
                .IsRequired();

            builder.Property(e => e.MerchantName)
                .HasMaxLength(255);

            builder.Property(e => e.ExtractionMethod)
                .IsRequired()
                .HasConversion(
                    v => v.ToString(),
                    v => Enum.Parse<ExtractionMethod>(v));

            builder.Property(e => e.BillDate)
                .IsRequired()
                .HasColumnType("date");

            builder.Property(e => e.BillTime);

            builder.Property(e => e.ImgUrl)
                .HasMaxLength(2048);

            builder.Property(e => e.MerchantBankName)
                .HasMaxLength(100);

            builder.Property(e => e.MerchantBankNumber)
                .HasMaxLength(34);

            builder.Property(e => e.Currency)
                .HasMaxLength(3)
                .HasDefaultValue("VND");

            builder.HasMany(e => e.BillItems)
                .WithOne(e => e.Bill)
                .HasForeignKey(e => e.BillId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}