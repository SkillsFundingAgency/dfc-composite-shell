using DFC.Composite.Shell.Common;
using DFC.Composite.Shell.Models;
using DFC.Composite.Shell.Services.ContentProcessor;
using DFC.Composite.Shell.Services.ContentRetrieve;
using DFC.Composite.Shell.Services.Paths;
using DFC.Composite.Shell.Services.Regions;
using Microsoft.AspNetCore.Html;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace DFC.Composite.Shell.Services.Application
{
    public class ApplicationService : IApplicationService
    {
        private readonly IPathDataService pathDataService;
        private readonly IRegionService regionService;
        private readonly IContentRetriever contentRetriever;
        private readonly IContentProcessorService contentProcessorService;

        public ApplicationService(
            IPathDataService pathDataService,
            IRegionService regionService,
            IContentRetriever contentRetriever,
            IContentProcessorService contentProcessorService)
        {
            this.pathDataService = pathDataService;
            this.regionService = regionService;
            this.contentRetriever = contentRetriever;
            this.contentProcessorService = contentProcessorService;
        }

        public string RequestBaseUrl { get; set; }

        public async Task GetMarkupAsync(ApplicationModel application, string article, PageViewModel pageModel)
        {
            if (application == null)
            {
                throw new ArgumentNullException(nameof(application));
            }

            if (pageModel == null)
            {
                throw new ArgumentNullException(nameof(pageModel));
            }

            if (application.Path.IsOnline)
            {
                //Get the markup at the head url first. This will create the session if it doesnt already exist
                var applicationHeadRegionOutput = await GetApplicationMarkUpAsync(application.Regions.First(x => x.PageRegion == PageRegion.Head), article).ConfigureAwait(false);
                pageModel.PageRegionContentModels.First(x => x.PageRegionType == PageRegion.Head).Content = new HtmlString(applicationHeadRegionOutput);

                //Get the markup at this url
                var applicationBodyRegionTask = GetApplicationMarkUpAsync(application, article);

                //Load related regions
                var otherRegionsTask = LoadRelatedRegions(application, pageModel, article);

                //Wait until everything is done
                await Task.WhenAll(applicationBodyRegionTask, otherRegionsTask).ConfigureAwait(false);

                //Ensure that the application body markup is attached to the model
                PopulatePageRegionContent(application, pageModel, PageRegion.Body, applicationBodyRegionTask);
            }
            else
            {
                var pageRegionContentModel = pageModel.PageRegionContentModels.First(x => x.PageRegionType == PageRegion.Body);

                if (pageRegionContentModel != null)
                {
                    pageRegionContentModel.Content = new HtmlString(application.Path.OfflineHtml);
                }
            }
        }

        public async Task PostMarkupAsync(ApplicationModel application, string path, string article, IEnumerable<KeyValuePair<string, string>> formParameters, PageViewModel pageModel)
        {
            if (application != null && application.Path.IsOnline)
            {
                //Get the markup at the post back url
                var applicationBodyRegionTask = GetPostMarkUpAsync(application, article, formParameters);

                //Load related regions
                var otherRegionsTask = LoadRelatedRegions(application, pageModel, string.Empty);

                //Wait until everything is done
                await Task.WhenAll(applicationBodyRegionTask, otherRegionsTask).ConfigureAwait(false);

                //Ensure that the application body markup is attached to the model
                PopulatePageRegionContent(application, pageModel, PageRegion.Body, applicationBodyRegionTask);
            }
            else
            {
                var pageRegionContentModel = pageModel?.PageRegionContentModels.First(x => x.PageRegionType == PageRegion.Body);

                if (pageRegionContentModel != null)
                {
                    pageRegionContentModel.Content = new HtmlString(application?.Path.OfflineHtml);
                }
            }
        }

        public async Task<ApplicationModel> GetApplicationAsync(string path)
        {
            var applicationModel = new ApplicationModel();

            var pathModel = await pathDataService.GetPath(path).ConfigureAwait(false);

            if (pathModel == null)
            {
                return applicationModel;
            }

            applicationModel.Path = pathModel;
            applicationModel.Regions = await regionService.GetRegions(pathModel.Path).ConfigureAwait(false);

            var bodyRegion = applicationModel.Regions?.FirstOrDefault(x => x.PageRegion == PageRegion.Body);

            if (bodyRegion != null && !string.IsNullOrWhiteSpace(bodyRegion.RegionEndpoint))
            {
                var uri = new Uri(bodyRegion.RegionEndpoint);
                var url = $"{uri.Scheme}://{uri.Authority}";

                applicationModel.RootUrl = url;
            }

            return applicationModel;
        }

        private static string FormatArticleUrl(string regionEndpoint, string article)
        {
            const string Placeholder = "{0}";
            const string SlashedPlaceholder = "/" + Placeholder;

            var urlFormatString = regionEndpoint;

            if (!urlFormatString.Contains(Placeholder, StringComparison.OrdinalIgnoreCase))
            {
                urlFormatString += SlashedPlaceholder;
            }

            return !string.IsNullOrWhiteSpace(article)
                ? string.Format(CultureInfo.InvariantCulture, urlFormatString, article)
                : urlFormatString.Replace(SlashedPlaceholder, string.Empty, StringComparison.OrdinalIgnoreCase);
        }

        private Task<string> GetApplicationMarkUpAsync(ApplicationModel application, string article)
        {
            //Get the body region
            var bodyRegion = application.Regions.FirstOrDefault(x => x.PageRegion == PageRegion.Body);

            if (bodyRegion == null || string.IsNullOrWhiteSpace(bodyRegion.RegionEndpoint))
            {
                return null;
            }

            var url = FormatArticleUrl(bodyRegion.RegionEndpoint, article);

            var result = contentRetriever.GetContent(url, bodyRegion, false, RequestBaseUrl);

            return result;
        }

        private Task<string> GetApplicationMarkUpAsync(RegionModel regionModel, string article)
        {
            var url = FormatArticleUrl(regionModel.RegionEndpoint, article);

            var result = this.contentRetriever.GetContent(url, regionModel, false, RequestBaseUrl);

            return result;
        }

        private Task<string> GetPostMarkUpAsync(ApplicationModel application, string article, IEnumerable<KeyValuePair<string, string>> formParameters)
        {
            //Get the body region
            var bodyRegion = application.Regions.FirstOrDefault(x => x.PageRegion == PageRegion.Body);

            if (bodyRegion == null || string.IsNullOrWhiteSpace(bodyRegion.RegionEndpoint))
            {
                return null;
            }

            var uri = new Uri(bodyRegion.RegionEndpoint);
            var url = $"{uri.Scheme}://{uri.Authority}/{article}";

            var result = contentRetriever.PostContent(url, bodyRegion, formParameters, RequestBaseUrl);

            return result;
        }

        private async Task LoadRelatedRegions(ApplicationModel application, PageViewModel pageModel, string article)
        {
            var tasks = new List<Task<string>>();

            var breadcrumbRegionTask = GetMarkupAsync(tasks, PageRegion.Breadcrumb, application.Regions, article);
            var bodyTopRegionTask = GetMarkupAsync(tasks, PageRegion.BodyTop, application.Regions, article);
            var sidebarLeftRegionTask = GetMarkupAsync(tasks, PageRegion.SidebarLeft, application.Regions, article);
            var sidebarRightRegionTask = GetMarkupAsync(tasks, PageRegion.SidebarRight, application.Regions, article);
            var bodyFooterRegionTask = GetMarkupAsync(tasks, PageRegion.BodyFooter, application.Regions, article);

            await Task.WhenAll(tasks).ConfigureAwait(false);

            PopulatePageRegionContent(application, pageModel, PageRegion.Breadcrumb, breadcrumbRegionTask);
            PopulatePageRegionContent(application, pageModel, PageRegion.BodyTop, bodyTopRegionTask);
            PopulatePageRegionContent(application, pageModel, PageRegion.SidebarLeft, sidebarLeftRegionTask);
            PopulatePageRegionContent(application, pageModel, PageRegion.SidebarRight, sidebarRightRegionTask);
            PopulatePageRegionContent(application, pageModel, PageRegion.BodyFooter, bodyFooterRegionTask);
        }

        private Task<string> GetMarkupAsync(List<Task<string>> tasks, PageRegion regionType, IEnumerable<RegionModel> regions, string article)
        {
            var pageRegionModel = regions.FirstOrDefault(x => x.PageRegion == regionType);

            if (pageRegionModel == null || string.IsNullOrWhiteSpace(pageRegionModel.RegionEndpoint))
            {
                return null;
            }

            var url = FormatArticleUrl(pageRegionModel.RegionEndpoint, article);

            var task = contentRetriever.GetContent(url, pageRegionModel, true, RequestBaseUrl);

            tasks.Add(task);

            return task;
        }

        private void PopulatePageRegionContent(ApplicationModel application, PageViewModel pageModel, PageRegion regionType, Task<string> task)
        {
            if (task != null)
            {
                var pageRegionContentModel = pageModel?.PageRegionContentModels.First(x => x.PageRegionType == regionType);
                var content = string.Empty;

                if (task.IsCompletedSuccessfully)
                {
                    content = task.Result;
                    content = contentProcessorService.Process(content, RequestBaseUrl, application.RootUrl);
                }
                else
                {
                    var pageRegionModel = application.Regions.FirstOrDefault(x => x.PageRegion == regionType);

                    if (pageRegionModel != null)
                    {
                        content = pageRegionModel.OfflineHTML;
                    }
                }

                pageRegionContentModel.Content = new HtmlString(content);
            }
        }
    }
}