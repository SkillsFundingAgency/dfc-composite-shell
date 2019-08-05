using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using DFC.Composite.Shell.Common;
using DFC.Composite.Shell.Exceptions;
using DFC.Composite.Shell.Extensions;
using DFC.Composite.Shell.HttpResponseMessageHandlers;
using DFC.Composite.Shell.Models;
using DFC.Composite.Shell.Services.Regions;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.Logging;
using Polly.CircuitBreaker;

namespace DFC.Composite.Shell.Services.ContentRetrieve
{

    public class ContentRetriever : IContentRetriever
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<ContentRetriever> _logger;
        private readonly IRegionService _regionService;
        private readonly IHttpResponseMessageHandler _responseHandler;

        public ContentRetriever(HttpClient httpClient, ILogger<ContentRetriever> logger, IRegionService regionService, IHttpResponseMessageHandler responseHandler)
        {
            _httpClient = httpClient;
            _logger = logger;
            _regionService = regionService;
            _responseHandler = responseHandler;
        }

        public async Task<string> GetContent(string url, RegionModel regionModel, bool followRedirects, string requestBaseUrl)
        {
            string results = null;

            try
            {
                if (regionModel.IsHealthy)
                {
                    _logger.LogInformation($"{nameof(GetContent)}: Getting child response from: {url}");

                    HttpResponseMessage response = null;

                    for (int i = 0; i < 10; i++)
                    {                        
                        var request = new HttpRequestMessage(HttpMethod.Get, url);
                        
                        response = await _httpClient.SendAsync(request);

                        if (response.IsRedirectionStatus())
                        {
                            if (followRedirects)
                            {
                                if (response.Headers.Location.IsAbsoluteUri)
                                {
                                    url = response.Headers.Location.ToString();
                                }
                                else
                                {
                                    url = $"{requestBaseUrl}/{response.Headers.Location.ToString().TrimStart('/')}";
                                }

                                _logger.LogInformation($"{nameof(GetContent)}: Redirecting get of child response from: {url}");
                            }
                            else
                            {
                                var relativeUrl = response.Headers.Location.IsAbsoluteUri
                                    ? response.Headers.Location.PathAndQuery
                                    : response.Headers.Location.ToString();
                                string redirectUrl = $"{requestBaseUrl}{relativeUrl}";

                                throw new RedirectException(new Uri(url), new Uri(redirectUrl), 
                                    response.StatusCode == HttpStatusCode.PermanentRedirect);
                            }
                        }
                        else
                        {
                            break;
                        }
                    }

                    response.EnsureSuccessStatusCode();

                    _responseHandler.Process(response);

                    results = await response.Content.ReadAsStringAsync();

                    _logger.LogInformation($"{nameof(GetContent)}: Received child response from: {url}");
                }
                else
                {
                    if (!string.IsNullOrEmpty(regionModel.OfflineHTML))
                    {
                        results = regionModel.OfflineHTML;
                    }
                }
            }
            catch (RedirectException)
            {
                throw;
            }
            catch (BrokenCircuitException ex)
            {
                _logger.LogError(ex, $"{nameof(ContentRetriever)}: BrokenCircuit: {url} - {ex.Message}");

                if (regionModel.HeathCheckRequired)
                {
                    await _regionService.MarkAsUnhealthyAsync(regionModel.Path, regionModel.PageRegion);
                }

                if (!string.IsNullOrEmpty(regionModel.OfflineHTML))
                {
                    results = regionModel.OfflineHTML;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"{nameof(ContentRetriever)}: {url} - {ex.Message}");

                if (!string.IsNullOrEmpty(regionModel.OfflineHTML))
                {
                    results = regionModel.OfflineHTML;
                }
            }

            return results;
        }

        public async Task<string> PostContent(string url, RegionModel regionModel, IEnumerable<KeyValuePair<string, string>> formParameters, string requestBaseUrl)
        {
            string results = null;

            try
            {
                if (regionModel.IsHealthy)
                {
                    _logger.LogInformation($"{nameof(PostContent)}: Posting child response from: {url}");

                    var request = new HttpRequestMessage(HttpMethod.Post, url)
                    {
                        Content = new FormUrlEncodedContent(formParameters),
                    };
                                        
                    var response = await _httpClient.SendAsync(request);

                    if (response.StatusCode == HttpStatusCode.Found)
                    {
                        string redirectUrl = requestBaseUrl;

                        if (response.Headers.Location.IsAbsoluteUri) {
                            redirectUrl += response.Headers.Location.PathAndQuery;
                        }
                        else
                        {
                            redirectUrl += response.Headers.Location;
                        }

                        throw new RedirectException(new Uri(url), new Uri(redirectUrl), response.StatusCode == HttpStatusCode.PermanentRedirect);
                    }

                    response.EnsureSuccessStatusCode();

                    results = await response.Content.ReadAsStringAsync();

                    _logger.LogInformation($"{nameof(PostContent)}: Received child response from: {url}");
                }
                else
                {
                    if (!string.IsNullOrEmpty(regionModel.OfflineHTML))
                    {
                        results = regionModel.OfflineHTML;
                    }
                }
            }
            catch (RedirectException)
            {
                throw;
            }
            catch (BrokenCircuitException ex)
            {
                _logger.LogError(ex, $"{nameof(ContentRetriever)}: BrokenCircuit: {url} - {ex.Message}");

                if (regionModel.HeathCheckRequired)
                {
                    await _regionService.MarkAsUnhealthyAsync(regionModel.Path, regionModel.PageRegion);
                }

                if (!string.IsNullOrEmpty(regionModel.OfflineHTML))
                {
                    results = regionModel.OfflineHTML;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"{nameof(ContentRetriever)}: {url} - {ex.Message}");

                if (!string.IsNullOrEmpty(regionModel.OfflineHTML))
                {
                    results = regionModel.OfflineHTML;
                }
            }

            return results;
        }
    }
}
