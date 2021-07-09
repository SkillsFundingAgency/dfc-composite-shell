using DFC.Composite.Shell.HttpResponseMessageHandlers;
using DFC.Composite.Shell.Models;
using DFC.Composite.Shell.Models.AppRegistration;
using DFC.Composite.Shell.Models.Exceptions;
using DFC.Composite.Shell.Services.AppRegistry;
using DFC.Composite.Shell.Services.Extensions;
using DFC.Composite.Shell.Services.UriSpecifcHttpClient;
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
        private readonly IUriSpecifcHttpClientFactory httpClientFactory;
        private readonly ILogger<ContentRetriever> logger;
        private readonly IAppRegistryService appRegistryDataService;
        private readonly IHttpResponseMessageHandler responseHandler;
        private readonly MarkupMessages markupMessages;

        public ContentRetriever(
            IUriSpecifcHttpClientFactory httpClientFactory,
            ILogger<ContentRetriever> logger,
            IAppRegistryService appRegistryDataService,
            IHttpResponseMessageHandler responseHandler,
            MarkupMessages markupMessages)
        {
            this.httpClientFactory = httpClientFactory;
            this.logger = logger;
            this.appRegistryDataService = appRegistryDataService;
            this.responseHandler = responseHandler;
            this.markupMessages = markupMessages;
        }

        public async Task<string> GetContentAsync(
            string url,
            string path,
            RegionModel regionModel,
            bool followRedirects,
            Uri requestBaseUrl)
        {
            _ = regionModel ?? throw new ArgumentNullException(nameof(regionModel));
            var results = default(string);
            const int MaxRedirections = 10;

            try
            {
                if (!regionModel.IsHealthy)
                {
                    return !string.IsNullOrWhiteSpace(regionModel.OfflineHtml)
                        ? regionModel.OfflineHtml : markupMessages.GetRegionOfflineHtml(regionModel.PageRegion);
                }

                logger.LogInformation("{methodName}: Getting child response from: {url}", nameof(GetContentAsync), url);

                var response = await GetContentHonouringRedirectionsAsync(requestBaseUrl, url, followRedirects, MaxRedirections, regionModel);

                if (response?.IsSuccessStatusCode == false)
                {
                    throw new HttpException(response.StatusCode, response.ReasonPhrase, url);
                }

                responseHandler.Process(response);

                if (response != null)
                {
                    results = await response.Content.ReadAsStringAsync();
                }

                logger.LogInformation("{methodName}: Received child response from: {url}", nameof(GetContentAsync), url);
            }
            catch (BrokenCircuitException ex)
            {
                logger.LogError(ex, "{controllerName}: BrokenCircuit: {url} - {exMessage}", nameof(ContentRetriever), url, ex.Message);

                if (regionModel.HealthCheckRequired)
                {
                    await appRegistryDataService.SetRegionHealthState(path, regionModel.PageRegion, false);
                }

                results = !string.IsNullOrWhiteSpace(regionModel.OfflineHtml) ?
                    regionModel.OfflineHtml : markupMessages.GetRegionOfflineHtml(regionModel.PageRegion);
            }

            return results;
        }

        public async Task<string> PostContentAsync(
            string url,
            string path,
            RegionModel regionModel,
            IEnumerable<KeyValuePair<string, string>> formParameters,
            Uri requestBaseUrl)
        {
            _ = regionModel ?? throw new ArgumentNullException(nameof(regionModel));
            var results = default(string);

            try
            {
                if (!regionModel.IsHealthy)
                {
                    return !string.IsNullOrWhiteSpace(regionModel.OfflineHtml)
                        ? regionModel.OfflineHtml : markupMessages.GetRegionOfflineHtml(regionModel.PageRegion);
                }

                logger.LogInformation(
                    "{content}: Posting child response from: {url}",
                    nameof(PostContentAsync),
                    url);

                var request = new HttpRequestMessage(HttpMethod.Post, url)
                {
                    Content = formParameters != null ? new FormUrlEncodedContent(formParameters) : null,
                };

                var httpClient = httpClientFactory.GetClientForRegionEndpoint(regionModel.RegionEndpoint);
                var response = await httpClient.SendAsync(request);

                if (response.IsRedirectionStatus())
                {
                    responseHandler.Process(response);

                    var redirectUrl = requestBaseUrl?.ToString();
                    redirectUrl += response.Headers.Location.IsAbsoluteUri
                        ? response.Headers.Location.PathAndQuery.ToString(CultureInfo.InvariantCulture)
                        : response.Headers.Location.ToString();

                    throw new RedirectRequest(
                        new Uri(url),
                        new Uri(redirectUrl),
                        response.StatusCode == HttpStatusCode.PermanentRedirect);
                }

                if (!response.IsSuccessStatusCode)
                {
                    throw new HttpException(response.StatusCode, response.ReasonPhrase, url);
                }

                results = await response.Content.ReadAsStringAsync();

                logger.LogInformation("{content}: Received child response from: {url}", nameof(GetContentAsync), url);
            }
            catch (BrokenCircuitException ex)
            {
                logger.LogError(ex, "{content}: BrokenCircuit: {url} - {message}", nameof(ContentRetriever), url, ex.Message);

                if (regionModel.HealthCheckRequired)
                {
                    await appRegistryDataService.SetRegionHealthState(path, regionModel.PageRegion, false);
                }

                results = !string.IsNullOrWhiteSpace(regionModel.OfflineHtml) ?
                    regionModel.OfflineHtml : markupMessages.GetRegionOfflineHtml(regionModel.PageRegion);
            }

            return results;
        }

        private async Task<HttpResponseMessage> GetContentHonouringRedirectionsAsync(
            Uri requestBaseUrl,
            string url,
            bool followRedirects,
            int maxRedirections,
            RegionModel regionModel)
        {
            var attempt = 0;

            do
            {
                var request = new HttpRequestMessage(HttpMethod.Get, url);

                var httpClient = httpClientFactory.GetClientForRegionEndpoint(regionModel.RegionEndpoint);
                var response = await httpClient.SendAsync(request);

                if (!response.IsRedirectionStatus())
                {
                    return response;
                }

                if (!followRedirects)
                {
                    var redirectUrl = response.Headers.Location.IsAbsoluteUri
                        ? response.Headers.Location.ToString()
                        : $"{requestBaseUrl}{response.Headers.Location}";

                    throw new RedirectRequest(
                        new Uri(url),
                        new Uri(redirectUrl),
                        response.StatusCode == HttpStatusCode.PermanentRedirect);
                }

                url = response.Headers.Location.IsAbsoluteUri
                    ? response.Headers.Location.ToString()
                    : $"{requestBaseUrl}/{response.Headers.Location.ToString().TrimStart('/')}";

                logger.LogWarning("{content}: Redirecting get of child response from: {url}", nameof(GetContentAsync), url);
            }
            while (attempt++ > maxRedirections);

            return null;
        }
    }
}