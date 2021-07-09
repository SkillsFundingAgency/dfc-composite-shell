using System.Net;
using System.Net.Http;

namespace DFC.Composite.Shell.ClientHandlers
{
    public class DefaultHttpClientHandler : HttpClientHandler
    {
        public DefaultHttpClientHandler() =>
            AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip;
    }
}
