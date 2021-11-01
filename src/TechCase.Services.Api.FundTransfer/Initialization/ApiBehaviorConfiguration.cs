using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace TechCase.Services.Api.FundTransfer.Initialization
{
    internal static class ApiBehaviorConfiguration
    {
        internal static void Get(ApiBehaviorOptions behaviorOptions)
        {
            // Configuration from https://docs.microsoft.com/en-us/aspnet/core/web-api/?view=aspnetcore-2.2#log-automatic-400-responses-1
            behaviorOptions.InvalidModelStateResponseFactory = context =>
            {
                var loggerFactory = context.HttpContext.RequestServices.GetRequiredService<ILoggerFactory>();
                var logger = loggerFactory.CreateLogger("TechCase.Services.Api.FundTransfer");
                logger.LogError("Model state {@ModelState}", context.ModelState);
                return new BadRequestObjectResult(context.ModelState);
            };
        }
    }
}
