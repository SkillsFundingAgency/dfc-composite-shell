﻿using DFC.Composite.Shell.HttpResponseMessageHandlers;
using DFC.Composite.Shell.Models;
using DFC.Composite.Shell.Models.Exceptions;
using DFC.Composite.Shell.Services.Extensions;
using DFC.Composite.Shell.Services.Regions;
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
        private readonly IRegionService regionService;
        private readonly IHttpResponseMessageHandler responseHandler;

        public ContentRetriever(HttpClient httpClient, ILogger<ContentRetriever> logger, IRegionService regionService, IHttpResponseMessageHandler responseHandler)
        {
            this.httpClient = httpClient;
            this.logger = logger;
            this.regionService = regionService;
            this.responseHandler = responseHandler;
        }

        public async Task<string> GetContent(string url, RegionModel regionModel, bool followRedirects, string requestBaseUrl)
        {
            const int MaxRedirections = 10;

            if (regionModel == null)
            {
                throw new ArgumentNullException(nameof(regionModel));
            }

            string results = null;

            try
            {
                if (regionModel.IsHealthy)
                {
                    logger.LogInformation($"{nameof(GetContent)}: Getting child response from: {url}");

                    var response = await GetContentIfRedirectedAsync(requestBaseUrl, url, followRedirects, MaxRedirections).ConfigureAwait(false);

                    if (response != null)
                    {
                        if (response.StatusCode == HttpStatusCode.NotFound)
                        {
                            var errorString = $"The content {url} is not found";

                            logger.LogWarning($"{nameof(Action)}: {errorString}");

                            var redirectTo = new Uri($"/alert/{(int)HttpStatusCode.NotFound}", UriKind.Relative);

                            throw new RedirectException(new Uri(url, UriKind.Absolute), redirectTo, false);
                        }

                        if (!response.IsSuccessStatusCode)
                        {
                            throw new EnhancedHttpException(response.StatusCode, response.ReasonPhrase);
                        }
                    }

                    responseHandler.Process(response);

                    if (response != null)
                    {
                        results = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                    }

                    logger.LogInformation($"{nameof(GetContent)}: Received child response from: {url}");
                }
                else if (!string.IsNullOrWhiteSpace(regionModel.OfflineHTML))
                {
                    results = regionModel.OfflineHTML;
                }
            }
            catch (BrokenCircuitException ex)
            {
                logger.LogError(ex, $"{nameof(ContentRetriever)}: BrokenCircuit: {url} - {ex.Message}");

                if (regionModel.HeathCheckRequired)
                {
                    await regionService.SetRegionHealthState(regionModel.Path, regionModel.PageRegion, false).ConfigureAwait(false);
                }

                results = regionModel.OfflineHTML;
            }

            return results;
        }

        public async Task<string> PostContent(string url, RegionModel regionModel, IEnumerable<KeyValuePair<string, string>> formParameters, string requestBaseUrl)
        {
            if (regionModel == null)
            {
                throw new ArgumentNullException(nameof(regionModel));
            }

            string results = null;

            try
            {
                if (regionModel.IsHealthy)
                {
                    logger.LogInformation($"{nameof(PostContent)}: Posting child response from: {url}");

                    var request = new HttpRequestMessage(HttpMethod.Post, url)
                    {
                        Content = new FormUrlEncodedContent(formParameters),
                    };

                    var response = await httpClient.SendAsync(request).ConfigureAwait(false);

                    if (response.StatusCode == HttpStatusCode.Found)
                    {
                        var redirectUrl = requestBaseUrl;

                        redirectUrl += response.Headers.Location.IsAbsoluteUri
                            ? response.Headers.Location.PathAndQuery.ToString(CultureInfo.InvariantCulture)
                            : response.Headers.Location.ToString();

                        throw new RedirectException(new Uri(url), new Uri(redirectUrl), response.StatusCode == HttpStatusCode.PermanentRedirect);
                    }

                    if (response.StatusCode == HttpStatusCode.NotFound)
                    {
                        var errorString = $"The content {url} is not found";

                        logger.LogWarning($"{nameof(Action)}: {errorString}");

                        var redirectTo = new Uri($"/alert/{(int)HttpStatusCode.NotFound}", UriKind.Relative);

                        throw new RedirectException(new Uri(url, UriKind.Absolute), redirectTo, false);
                    }

                    if (!response.IsSuccessStatusCode)
                    {
                        throw new EnhancedHttpException(response.StatusCode, response.ReasonPhrase);
                    }

                    results = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

                    logger.LogInformation($"{nameof(PostContent)}: Received child response from: {url}");
                }
                else if (!string.IsNullOrWhiteSpace(regionModel.OfflineHTML))
                {
                    results = regionModel.OfflineHTML;
                }
            }
            catch (BrokenCircuitException ex)
            {
                logger.LogError(ex, $"{nameof(ContentRetriever)}: BrokenCircuit: {url} - {ex.Message}");

                if (regionModel.HeathCheckRequired)
                {
                    await regionService.SetRegionHealthState(regionModel.Path, regionModel.PageRegion, false).ConfigureAwait(false);
                }

                results = regionModel.OfflineHTML;
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
                    var relativeUrl = response.Headers.Location.IsAbsoluteUri
                        ? response.Headers.Location.PathAndQuery
                        : response.Headers.Location.ToString();
                    var redirectUrl = $"{requestBaseUrl}{relativeUrl}";

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