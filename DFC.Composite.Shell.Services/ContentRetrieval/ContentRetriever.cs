using DFC.Composite.Shell.HttpResponseMessageHandlers;
using DFC.Composite.Shell.Models;
using DFC.Composite.Shell.Models.AppRegistrationModels;
using DFC.Composite.Shell.Models.Exceptions;
using DFC.Composite.Shell.Services.AppRegistry;
using DFC.Composite.Shell.Services.Extensions;
using DFC.Composite.Shell.Services.UriSpecifcHttpClient;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

using Polly.CircuitBreaker;

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace DFC.Composite.Shell.Services.ContentRetrieval
{
    public class ContentRetriever : IContentRetriever
    {
        private readonly IUriSpecifcHttpClientFactory httpClientFactory;
        private readonly ILogger<ContentRetriever> logger;
        private readonly IAppRegistryDataService appRegistryDataService;
        private readonly IHttpResponseMessageHandler responseHandler;
        private readonly MarkupMessages markupMessages;
        private readonly IMemoryCache memoryCache;
        private readonly PassOnHeaderSettings headerSettings;

        public ContentRetriever(IUriSpecifcHttpClientFactory httpClientFactory, ILogger<ContentRetriever> logger, IAppRegistryDataService appRegistryDataService, IHttpResponseMessageHandler responseHandler, MarkupMessages markupMessages, IMemoryCache memoryCache, IOptions<PassOnHeaderSettings> headerSettings)
        {
            this.httpClientFactory = httpClientFactory;
            this.logger = logger;
            this.appRegistryDataService = appRegistryDataService;
            this.responseHandler = responseHandler;
            this.markupMessages = markupMessages;
            this.memoryCache = memoryCache;
            this.headerSettings = headerSettings?.Value ?? new PassOnHeaderSettings();
        }

        public async Task<string> GetContent(string url, string path, RegionModel regionModel, bool followRedirects, string requestBaseUrl, IHeaderDictionary headers)
        {
            const int CacheDurationInSeconds = 30;
            var cacheKey = BuildCacheKey(url, followRedirects, requestBaseUrl);

            if (!memoryCache.TryGetValue(cacheKey, out string content))
            {
                content = await GetContent_WithoutCaching(url, path, regionModel, followRedirects, requestBaseUrl, headers);

                if (IsInteractiveContent(url) || string.IsNullOrWhiteSpace(content))
                {
                    return content;
                }

                memoryCache.Set(cacheKey, content, TimeSpan.FromSeconds(CacheDurationInSeconds));
            }

            return content;
        }

        private bool IsInteractiveContent(string url)
        {
            return !(
                url?.Contains("app-pages-as", StringComparison.OrdinalIgnoreCase) == true
                || url?.Contains("app-jobprof-as", StringComparison.OrdinalIgnoreCase) == true
                || url?.Contains("app-contactus-as", StringComparison.OrdinalIgnoreCase) == true);
        }

        private string BuildCacheKey(string url, bool followRedirects, string requestBaseUrl)
        {
            return $"{nameof(ContentRetriever)}_Url:{url}_FollowRedirects:{followRedirects}_RequestBaseUrl:{requestBaseUrl}";
        }

        private async Task<string> GetContent_WithoutCaching(string url, string path, RegionModel regionModel, bool followRedirects, string requestBaseUrl, IHeaderDictionary headers)
        {
            const int MaxRedirections = 10;

            _ = regionModel ?? throw new ArgumentNullException(nameof(regionModel));

            string results = null;

            try
            {
                if (regionModel.IsHealthy)
                {
                    logger.LogInformation($"{nameof(GetContent)}: Getting child response from: {url}");

                    var response = await GetContentIfRedirectedAsync(requestBaseUrl, url, followRedirects, MaxRedirections, regionModel, headers);

                    if (response != null && !response.IsSuccessStatusCode)
                    {
                        throw new EnhancedHttpException(response.StatusCode, response.ReasonPhrase, url);
                    }

                    responseHandler.Process(response);

                    if (response != null)
                    {
                        results = await response.Content.ReadAsStringAsync();
                    }

                    logger.LogInformation($"{nameof(GetContent)}: Received child response from: {url}");
                }
                else
                {
                    results = !string.IsNullOrWhiteSpace(regionModel.OfflineHtml) ? regionModel.OfflineHtml : markupMessages.GetRegionOfflineHtml(regionModel.PageRegion);
                }
            }
            catch (BrokenCircuitException ex)
            {
                logger.LogError(ex, $"{nameof(ContentRetriever)}: BrokenCircuit: {url} - {ex.Message}");

                if (regionModel.HealthCheckRequired)
                {
                    await appRegistryDataService.SetRegionHealthState(path, regionModel.PageRegion, false);
                }

                results = !string.IsNullOrWhiteSpace(regionModel.OfflineHtml) ? regionModel.OfflineHtml : markupMessages.GetRegionOfflineHtml(regionModel.PageRegion);
            }

            return results;
        }

        public async Task<string> PostContent(string url, string path, RegionModel regionModel, IEnumerable<KeyValuePair<string, string>> formParameters, string requestBaseUrl)
        {
            _ = regionModel ?? throw new ArgumentNullException(nameof(regionModel));

            string results = null;

            try
            {
                if (regionModel.IsHealthy)
                {
                    logger.LogInformation($"{nameof(PostContent)}: Posting child response from: {url}");

                    var request = new HttpRequestMessage(HttpMethod.Post, url)
                    {
                        Content = formParameters != null ? new FormUrlEncodedContent(formParameters) : null,
                    };

                    var httpClient = httpClientFactory.GetClientForRegionEndpoint(regionModel.RegionEndpoint);
                    var response = await httpClient.SendAsync(request);

                    if (response.IsRedirectionStatus())
                    {
                        responseHandler.Process(response);

                        var redirectUrl = requestBaseUrl;

                        redirectUrl += response.Headers.Location.IsAbsoluteUri
                            ? response.Headers.Location.PathAndQuery.ToString(CultureInfo.InvariantCulture)
                            : response.Headers.Location.ToString();

                        throw new RedirectException(new Uri(url), new Uri(redirectUrl), response.StatusCode == HttpStatusCode.PermanentRedirect);
                    }

                    if (!response.IsSuccessStatusCode)
                    {
                        throw new EnhancedHttpException(response.StatusCode, response.ReasonPhrase, url);
                    }

                    results = await response.Content.ReadAsStringAsync();

                    logger.LogInformation($"{nameof(PostContent)}: Received child response from: {url}");
                }
                else
                {
                    results = !string.IsNullOrWhiteSpace(regionModel.OfflineHtml) ? regionModel.OfflineHtml : markupMessages.GetRegionOfflineHtml(regionModel.PageRegion);
                }
            }
            catch (BrokenCircuitException ex)
            {
                logger.LogError(ex, $"{nameof(ContentRetriever)}: BrokenCircuit: {url} - {ex.Message}");

                if (regionModel.HealthCheckRequired)
                {
                    await appRegistryDataService.SetRegionHealthState(path, regionModel.PageRegion, false);
                }

                results = !string.IsNullOrWhiteSpace(regionModel.OfflineHtml) ? regionModel.OfflineHtml : markupMessages.GetRegionOfflineHtml(regionModel.PageRegion);
            }

            return results;
        }

        private async Task<HttpResponseMessage> GetContentIfRedirectedAsync(string requestBaseUrl, string url, bool followRedirects, int maxRedirections, RegionModel regionModel, IHeaderDictionary headers)
        {
            HttpResponseMessage response = null;
            var httpClient = httpClientFactory.GetClientForRegionEndpoint(regionModel.RegionEndpoint);

            for (int i = 0; i < maxRedirections; i++)
            {
                var request = new HttpRequestMessage(HttpMethod.Get, url);
                foreach (var (key, value) in headers.Where(h => headerSettings.SupportedHeaders
                    .Any(sh => string.Equals(sh, h.Key, StringComparison.CurrentCultureIgnoreCase))))
                {
                    request.Headers.Add(key, value.ToArray());
                }

                response = await httpClient.SendAsync(request);

                if (!response.IsRedirectionStatus())
                {
                    return response;
                }

                if (!followRedirects)
                {
                    var redirectUrl = response.Headers.Location.IsAbsoluteUri
                        ? response.Headers.Location.ToString()
                        : $"{requestBaseUrl}{response.Headers.Location.ToString()}";

                    throw new RedirectException(new Uri(url), new Uri(redirectUrl), response.StatusCode == HttpStatusCode.PermanentRedirect);
                }

                url = response.Headers.Location.IsAbsoluteUri
                    ? response.Headers.Location.ToString()
                    : $"{requestBaseUrl}/{response.Headers.Location.ToString().TrimStart('/')}";

                logger.LogWarning($"{nameof(GetContent)}: Redirecting get of child response from: {url}");
            }

            return null;
        }
    }
}