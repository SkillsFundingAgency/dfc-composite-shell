using DFC.Composite.Shell.Models;
using DFC.Composite.Shell.Services.HttpClientService;
using DFC.Composite.Shell.Services.Regions;
using DFC.Composite.Shell.Test.ClientHandlers;
using FakeItEasy;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Threading.Tasks;
using Xunit;

namespace DFC.Composite.Shell.Test.ServicesTests
{
    public class RegionServiceTests
    {
        private readonly List<RegionModel> defaultRegionModels;
        private readonly HttpClient httpClient;
        private readonly HttpResponseMessage httpResponse;
        private readonly FakeHttpMessageHandler fakeHttpMessageHandler;

        public RegionServiceTests()
        {
            defaultRegionModels = new List<RegionModel>
            {
                new RegionModel
                {
                    IsHealthy = true,
                    RegionEndpoint = "FakeEndpoint",
                    PageRegion = PageRegion.Head,
                },
                new RegionModel
                {
                    IsHealthy = true,
                    RegionEndpoint = "SecondFakeEndpoint",
                    PageRegion = PageRegion.Body,
                },
            };

            httpResponse = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new ObjectContent(typeof(List<RegionModel>), defaultRegionModels, new JsonMediaTypeFormatter()),
            };

            var fakeHttpRequestSender = A.Fake<IFakeHttpRequestSender>();
            A.CallTo(() => fakeHttpRequestSender.Send(A<HttpRequestMessage>.Ignored)).Returns(httpResponse);

            fakeHttpMessageHandler = new FakeHttpMessageHandler(fakeHttpRequestSender);
            httpClient = new HttpClient(fakeHttpMessageHandler)
            {
                BaseAddress = new Uri("http://SomeRegionBaseAddress"),
            };
        }

        ~RegionServiceTests()
        {
            httpResponse.Dispose();
            fakeHttpMessageHandler.Dispose();
        }

        [Fact]
        public async Task GetRegionsReturnsRegionModelResults()
        {
            var service = new RegionService(httpClient);
            var result = await service.GetRegions("SomeRegionPath").ConfigureAwait(false);

            Assert.Equal(defaultRegionModels, result);
        }

        [Fact]
        public async Task MarkAsHealthyAsync()
        {
            var service = new RegionService(httpClient);

            var result = await service.SetRegionHealthState("SomeRegionPath", PageRegion.Body, false).ConfigureAwait(false);

            Assert.True(result);
            httpClient.Dispose();
        }
    }
}