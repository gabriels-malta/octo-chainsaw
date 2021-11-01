using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TechCase.FundTransfer.Core.Domain;

namespace TechCase.FundTransfer.Infrastructure.Database.Configurations
{
    internal class TransferRequestConfiguration : IEntityTypeConfiguration<TransferRequest>
    {
        public void Configure(EntityTypeBuilder<TransferRequest> builder)
        {
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Id).HasDefaultValueSql("uuid_generate_v4()");
            builder.Property(x => x.OriginAcc).HasMaxLength(20).IsRequired();
            builder.Property(x => x.DestinationAcc).HasMaxLength(20).IsRequired();
            builder.Property(x => x.Value).IsRequired();
            builder.Property(x => x.CreatedOn).IsRequired();
            builder.Property(x => x.Status).IsRequired().HasMaxLength(20);
            builder.Property(x => x.Comments).HasMaxLength(255);
        }
    }
}
