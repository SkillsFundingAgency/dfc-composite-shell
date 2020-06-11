using DFC.Composite.Shell.Middleware;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace DFC.Composite.Shell.ClientHandlers
{
    public class CompositeSessionIdDelegatingHandler : DelegatingHandler
    {
        internal const string HeaderName = "x-dfc-composite-sessionid";

        private readonly IHttpContextAccessor httpContextAccessor;
        private readonly ITempDataDictionaryFactory tempDataDictionaryFactory;

        public CompositeSessionIdDelegatingHandler(IHttpContextAccessor httpContextAccessor, ITempDataDictionaryFactory tempDataDictionaryFactory)
        {
            this.httpContextAccessor = httpContextAccessor;
            this.tempDataDictionaryFactory = tempDataDictionaryFactory;
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var compositeSessionId = httpContextAccessor?.HttpContext?.Request?.Cookies[CompositeSessionIdMiddleware.NcsSessionCookieName];

            if (httpContextAccessor?.HttpContext != null && string.IsNullOrWhiteSpace(compositeSessionId))
            {
                var tempData = tempDataDictionaryFactory.GetTempData(httpContextAccessor?.HttpContext);

                compositeSessionId = tempData[HeaderName].ToString();
            }

            if (request != null && !request.Headers.Contains(HeaderName) && !string.IsNullOrWhiteSpace(compositeSessionId))
            {
                request.Headers.Add(HeaderName, compositeSessionId);
            }

            return base.SendAsync(request, cancellationToken);
        }
    }
}
