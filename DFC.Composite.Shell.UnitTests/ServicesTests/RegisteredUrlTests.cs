using DFC.Composite.Shell.Services.UriSpecifcHttpClient;
using FluentAssertions;
using System.Collections.Generic;
using Xunit;

namespace DFC.Composite.Shell.Test.ServicesTests
{
    public class RegisteredUrlTests
    {
        [Fact]
        public void WhenCallingGetAllThenAllSetupUrlsAreReturnedUnchanged()
        {
            var setupUrls = new List<string>
            {
                "expected-url-1",
                "expected-url-2",
            };

            var registeredUrls = new RegisteredUrls(setupUrls);
            var actualUrls = registeredUrls.GetAll();

            setupUrls.Should().Equal(actualUrls);
        }
    }
}