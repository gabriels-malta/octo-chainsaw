using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using Serilog;
using TechCase.Services.Api.FundTransfer.Initialization;
using TechCase.Services.Api.FundTransfer.Interfaces;
using TechCase.Services.Api.FundTransfer.Services;

namespace TechCase.Services.Api.FundTransfer
{
    public class Startup
    {
        public Startup(IConfiguration configuration) => Configuration = configuration;


        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {

            TechCase.FundTransfer.Infrastructure.Database.DbInstaller.RegisterSelf(services);
            TechCase.FundTransfer.Infrastructure.Queue.QueueInstaller.RegisterPublisher(services);

            services.AddScoped<IFundTransferService, FundTransferService>();

            services
                .AddControllers()
                .ConfigureApiBehaviorOptions(ApiBehaviorConfiguration.Get);

            services.AddSwaggerGen(c =>
            {
                OpenApiInfo apiInfo = new()
                {
                    Title = "TechCase - FundTransfer API",
                    Version = "v1",
                    Contact = new()
                    {
                        Name = "Gabriel Malta",
                        Email = "gabriels.malta@outlook.com"
                    },
                    Description = "Tech case for Acesso Bankly"
                };
                c.SwaggerDoc("v1", apiInfo);
            });
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "TechCase - FundTransfer API v1"));
            }

            app.UseSerilogRequestLogging();
            app.UseRouting();

            app.UseEndpoints(endpoints => endpoints.MapControllers());
        }
    }
}
