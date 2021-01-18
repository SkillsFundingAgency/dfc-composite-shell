using DFC.Common.Standard.Logging;
using DFC.Composite.Shell.ClientHandlers;
using DFC.Composite.Shell.Controllers;
using DFC.Composite.Shell.Extensions;
using DFC.Composite.Shell.HttpResponseMessageHandlers;
using DFC.Composite.Shell.Models;
using DFC.Composite.Shell.Models.Common;
using DFC.Composite.Shell.Policies.Options;
using DFC.Composite.Shell.Services.AjaxRequest;
using DFC.Composite.Shell.Services.Application;
using DFC.Composite.Shell.Services.ApplicationHealth;
using DFC.Composite.Shell.Services.ApplicationRobot;
using DFC.Composite.Shell.Services.ApplicationSitemap;
using DFC.Composite.Shell.Services.AppRegistry;
using DFC.Composite.Shell.Services.Auth;
using DFC.Composite.Shell.Services.Auth.Models;
using DFC.Composite.Shell.Services.BaseUrl;
using DFC.Composite.Shell.Services.ContentProcessor;
using DFC.Composite.Shell.Services.ContentRetrieval;
using DFC.Composite.Shell.Services.CookieParsers;
using DFC.Composite.Shell.Services.DataProtectionProviders;
using DFC.Composite.Shell.Services.HeaderCount;
using DFC.Composite.Shell.Services.HeaderRenamer;
using DFC.Composite.Shell.Services.HttpClientService;
using DFC.Composite.Shell.Services.Mapping;
using DFC.Composite.Shell.Services.Neo4J;
using DFC.Composite.Shell.Services.PathLocator;
using DFC.Composite.Shell.Services.ShellRobotFile;
using DFC.Composite.Shell.Services.TokenRetriever;
using DFC.Composite.Shell.Services.UrlRewriter;
using DFC.Composite.Shell.Services.Utilities;
using DFC.Composite.Shell.Utilities;
using DFC.Compui.Telemetry.ApplicationBuilderExtensions;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Diagnostics.CodeAnalysis;
using System.IdentityModel.Tokens.Jwt;

