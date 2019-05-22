using DFC.Composite.Shell.Models;
using DFC.Composite.Shell.Services.Application;
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
using Polly;
using Polly.Extensions.Http;
using System;

namespace DFC.Composite.Shell
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

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
            services.AddTransient<IContentRetriever, RealContentRetriever>();
            services.AddTransient<IMapper<ApplicationModel, PageViewModel>, ApplicationToPageModelMapper>();
            services.AddTransient<IPathService, LocalPathService>();
            services.AddTransient<IPathLocator, UrlPathLocator>();
            services.AddTransient<IRegionService, LocalRegionService>();
            services.AddTransient<IUrlRewriter, UrlRewriter>();

            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);

            ConfigureCircuitBreaker(services);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, IPathService pathService)
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

            ConfigureRouting(app, pathService);
        }

        private void ConfigureCircuitBreaker(IServiceCollection services)
        {
            var policyName = "CircuitBreaker";
            var policyRegistry = services.AddPolicyRegistry();
            policyRegistry.Add(
                policyName,
                HttpPolicyExtensions.HandleTransientHttpError().CircuitBreakerAsync(2, TimeSpan.FromSeconds(30))
            );

            services.AddHttpClient<IContentRetriever, RealContentRetriever>()
                .AddPolicyHandlerFromRegistry(policyName);
        }

        private void ConfigureRouting(IApplicationBuilder app, IPathService pathService)
        {
            var paths = pathService.GetPaths().Result;

            app.UseMvc(routes =>
            {
                // map any incoming routes for each path
                foreach (var path in paths)
                {
                    routes.MapRoute(
                        name: $"path-{path.Path}-Action",
                        template: path.Path + "/{**data}",
                        defaults: new { controller = "Application", action = "Action", Path = path.Path }
                    );
                }

                // add the default route
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
