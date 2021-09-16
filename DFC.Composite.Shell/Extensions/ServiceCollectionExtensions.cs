using DFC.Composite.Shell.ClientHandlers;
using DFC.Composite.Shell.Models.Common;
using DFC.Composite.Shell.Policies.Options;
using DFC.Composite.Shell.Services.AjaxRequest;
using DFC.Composite.Shell.Services.ApplicationHealth;
using DFC.Composite.Shell.Services.ApplicationRobot;
using DFC.Composite.Shell.Services.ApplicationSitemap;
using DFC.Composite.Shell.Services.AppRegistry;
using DFC.Composite.Shell.Services.Banner;
using DFC.Composite.Shell.Services.Neo4J;
using DFC.Composite.Shell.Services.UriSpecifcHttpClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Net.Http.Headers;
using Polly;
using Polly.Extensions.Http;
using Polly.Registry;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net.Http;
using System.Net.Mime;

namespace DFC.Composite.Shell.Extensions
{
    [ExcludeFromCodeCoverage]
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddPolicies(
            this IServiceCollection services,
            IPolicyRegistry<string> policyRegistry,
            string retryPolicyKey,
            string circuitBreakerKey,
            PolicyOptions policyOptions)
        {
            if (policyRegistry == null || policyOptions == null)
            {
                return services;
            }

            policyRegistry.Add(
                retryPolicyKey,
                HttpPolicyExtensions
                    .HandleTransientHttpError()
                    .OrResult(msg => msg.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
                    .OrResult(msg => msg?.Headers?.RetryAfter != null)
                    .WaitAndRetryAsync(
                        policyOptions.HttpRetry.Count,
                        retryAttempt =>
                            TimeSpan.FromSeconds(Math.Pow(policyOptions.HttpRetry.BackoffPower, retryAttempt))));

            policyRegistry.Add(
                circuitBreakerKey,
                HttpPolicyExtensions
                    .HandleTransientHttpError()
                    .CircuitBreakerAsync(
                        policyOptions.HttpCircuitBreaker.ExceptionsAllowedBeforeBreaking,
                        policyOptions.HttpCircuitBreaker.DurationOfBreak));

            return services;
        }

        public static IHttpClientBuilder AddNamedHttpClient<TClient, TImplementation, TClientOptions>(
            this IServiceCollection services,
            IConfiguration configuration,
            string configurationSectionName,
            string retryPolicyKey,
            string circuitBreakerPolicyKey,
            string httpClientName)
                where TClient : class
                where TImplementation : class, TClient
                where TClientOptions : HttpClientOptions, new() =>
            services
                .Configure<TClientOptions>(configuration?.GetSection(configurationSectionName))
                .AddHttpClient<TClient, TImplementation>(httpClientName)
                .AddClientBuilder<TClientOptions>(retryPolicyKey, circuitBreakerPolicyKey);

        public static IHttpClientBuilder AddUnnamedHttpClient<TClient, TImplementation, TClientOptions>(
            this IServiceCollection services,
            IConfiguration configuration,
            string configurationSectionName,
            string retryPolicyKey,
            string circuitBreakerPolicyKey)
                where TClient : class
                where TImplementation : class, TClient
                where TClientOptions : HttpClientOptions, new() =>
            services
                .Configure<TClientOptions>(configuration?.GetSection(configurationSectionName))
                .AddHttpClient<TClient, TImplementation>()
                .AddClientBuilder<TClientOptions>(retryPolicyKey, circuitBreakerPolicyKey);

        public static IHttpClientBuilder AddClientBuilder<TClientOptions>(
            this IHttpClientBuilder clientBuilder,
            string retryPolicyKey,
            string circuitBreakerPolicyKey)
                where TClientOptions : HttpClientOptions, new() =>
            clientBuilder
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
                .AddPolicyHandlerFromRegistry(retryPolicyKey)
                .AddPolicyHandlerFromRegistry(circuitBreakerPolicyKey)
                .AddHttpMessageHandler<UserAgentDelegatingHandler>()
                .AddHttpMessageHandler<OriginalHostDelegatingHandler>()
                .AddHttpMessageHandler<CompositeRequestDelegatingHandler>();

        public static void ConfigureHttpClients(this IServiceCollection services, IConfiguration configuration)
        {
            var policyRegistry = services.AddPolicyRegistry();
            var policyOptions = configuration?.GetSection(Constants.Policies)?.Get<PolicyOptions>();

            if (policyOptions == null)
            {
                throw new ArgumentNullException(nameof(configuration), "Policy options could not be found");
            }

            AddHttpClientWithPolicies<INeo4JService, Neo4JService, VisitClientOptions>(
                services,
                policyRegistry,
                $"{nameof(VisitClientOptions)}_{nameof(PolicyOptions.HttpRetry)}",
                $"{nameof(VisitClientOptions)}_{nameof(PolicyOptions.HttpCircuitBreaker)}",
                nameof(VisitClientOptions),
                policyOptions,
                configuration);

            if (!services.AppRegistryRequestRegistered())
            {
                AddHttpClientWithPolicies<IAppRegistryService, AppRegistryService, AppRegistryClientOptions>(
                    services,
                    policyRegistry,
                    $"{nameof(AppRegistryClientOptions)}_{nameof(PolicyOptions.HttpRetry)}",
                    $"{nameof(AppRegistryClientOptions)}_{nameof(PolicyOptions.HttpCircuitBreaker)}",
                    nameof(AppRegistryClientOptions),
                    policyOptions,
                    configuration);
            }

            services.AddApplicationClientHttp(configuration, policyOptions, policyRegistry);

            AddHttpClientWithPolicies<IAjaxRequestService, AjaxRequestService, AjaxRequestClientOptions>(
                services,
                policyRegistry,
                $"{nameof(AjaxRequestClientOptions)}_{nameof(PolicyOptions.HttpRetry)}",
                $"{nameof(AjaxRequestClientOptions)}_{nameof(PolicyOptions.HttpCircuitBreaker)}",
                nameof(AjaxRequestClientOptions),
                policyOptions,
                configuration);

            services
                .AddPolicies(
                    policyRegistry,
                    $"{nameof(AuthClientOptions)}_{nameof(PolicyOptions.HttpRetry)}",
                    $"{nameof(AuthClientOptions)}_{nameof(PolicyOptions.HttpCircuitBreaker)}",
                    policyOptions);

            AddHttpClientWithPolicies<IApplicationHealthService, ApplicationHealthService, HealthClientOptions>(
                services,
                policyRegistry,
                $"{nameof(HealthClientOptions)}_{nameof(PolicyOptions.HttpRetry)}",
                $"{nameof(HealthClientOptions)}_{nameof(PolicyOptions.HttpCircuitBreaker)}",
                nameof(HealthClientOptions),
                policyOptions,
                configuration);

            AddHttpClientWithPolicies<IApplicationSitemapService, ApplicationSitemapService, SitemapClientOptions>(
                services,
                policyRegistry,
                $"{nameof(SitemapClientOptions)}_{nameof(PolicyOptions.HttpRetry)}",
                $"{nameof(SitemapClientOptions)}_{nameof(PolicyOptions.HttpCircuitBreaker)}",
                nameof(SitemapClientOptions),
                policyOptions,
                configuration);

            AddHttpClientWithPolicies<IApplicationRobotService, ApplicationRobotService, RobotClientOptions>(
                services,
                policyRegistry,
                $"{nameof(RobotClientOptions)}_{nameof(PolicyOptions.HttpRetry)}",
                $"{nameof(RobotClientOptions)}_{nameof(PolicyOptions.HttpCircuitBreaker)}",
                nameof(RobotClientOptions),
                policyOptions,
                configuration);

            AddHttpClientWithPolicies<IBannerService, BannerService, BannerClientOptions>(
               services,
               policyRegistry,
               $"{nameof(BannerClientOptions)}_{nameof(PolicyOptions.HttpRetry)}",
               $"{nameof(BannerClientOptions)}_{nameof(PolicyOptions.HttpCircuitBreaker)}",
               nameof(BannerClientOptions),
               policyOptions,
               configuration);
        }

        private static void AddApplicationClientHttp(
            this IServiceCollection services,
            IConfiguration configuration,
            PolicyOptions policyOptions,
            IPolicyRegistry<string> policyRegistry)
        {
            var registeredUrls = GetRegisteredUrls(services);
            services.AddSingleton<IRegisteredUrls>(registeredUrls);

            foreach (var registeredUrl in registeredUrls.GetAll())
            {
                AddHttpClientWithPolicies<IUriSpecifcHttpClientFactory, UriSpecifcHttpClientFactory, ApplicationClientOptions>(
                    services,
                    policyRegistry,
                    $"{registeredUrl}_{nameof(ApplicationClientOptions)}_{nameof(PolicyOptions.HttpRetry)}",
                    $"{registeredUrl}_{nameof(ApplicationClientOptions)}_{nameof(PolicyOptions.HttpCircuitBreaker)}",
                    nameof(ApplicationClientOptions),
                    policyOptions,
                    configuration,
                    $"{registeredUrl}_{nameof(UriSpecifcHttpClientFactory)}")
                .AddHttpMessageHandler<CompositeSessionIdDelegatingHandler>()
                .AddHttpMessageHandler<CookieDelegatingHandler>();
            }
        }

        private static RegisteredUrls GetRegisteredUrls(IServiceCollection services)
        {
            var serviceProvider = services.BuildServiceProvider();
            var registeredUrlList = new List<string>();

            try
            {
                const int MaxRetries = 3;
                const int BaseRetrySeconds = 5;

                Policy
                    .Handle<Exception>()
                    .WaitAndRetry(MaxRetries, retryAttempt => TimeSpan.FromSeconds(retryAttempt * BaseRetrySeconds))
                    .Execute(() =>
                        registeredUrlList.AddRange(serviceProvider
                            .GetService<IAppRegistryDataService>()
                            .GetAppRegistrationModels()
                            .GetAwaiter()
                            .GetResult()
                            .Where(model => model.Regions != null)
                            .SelectMany(model => model.Regions)
                            .Select(region => region.RegionEndpoint)
                            .Distinct()
                            .ToList()));
            }
            #pragma warning disable CA1031
            catch (Exception exception)
            #pragma warning restore CA1031
            {
                AttemptToLog(exception, "Failure getting paths from app registry");
            }

            registeredUrlList.Add(RegisteredUrlConstants.DefaultKey);
            return new RegisteredUrls(registeredUrlList);
        }

        private static void AttemptToLog(Exception exception, string message)
        {
            try
            {
                using var loggerFactory = LoggerFactory.Create(builder =>
                {
                    builder.AddApplicationInsights();
                });

                var logger = loggerFactory.CreateLogger<Startup>();
                logger?.LogError(exception, message);
            }
            #pragma warning disable CA1031
            catch
            #pragma warning restore CA1031
            {
                // Swallow the exception - it will be lost unfortunately as we dont have a fallback logger
            }
        }

        private static IHttpClientBuilder AddHttpClientWithPolicies<TClient, TImplementation, TClientOptions>(
            IServiceCollection services,
            IPolicyRegistry<string> policyRegistry,
            string retryPolicyKey,
            string circuitBreakerKey,
            string configurationSectionName,
            PolicyOptions policyOptions,
            IConfiguration configuration,
            string httpClientName = null)
                where TClient : class
                where TImplementation : class, TClient
                where TClientOptions : HttpClientOptions, new()
        {
            var policies = services
                .AddPolicies(
                    policyRegistry,
                    retryPolicyKey,
                    circuitBreakerKey,
                    policyOptions);

            if (!string.IsNullOrEmpty(httpClientName))
            {
                return policies.AddNamedHttpClient<TClient, TImplementation, TClientOptions>(
                    configuration,
                    configurationSectionName,
                    retryPolicyKey,
                    circuitBreakerKey,
                    httpClientName);
            }

            return policies.AddUnnamedHttpClient<TClient, TImplementation, TClientOptions>(
                configuration,
                configurationSectionName,
                retryPolicyKey,
                circuitBreakerKey);
        }

        private static bool AppRegistryRequestRegistered(this IServiceCollection services)
        {
            return services.Any(service => service.ServiceType == typeof(IAppRegistryService));
        }
    }
}