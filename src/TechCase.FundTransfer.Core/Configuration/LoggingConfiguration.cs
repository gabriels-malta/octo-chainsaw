using Serilog;
using Serilog.Events;
using Serilog.Exceptions;

namespace TechCase.FundTransfer.Core.Configuration
{
    public sealed class LoggingConfiguration
    {
        public static LoggerConfiguration Get = new LoggerConfiguration()
            .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
            .MinimumLevel.Override("System", LogEventLevel.Warning)
            .Enrich.FromLogContext()
            .Enrich.WithMachineName()
            .Enrich.WithClientAgent()
            .Enrich.WithExceptionDetails()
            .WriteTo.Console(restrictedToMinimumLevel: LogEventLevel.Information)
            .WriteTo.Seq("http://localhost:5341", restrictedToMinimumLevel: LogEventLevel.Information);
    }
}
