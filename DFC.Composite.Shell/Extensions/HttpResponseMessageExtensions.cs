using System.Net;
using System.Net.Http;

namespace DFC.Composite.Shell.Extensions
{
    public static class HttpResponseMessageExtensions
    {
        public static bool IsRedirectionStatus(this HttpResponseMessage httpResponseMessage)
        {
            if (httpResponseMessage.StatusCode >= HttpStatusCode.MultipleChoices && 
                httpResponseMessage.StatusCode <= HttpStatusCode.PermanentRedirect)
            {
                return true;
            }

            return false;
        }
    }
}
