﻿using DFC.Composite.Shell.Integration.Test.Framework;
using System;
using System.Net.Mime;
using System.Threading.Tasks;
using Xunit;

namespace DFC.Composite.Shell.Integration.Test
{
    public class SiteMapTests : IClassFixture<ShellTestWebApplicationFactory<Startup>>
    {
        private readonly ShellTestWebApplicationFactory<Startup> factory;

        public SiteMapTests(ShellTestWebApplicationFactory<Startup> shellTestWebApplicationFactory)
        {
            factory = shellTestWebApplicationFactory;
        }

        [Fact]
        public async Task Should_ReturnValidContent()
        {
            var client = factory.CreateClientWithWebHostBuilder();

            var response = await client.GetAsync(new Uri("/sitemap.xml", UriKind.Relative));

            response.EnsureSuccessStatusCode();
            await response.Content.ReadAsStringAsync();

            Assert.Equal(MediaTypeNames.Application.Xml, response.Content.Headers.ContentType.MediaType);
        }

        [Fact]
        public async Task Should_BeUtf8()
        {
            var expected = "encoding=\"utf-8\"";

            var client = factory.CreateClientWithWebHostBuilder();

            var response = await client.GetAsync(new Uri("/sitemap.xml", UriKind.Relative));

            response.EnsureSuccessStatusCode();
            var responseHtml = await response.Content.ReadAsStringAsync();

            Assert.Contains(expected, responseHtml, StringComparison.OrdinalIgnoreCase);
        }
    }
}
