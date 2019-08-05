using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.ApplicationInsights;

namespace DFC.Composite.Shell
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            WebHost.CreateDefaultBuilder(args)
                   .UseApplicationInsights()
                   .ConfigureLogging((webHostBuilderContext, loggingBuilder) =>
                   {
                       loggingBuilder.AddFilter<ApplicationInsightsLoggerProvider>(string.Empty, LogLevel.Trace);
                   })
                   .UseStartup<Startup>()
                   .Build()
                   .Run();
        }
    }
}