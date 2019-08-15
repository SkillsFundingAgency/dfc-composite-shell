using DFC.Composite.Shell.Integration.Test.Framework;
using System.Net;
using System.Threading.Tasks;
using Xunit;

namespace DFC.Composite.Shell.Integration.Test.Tests
{
    public class ExternalApplicationTests : IClassFixture<ShellTestWebApplicationFactory<Startup>>
    {
        private readonly ShellTestWebApplicationFactory<Startup> _factory;

        public ExternalApplicationTests(ShellTestWebApplicationFactory<Startup> shellTestWebApplicationFactory)
        {
            _factory = shellTestWebApplicationFactory;
        }

        [Fact]
        public async Task CanRedirectToExternalUrl()
        {
            _factory.ClientOptions.AllowAutoRedirect = false;
            var client = _factory.CreateClientWithWebHostBuilder();

            var response = await client.GetAsync("/externalpath1");

            Assert.Equal(HttpStatusCode.Found, response.StatusCode);
            Assert.Equal("http://www.externalpath1.com/", response.Headers.Location.AbsoluteUri);
        }
    }
}
