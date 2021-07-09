using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Net.Mime;
using System.Threading.Tasks;

namespace DFC.Composite.Shell.Services.Neo4J
{
    public class Neo4JService : INeo4JService
    {
        public const string CreateEndpoint = "api/CreateVisit";
        private readonly bool sendData;
        private readonly HttpClient httpClient;
        private readonly ILogger<Neo4JService> logger;

        public Neo4JService(IOptions<Neo4JSettings> settings, HttpClient httpClient, ILogger<Neo4JService> logger)
        {
            if (settings == null)
            {
                throw new ArgumentNullException(nameof(settings));
            }

            _ = httpClient ?? throw new ArgumentNullException(nameof(httpClient));

            this.httpClient = httpClient;
            sendData = settings.Value.SendData;
            this.logger = logger;
        }

        public Task InsertNewRequest(HttpRequest request)
        {
            if (!sendData)
            {
                return Task.CompletedTask;
            }

            if (request != null)
            {
                return InsertNewRequestInternal(request);
            }

            logger.LogWarning("{action}: Visit API request failed Request parameter null", nameof(Action));
            return Task.CompletedTask;
        }

        private static Guid GetSessionId(HttpRequest request)
        {
            if (request == null)
            {
                return default;
            }

            const string compositeSessionIdHeaderName = "ncs_session_cookie";
            request.Cookies.TryGetValue(compositeSessionIdHeaderName, out var headerValue);

            return Guid.TryParse(headerValue, out var guidValue) ? guidValue : default;
        }

        private static string GetRefererLocalPath(HttpRequest request)
        {
            var refererHeader = GetRefererString(request);

            return string.IsNullOrEmpty(refererHeader) ? string.Empty : new Uri(refererHeader).LocalPath;
        }

        private static string GetRefererString(HttpRequest request)
        {
            return request.GetTypedHeaders().Referer?.ToString();
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "Continuing to use existing swallow pattern")]
        private async Task InsertNewRequestInternal(HttpRequest request)
        {
            var model = new VisitRequestModel
            {
                Referer = GetRefererLocalPath(request),
                RequestPath = request.Path,
                SessionId = GetSessionId(request),
                UserAgent = request.Headers["User-agent"].ToString(),
                VisitTime = DateTime.UtcNow,
            };

            using var msg = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri($"{httpClient.BaseAddress}{CreateEndpoint}"),
                Content = new ObjectContent(typeof(VisitRequestModel), model, new JsonMediaTypeFormatter(), MediaTypeNames.Application.Json),
            };
            try
            {
                var response = await httpClient.SendAsync(msg);
                response.EnsureSuccessStatusCode();
            }
            catch (Exception e)
            {
                logger.LogWarning("{action}: Visit API request failed with error {message}", nameof(Action), e.Message);
            }
        }
    }
}
