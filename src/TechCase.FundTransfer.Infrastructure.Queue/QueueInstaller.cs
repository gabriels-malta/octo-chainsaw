using Microsoft.Extensions.DependencyInjection;
using TechCase.FundTransfer.Core.Interfaces;

namespace TechCase.FundTransfer.Infrastructure.Queue
{
    public static class QueueInstaller
    {
        public static void RegisterPublisher(IServiceCollection serviceCollection) => serviceCollection.AddScoped<IPublisher, RabbitMqServices>();
    }
}
