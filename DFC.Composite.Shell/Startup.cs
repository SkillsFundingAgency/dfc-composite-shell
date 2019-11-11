using CorrelationId;
using DFC.Common.Standard.Logging;
using DFC.Composite.Shell.ClientHandlers;
using DFC.Composite.Shell.Extensions;
using DFC.Composite.Shell.HttpResponseMessageHandlers;
using DFC.Composite.Shell.Models;
using DFC.Composite.Shell.Models.Common;
using DFC.Composite.Shell.Policies.Options;
using DFC.Composite.Shell.Services.Application;
using DFC.Composite.Shell.Services.ApplicationHealth;
using DFC.Composite.Shell.Services.ApplicationRobot;
using DFC.Composite.Shell.Services.ApplicationSitemap;
using DFC.Composite.Shell.Services.AssetLocationAndVersion;
using DFC.Composite.Shell.Services.BaseUrl;
using DFC.Composite.Shell.Services.ContentProcessor;
using DFC.Composite.Shell.Services.ContentRetrieval;
using DFC.Composite.Shell.Services.CookieParsers;
using DFC.Composite.Shell.Services.HttpClientService;
using DFC.Composite.Shell.Services.Mapping;
using DFC.Composite.Shell.Services.PathLocator;
using DFC.Composite.Shell.Services.Paths;
using DFC.Composite.Shell.Services.Regions;
using DFC.Composite.Shell.Services.ShellRobotFile;
using DFC.Composite.Shell.Services.TokenRetriever;
using DFC.Composite.Shell.Services.UrlRewriter;
using DFC.Composite.Shell.Services.Utilities;
using DFC.Composite.Shell.Utilities;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace DFC.Composite.Shell
{
    public class Startup
    {
        private readonly Guid correlationId;

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
            correlationId = Guid.NewGuid();
        }

        private IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddCorrelationId();

            services.Configure<CookiePolicyOptions>(options =>
            {
                // This lambda determines whether user consent for non-essential cookies is needed for a given request.
                options.CheckConsentNeeded = context => false;
                options.MinimumSameSitePolicy = SameSiteMode.None;
            });

            services.AddHttpContextAccessor();

            services.AddTransient<IApplicationService, ApplicationService>();
            services.AddTransient<IAsyncHelper, AsyncHelper>();
            services.AddTransient<IContentProcessorService, ContentProcessorService>();
            services.AddTransient<IHttpResponseMessageHandler, CookieHttpResponseMessageHandler>();
            services.AddTransient<ILoggerHelper, LoggerHelper>();
            services.AddTransient<IMapper<ApplicationModel, PageViewModel>, ApplicationToPageModelMapper>();
            services.AddTransient<ISetCookieParser, SetCookieParser>();
            services.AddTransient<IUrlRewriterService, UrlRewriterService>();

            services.AddTransient<CookieDelegatingHandler>();
            services.AddTransient<CorrelationIdDelegatingHandler>();
            services.AddTransient<UserAgentDelegatingHandler>();
            services.AddTransient<IFakeHttpRequestSender, FakeHttpRequestSender>();

            services.AddScoped<IPathLocator, UrlPathLocator>();
            services.AddScoped<IPathDataService, PathDataService>();

            services.AddSingleton<IVersionedFiles, VersionedFiles>();
            services.AddSingleton<IBearerTokenRetriever, BearerTokenRetriever>();
            services.AddSingleton<IShellRobotFileService, ShellRobotFileService>();
            services.AddSingleton<IBaseUrlService, BaseUrlService>();
            services.AddSingleton<IFileInfoHelper, FileInfoHelper>();
            services.AddSingleton<ITaskHelper, TaskHelper>();

            var policyOptions = Configuration.GetSection(Constants.Policies).Get<PolicyOptions>();
            var policyRegistry = services.AddPolicyRegistry();

            services
                .AddPolicies(policyRegistry, nameof(PathClientOptions), policyOptions)
                .AddHttpClient<IPathService, PathService, PathClientOptions>(Configuration, nameof(PathClientOptions), nameof(PolicyOptions.HttpRetry), nameof(PolicyOptions.HttpCircuitBreaker));

            services
                .AddPolicies(policyRegistry, nameof(RegionClientOptions), policyOptions)
                .AddHttpClient<IRegionService, RegionService, RegionClientOptions>(Configuration, nameof(RegionClientOptions), nameof(PolicyOptions.HttpRetry), nameof(PolicyOptions.HttpCircuitBreaker));

            services
                .AddPolicies(policyRegistry, nameof(ApplicationClientOptions), policyOptions)
                .AddHttpClient<IContentRetriever, ContentRetriever, ApplicationClientOptions>(Configuration, nameof(ApplicationClientOptions), nameof(PolicyOptions.HttpRetry), nameof(PolicyOptions.HttpCircuitBreaker))
                .AddHttpMessageHandler<CookieDelegatingHandler>()
                .Services
                .AddHttpClient<IAssetLocationAndVersionService, AssetLocationAndVersionService, ApplicationClientOptions>(Configuration, nameof(ApplicationClientOptions), nameof(PolicyOptions.HttpRetry), nameof(PolicyOptions.HttpCircuitBreaker));

            services
                .AddPolicies(policyRegistry, nameof(HealthClientOptions), policyOptions)
                .AddHttpClient<IApplicationHealthService, ApplicationHealthService, HealthClientOptions>(Configuration, nameof(HealthClientOptions), nameof(PolicyOptions.HttpRetry), nameof(PolicyOptions.HttpCircuitBreaker));

            services
                .AddPolicies(policyRegistry, nameof(SitemapClientOptions), policyOptions)
                .AddHttpClient<IApplicationSitemapService, ApplicationSitemapService, SitemapClientOptions>(Configuration, nameof(SitemapClientOptions), nameof(PolicyOptions.HttpRetry), nameof(PolicyOptions.HttpCircuitBreaker));

            services
                .AddPolicies(policyRegistry, nameof(RobotClientOptions), policyOptions)
                .AddHttpClient<IApplicationRobotService, ApplicationRobotService, RobotClientOptions>(Configuration, nameof(RobotClientOptions), nameof(PolicyOptions.HttpRetry), nameof(PolicyOptions.HttpCircuitBreaker));

            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            app.UseCorrelationId(new CorrelationIdOptions
            {
                Header = Constants.CorrelationIdHeaderName,
                UseGuidForCorrelationId = true,
                UpdateTraceIdentifier = false,
            });

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");

                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseCookiePolicy();

            ConfigureRouting(app);
        }

        private static void ConfigureRouting(IApplicationBuilder app)
        {
            app.UseMvc(routes =>
            {
                // add the site map route
                routes.MapRoute(
                    name: "Sitemap",
                    template: "Sitemap.xml",
                    defaults: new { controller = "Sitemap", action = "Sitemap" });

                // add the robots.txt route
                routes.MapRoute(
                    name: "Robots",
                    template: "Robots.txt",
                    defaults: new { controller = "Robot", action = "Robot" });

                // add the default route
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");

                routes.MapRoute("Application.Get", "{path}/{**data}", new { controller = "Application", action = "Action" });
            });
        }
    }
}