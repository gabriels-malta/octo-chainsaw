using Microsoft.Extensions.DependencyInjection;
using TechCase.FundTransfer.Core.Interfaces;

namespace TechCase.FundTransfer.Infrastructure.Database
{
    public static class DbInstaller
    {
        public static void RegisterSelf(IServiceCollection serviceCollection)
        {
            serviceCollection.AddDbContext<FundTransferContext>();
            serviceCollection.AddScoped(typeof(IRepository<>), typeof(Repository<>));
            serviceCollection.AddScoped<ITransferRepository, TransferRepository>();
        }
    }
}
