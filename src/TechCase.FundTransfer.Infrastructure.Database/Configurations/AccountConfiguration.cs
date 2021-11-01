using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TechCase.FundTransfer.Core.Domain;

namespace TechCase.FundTransfer.Infrastructure.Database.Configurations
{
    internal class AccountConfiguration : IEntityTypeConfiguration<Account>
    {
        public void Configure(EntityTypeBuilder<Account> builder)
        {
            builder.HasKey(x => x.Id);
            
            //https://www.npgsql.org/efcore/modeling/generated-properties.html#defining-the-strategy-for-a-single-property
            builder.Property(x => x.Id).UseIdentityAlwaysColumn();
            
            builder.Property(x => x.AccountNumber).HasMaxLength(20).IsRequired();
            builder.Property(x => x.Balance).IsRequired().HasPrecision(18, 8);
            builder.Property(x => x.CreatedOn).IsRequired();
        }
    }
}
