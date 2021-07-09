using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Net.Http.Headers;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace DFC.Composite.Shell.ClientHandlers
{
    public class UserAgentDelegatingHandler : DelegatingHandler
    {
        private readonly IHttpContextAccessor httpContextAccessor;
        private readonly ILogger<UserAgentDelegatingHandler> logger;

        public UserAgentDelegatingHandler(
            IHttpContextAccessor httpContextAccessor,
            ILogger<UserAgentDelegatingHandler> logger)
        {
            this.logger = logger;
            this.httpContextAccessor = httpContextAccessor;
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            if (request?.Headers.Contains(HeaderNames.UserAgent) != false || httpContextAccessor?.HttpContext == null)
            {
                return base.SendAsync(request, cancellationToken);
            }

            foreach (var item in httpContextAccessor.HttpContext.Request.Headers[HeaderNames.UserAgent])
            {
                logger.LogInformation("Setting UserAgent to {item}", item);

                //Added without validation because external host headers with a ; after the product name were failing
                //+http://code.google.com/appengine; - would fail with a format exception, if the just the add method is used.
                if (!request.Headers.TryAddWithoutValidation(HeaderNames.UserAgent, item))
                {
                    logger.LogWarning("Could not add {userAgent} - {item}", HeaderNames.UserAgent, item);
                }
            }

            return base.SendAsync(request, cancellationToken);
        }
    }
}
