using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Efcore.Configurations
{
    public class PaymentMethodConfiguration : IEntityTypeConfiguration<PaymentMethod>
    {
        public void Configure(EntityTypeBuilder<PaymentMethod> builder)
        {
            builder.ToTable("payment_methods");

            builder.Property(e => e.BillId)
                .IsRequired();

            builder.Property(e => e.MethodType)
                .IsRequired()
                .HasConversion(
                    v => v.ToString(),
                    v => Enum.Parse<MethodType>(v));

            builder.Property(e => e.BankCode)
                .HasMaxLength(6);

            builder.Property(e => e.BankName)
                .HasMaxLength(100);

            builder.Property(e => e.BankAccount)
                .HasMaxLength(34);

            builder.Property(e => e.AccountHolder)
                .HasMaxLength(255);

            builder.Property(e => e.TransactionAmount)
                .HasPrecision(15, 2)
                .IsRequired();

            builder.Property(e => e.Currency)
                .HasMaxLength(3)
                .HasDefaultValue("VND");

            builder.Property(e => e.PaymentContent)
                .HasMaxLength(19);

            builder.HasOne(e => e.Bill)
                .WithMany(e => e.PaymentMethods)
                .HasForeignKey(e => e.BillId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}