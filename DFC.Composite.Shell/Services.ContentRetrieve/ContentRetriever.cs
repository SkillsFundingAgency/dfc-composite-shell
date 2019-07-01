using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using DFC.Composite.Shell.Exceptions;
using Microsoft.Extensions.Logging;
using Polly.CircuitBreaker;

namespace DFC.Composite.Shell.Services.ContentRetrieve
{
    public class ContentRetriever : IContentRetriever
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<ContentRetriever> _logger;

        public ContentRetriever(HttpClient httpClient, ILogger<ContentRetriever> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        public async Task<string> GetContent(string url, bool isHealthy, string offlineHtml, bool followRedirects)
        {
            string results = null;

            try
            {
                if (isHealthy)
                {
                    _logger.LogInformation($"{nameof(GetContent)}: Getting child response from: {url}");

                    HttpResponseMessage response = null;

                    for (int i = 0; i < 10; i++)
                    {
                        response = await _httpClient.GetAsync(url);

                        if (response.StatusCode == HttpStatusCode.MovedPermanently)
                        {
                            if (followRedirects)
                            {
                                url = response.Headers.Location.ToString();

                                _logger.LogInformation($"{nameof(GetContent)}: Redirecting child response to: {url}");
                            }
                            else
                            {
                                string redirectUrl = response.Headers.Location.ToString();

                                if (!response.Headers.Location.IsAbsoluteUri)
                                {
                                    var redirectUri = new Uri(url);
                                    string postScheme = redirectUri.Scheme;
                                    string postAuthority = redirectUri.Authority;

                                    redirectUrl = $"{postScheme}://{postAuthority}{redirectUrl}";
                                }

                                throw new RedirectException(new Uri(url), new Uri(redirectUrl));
                            }
                        }
                        else
                        {
                            break;
                        }
                    }

                    response.EnsureSuccessStatusCode();

                    results = await response.Content.ReadAsStringAsync();

                    _logger.LogInformation($"{nameof(GetContent)}: Received child response from: {url}");
                }
                else
                {
                    if (!string.IsNullOrEmpty(offlineHtml))
                    {
                        results = offlineHtml;
                    }
                }
            }
            catch (RedirectException ex)
            {
                throw;
            }
            catch (BrokenCircuitException ex)
            {
                _logger.LogError(ex, $"{nameof(ContentRetriever)}: BrokenCircuit: {url} - {ex.Message}");

                if (!string.IsNullOrEmpty(offlineHtml))
                {
                    results = offlineHtml;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"{nameof(ContentRetriever)}: {url} - {ex.Message}");

                if (!string.IsNullOrEmpty(offlineHtml))
                {
                    results = offlineHtml;
                }
            }

            return results;
        }

        public async Task<string> PostContent(string url, bool isHealthy, string offlineHtml, IEnumerable<KeyValuePair<string, string>> formParameters)
        {
            string results = null;

            try
            {
                if (isHealthy)
                {
                    _logger.LogInformation($"{nameof(GetContent)}: posting child response from: {url}");

                    var request = new HttpRequestMessage(HttpMethod.Post, url)
                    {
                        Content = new FormUrlEncodedContent(formParameters)
                    };

                    var response = await _httpClient.SendAsync(request);

                    if (response.StatusCode == HttpStatusCode.Found)
                    {
                        string redirectUrl = response.Headers.Location.ToString();

                        if (!response.Headers.Location.IsAbsoluteUri)
                        {
                            var postUri = new Uri(url);
                            string postScheme = postUri.Scheme;
                            string postAuthority = postUri.Authority;

                            redirectUrl = $"{postScheme}://{postAuthority}{redirectUrl}";
                        }

                        throw new RedirectException(new Uri(url), new Uri(redirectUrl));
                    }

                    response.EnsureSuccessStatusCode();

                    results = await response.Content.ReadAsStringAsync();

                    _logger.LogInformation($"{nameof(GetContent)}: Received child response from: {url}");
                }
                else
                {
                    if (!string.IsNullOrEmpty(offlineHtml))
                    {
                        results = offlineHtml;
                    }
                }
            }
            catch (RedirectException ex)
            {
                throw;
            }
            catch (BrokenCircuitException ex)
            {
                _logger.LogError(ex, $"{nameof(ContentRetriever)}: BrokenCircuit: {url} - {ex.Message}");

                if (!string.IsNullOrEmpty(offlineHtml))
                {
                    results = offlineHtml;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"{nameof(ContentRetriever)}: {url} - {ex.Message}");

                if (!string.IsNullOrEmpty(offlineHtml))
                {
                    results = offlineHtml;
                }
            }

            return results;
        }
    }
}
