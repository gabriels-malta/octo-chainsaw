using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using System;
using System.Threading;
using System.Threading.Tasks;
using TechCase.FundTransfer.Core.Configuration;
using TechCase.FundTransfer.Core.Interfaces;

namespace TechCase.Services.Worker.FundTransferStarter
{
    class Program
    {
        private static readonly ManualResetEvent _quitEvent = new(false);

        static Task Main(string[] args)
        {
            Console.CancelKeyPress += (sender, eArgs) =>
            {
                _quitEvent.Set();
                Console.WriteLine("CancelEvent recieved, shutting down...");
                Thread.Sleep(1000);
                Console.Write("Bye");
            };

            Log.Logger = LoggingConfiguration.Get.CreateLogger();

            try
            {
                Log.Information("Worker TechCase.Services.Worker.FundTransferStarter started at {DateTime}", DateTime.UtcNow);
                IHost host = CreateHostBuilder(args).Build();

                IServiceScope serviceScope = host.Services.CreateScope();

                var _publisher = serviceScope.ServiceProvider.GetRequiredService<IPublisher>();
                var _transferRepository = serviceScope.ServiceProvider.GetRequiredService<ITransferRepository>();

                var consumer = new Consumer(_transferRepository, _publisher,  _quitEvent, Log.Logger);
                var consumerThread = new Thread(consumer.ConsumeQueue) { IsBackground = true };
                consumerThread.Start();

                return host.RunAsync();
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "TechCase.Services.Worker.FundTransferStarter crashed!");
                throw;
            }
        }

        static IHostBuilder CreateHostBuilder(string[] args)
            => Host.CreateDefaultBuilder(args)
            .UseSerilog()
            .ConfigureServices((hostContext, services) =>
            {
                FundTransfer.Infrastructure.Database.DbInstaller.RegisterSelf(services);
                FundTransfer.Infrastructure.Queue.QueueInstaller.RegisterPublisher(services);
            });
    }
}
