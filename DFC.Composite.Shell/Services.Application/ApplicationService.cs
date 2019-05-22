using DFC.Composite.Shell.Common;
using DFC.Composite.Shell.Models;
using DFC.Composite.Shell.Services.ContentProcessor;
using DFC.Composite.Shell.Services.ContentRetrieve;
using DFC.Composite.Shell.Services.Paths;
using DFC.Composite.Shell.Services.Regions;
using Polly.CircuitBreaker;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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

        public async Task GetMarkupAsync(string path, string url, PageModel pageModel)
        {
            //Get the application
            var application = await GetApplicationAsync(path);

            //Get the markup at this url
            var applicationBodyRegionTask = GetApplicationMarkUpAsync(application, url);

            //Load related regions
            var otherRegionsTask = LoadRelatedRegions(application, pageModel);

            //Wait until everything is done
            await Task.WhenAll(applicationBodyRegionTask, otherRegionsTask);

            //Ensure that the application body markup is attached to the model
            PopulatePageRegionContent(application, pageModel, PageRegion.Body, applicationBodyRegionTask);
        }

        public async Task PostMarkupAsync(string path, string url, IEnumerable<KeyValuePair<string, string>> formParameters, PageModel pageModel)
        {
            await Task.CompletedTask;
        }

        public async Task<ApplicationModel> GetApplicationAsync(string path)
        {
            await InitApplicationsAsync();
            return _applications.FirstOrDefault(x => x.Path.Path == path);
        }

        private Task<string> GetApplicationMarkUpAsync(ApplicationModel application, string url)
        {
            //Get the body region
            var bodyRegion = application.Regions.FirstOrDefault(x => x.PageRegion == PageRegion.Body);
            if (bodyRegion == null || string.IsNullOrWhiteSpace(bodyRegion.RegionEndpoint))
            {
                return null;
            }

            if (string.IsNullOrWhiteSpace(url))
            {
                //If we didnt specify a url, then assume we want content from the body region endpoint
                url = bodyRegion.RegionEndpoint;
            }
            else
            {
                //If we did specify a url, then assume its relative to the body
                //Get the body region rootUrl
                var bodyRegionRootUrl = GetBodyRegionRootUrl(bodyRegion.RegionEndpoint);

                if (url.StartsWith("/"))
                {
                    url = url.Substring(1);
                }

                url = $"{bodyRegionRootUrl}{url}";
            }

            var result = _contentRetriever.GetContent(url);
            return result;
        }

        private string GetBodyRegionRootUrl(string bodyRegionEndpoint)
        {
            var bodyRegionEndpointUri = new Uri(bodyRegionEndpoint);
            return $"{bodyRegionEndpointUri.AbsoluteUri.Replace(bodyRegionEndpointUri.PathAndQuery, string.Empty)}/";
        }

        private async Task LoadRelatedRegions(ApplicationModel application, PageModel pageModel)
        {
            var tasks = new List<Task<string>>();

            var headRegionTask = GetMarkupAsync(pageModel, tasks, PageRegion.Head, application.Regions);
            var breadcrumbRegionTask = GetMarkupAsync(pageModel, tasks, PageRegion.Breadcrumb, application.Regions);
            var bodyTopRegionTask = GetMarkupAsync(pageModel, tasks, PageRegion.BodyTop, application.Regions);
            var sidebarLeftRegionTask = GetMarkupAsync(pageModel, tasks, PageRegion.SidebarLeft, application.Regions);
            var sidebarRightRegionTask = GetMarkupAsync(pageModel, tasks, PageRegion.SidebarRight, application.Regions);
            var footerRegionTask = GetMarkupAsync(pageModel, tasks, PageRegion.Footer, application.Regions);

            await Task.WhenAll(tasks);

            PopulatePageRegionContent(application, pageModel, PageRegion.Head, headRegionTask);
            PopulatePageRegionContent(application, pageModel, PageRegion.Breadcrumb, breadcrumbRegionTask);
            PopulatePageRegionContent(application, pageModel, PageRegion.BodyTop, bodyTopRegionTask);
            PopulatePageRegionContent(application, pageModel, PageRegion.SidebarLeft, sidebarLeftRegionTask);
            PopulatePageRegionContent(application, pageModel, PageRegion.SidebarRight, sidebarRightRegionTask);
            PopulatePageRegionContent(application, pageModel, PageRegion.Footer, footerRegionTask);
        }

        private Task<string> GetMarkupAsync(PageModel pageModel, List<Task<string>> tasks, PageRegion regionType, IEnumerable<RegionModel> regions)
        {
            var pageRegionModel = regions.FirstOrDefault(x => x.PageRegion == regionType);
            if (pageRegionModel == null || string.IsNullOrWhiteSpace(pageRegionModel.RegionEndpoint))
            {
                return null;
            }

            var task = _contentRetriever.GetContent(pageRegionModel.RegionEndpoint);
            tasks.Add(task);

            return task;
        }

        private void PopulatePageRegionContent(ApplicationModel application, PageModel pageModel, PageRegion regionType, Task<string> task)
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

                pageRegionContentModel.Content = content;
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
                    var application = new ApplicationModel();
                    application.Path = path;
                    application.Regions = await _regionService.GetRegions(path.Path);
                    _applications.Add(application);
                }
            }
        }

    }
}
