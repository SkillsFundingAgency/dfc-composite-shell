using DFC.Composite.Shell.Services.UriSpecificHttpClient;
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
            var setupUrls = new List<RegisteredUrlModel>
            {
                new RegisteredUrlModel { Url = "expected-url-1" },
                new RegisteredUrlModel { Url = "expected-url-2" },
            };

            var registeredUrls = new RegisteredUrls(setupUrls);
            var actualUrls = registeredUrls.GetAll();

            setupUrls.Should().Equal(actualUrls);
        }
    }
}