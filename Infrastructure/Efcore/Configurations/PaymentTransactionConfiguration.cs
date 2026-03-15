using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Efcore.Configurations
{
    public class PaymentTransactionConfiguration : IEntityTypeConfiguration<PaymentTransaction>
    {
        public void Configure(EntityTypeBuilder<PaymentTransaction> builder)
        {
            builder.ToTable("payment_transactions");

            builder.Property(e => e.BillId)
                .IsRequired();

            builder.Property(e => e.PaymentType)
                .IsRequired()
                .HasConversion(
                    v => v.ToString(),
                    v => Enum.Parse<PaymentType>(v));

            builder.Property(e => e.BankId);

            builder.HasOne(e => e.Bank)
                .WithMany()
                .HasForeignKey(e => e.BankId)
                .OnDelete(DeleteBehavior.SetNull);

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
        }
    }
}
