using System;
using System.Collections.Generic;
using System.Linq;
using DFC.Common.Standard.Logging;
using DFC.Composite.Shell.Extensions;
using DFC.Composite.Shell.Models;
using DFC.Composite.Shell.Policies.Options;
using DFC.Composite.Shell.Services.Application;
using DFC.Composite.Shell.Services.ApplicationSitemap;
using DFC.Composite.Shell.Services.ContentProcessor;
using DFC.Composite.Shell.Services.ContentRetrieve;
using DFC.Composite.Shell.Services.Mapping;
using DFC.Composite.Shell.Services.PathLocator;
using DFC.Composite.Shell.Services.Paths;
using DFC.Composite.Shell.Services.Regions;
using DFC.Composite.Shell.Services.UrlRewriter;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace DFC.Composite.Shell
{
    public class Startup
    {
        private IConfiguration Configuration { get; }
        private Guid _correlationId;
        
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
            _correlationId = Guid.NewGuid();
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.Configure<CookiePolicyOptions>(options =>
            {
                // This lambda determines whether user consent for non-essential cookies is needed for a given request.
                options.CheckConsentNeeded = context => true;
                options.MinimumSameSitePolicy = SameSiteMode.None;
            });

            services.AddHttpContextAccessor();

            services.AddTransient<IApplicationService, ApplicationService>();
            services.AddTransient<IContentProcessor, ContentProcessor>();
            services.AddTransient<IMapper<ApplicationModel, PageViewModel>, ApplicationToPageModelMapper>();
            services.AddScoped<IPathLocator, UrlPathLocator>();
            services.AddTransient<IUrlRewriter, UrlRewriter>();
            services.AddTransient<ILoggerHelper, LoggerHelper>();

            services.Configure<PolicyOptions>(Configuration);

            var policyRegistry = services.AddPolicyRegistry();

            services
                .AddPolicies(policyRegistry, Configuration, nameof(PathClientOptions))
                .AddHttpClient<IPathService, PathService, PathClientOptions>(Configuration, nameof(PathClientOptions), nameof(PolicyOptions.HttpRetry), nameof(PolicyOptions.HttpCircuitBreaker));

            services
                .AddPolicies(policyRegistry, Configuration, nameof(RegionClientOptions))
                .AddHttpClient<IRegionService, RegionService, RegionClientOptions>(Configuration, nameof(RegionClientOptions), nameof(PolicyOptions.HttpRetry), nameof(PolicyOptions.HttpCircuitBreaker));

            services
                .AddPolicies(policyRegistry, Configuration, nameof(ApplicationClientOptions))
                .AddHttpClient<IContentRetriever, ContentRetriever, ApplicationClientOptions>(Configuration, nameof(ApplicationClientOptions), nameof(PolicyOptions.HttpRetry), nameof(PolicyOptions.HttpCircuitBreaker));

            services
                .AddPolicies(policyRegistry, Configuration, nameof(SitemapClientOptions))
                .AddHttpClient<IApplicationSitemapService, ApplicationSitemapService, SitemapClientOptions>(Configuration, nameof(SitemapClientOptions), nameof(PolicyOptions.HttpRetry), nameof(PolicyOptions.HttpCircuitBreaker));

            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, IPathService pathService, ILogger<Startup> logger, ILoggerHelper loggerHelper)
        {
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

            ConfigureRouting(app, pathService, logger, loggerHelper);
        }

        private void ConfigureRouting(IApplicationBuilder app, IPathService pathService, ILogger<Startup> logger, ILoggerHelper loggerHelper)
        {
            IEnumerable<PathModel> paths = new List<PathModel>();

            try
            {
                paths = pathService.GetPaths().Result;
                Log(logger, loggerHelper, $"Registering routes for the following paths: {string.Join(",", paths.Select(x => x.Path))}");
            }
            catch (Exception ex)
            {
                loggerHelper.LogException(logger, _correlationId, ex);
            }

            app.UseMvc(routes =>
            {
                if (paths != null)
                {
                    // map any incoming routes for each path
                    foreach (var path in paths)
                    {
                        routes.MapRoute(
                            name: $"path-{path.Path}-Action",
                            template: path.Path + "/{**data}",
                            defaults: new { controller = "Application", action = "Action", path.Path }
                        );
                    }
                }

                // add the site map route
                routes.MapRoute(
                    name: "Sitemap",
                    template: "Sitemap.xml",
                    defaults: new { controller = "Sitemap", action = "Sitemap" }
                );

                // add the robots.txt route
                routes.MapRoute(
                    name: "Robots",
                    template: "Robots.txt",
                    defaults: new { controller = "Robot", action = "Robot" }
                );

                // add the default route
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });
        }

        private void Log(ILogger<Startup> logger, ILoggerHelper loggerHelper, string message)
        {
            loggerHelper.LogInformationMessage(logger, _correlationId, message);
        }
    }
}
