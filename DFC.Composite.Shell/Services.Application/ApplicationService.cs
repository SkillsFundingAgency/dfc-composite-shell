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
        private List<ApplicationModel> _applications;

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

        public async Task GetMarkupAsync(string path, string contentUrl, PageViewModel pageModel)
        {
            //Get the application
            var application = await GetApplicationAsync(path);

            //Get the markup at this url
            var applicationBodyRegionTask = GetApplicationMarkUpAsync(application, contentUrl);

            //Load related regions
            var otherRegionsTask = LoadRelatedRegions(application, pageModel);

            //Wait until everything is done
            await Task.WhenAll(applicationBodyRegionTask, otherRegionsTask);

            //Ensure that the application body markup is attached to the model
            PopulatePageRegionContent(application, pageModel, PageRegion.Body, applicationBodyRegionTask);
        }

        public async Task PostMarkupAsync(string path, string contentUrl, IEnumerable<KeyValuePair<string, string>> formParameters, PageViewModel pageModel)
        {
            await Task.CompletedTask;
        }

        public async Task<ApplicationModel> GetApplicationAsync(string path)
        {
            await InitApplicationsAsync();
            return _applications.FirstOrDefault(x => x.Path.Path == path);
        }

        private Task<string> GetApplicationMarkUpAsync(ApplicationModel application, string contentUrl)
        {
            //Get the body region
            var bodyRegion = application.Regions.FirstOrDefault(x => x.PageRegion == PageRegion.Body);
            if (bodyRegion == null || string.IsNullOrWhiteSpace(bodyRegion.RegionEndpoint))
            {
                return null;
            }

            if (string.IsNullOrWhiteSpace(contentUrl))
            {
                //If we didn't specify a url, then assume we want content from the body region endpoint
                contentUrl = bodyRegion.RegionEndpoint;
            }
            else
            {
                //If we did specify a url, then assume its relative to the body
                //Get the body region rootUrl
                var bodyRegionRootUrl = GetBodyRegionRootUrl(bodyRegion.RegionEndpoint);

                if (contentUrl.StartsWith("/"))
                {
                    contentUrl = contentUrl.Substring(1);
                }

                contentUrl = $"{bodyRegionRootUrl}{application.Path.Path}/{contentUrl}";
            }

            var result = _contentRetriever.GetContent(contentUrl, bodyRegion.IsHealthy, bodyRegion.OfflineHTML);

            return result;
        }

        private string GetBodyRegionRootUrl(string bodyRegionEndpoint)
        {
            var bodyRegionEndpointUri = new Uri(bodyRegionEndpoint);
            return $"{bodyRegionEndpointUri.AbsoluteUri.Replace(bodyRegionEndpointUri.PathAndQuery, string.Empty)}/";
        }

        private async Task LoadRelatedRegions(ApplicationModel application, PageViewModel pageModel)
        {
            var tasks = new List<Task<string>>();

            var headRegionTask = GetMarkupAsync(tasks, PageRegion.Head, application.Regions);
            var breadcrumbRegionTask = GetMarkupAsync(tasks, PageRegion.Breadcrumb, application.Regions);
            var bodyTopRegionTask = GetMarkupAsync(tasks, PageRegion.BodyTop, application.Regions);
            var sidebarLeftRegionTask = GetMarkupAsync(tasks, PageRegion.SidebarLeft, application.Regions);
            var sidebarRightRegionTask = GetMarkupAsync(tasks, PageRegion.SidebarRight, application.Regions);
            var bodyFooterRegionTask = GetMarkupAsync(tasks, PageRegion.BodyFooter, application.Regions);

            await Task.WhenAll(tasks);

            PopulatePageRegionContent(application, pageModel, PageRegion.Head, headRegionTask);
            PopulatePageRegionContent(application, pageModel, PageRegion.Breadcrumb, breadcrumbRegionTask);
            PopulatePageRegionContent(application, pageModel, PageRegion.BodyTop, bodyTopRegionTask);
            PopulatePageRegionContent(application, pageModel, PageRegion.SidebarLeft, sidebarLeftRegionTask);
            PopulatePageRegionContent(application, pageModel, PageRegion.SidebarRight, sidebarRightRegionTask);
            PopulatePageRegionContent(application, pageModel, PageRegion.BodyFooter, bodyFooterRegionTask);
        }

        private Task<string> GetMarkupAsync(List<Task<string>> tasks, PageRegion regionType, IEnumerable<RegionModel> regions)
        {
            var pageRegionModel = regions.FirstOrDefault(x => x.PageRegion == regionType);
            if (pageRegionModel == null || string.IsNullOrWhiteSpace(pageRegionModel.RegionEndpoint))
            {
                return null;
            }

            string url = pageRegionModel.RegionEndpoint;

            var task = _contentRetriever.GetContent(url, pageRegionModel.IsHealthy, pageRegionModel.OfflineHTML);

            tasks.Add(task);

            return task;
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
                    content = application.Path.OfflineHtml;
                }

                pageRegionContentModel.Content = new HtmlString(content);
            }
        }

        private async Task InitApplicationsAsync()
        {
            if (_applications == null)
            {
                _applications = new List<ApplicationModel>();

                var paths = await _pathService.GetPaths();

                foreach (var path in paths)
                {
                    var application = new ApplicationModel
                    {
                        Path = path,
                        Regions = await _regionService.GetRegions(path.Path)
                    };

                    _applications.Add(application);
                }
            }
        }

    }
}
