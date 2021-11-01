using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TechCase.FundTransfer.Core.Domain;

namespace TechCase.FundTransfer.Infrastructure.Database.Configurations
{
    internal class TransactionConfiguration : IEntityTypeConfiguration<Transaction>
    {
        public void Configure(EntityTypeBuilder<Transaction> builder)
        {
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Id);
            builder.Property(x => x.AccountNumber).HasMaxLength(20).IsRequired();
            builder.Property(x => x.Value).IsRequired();
            builder.Property(x => x.CreatedOn).IsRequired();
            builder.Property(x => x.Status).IsRequired().HasMaxLength(20);
            builder.Property(x => x.Type).IsRequired().HasMaxLength(10);
            builder.Property(x => x.TransferRequestId);
        }
    }
}
