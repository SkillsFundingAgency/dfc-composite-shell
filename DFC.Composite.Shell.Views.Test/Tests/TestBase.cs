using Microsoft.Extensions.Configuration;

namespace DFC.Composite.Shell.Views.Test.Tests
{
    public class TestBase
    {
        private IConfigurationRoot Configuration;
        protected string ViewRootPath;

        public TestBase()
        {
            var config = new ConfigurationBuilder();
            config.AddJsonFile("appsettings.json");
            Configuration = config.Build();

            ViewRootPath = Configuration["ViewRootPath"];
        }
    }
}
