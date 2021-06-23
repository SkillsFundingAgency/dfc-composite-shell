using DFC.Composite.Shell.ClientHandlers;
using DFC.Composite.Shell.Policies.Options;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.Net.Http.Headers;
using Polly;
using Polly.Extensions.Http;
using Polly.Registry;
using System;
using System.Net.Http;
using System.Net.Mime;

namespace DFC.Composite.Shell.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddPolicies(
            this IServiceCollection services,
            IPolicyRegistry<string> policyRegistry,
            string keyPrefix,
            PolicyOptions policyOptions)
        {
            if (policyRegistry == null || policyOptions == null)
            {
                return services;
            }

            policyRegistry.Add(
                $"{keyPrefix}_{nameof(PolicyOptions.HttpRetry)}",
                HttpPolicyExtensions
                    .HandleTransientHttpError()
                    .OrResult(msg => msg.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
                    .OrResult(r => r?.Headers?.RetryAfter != null)
                    .WaitAndRetryAsync(
                        policyOptions.HttpRetry.Count,
                        retryAttempt =>
                            TimeSpan.FromSeconds(Math.Pow(policyOptions.HttpRetry.BackoffPower, retryAttempt))));

            policyRegistry.Add(
                $"{keyPrefix}_{nameof(PolicyOptions.HttpCircuitBreaker)}",
                HttpPolicyExtensions
                    .HandleTransientHttpError()
                    .CircuitBreakerAsync(
                        handledEventsAllowedBeforeBreaking: policyOptions.HttpCircuitBreaker
                            .ExceptionsAllowedBeforeBreaking,
                        durationOfBreak: policyOptions.HttpCircuitBreaker.DurationOfBreak));

            return services;
        }

        public static IHttpClientBuilder AddHttpClient<TClient, TImplementation, TClientOptions>(
                    this IServiceCollection services,
                    IConfiguration configuration,
                    string configurationSectionName,
                    string retryPolicyName,
                    string circuitBreakerPolicyName)
                    where TClient : class
                    where TImplementation : class, TClient
                    where TClientOptions : HttpClientOptions, new() =>
                    services
                        .Configure<TClientOptions>(configuration?.GetSection(configurationSectionName))
                        .AddHttpClient<TClient, TImplementation>()
                        .ConfigureHttpClient((sp, options) =>
                        {
                            var httpClientOptions = sp
                                .GetRequiredService<IOptions<TClientOptions>>()
                                .Value;
                            options.BaseAddress = httpClientOptions.BaseAddress;
                            options.Timeout = httpClientOptions.Timeout;
                            options.DefaultRequestHeaders.Add(HeaderNames.Accept, MediaTypeNames.Text.Html);

                            if (!string.IsNullOrWhiteSpace(httpClientOptions.ApiKey))
                            {
                                options.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", httpClientOptions.ApiKey);
                            }
                        })
                        .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler()
                        {
                            // This prevents asp.net core from adding its own cookie values to the outgoing request
                            UseCookies = false,
                            AllowAutoRedirect = false,
                        })
                        .AddPolicyHandlerFromRegistry($"{configurationSectionName}_{retryPolicyName}")
                        .AddPolicyHandlerFromRegistry($"{configurationSectionName}_{circuitBreakerPolicyName}")
                        .AddHttpMessageHandler<UserAgentDelegatingHandler>()
                        .AddHttpMessageHandler<OriginalHostDelegatingHandler>()
                        .AddHttpMessageHandler<CompositeRequestDelegatingHandler>();
    }
}