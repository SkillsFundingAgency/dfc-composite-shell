using System.Net.Http;

namespace DFC.Composite.Shell.Services.HttpClientService
{
    public interface IFakeHttpRequestSender
    {
        HttpResponseMessage Send(HttpRequestMessage request);
    }
}