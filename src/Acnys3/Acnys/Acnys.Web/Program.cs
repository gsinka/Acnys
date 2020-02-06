using Acnys.Core.Hosting;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Sinks.SystemConsole.Themes;

namespace Acnys.Web
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Host.CreateDefaultBuilder(args)
                
                .UseAutofac()
                .UseSerilog((context, config) => config.WriteTo.Console(theme: AnsiConsoleTheme.Code).MinimumLevel.Verbose())
                .UseComputerClock()
                .UseRequest((context, builder) =>
                {
                    
                })

                .Build().Run();

        }

    }
}
