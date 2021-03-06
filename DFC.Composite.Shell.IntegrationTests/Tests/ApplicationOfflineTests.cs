﻿using DFC.Composite.Shell.Integration.Test.Framework;
using System;
using System.Threading.Tasks;
using Xunit;

namespace DFC.Composite.Shell.Integration.Test
{
    public class ApplicationOfflineTests : IClassFixture<ShellTestWebApplicationFactory<Startup>>
    {
        private readonly ShellTestWebApplicationFactory<Startup> factory;

        public ApplicationOfflineTests(ShellTestWebApplicationFactory<Startup> shellTestWebApplicationFactory)
        {
            factory = shellTestWebApplicationFactory;
        }

        [Fact]
        public async Task WhenAnApplicationIsOfflineItContainsTheApplicationsOfflineMessage()
        {
            var shellUri = new Uri("path3", UriKind.Relative);
            var client = factory.CreateClientWithWebHostBuilder();

            var response = await client.GetAsync(shellUri).ConfigureAwait(false);

            response.EnsureSuccessStatusCode();
            var responseHtml = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            Assert.Contains("Path3 is offline", responseHtml, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public async Task WhenAnApplicationIsOfflineItDoesntContainsContentFromAnyRegions()
        {
            var shellUri = new Uri("path3", UriKind.Relative);
            var client = factory.CreateClientWithWebHostBuilder();

            var response = await client.GetAsync(shellUri).ConfigureAwait(false);

            response.EnsureSuccessStatusCode();
            var responseHtml = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            Assert.DoesNotContain("GET, http://www.path3.com/path3/head, path3, Head", responseHtml, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain("GET, http://www.path3.com/path3/body, path3, Body", responseHtml, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain("GET, http://www.path3.com/path3/breadcrumb, path3, Breadcrumb", responseHtml, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain("GET, http://www.path3.com/path3/bodyfooter, path3, Bodyfooter", responseHtml, StringComparison.OrdinalIgnoreCase);
        }
    }
}