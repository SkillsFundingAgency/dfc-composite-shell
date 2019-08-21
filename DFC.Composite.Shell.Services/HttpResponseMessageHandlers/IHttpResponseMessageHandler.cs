using System.Net.Http;

namespace DFC.Composite.Shell.HttpResponseMessageHandlers
{
    /// <summary>
    /// Allows processing of a HttpResponseMessage
    /// </summary>
    public interface IHttpResponseMessageHandler
    {
        void Process(HttpResponseMessage httpResponseMessage);
    }
}
