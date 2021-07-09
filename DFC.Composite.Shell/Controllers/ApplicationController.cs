using DFC.Composite.Shell.Extensions;
using DFC.Composite.Shell.Models;
using DFC.Composite.Shell.Models.Enums;
using DFC.Composite.Shell.Models.Exceptions;
using DFC.Composite.Shell.Services.Application;
using DFC.Composite.Shell.Services.BaseUrl;
using DFC.Composite.Shell.Services.Mapping;
using DFC.Composite.Shell.Services.Neo4J;
using DFC.Composite.Shell.Utilities;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace DFC.Composite.Shell.Controllers
{
    public class ApplicationController : Controller
    {
        public const string AlertPathName = "alerts";
        private const string MainRenderViewName = "Application/RenderView";

        private readonly IMapper<ApplicationModel, PageViewModel> mapper;
        private readonly ILogger<ApplicationController> logger;
        private readonly IApplicationService applicationService;
        private readonly IVersionedFiles versionedFiles;
        private readonly IConfiguration configuration;
        private readonly IBaseUrlService baseUrlService;
        private readonly INeo4JService neo4JService;

        public ApplicationController(
            IMapper<ApplicationModel, PageViewModel> mapper,
            ILogger<ApplicationController> logger,
            IApplicationService applicationService,
            IVersionedFiles versionedFiles,
            IConfiguration configuration,
            IBaseUrlService baseUrlService,
            INeo4JService neo4JService)
        {
            this.mapper = mapper;
            this.logger = logger;
            this.applicationService = applicationService;
            this.versionedFiles = versionedFiles;
            this.configuration = configuration;
            this.baseUrlService = baseUrlService;
            this.neo4JService = neo4JService;
        }

        [HttpGet]
        public async Task<IActionResult> Action(ActionGetRequestModel requestViewModel)
        {
            var viewModel = versionedFiles.BuildDefaultPageViewModel(configuration);

            if (requestViewModel == null)
            {
                return View(MainRenderViewName, Map(viewModel));
            }

            NormalisePathAndData(requestViewModel);

            var errorRequestViewModel = new ActionGetRequestModel
            {
                Path = AlertPathName,
                Data = ((int)HttpStatusCode.NotFound).ToString(CultureInfo.InvariantCulture),
            };

            var requestItems = new[]
            {
                requestViewModel,
                errorRequestViewModel,
                new ActionGetRequestModel
                {
                    Path = AlertPathName,
                    Data = ((int)HttpStatusCode.InternalServerError).ToString(CultureInfo.InvariantCulture),
                },
            };

            foreach (var requestItem in requestItems)
            {
                try
                {
                    logger.LogInformation(
                        "{action}: Getting child response for: {path}/{data}",
                        nameof(Action),
                        requestItem.Path,
                        requestItem.Data);

                    await neo4JService.InsertNewRequest(Request);

                    var application = await applicationService.GetApplicationAsync(requestItem);

                    if (application?.AppRegistrationModel == null)
                    {
                        logger.LogWarning("{action}: The path '{path}' is not registered", nameof(Action), requestItem.Path);
                        Response.StatusCode = (int)HttpStatusCode.NotFound;
                    }
                    else if (application.AppRegistrationModel.ExternalURL != null)
                    {
                        logger.LogInformation(
                            "{action}: Redirecting to external for: {path}/{article}",
                            nameof(Action),
                            application.AppRegistrationModel.Path,
                            application.Article);

                        return Redirect(application.AppRegistrationModel.ExternalURL.ToString());
                    }
                    else
                    {
                        await mapper.Map(application, viewModel);

                        applicationService.RequestBaseUrl = baseUrlService.GetBaseUrl(Request, Url);
                        await applicationService.GetMarkupAsync(application, viewModel, Request.QueryString.Value);

                        logger.LogInformation(
                            "{action}: Received child response for: {path}/{article}",
                            nameof(Action),
                            application.AppRegistrationModel.Path,
                            application.Article);

                        if (AlertPathName.Equals(application.AppRegistrationModel.Path, StringComparison.InvariantCulture)
                            && int.TryParse(application.Article, out var statusCode))
                        {
                            Response.StatusCode = statusCode;
                        }

                        break;
                    }
                }
                catch (HttpException httpException)
                {
                    logger.LogWarning(
                        httpException,
                        "{action}: The content {url} responded with {statusCode}",
                        nameof(Action),
                        httpException.Url,
                        httpException.StatusCode);

                    Response.StatusCode = (int)httpException.StatusCode;
                    errorRequestViewModel.Data = Response.StatusCode.ToString(CultureInfo.InvariantCulture);
                }
                catch (RedirectRequest redirectRequest)
                {
                    var redirectTo = redirectRequest.Location?.OriginalString;
                    logger.LogInformation(
                        "{action}: Redirecting from: {oldLocation} to: {redirectTo}",
                        nameof(Action),
                        redirectRequest.OldLocation?.ToString(),
                        redirectTo);

                    Response.Redirect(redirectTo, redirectRequest.IsPermenant);
                    break;
                }
            }

            return View(MainRenderViewName, Map(viewModel));
        }

        [HttpPost]
        public async Task<IActionResult> Action(ActionPostRequestModel requestViewModel)
        {
            var viewModel = versionedFiles.BuildDefaultPageViewModel(configuration);

            if (requestViewModel == null)
            {
                return View(MainRenderViewName, Map(viewModel));
            }

            NormalisePathAndData(requestViewModel);

            var postFirstRequest = true;
            var errorRequestViewModel = new ActionPostRequestModel
            {
                Path = AlertPathName,
                Data = ((int)HttpStatusCode.NotFound).ToString(CultureInfo.InvariantCulture),
            };

            var requestItems = new[]
            {
                requestViewModel,
                errorRequestViewModel,
                new ActionPostRequestModel
                {
                    Path = AlertPathName,
                    Data = ((int)HttpStatusCode.InternalServerError).ToString(CultureInfo.InvariantCulture),
                },
            };

            foreach (var requestItem in requestItems)
            {
                try
                {
                    logger.LogInformation(
                        "{action}: Getting child response for: {path}/{data}",
                        nameof(Action),
                        requestItem.Path,
                        requestItem.Data);

                    var application = await applicationService.GetApplicationAsync(requestItem);

                    if (application?.AppRegistrationModel == null)
                    {
                        logger.LogWarning(
                            "{action}: The path '{path}' is not registered",
                            nameof(Action),
                            requestItem.Path);

                        Response.StatusCode = (int)HttpStatusCode.NotFound;
                    }
                    else
                    {
                        await mapper.Map(application, viewModel);
                        applicationService.RequestBaseUrl = baseUrlService.GetBaseUrl(Request, Url);

                        var formParameters = requestItem.FormCollection?.Any() == true ?
                            (from formItem in requestItem.FormCollection
                                select new KeyValuePair<string, string>(formItem.Key, formItem.Value)).ToArray()
                            : null;

                        if (postFirstRequest)
                        {
                            await applicationService.PostMarkupAsync(application, formParameters, viewModel);
                            postFirstRequest = false;
                        }
                        else
                        {
                            await applicationService.GetMarkupAsync(application, viewModel, string.Empty);
                        }

                        logger.LogInformation(
                            "{action}: Received child response for: {path}/{article}",
                            nameof(Action),
                            application.AppRegistrationModel.Path,
                            application.Article);

                        if (AlertPathName.Equals(application.AppRegistrationModel.Path, StringComparison.InvariantCulture)
                            && int.TryParse(application.Article, out var statusCode))
                        {
                            Response.StatusCode = statusCode;
                        }

                        break;
                    }
                }
                catch (HttpException httpException)
                {
                    logger.LogWarning(
                        httpException,
                        "{action}: The content {url} responded with {statusCode}",
                        nameof(Action),
                        httpException.Url,
                        httpException.StatusCode);

                    Response.StatusCode = (int)httpException.StatusCode;
                    errorRequestViewModel.Data = Response.StatusCode.ToString(CultureInfo.InvariantCulture);
                }
                catch (RedirectRequest redirectRequest)
                {
                    var redirectTo = redirectRequest.Location?.OriginalString;
                    logger.LogInformation(
                        "{action}: Redirecting from: {oldLocation} to: {redirectTo}",
                        nameof(Action),
                        redirectRequest.OldLocation?.ToString(),
                        redirectTo);

                    Response.Redirect(redirectTo, redirectRequest.IsPermenant);
                    break;
                }
            }

            return View(MainRenderViewName, Map(viewModel));
        }

        private static PageViewModelResponse Map(PageViewModel source)
        {
            return new PageViewModelResponse
            {
                BrandingAssetsCdn = source.BrandingAssetsCdn,
                ScriptIds = source.ScriptIds,
                LayoutName = source.LayoutName,
                PageTitle = source.PageTitle,
                Path = source.Path,
                PhaseBannerHtml = source.PhaseBannerHtml,

                VersionedPathForCssScripts = source.VersionedPathForCssScripts,
                VersionedPathForJavaScripts = source.VersionedPathForJavaScripts,
                VersionedPathForWebChatJs = source.VersionedPathForWebChatJs,

                WebchatEnabled = source.WebchatEnabled,

                ContentBody = GetContent(source, PageRegion.Body),
                ContentBodyFooter = GetContent(source, PageRegion.BodyFooter),
                ContentBodyTop = GetContent(source, PageRegion.BodyTop),
                ContentHeroBanner = GetContent(source, PageRegion.HeroBanner),
                ContentBreadcrumb = GetContent(source, PageRegion.Breadcrumb),
                ContentHead = GetContent(source, PageRegion.Head),
                ContentSidebarLeft = GetContent(source, PageRegion.SidebarLeft),
                ContentSidebarRight = GetContent(source, PageRegion.SidebarRight),
            };
        }

        private static HtmlString GetContent(PageViewModel pageViewModel, PageRegion pageRegionType)
        {
            var result = string.Empty;
            var pageRegionContentModel = pageViewModel.PageRegionContentModels?
                .FirstOrDefault(regionModel => regionModel.PageRegionType == pageRegionType);

            if (pageRegionContentModel?.Content != null)
            {
                result = pageRegionContentModel.Content.Value;
            }

            return new HtmlString(result);
        }

        [SuppressMessage(
            "Globalization",
            "CA1308:Normalize strings to uppercase",
            Justification = "Existing functionality is to lowercase - dont wish to make incompatible")]

        private static void NormalisePathAndData(ActionGetRequestModel model)
        {
            if (model == null)
            {
                return;
            }

            model.Path = model?.Path?.ToLowerInvariant();
            model.Data = model?.Data?.ToLowerInvariant();
        }
    }
}
