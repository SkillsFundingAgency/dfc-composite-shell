using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DFC.Composite.Shell.Common;
using DFC.Composite.Shell.Models;
using DFC.Composite.Shell.Services.ContentProcessor;
using DFC.Composite.Shell.Services.ContentRetrieve;
using DFC.Composite.Shell.Services.Paths;
using DFC.Composite.Shell.Services.Regions;
using Microsoft.AspNetCore.Html;

namespace DFC.Composite.Shell.Services.Application
{
    public class ApplicationService : IApplicationService
    {
        private readonly IPathService _pathService;
        private readonly IRegionService _regionService;
        private readonly IContentRetriever _contentRetriever;
        private readonly IContentProcessor _contentProcessor;

        public ApplicationService(
            IPathService pathService,
            IRegionService regionService,
            IContentRetriever contentRetriever,
            IContentProcessor contentProcessor)
        {
            _pathService = pathService;
            _regionService = regionService;
            _contentRetriever = contentRetriever;
            _contentProcessor = contentProcessor;
        }

        public async Task GetMarkupAsync(ApplicationModel application, string article, PageViewModel pageModel)
        {
            if (application.Path.IsOnline)
            {
                if (string.IsNullOrWhiteSpace(article))
                {
                    article = "index";
                }
                if (article.StartsWith("/"))
                {
                    article = article.Substring(1);
                }

                //Get the markup at this url
                var applicationBodyRegionTask = GetApplicationMarkUpAsync(application, article);

                //Load related regions
                var otherRegionsTask = LoadRelatedRegions(application, pageModel, article);

                //Wait until everything is done
                await Task.WhenAll(applicationBodyRegionTask, otherRegionsTask);

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

        public async Task PostMarkupAsync(ApplicationModel application, string article, IEnumerable<KeyValuePair<string, string>> formParameters, PageViewModel pageModel)
        {
            await Task.CompletedTask;
        }

        public async Task<ApplicationModel> GetApplicationAsync(string path)
        {
            var applicationModel = new ApplicationModel();

            var pathModel = await _pathService.GetPath(path);

            if (pathModel != null)
            {
                applicationModel.Path = pathModel;
                applicationModel.Regions = await _regionService.GetRegions(pathModel.Path);
            }

            return applicationModel;
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

            var result = _contentRetriever.GetContent(url, bodyRegion.IsHealthy, bodyRegion.OfflineHTML);

            return result;
        }

        private async Task LoadRelatedRegions(ApplicationModel application, PageViewModel pageModel, string article)
        {
            var tasks = new List<Task<string>>();

            var headRegionTask = GetMarkupAsync(tasks, PageRegion.Head, application.Regions, article);
            var breadcrumbRegionTask = GetMarkupAsync(tasks, PageRegion.Breadcrumb, application.Regions, article);
            var bodyTopRegionTask = GetMarkupAsync(tasks, PageRegion.BodyTop, application.Regions, article);
            var sidebarLeftRegionTask = GetMarkupAsync(tasks, PageRegion.SidebarLeft, application.Regions, article);
            var sidebarRightRegionTask = GetMarkupAsync(tasks, PageRegion.SidebarRight, application.Regions, article);
            var bodyFooterRegionTask = GetMarkupAsync(tasks, PageRegion.BodyFooter, application.Regions, article);

            await Task.WhenAll(tasks);

            PopulatePageRegionContent(application, pageModel, PageRegion.Head, headRegionTask);
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

            var task = _contentRetriever.GetContent(url, pageRegionModel.IsHealthy, pageRegionModel.OfflineHTML);

            tasks.Add(task);

            return task;
        }

        private string FormatArticleUrl(string regionEndpoint, string article)
        {
            string urlFormatString = regionEndpoint;

            if (!urlFormatString.Contains("{0}"))
            {
                urlFormatString += "/{0}";
            }

            string url = string.Format(urlFormatString, article);

            return url;
        }

        private void PopulatePageRegionContent(ApplicationModel application, PageViewModel pageModel, PageRegion regionType, Task<string> task)
        {
            if (task != null)
            {
                var pageRegionContentModel = pageModel.PageRegionContentModels.First(x => x.PageRegionType == regionType);
                var content = string.Empty;

                if (task.IsCompletedSuccessfully)
                {
                    content = task.Result;
                    content = _contentProcessor.Process(content);
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
