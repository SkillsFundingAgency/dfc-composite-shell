using System.Net.Http;

namespace DFC.Composite.Shell.UnitTests.HttpClientService
{
    public interface IFakeHttpRequestSender
    {
        HttpResponseMessage Send(HttpRequestMessage request);
    }
}
