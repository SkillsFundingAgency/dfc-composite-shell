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

        public Neo4JService(IOptions<Neo4JSettings> settings, HttpClient client, ILogger<Neo4JService> logger)
        {
            if (settings == null)
            {
                throw new ArgumentNullException(nameof(settings));
            }

            _ = client ?? throw new ArgumentNullException(nameof(client));

            httpClient = client;
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

            logger.LogWarning($"{nameof(Action)}: Visit API request failed Request parameter null");
            return Task.CompletedTask;
        }

        private static Guid GetSessionId(HttpRequest request)
        {
            const string compositeSessionIdHeaderName = "ncs_session_cookie";

            if (request != null && request.Cookies.TryGetValue(compositeSessionIdHeaderName, out var headerValue) && Guid.TryParse(headerValue, out var guidValue))
            {
                return guidValue;
            }

            return default;
        }

        private static string GetReferer(HttpRequest request)
        {
            var refererHeader = request.Headers["Referer"].ToString();

            return string.IsNullOrEmpty(refererHeader) ? string.Empty : new Uri(refererHeader).LocalPath;
        }

        private async Task InsertNewRequestInternal(HttpRequest request)
        {
            var model = new VisitRequestModel
            {
                Referer = GetReferer(request),
                RequestPath = request.Path,
                SessionId = GetSessionId(request),
                UserAgent = request.Headers["User-agent"].ToString(),
                VisitTime = DateTime.UtcNow,
            };

            using var msg = new HttpRequestMessage()
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri($"{httpClient.BaseAddress}{CreateEndpoint}"),
                Content = new ObjectContent(typeof(VisitRequestModel), model, new JsonMediaTypeFormatter(), MediaTypeNames.Application.Json),
            };
            try
            {
                using var response = await httpClient.SendAsync(msg).ConfigureAwait(false);
                response.EnsureSuccessStatusCode();
            }
            catch (Exception e)
            {
                logger.LogWarning($"{nameof(Action)}: Visit API request failed with error {e.Message}");
            }
        }
    }
}
