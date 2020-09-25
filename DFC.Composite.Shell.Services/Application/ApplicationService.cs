using DFC.Composite.Shell.Models;
using DFC.Composite.Shell.Models.AppRegistrationModels;
using DFC.Composite.Shell.Services.AppRegistry;
using DFC.Composite.Shell.Services.ContentProcessor;
using DFC.Composite.Shell.Services.ContentRetrieval;
using DFC.Composite.Shell.Services.Utilities;
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
        private readonly IAppRegistryDataService appRegistryDataService;
        private readonly IContentRetriever contentRetriever;
        private readonly IContentProcessorService contentProcessorService;
        private readonly ITaskHelper taskHelper;
        private readonly MarkupMessages markupMessages;

        public ApplicationService(
            IAppRegistryDataService appRegistryDataService,
            IContentRetriever contentRetriever,
            IContentProcessorService contentProcessorService,
            ITaskHelper taskHelper,
            MarkupMessages markupMessages)
        {
            this.appRegistryDataService = appRegistryDataService;
            this.contentRetriever = contentRetriever;
            this.contentProcessorService = contentProcessorService;
            this.taskHelper = taskHelper;
            this.markupMessages = markupMessages;
        }

        public string RequestBaseUrl { get; set; }

        public async Task GetMarkupAsync(ApplicationModel application, string article, PageViewModel pageModel, string queryString)
        {
            if (application == null)
            {
                throw new ArgumentNullException(nameof(application));
            }

            if (pageModel == null)
            {
                throw new ArgumentNullException(nameof(pageModel));
            }

            if (application.AppRegistrationModel.IsOnline)
            {
                //Load related regions
                var otherRegionsTask = LoadRelatedRegions(application, pageModel, article, queryString);

                //Wait until everything is done
                await Task.WhenAll(otherRegionsTask).ConfigureAwait(false);

                //Get the markup at this url
                var applicationBodyRegionTask = GetApplicationBodyRegionMarkUpAsync(application, article, queryString);

                await Task.WhenAll(applicationBodyRegionTask).ConfigureAwait(false);

                //Ensure that the application body markup is attached to the model
                PopulatePageRegionContent(application, pageModel, PageRegion.Body, applicationBodyRegionTask);
            }
            else
            {
                var pageRegionContentModel = pageModel.PageRegionContentModels.First(x => x.PageRegionType == PageRegion.Body);

                if (pageRegionContentModel != null)
                {
                    if (!string.IsNullOrWhiteSpace(application.AppRegistrationModel.OfflineHtml))
                    {
                        pageRegionContentModel.Content = new HtmlString(application.AppRegistrationModel.OfflineHtml);
                    }
                    else
                    {
                        pageRegionContentModel.Content = new HtmlString(markupMessages.AppOfflineHtml);
                    }
                }
            }
        }

        public async Task PostMarkupAsync(ApplicationModel application, string path, string article, IEnumerable<KeyValuePair<string, string>> formParameters, PageViewModel pageModel)
        {
            if (application != null && application.AppRegistrationModel.IsOnline)
            {
                //Get the markup at the post back url
                var applicationBodyRegionTask = GetPostMarkUpAsync(application, article, formParameters);

                //Load related regions
                var otherRegionsTask = LoadRelatedRegions(application, pageModel, article, string.Empty);

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
                    if (!string.IsNullOrWhiteSpace(application?.AppRegistrationModel.OfflineHtml))
                    {
                        pageRegionContentModel.Content = new HtmlString(application.AppRegistrationModel.OfflineHtml);
                    }
                    else
                    {
                        pageRegionContentModel.Content = new HtmlString(markupMessages.AppOfflineHtml);
                    }
                }
            }
        }

        public async Task<ApplicationModel> GetApplicationAsync(string path, string data)
        {
            var applicationModel = await DetermineArticleLocation(path, data).ConfigureAwait(false);

            if (applicationModel.AppRegistrationModel == null)
            {
                return applicationModel;
            }

            var bodyRegion = applicationModel.AppRegistrationModel.Regions?.FirstOrDefault(x => x.PageRegion == PageRegion.Body);

            if (bodyRegion != null && !string.IsNullOrWhiteSpace(bodyRegion.RegionEndpoint))
            {
                var uri = new Uri(bodyRegion.RegionEndpoint);
                var url = $"{uri.Scheme}://{uri.Authority}";

                applicationModel.RootUrl = url;
            }

            return applicationModel;
        }

        private async Task<ApplicationModel> DetermineArticleLocation(string path, string data)
        {
            const string appRegistryPathNameForPagesApp = "pages";
            var pageLocation = string.Join("/", new[] { path, data });
            var pageLocations = pageLocation.Split("/", StringSplitOptions.RemoveEmptyEntries);
            var article = string.Join("/", pageLocations);
            var applicationModel = new ApplicationModel();
            var pagesAppRegistrationModel = await appRegistryDataService.GetAppRegistrationModel(appRegistryPathNameForPagesApp).ConfigureAwait(false);

            if (pagesAppRegistrationModel?.PageLocations != null && pagesAppRegistrationModel.IsOnline && pagesAppRegistrationModel.PageLocations.Values.SelectMany(s => s.Locations).Contains("/" + article))
            {
                applicationModel.AppRegistrationModel = pagesAppRegistrationModel;
                applicationModel.Article = article;
            }

            if (applicationModel.AppRegistrationModel == null)
            {
                applicationModel.AppRegistrationModel = await appRegistryDataService.GetAppRegistrationModel(path).ConfigureAwait(false);
                applicationModel.Article = data;
            }

            return applicationModel;
        }

        private static string FormatArticleUrl(string regionEndpoint, string article, string queryString)
        {
            const string ArticlePlaceholder = "{0}";
            const string QueryStringPlaceholder = "{1}";
            const string SlashedPlaceholder = "/" + ArticlePlaceholder;

            var urlFormatString = regionEndpoint;

            if (!urlFormatString.Contains(ArticlePlaceholder, StringComparison.OrdinalIgnoreCase))
            {
                urlFormatString += SlashedPlaceholder;
            }

            if (!string.IsNullOrWhiteSpace(queryString) && queryString.TrimStart().StartsWith('?'))
            {
                urlFormatString += QueryStringPlaceholder;
            }

            return !string.IsNullOrWhiteSpace(article)
                ? string.Format(CultureInfo.InvariantCulture, urlFormatString, article, queryString)
                : urlFormatString
                    .Replace(SlashedPlaceholder, string.Empty, StringComparison.OrdinalIgnoreCase)
                    .Replace(QueryStringPlaceholder, string.Empty, StringComparison.OrdinalIgnoreCase);
        }

        private Task<string> GetApplicationBodyRegionMarkUpAsync(ApplicationModel application, string article, string queryString)
        {
            //Get the body region
            var bodyRegion = application.AppRegistrationModel.Regions.FirstOrDefault(x => x.PageRegion == PageRegion.Body);

            if (bodyRegion == null || string.IsNullOrWhiteSpace(bodyRegion.RegionEndpoint))
            {
                return Task.FromResult(string.Empty);
            }

            var url = FormatArticleUrl(bodyRegion.RegionEndpoint, article, queryString);

            return contentRetriever.GetContent(url, application.AppRegistrationModel.Path, bodyRegion, false, RequestBaseUrl);
        }

        private async Task<string> GetApplicationHeadRegionMarkUpAsync(ApplicationModel application, RegionModel regionModel, string article, string queryString)
        {
            var url = FormatArticleUrl(regionModel.RegionEndpoint, article, queryString);

            var result = await contentRetriever.GetContent(url, application.AppRegistrationModel.Path, regionModel, false, RequestBaseUrl).ConfigureAwait(false);

            return contentProcessorService.Process(result, RequestBaseUrl, application.RootUrl);
        }

        private Task<string> GetPostMarkUpAsync(ApplicationModel application, string article, IEnumerable<KeyValuePair<string, string>> formParameters)
        {
            //Get the body region
            var bodyRegion = application.AppRegistrationModel.Regions.FirstOrDefault(x => x.PageRegion == PageRegion.Body);

            if (bodyRegion == null || string.IsNullOrWhiteSpace(bodyRegion.RegionEndpoint))
            {
                return Task.FromResult(string.Empty);
            }

            var url = FormatArticleUrl(bodyRegion.RegionEndpoint, article, string.Empty);

            return contentRetriever.PostContent(url, application.AppRegistrationModel.Path, bodyRegion, formParameters, RequestBaseUrl);
        }

        private async Task LoadRelatedRegions(ApplicationModel application, PageViewModel pageModel, string article, string queryString)
        {
            //Get the markup at the head url first. This will create the session if it doesn't already exist
            var applicationHeadRegionOutput = await GetApplicationHeadRegionMarkUpAsync(application, application.AppRegistrationModel.Regions.First(x => x.PageRegion == PageRegion.Head), article, queryString).ConfigureAwait(false);
            pageModel.PageRegionContentModels.First(x => x.PageRegionType == PageRegion.Head).Content = new HtmlString(applicationHeadRegionOutput);

            var tasks = new List<Task<string>>();

            var heroBannerRegionTask = GetMarkup(tasks, application.AppRegistrationModel.Path, PageRegion.HeroBanner, application.AppRegistrationModel.Regions, article, queryString);
            var breadcrumbRegionTask = GetMarkup(tasks, application.AppRegistrationModel.Path, PageRegion.Breadcrumb, application.AppRegistrationModel.Regions, article, queryString);
            var bodyTopRegionTask = GetMarkup(tasks, application.AppRegistrationModel.Path, PageRegion.BodyTop, application.AppRegistrationModel.Regions, article, queryString);
            var sidebarLeftRegionTask = GetMarkup(tasks, application.AppRegistrationModel.Path, PageRegion.SidebarLeft, application.AppRegistrationModel.Regions, article, queryString);
            var sidebarRightRegionTask = GetMarkup(tasks, application.AppRegistrationModel.Path, PageRegion.SidebarRight, application.AppRegistrationModel.Regions, article, queryString);
            var bodyFooterRegionTask = GetMarkup(tasks, application.AppRegistrationModel.Path, PageRegion.BodyFooter, application.AppRegistrationModel.Regions, article, queryString);

            await Task.WhenAll(tasks).ConfigureAwait(false);

            PopulatePageRegionContent(application, pageModel, PageRegion.HeroBanner, heroBannerRegionTask);
            PopulatePageRegionContent(application, pageModel, PageRegion.Breadcrumb, breadcrumbRegionTask);
            PopulatePageRegionContent(application, pageModel, PageRegion.BodyTop, bodyTopRegionTask);
            PopulatePageRegionContent(application, pageModel, PageRegion.SidebarLeft, sidebarLeftRegionTask);
            PopulatePageRegionContent(application, pageModel, PageRegion.SidebarRight, sidebarRightRegionTask);
            PopulatePageRegionContent(application, pageModel, PageRegion.BodyFooter, bodyFooterRegionTask);
        }

        private Task<string> GetMarkup(List<Task<string>> tasks, string path, PageRegion regionType, IEnumerable<RegionModel> regions, string article, string queryString)
        {
            var pageRegionModel = regions.FirstOrDefault(x => x.PageRegion == regionType);

            if (pageRegionModel == null || string.IsNullOrWhiteSpace(pageRegionModel.RegionEndpoint))
            {
                return Task.FromResult<string>(null);
            }

            if (!pageRegionModel.IsHealthy && pageRegionModel.PageRegion != PageRegion.Head)
            {
                if (!string.IsNullOrWhiteSpace(pageRegionModel.OfflineHtml))
                {
                    return Task.FromResult(pageRegionModel.OfflineHtml);
                }
                else
                {
                    return Task.FromResult(markupMessages.RegionOfflineHtml);
                }
            }

            var url = FormatArticleUrl(pageRegionModel.RegionEndpoint, article, queryString);

            var task = contentRetriever.GetContent(url, path, pageRegionModel, true, RequestBaseUrl);

            tasks.Add(task);

            return task;
        }

        private void PopulatePageRegionContent(ApplicationModel application, PageViewModel pageModel, PageRegion regionType, Task<string> task)
        {
            if (task == null)
            {
                return;
            }

            string outputHtmlMarkup = string.Empty;

            if (taskHelper.TaskCompletedSuccessfully(task))
            {
                var taskResult = task.Result;
                var result = contentProcessorService.Process(taskResult, RequestBaseUrl, application.RootUrl);
                outputHtmlMarkup = result;
            }
            else
            {
                var pageRegionModel = application.AppRegistrationModel.Regions.FirstOrDefault(x => x.PageRegion == regionType);
                if (pageRegionModel != null && pageRegionModel.PageRegion != PageRegion.Head)
                {
                    if (!string.IsNullOrWhiteSpace(pageRegionModel.OfflineHtml))
                    {
                        outputHtmlMarkup = pageRegionModel.OfflineHtml;
                    }
                    else
                    {
                        outputHtmlMarkup = markupMessages.RegionOfflineHtml;
                    }
                }
            }

            var pageRegionContentModel = pageModel.PageRegionContentModels.FirstOrDefault(x => x.PageRegionType == regionType);

            if (pageRegionContentModel != null)
            {
                pageRegionContentModel.Content = new HtmlString(outputHtmlMarkup);
            }
        }
    }
}