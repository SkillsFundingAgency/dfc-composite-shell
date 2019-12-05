using System;
using System.Diagnostics.CodeAnalysis;
using System.Net.Http;

namespace DFC.Composite.Shell.Services.HttpClientService
{
    [ExcludeFromCodeCoverage]
    public class FakeHttpRequestSender : IFakeHttpRequestSender
    {
        public HttpResponseMessage Send(HttpRequestMessage request)
        {
            throw new NotImplementedException("Now we can setup this method with our mocking framework");
        }
    }
}