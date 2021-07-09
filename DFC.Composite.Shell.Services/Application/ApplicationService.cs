using DFC.Composite.Shell.Models;
using DFC.Composite.Shell.Models.AppRegistration;
using DFC.Composite.Shell.Models.Enums;
using DFC.Composite.Shell.Services.AppRegistry;
using DFC.Composite.Shell.Services.ContentProcessor;
using DFC.Composite.Shell.Services.ContentRetrieval;
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
        private readonly IAppRegistryService appRegistryDataService;
        private readonly IContentRetriever contentRetriever;
        private readonly IContentProcessorService contentProcessorService;
        private readonly MarkupMessages markupMessages;

        public ApplicationService(
            IAppRegistryService appRegistryDataService,
            IContentRetriever contentRetriever,
            IContentProcessorService contentProcessorService,
            MarkupMessages markupMessages)
        {
            this.appRegistryDataService = appRegistryDataService;
            this.contentRetriever = contentRetriever;
            this.contentProcessorService = contentProcessorService;
            this.markupMessages = markupMessages;
        }

        /// <inheritdoc/>
        public Uri RequestBaseUrl { get; set; }

        /// <inheritdoc/>
        public async Task GetMarkupAsync(ApplicationModel application, PageViewModel pageModel, string queryString)
        {
            _ = application ?? throw new ArgumentNullException(nameof(application));
            _ = pageModel ?? throw new ArgumentNullException(nameof(pageModel));

            try
            {
                if (!application.AppRegistrationModel.IsOnline)
                {
                    SetBodyOffline(application, pageModel);
                    return;
                }

                var bodyRegion = GetBodyMarkupAsync(application, queryString);
                await GetAndPopulateAdditionalMarkupAsync(application, pageModel, queryString);

                PopulatePageRegionContent(application, pageModel, PageRegion.Body, await bodyRegion);
            }
            catch
            {
                SetBodyOffline(application, pageModel);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task PostMarkupAsync(
            ApplicationModel application,
            IEnumerable<KeyValuePair<string, string>> formParameters,
            PageViewModel pageModel)
        {
            _ = pageModel ?? throw new ArgumentNullException(nameof(pageModel));

            if (application?.AppRegistrationModel.IsOnline != true)
            {
                var pageRegionContentModel = pageModel.PageRegionContentModels
                    .First(region => region.PageRegionType == PageRegion.Body);

                if (pageRegionContentModel != null)
                {
                    pageRegionContentModel.Content = GetOfflineHtml(application);
                }

                return;
            }

            var bodyHtml = GetPostedBodyMarkupAsync(application, formParameters);
            await GetAndPopulateAdditionalMarkupAsync(application, pageModel);

            PopulatePageRegionContent(application, pageModel, PageRegion.Body, await bodyHtml);
        }

        /// <inheritdoc/>
        public async Task<ApplicationModel> GetApplicationAsync(ActionGetRequestModel data)
        {
            _ = data ?? throw new ArgumentNullException(nameof(data));
            var applicationModel = await DetermineArticleLocation(data);

            if (applicationModel?.AppRegistrationModel == null)
            {
                return applicationModel;
            }

            var bodyRegion = applicationModel.AppRegistrationModel.Regions?
                .FirstOrDefault(region => region.PageRegion == PageRegion.Body);

            if (string.IsNullOrWhiteSpace(bodyRegion?.RegionEndpoint) == false)
            {
                var uri = new Uri(bodyRegion.RegionEndpoint);
                var url = new Uri($"{uri.Scheme}://{uri.Authority}");

                applicationModel.RootUrl = url;
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

            if (queryString?.TrimStart().StartsWith('?') == true)
            {
                urlFormatString += QueryStringPlaceholder;
            }

            return !string.IsNullOrWhiteSpace(article)
                ? string.Format(CultureInfo.InvariantCulture, urlFormatString, article, queryString)
                : urlFormatString
                    .Replace(SlashedPlaceholder, string.Empty, StringComparison.OrdinalIgnoreCase)
                    .Replace(QueryStringPlaceholder, string.Empty, StringComparison.OrdinalIgnoreCase);
        }

        private void SetBodyOffline(ApplicationModel application, PageViewModel pageModel)
        {
            var pageRegionContentModel = pageModel.PageRegionContentModels?
                .SingleOrDefault(regionContent => regionContent.PageRegionType == PageRegion.Body);

            if (pageRegionContentModel == null)
            {
                return;
            }

            pageRegionContentModel.Content = GetOfflineHtml(application);
        }

        private HtmlString GetOfflineHtml(ApplicationModel application)
        {
            return new HtmlString(!string.IsNullOrWhiteSpace(application.AppRegistrationModel.OfflineHtml)
                ? application.AppRegistrationModel.OfflineHtml
                    : markupMessages.AppOfflineHtml);
        }

        private async Task<ApplicationModel> DetermineArticleLocation(ActionGetRequestModel data)
        {
            const string appRegistryPathNameForPagesApp = "pages";

            var pageLocation = string.Join("/", new[] { data.Path, data.Data });
            var pageLocations = pageLocation.Split("/", StringSplitOptions.RemoveEmptyEntries);
            var article = string.Join("/", pageLocations);
            var applicationModel = new ApplicationModel();

            var pagesAppRegistrationModel = await appRegistryDataService.GetAppRegistrationModel(appRegistryPathNameForPagesApp);
            var locationsContainsArticle = pagesAppRegistrationModel?.PageLocations?.Values
                .SelectMany(pageLocation => pageLocation.Locations)
                .Contains($"/{article}") == true;

            if (locationsContainsArticle)
            {
                applicationModel.AppRegistrationModel = pagesAppRegistrationModel;
                applicationModel.Article = article;
            }

            if (applicationModel.AppRegistrationModel == null)
            {
                applicationModel.AppRegistrationModel = await appRegistryDataService.GetAppRegistrationModel(article) ??
                    await appRegistryDataService.GetAppRegistrationModel(data.Path);

                if (applicationModel.AppRegistrationModel != null)
                {
                    applicationModel.Article = article.Length > applicationModel.AppRegistrationModel.Path.Length
                        ? article[(applicationModel.AppRegistrationModel.Path.Length + 1) ..] : null;
                }
            }

            return applicationModel;
        }

        private Task<string> GetBodyMarkupAsync(ApplicationModel application, string queryString)
        {
            var bodyRegion = application.AppRegistrationModel.Regions?.FirstOrDefault(region => region.PageRegion == PageRegion.Body);

            if (string.IsNullOrWhiteSpace(bodyRegion?.RegionEndpoint))
            {
                return Task.FromResult(string.Empty);
            }

            var url = FormatArticleUrl(bodyRegion.RegionEndpoint, application.Article, queryString);
            return contentRetriever.GetContentAsync(url, application.AppRegistrationModel.Path, bodyRegion, false, RequestBaseUrl);
        }

        private async Task<string> GetMarkupAsync(
            ApplicationModel application,
            RegionModel regionModel,
            string article,
            string queryString)
        {
            if (string.IsNullOrEmpty(regionModel?.RegionEndpoint))
            {
                return null;
            }

            var url = FormatArticleUrl(regionModel.RegionEndpoint, article, queryString);
            var html = await contentRetriever.GetContentAsync(
                url, application.AppRegistrationModel.Path, regionModel, false, RequestBaseUrl);

            return contentProcessorService.Process(html, RequestBaseUrl, application.RootUrl);
        }

        private Task<string> GetPostedBodyMarkupAsync(ApplicationModel application, IEnumerable<KeyValuePair<string, string>> formParameters)
        {
            var bodyRegion = application.AppRegistrationModel.Regions?.FirstOrDefault(region => region.PageRegion == PageRegion.Body);

            if (string.IsNullOrWhiteSpace(bodyRegion?.RegionEndpoint))
            {
                return Task.FromResult(string.Empty);
            }

            var url = FormatArticleUrl(bodyRegion.RegionEndpoint, application.Article, string.Empty);
            return contentRetriever.PostContentAsync(url, application.AppRegistrationModel.Path, bodyRegion, formParameters, RequestBaseUrl);
        }

        private async Task GetAndPopulateAdditionalMarkupAsync(ApplicationModel application, PageViewModel pageModel, string queryString = "")
        {
            // This will create the session if it doesn't already exist
            var headMarkup = await GetMarkupAsync(
                application,
                application.AppRegistrationModel?.Regions?.SingleOrDefault(region => region.PageRegion == PageRegion.Head),
                application.Article,
                queryString);

            var outputModelHead = pageModel.PageRegionContentModels?
                .SingleOrDefault(regionContent => regionContent.PageRegionType == PageRegion.Head);

            if (outputModelHead == null)
            {
                return;
            }

            outputModelHead.Content = new HtmlString(headMarkup);

            var path = application.AppRegistrationModel.Path;
            var regions = application.AppRegistrationModel.Regions;

            if (regions?.Any() != true)
            {
                return;
            }

            var getMarkupTasks = Enum.GetValues(typeof(PageRegion))
                .Cast<PageRegion>()
                .Where(regionType => regionType != PageRegion.Head && regionType != PageRegion.Body)
                .Select(regionType => GetMarkupAsync(path, regionType, regions, application.Article, queryString));

            foreach (var getMarkupTask in getMarkupTasks)
            {
                var (regionType, markup) = await getMarkupTask;
                PopulatePageRegionContent(application, pageModel, regionType, markup);
            }
        }

        private async Task<(PageRegion regionType, string markup)> GetMarkupAsync(
            string path,
            PageRegion regionType,
            IEnumerable<RegionModel> regions,
            string article,
            string queryString)
        {
            var pageRegionModel = regions.FirstOrDefault(region => region.PageRegion == regionType);

            if (string.IsNullOrWhiteSpace(pageRegionModel?.RegionEndpoint))
            {
                return (regionType, null);
            }

            if (!pageRegionModel.IsHealthy)
            {
                var offlineMarkup = !string.IsNullOrWhiteSpace(pageRegionModel.OfflineHtml)
                    ? pageRegionModel.OfflineHtml : markupMessages.GetRegionOfflineHtml(pageRegionModel.PageRegion);

                return (regionType, offlineMarkup);
            }

            var url = FormatArticleUrl(pageRegionModel.RegionEndpoint, article, queryString);
            var html = await contentRetriever.GetContentAsync(url, path, pageRegionModel, true, RequestBaseUrl);

            return (regionType, html);
        }

        private void PopulatePageRegionContent(ApplicationModel application, PageViewModel pageModel, PageRegion regionType, string markup)
        {
            var pageRegionContentModel = pageModel.PageRegionContentModels?
                .FirstOrDefault(region => region.PageRegionType == regionType);

            if (pageRegionContentModel == null)
            {
                return;
            }

            var outputHtmlMarkup = contentProcessorService.Process(markup, RequestBaseUrl, application.RootUrl);
            pageRegionContentModel.Content ??= new HtmlString(outputHtmlMarkup);
        }
    }
}