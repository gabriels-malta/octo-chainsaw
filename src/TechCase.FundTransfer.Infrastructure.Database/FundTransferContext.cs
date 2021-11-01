using Microsoft.EntityFrameworkCore;
using TechCase.FundTransfer.Core.Domain;

namespace TechCase.FundTransfer.Infrastructure.Database
{
    public class FundTransferContext : DbContext
    {
        public FundTransferContext(DbContextOptions<FundTransferContext> options) : base(options)
        { }

        public DbSet<Transaction> Transactions { get; set; }
        public DbSet<Account> Accounts { get; set; }
        public DbSet<TransferRequest> Transfers { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseNpgsql("host=localhost;database=TechCase;user id=utechcase;password=qwerty;");
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            //https://www.npgsql.org/efcore/modeling/generated-properties.html#guiduuid-generation
            modelBuilder.HasPostgresExtension("uuid-ossp");

            modelBuilder.ApplyConfigurationsFromAssembly(typeof(FundTransferContext).Assembly);
        }

    }
}
