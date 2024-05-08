using DFC.Composite.Shell.Integration.Test.Framework;
using System;
using System.Net.Mime;
using System.Threading.Tasks;
using Xunit;

namespace DFC.Composite.Shell.Integration.Test
{
    public class RobotsTests : IClassFixture<ShellTestWebApplicationFactory<Startup>>
    {
        private readonly ShellTestWebApplicationFactory<Startup> factory;

        public RobotsTests(ShellTestWebApplicationFactory<Startup> shellTestWebApplicationFactory)
        {
            factory = shellTestWebApplicationFactory;
        }

        [Fact]
        public async Task Should_DevDraftReturnValidContent()
        {
            var client = factory.CreateClientWithWebHostBuilder();

            var response = await client.GetAsync(new Uri("https://dev-draft.nationalcareersservice.org.uk/robots.txt", UriKind.Absolute));

            response.EnsureSuccessStatusCode();
            var responseHtml = await response.Content.ReadAsStringAsync();

            Assert.Equal(MediaTypeNames.Text.Plain, response.Content.Headers.ContentType.MediaType);
            Assert.Equal(
@"User-agent: *
Disallow: /
Disallow: /find-a-course/details*
Disallow: /find-a-course/course-details*
Disallow: /find-a-course/tdetails*
Disallow: /find-a-course/tlevels*", responseHtml);
        }

        [Fact]
        public async Task Should_PreProdReturnValidContent()
        {
            var client = factory.CreateClientWithWebHostBuilder();

            var response = await client.GetAsync(new Uri("https://dfc-pp-compui-shell-as.ase-01.dfc.preprodazure.sfa.bis.gov.uk/robots.txt", UriKind.Absolute));

            response.EnsureSuccessStatusCode();
            var responseHtml = await response.Content.ReadAsStringAsync();

            Assert.Equal(MediaTypeNames.Text.Plain, response.Content.Headers.ContentType.MediaType);
            Assert.Equal(
@"User-agent: SemrushBot-SA
Disallow: /alerts/
Disallow: /ab/
Disallow: /webchat/
Sitemap: https://dfc-pp-compui-shell-as.ase-01.dfc.preprodazure.sfa.bis.gov.uk/sitemap.xml

User-agent: *
Disallow: /", responseHtml);
        }

        [Fact]
        public async Task Should_ProdReturnValidContent()
        {
            var client = factory.CreateClientWithWebHostBuilder();

            var response = await client.GetAsync(new Uri("https://dfc-prd-compui-shell-as.ase-01.dfc.prodazure.sfa.bis.gov.uk/robots.txt", UriKind.Absolute));

            response.EnsureSuccessStatusCode();
            var responseHtml = await response.Content.ReadAsStringAsync();

            Assert.Equal(MediaTypeNames.Text.Plain, response.Content.Headers.ContentType.MediaType);
            Assert.Equal(
@"User-agent: *
Disallow: /alerts/
Disallow: /ab/
Disallow: /webchat/
Sitemap: https://dfc-prd-compui-shell-as.ase-01.dfc.prodazure.sfa.bis.gov.uk/sitemap.xml", responseHtml);
        }
    }
}
