using DFC.Composite.Shell.HttpResponseMessageHandlers;
using DFC.Composite.Shell.Models;
using DFC.Composite.Shell.Models.AppRegistrationModels;
using DFC.Composite.Shell.Models.Exceptions;
using DFC.Composite.Shell.Services.AppRegistry;
using DFC.Composite.Shell.Services.Extensions;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Polly.CircuitBreaker;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace DFC.Composite.Shell.Services.ContentRetrieval
{
    public class ContentRetriever : IContentRetriever
    {
        private readonly HttpClient httpClient;
        private readonly ILogger<ContentRetriever> logger;
        private readonly IAppRegistryDataService appRegistryDataService;
        private readonly IHttpResponseMessageHandler responseHandler;
        private readonly MarkupMessages markupMessages;
        private readonly IMemoryCache memoryCache;

        public ContentRetriever(HttpClient httpClient, ILogger<ContentRetriever> logger, IAppRegistryDataService appRegistryDataService, IHttpResponseMessageHandler responseHandler, MarkupMessages markupMessages, IMemoryCache memoryCache)
        {
            this.httpClient = httpClient;
            this.logger = logger;
            this.appRegistryDataService = appRegistryDataService;
            this.responseHandler = responseHandler;
            this.markupMessages = markupMessages;
            this.memoryCache = memoryCache;
        }

        public async Task<string> GetContent(string url, string path, RegionModel regionModel, bool followRedirects, string requestBaseUrl)
        {
            var cacheKey = $"{url}_{followRedirects}_{requestBaseUrl}";

            if (!memoryCache.TryGetValue(cacheKey, out string content))
            {
                content = await GetContent_WithoutCaching(url, path, regionModel, followRedirects, requestBaseUrl);
                memoryCache.Set(cacheKey, content, TimeSpan.FromSeconds(30));
            }

            return content;
        }

        private async Task<string> GetContent_WithoutCaching(string url, string path, RegionModel regionModel, bool followRedirects, string requestBaseUrl)
        {
            const int MaxRedirections = 10;

            _ = regionModel ?? throw new ArgumentNullException(nameof(regionModel));

            string results = null;

            try
            {
                if (regionModel.IsHealthy)
                {
                    logger.LogInformation($"{nameof(GetContent)}: Getting child response from: {url}");

                    var response = await GetContentIfRedirectedAsync(requestBaseUrl, url, followRedirects, MaxRedirections).ConfigureAwait(false);

                    if (response != null && !response.IsSuccessStatusCode)
                    {
                        throw new EnhancedHttpException(response.StatusCode, response.ReasonPhrase, url);
                    }

                    responseHandler.Process(response);

                    if (response != null)
                    {
                        results = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
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
                    await appRegistryDataService.SetRegionHealthState(path, regionModel.PageRegion, false).ConfigureAwait(false);
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

                    var response = await httpClient.SendAsync(request).ConfigureAwait(false);

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

                    results = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

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
                    await appRegistryDataService.SetRegionHealthState(path, regionModel.PageRegion, false).ConfigureAwait(false);
                }

                results = !string.IsNullOrWhiteSpace(regionModel.OfflineHtml) ? regionModel.OfflineHtml : markupMessages.GetRegionOfflineHtml(regionModel.PageRegion);
            }

            return results;
        }

        private async Task<HttpResponseMessage> GetContentIfRedirectedAsync(string requestBaseUrl, string url, bool followRedirects, int maxRedirections)
        {
            HttpResponseMessage response = null;

            for (int i = 0; i < maxRedirections; i++)
            {
                var request = new HttpRequestMessage(HttpMethod.Get, url);

                response = await httpClient.SendAsync(request).ConfigureAwait(false);

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