namespace DFC.Composite.Shell
{
    [ExcludeFromCodeCoverage]
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        private IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                app.UseHsts(options => options.MaxAge(days: 365).IncludeSubdomains());
            }

            app.UseStatusCodePagesWithReExecute("/" + ApplicationController.AlertPathName + "/{0}");
            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseCookiePolicy();
            app.UseForwardedHeaders();
            app.AddOperationIdToRequests();

            var cdnLocation = Configuration.GetValue<string>(nameof(PageViewModel.BrandingAssetsCdn));
            var webchatOptionsScriptUrl = new Uri(Configuration.GetValue<string>("WebchatOptions:ScriptUrl") ?? "https://webchat.nationalcareersservice.org.uk:8080/js/chatRed.js", UriKind.Absolute);
            var webchatCspDomain = $"{webchatOptionsScriptUrl.Scheme}://{webchatOptionsScriptUrl.Host}:{webchatOptionsScriptUrl.Port}";

            // Configure security headers
            app.UseCsp(options => options
                .DefaultSources(s => s.Self())
                .ScriptSources(s => s
                    .Self()
                    .CustomSources(
                        "https://az416426.vo.msecnd.net/scripts/",
                        "www.google-analytics.com",
                        "sha256-OzxeCM8TJjksWkec74qsw2e3+vmC1ifof7TzRHngpoE=",
                        "www.googletagmanager.com",
                        $"{cdnLocation}/{Constants.NationalCareersToolkit}/js/",
                        webchatCspDomain + "/js/",
                        $"{ Configuration.GetValue<string>(Constants.ApplicationInsightsScriptResourceAddress)}"))
                .StyleSources(s => s
                    .Self()
                    .CustomSources(
                        $"{cdnLocation}/{Constants.NationalCareersToolkit}/css/",
                        webchatCspDomain + "/css/"))
                .FormActions(s => s
                    .Self())
                .FontSources(s => s
                    .Self()
                    .CustomSources($"{cdnLocation}/{Constants.NationalCareersToolkit}/fonts/"))
                .ImageSources(s => s
                    .Self()
                    .CustomSources(
                        $"{cdnLocation}/{Constants.NationalCareersToolkit}/images/",
                        webchatCspDomain + "/images/",
                        webchatCspDomain + "/var/",
                        "www.google-analytics.com",
                        "*.doubleclick.net"))
                .FrameAncestors(s => s.Self())
                .FrameSources(s => s
                    .Self()
                    .CustomSources(webchatCspDomain))
                .ConnectSources(s => s
                    .Self()
                    .CustomSources(
                        webchatCspDomain,
                        $"{Configuration.GetValue<string>(Constants.ApplicationInsightsConnectSources)}",
                        "https://dc.services.visualstudio.com/",
                        "https://www.google-analytics.com",
                        "https://www.googletagmanager.com")));

            app.UseXContentTypeOptions();
            app.UseReferrerPolicy(opts => opts.StrictOriginWhenCrossOrigin());
            app.UseXfo(options => options.SameOrigin());
            app.UseXXssProtection(options => options.EnabledWithBlockMode());

            app.Use(async (context, next) =>
            {
                context.Response.Headers["Feature-Policy"] = "sync-xhr 'self'";
                context.Response.Headers["Expect-CT"] = "max-age=86400, enforce";
                context.Response.Headers["X-Permitted-Cross-Domain-Policies"] = "none";
                await next().ConfigureAwait(false);
            });

            app.UseSession();
            app.UseCompositeSessionId();
            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();

            ConfigureRouting(app);
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.Configure<CookiePolicyOptions>(options =>
            {
                // This lambda determines whether user consent for non-essential cookies is needed for a given request.
                options.CheckConsentNeeded = context => false;
                options.MinimumSameSitePolicy = SameSiteMode.None;
            });

            services.AddDataProtection();
            services.AddHttpContextAccessor();
            services.AddApplicationInsightsTelemetry();

            services.AddTransient<IApplicationService, ApplicationService>();
            services.AddTransient<IAsyncHelper, AsyncHelper>();
            services.AddTransient<IContentProcessorService, ContentProcessorService>();
            services.AddTransient<IHttpResponseMessageHandler, CookieHttpResponseMessageHandler>();
            services.AddTransient<ILoggerHelper, LoggerHelper>();
            services.AddTransient<IMapper<ApplicationModel, PageViewModel>, ApplicationToPageModelMapper>();
            services.AddTransient<ISetCookieParser, SetCookieParser>();
            services.AddTransient<IUrlRewriterService, UrlRewriterService>();
            services.AddTransient<ICompositeDataProtectionDataProvider, CompositeDataProtectionDataProvider>();

            services.AddTransient<CompositeSessionIdDelegatingHandler>();
            services.AddTransient<CookieDelegatingHandler>();
            services.AddTransient<UserAgentDelegatingHandler>();
            services.AddTransient<OriginalHostDelegatingHandler>();
            services.AddTransient<CompositeRequestDelegatingHandler>();
            services.AddTransient<IFakeHttpRequestSender, FakeHttpRequestSender>();
            services.AddTransient<SecurityTokenHandler, JwtSecurityTokenHandler>();
            services.AddTransient<INeo4JService, Neo4JService>();
            services.AddTransient<SecurityTokenHandler, JwtSecurityTokenHandler>();

            services.AddScoped<IPathLocator, UrlPathLocator>();
            services.AddScoped<IAppRegistryDataService, AppRegistryDataService>();
            services.AddScoped<IHeaderRenamerService, HeaderRenamerService>();
            services.AddScoped<IHeaderCountService, HeaderCountService>();
            services.AddScoped<IOpenIdConnectClient, AzureB2CAuthClient>();
            services.AddScoped<IVersionedFiles, VersionedFiles>();

            services.AddSingleton<IBearerTokenRetriever, BearerTokenRetriever>();
            services.AddSingleton<IShellRobotFileService, ShellRobotFileService>();
            services.AddSingleton<IBaseUrlService, BaseUrlService>();
            services.AddSingleton<IFileInfoHelper, FileInfoHelper>();
            services.AddSingleton<ITaskHelper, TaskHelper>();
            services.AddSingleton(Configuration.GetSection(nameof(MarkupMessages)).Get<MarkupMessages>() ?? new MarkupMessages());
            services.AddSingleton(Configuration.GetSection(nameof(WebchatOptions)).Get<WebchatOptions>() ?? new WebchatOptions());

            var authSettings = new OpenIDConnectSettings();
            Configuration.GetSection("OIDCSettings").Bind(authSettings);

            services.Configure<Neo4JSettings>(Configuration.GetSection(nameof(Neo4JSettings)));

            services.AddSingleton<IConfigurationManager<OpenIdConnectConfiguration>>(provider => new ConfigurationManager<OpenIdConnectConfiguration>(authSettings.OIDCConfigMetaDataUrl, new OpenIdConnectConfigurationRetriever(), new HttpDocumentRetriever()));

            services.Configure<OpenIDConnectSettings>(Configuration.GetSection("OIDCSettings"));
            services.Configure<AuthSettings>(Configuration.GetSection(nameof(AuthSettings)));

            services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme).AddCookie(options =>
            {
                options.LoginPath = "/auth/signin";
            });

            var policyOptions = Configuration.GetSection(Constants.Policies).Get<PolicyOptions>();
            var policyRegistry = services.AddPolicyRegistry();

            services.AddPolicies(policyRegistry, nameof(VisitClientOptions), policyOptions)
                .AddHttpClient<INeo4JService, Neo4JService, VisitClientOptions>(Configuration, nameof(VisitClientOptions), nameof(PolicyOptions.HttpRetry), nameof(PolicyOptions.HttpCircuitBreaker));

            services
                .AddPolicies(policyRegistry, nameof(AppRegistryClientOptions), policyOptions)
                .AddHttpClient<IAppRegistryService, AppRegistryService, AppRegistryClientOptions>(Configuration, nameof(AppRegistryClientOptions), nameof(PolicyOptions.HttpRetry), nameof(PolicyOptions.HttpCircuitBreaker));

            services
                .AddPolicies(policyRegistry, nameof(ApplicationClientOptions), policyOptions)
                .AddHttpClient<IContentRetriever, ContentRetriever, ApplicationClientOptions>(Configuration, nameof(ApplicationClientOptions), nameof(PolicyOptions.HttpRetry), nameof(PolicyOptions.HttpCircuitBreaker))
                .AddHttpMessageHandler<CompositeSessionIdDelegatingHandler>()
                .AddHttpMessageHandler<CookieDelegatingHandler>();

            services
                .AddPolicies(policyRegistry, nameof(AjaxRequestClientOptions), policyOptions)
                .AddHttpClient<IAjaxRequestService, AjaxRequestService, AjaxRequestClientOptions>(Configuration, nameof(AjaxRequestClientOptions), nameof(PolicyOptions.HttpRetry), nameof(PolicyOptions.HttpCircuitBreaker));

            services
                .AddPolicies(policyRegistry, nameof(AuthClientOptions), policyOptions);

            services
                .AddPolicies(policyRegistry, nameof(HealthClientOptions), policyOptions)
                .AddHttpClient<IApplicationHealthService, ApplicationHealthService, HealthClientOptions>(Configuration, nameof(HealthClientOptions), nameof(PolicyOptions.HttpRetry), nameof(PolicyOptions.HttpCircuitBreaker));

            services
                .AddPolicies(policyRegistry, nameof(SitemapClientOptions), policyOptions)
                .AddHttpClient<IApplicationSitemapService, ApplicationSitemapService, SitemapClientOptions>(Configuration, nameof(SitemapClientOptions), nameof(PolicyOptions.HttpRetry), nameof(PolicyOptions.HttpCircuitBreaker));

            services
                .AddPolicies(policyRegistry, nameof(RobotClientOptions), policyOptions)
                .AddHttpClient<IApplicationRobotService, ApplicationRobotService, RobotClientOptions>(Configuration, nameof(RobotClientOptions), nameof(PolicyOptions.HttpRetry), nameof(PolicyOptions.HttpCircuitBreaker));

            services.AddSession();

            services.Configure<ForwardedHeadersOptions>(options =>
            {
                options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
            });

            services.AddRazorPages();
            services.AddControllersWithViews();
        }

        private static void ConfigureRouting(IApplicationBuilder app)
        {
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapRazorPages();

                // add the default route
                endpoints.MapControllerRoute("default", "{controller=Application}/{action=Action}/");

                // add the site map route
                endpoints.MapControllerRoute("Sitemap", "Sitemap.xml", new { controller = "Sitemap", action = "Sitemap" });

                // add the robots.txt route
                endpoints.MapControllerRoute("Robots", "Robots.txt", new { controller = "Robot", action = "Robot" });

                endpoints.MapControllerRoute("Application.GetOrPost", "{path}/{**data}", new { controller = "Application", action = "Action" });
            });
        }
    }
}