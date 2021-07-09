using DFC.Common.Standard.Logging;
using DFC.Composite.Shell.ClientHandlers;
using DFC.Composite.Shell.Controllers;
using DFC.Composite.Shell.Extensions;
using DFC.Composite.Shell.HttpResponseMessageHandlers;
using DFC.Composite.Shell.Models;
using DFC.Composite.Shell.Services.Application;
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
using DFC.Composite.Shell.Services.Mapping;
using DFC.Composite.Shell.Services.Neo4J;
using DFC.Composite.Shell.Services.PathLocator;
using DFC.Composite.Shell.Services.ShellRobotFile;
using DFC.Composite.Shell.Services.TokenRetriever;
using DFC.Composite.Shell.Services.UriSpecifcHttpClient;
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
using System.IdentityModel.Tokens.Jwt;

namespace DFC.Composite.Shell
{
    public class Startup
    {
        private readonly IConfiguration configuration;

        public Startup(IConfiguration configuration)
        {
            this.configuration = configuration;
        }

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

            app.UseStatusCodePagesWithReExecute($"/{ApplicationController.AlertPathName}/{{0}}");
            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseCookiePolicy();
            app.UseForwardedHeaders();
            app.AddOperationIdToRequests();

            app.ConfigureSecurityHeaders(configuration);

            app.UseXContentTypeOptions();
            app.UseReferrerPolicy(opts => opts.StrictOriginWhenCrossOrigin());
            app.UseXfo(options => options.SameOrigin());
            app.UseXXssProtection(options => options.EnabledWithBlockMode());

            app.Use(async (context, next) =>
            {
                context.Response.Headers["Feature-Policy"] = "sync-xhr 'self'";
                context.Response.Headers["Expect-CT"] = "max-age=86400, enforce";
                context.Response.Headers["X-Permitted-Cross-Domain-Policies"] = "none";
                await next();
            });

            app.UseSession();
            app.UseCompositeSessionId();
            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();
            app.ConfigureRouting();
        }

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
            services.AddTransient<SecurityTokenHandler, JwtSecurityTokenHandler>();
            services.AddTransient<INeo4JService, Neo4JService>();
            services.AddTransient<SecurityTokenHandler, JwtSecurityTokenHandler>();
            services.AddTransient<IContentRetriever, ContentRetriever>();

            services.AddScoped<IPathLocator, UrlPathLocator>();
            services.AddScoped<IAppRegistryService, AppRegistryService>();
            services.AddScoped<IHeaderRenamerService, HeaderRenamerService>();
            services.AddScoped<IHeaderCountService, HeaderCountService>();
            services.AddScoped<IOpenIdConnectClient, AzureB2CAuthClient>();
            services.AddScoped<IVersionedFiles, VersionedFiles>();

            services.AddSingleton<IBearerTokenRetriever, BearerTokenRetriever>();
            services.AddSingleton<IShellRobotFileService, ShellRobotFileService>();
            services.AddSingleton<IBaseUrlService, BaseUrlService>();
            services.AddSingleton<IFileInfoHelper, FileInfoHelper>();
            services.AddSingleton(configuration.GetSection(nameof(MarkupMessages)).Get<MarkupMessages>() ?? new MarkupMessages());
            services.AddSingleton(configuration.GetSection(nameof(WebchatOptions)).Get<WebchatOptions>() ?? new WebchatOptions());

            services.AddSingleton<IUriSpecifcHttpClientFactory, UriSpecifcHttpClientFactory>();
            services.Configure<GoogleScripts>(configuration.GetSection(nameof(GoogleScripts)));

            var authSettings = new OpenIDConnectSettings();
            configuration.GetSection("OIDCSettings").Bind(authSettings);

            services.Configure<Neo4JSettings>(configuration.GetSection(nameof(Neo4JSettings)));

            services.AddSingleton<IConfigurationManager<OpenIdConnectConfiguration>>(
                provider => new ConfigurationManager<OpenIdConnectConfiguration>(
                    authSettings.OIDCConfigMetaDataUrl,
                    new OpenIdConnectConfigurationRetriever(),
                    new HttpDocumentRetriever()));

            services.Configure<OpenIDConnectSettings>(configuration.GetSection("OIDCSettings"));
            services.Configure<AuthSettings>(configuration.GetSection(nameof(AuthSettings)));

            services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme).AddCookie(options =>
            {
                options.LoginPath = "/auth/signin";
            });

            services.ConfigureHttpClients(configuration);

            services.AddSession();
            services.Configure<ForwardedHeadersOptions>(options =>
            {
                options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
            });

            services.AddRazorPages();
            services.AddControllersWithViews();
        }
    }
}
