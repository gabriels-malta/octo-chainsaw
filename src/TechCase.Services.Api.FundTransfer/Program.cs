using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Serilog;
using System;
using TechCase.FundTransfer.Core.Configuration;

namespace TechCase.Services.Api.FundTransfer
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Log.Logger = LoggingConfiguration.Get.CreateLogger();

            try
            {
                Log.Information("API TechCase.Services.Api.FundTransfer started at {DateTime}", DateTime.UtcNow);
                CreateHostBuilder(args).Build().Run();
            }
            catch(Exception ex)
            {
                Log.Fatal(ex, "TechCase.Services.Api.FundTransfer crashed!");
                throw;
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .UseSerilog()
                .ConfigureWebHostDefaults(webBuilder =>
                {                    
                    webBuilder.UseStartup<Startup>();
                });
    }
}
