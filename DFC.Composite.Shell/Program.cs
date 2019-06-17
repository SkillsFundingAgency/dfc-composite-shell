using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;

namespace DFC.Composite.Shell
{
    public class Program
    {
        public static void Main(string[] args)
        {
            WebHost.CreateDefaultBuilder(args)
                   .UseApplicationInsights()
                   .UseStartup<Startup>()
                   .Build()
                   .Run();
        }
    }
}
