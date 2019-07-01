using DFC.Composite.Shell.ClientHandlers;
using DFC.Composite.Shell.Common;
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
            IConfiguration configuration,
            string keyPrefix,
            string configurationSectionName = Constants.Policies)
        {
            var section = configuration.GetSection(configurationSectionName);
            var policyOptions = section.Get<PolicyOptions>();

            policyRegistry.Add(
                keyPrefix + "_" + nameof(PolicyOptions.HttpRetry),
                HttpPolicyExtensions
                    .HandleTransientHttpError()
                    .WaitAndRetryAsync(
                        policyOptions.HttpRetry.Count,
                        retryAttempt => TimeSpan.FromSeconds(Math.Pow(policyOptions.HttpRetry.BackoffPower, retryAttempt))));

            policyRegistry.Add(
                keyPrefix + "_" + nameof(PolicyOptions.HttpCircuitBreaker),
                HttpPolicyExtensions
                    .HandleTransientHttpError()
                    .CircuitBreakerAsync(
                        handledEventsAllowedBeforeBreaking: policyOptions.HttpCircuitBreaker.ExceptionsAllowedBeforeBreaking,
                        durationOfBreak: policyOptions.HttpCircuitBreaker.DurationOfBreak));

            return services;
        }

        public static IServiceCollection AddHttpClient<TClient, TImplementation, TClientOptions>(
                    this IServiceCollection services,
                    IConfiguration configuration,
                    string configurationSectionName,
                    string retryPolicyName,
                    string circuitBreakerPolicyName)
                    where TClient : class
                    where TImplementation : class, TClient
                    where TClientOptions : HttpClientOptions, new() =>
                    services
                        .Configure<TClientOptions>(configuration.GetSection(configurationSectionName))
                        .AddTransient<CorrelationIdDelegatingHandler>()
                        .AddTransient<UserAgentDelegatingHandler>()
                        .AddHttpClient<TClient, TImplementation>()
                        .ConfigureHttpClient((sp, options) =>
                        {
                            var httpClientOptions = sp
                                .GetRequiredService<IOptions<TClientOptions>>()
                                .Value;
                            options.BaseAddress = httpClientOptions.BaseAddress;
                            options.Timeout = httpClientOptions.Timeout;
                            options.DefaultRequestHeaders.Add(HeaderNames.Accept, MediaTypeNames.Text.Html);
                        })
                        .ConfigurePrimaryHttpMessageHandler(x => new DefaultHttpClientHandler())
                        .AddPolicyHandlerFromRegistry(configurationSectionName + "_" + retryPolicyName)
                        .AddPolicyHandlerFromRegistry(configurationSectionName + "_" + circuitBreakerPolicyName)
                        .AddHttpMessageHandler<CorrelationIdDelegatingHandler>()
                        .AddHttpMessageHandler<UserAgentDelegatingHandler>()
                        .ConfigurePrimaryHttpMessageHandler(() =>
                        {
                            return new HttpClientHandler()
                            {
                                AllowAutoRedirect = true
                            };
                        })
                        .Services;

    }
}
