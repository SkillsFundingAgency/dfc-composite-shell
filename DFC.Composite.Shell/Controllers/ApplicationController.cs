using DFC.Composite.Shell.Extensions;
using DFC.Composite.Shell.Models;
using DFC.Composite.Shell.Models.Exceptions;
using DFC.Composite.Shell.Services.Application;
using DFC.Composite.Shell.Services.BaseUrl;
using DFC.Composite.Shell.Services.Mapping;
using DFC.Composite.Shell.Utilities;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
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

        public ApplicationController(
            IMapper<ApplicationModel, PageViewModel> mapper,
            ILogger<ApplicationController> logger,
            IApplicationService applicationService,
            IVersionedFiles versionedFiles,
            IConfiguration configuration,
            IBaseUrlService baseUrlService)
        {
            this.mapper = mapper;
            this.logger = logger;
            this.applicationService = applicationService;
            this.versionedFiles = versionedFiles;
            this.configuration = configuration;
            this.baseUrlService = baseUrlService;
        }

        [HttpGet]
        public async Task<IActionResult> Action(ActionGetRequestModel requestViewModel)
        {
            var viewModel = versionedFiles.BuildDefaultPageViewModel(configuration);
            if (Request.IsAjax())
            {
                var application = await applicationService.GetApplicationAsync(requestViewModel);
                applicationService.RequestBaseUrl = baseUrlService.GetBaseUrl(Request, Url);
                var content = await applicationService.GetAjaxModelAsync(application, Request.QueryString.Value, Request.Headers);
                return Ok(content);
            }

            if (requestViewModel != null)
            {
                requestViewModel.Path = requestViewModel.Path?.ToLowerInvariant();
                requestViewModel.Data = requestViewModel.Data?.ToLowerInvariant();

                var errorRequestViewModel = new ActionGetRequestModel
                {
                    Path = AlertPathName,
                    Data = $"{(int)HttpStatusCode.NotFound}",
                };
                var requestItems = new[]
                {
                    requestViewModel,
                    errorRequestViewModel,
                    new ActionGetRequestModel
                    {
                        Path = AlertPathName,
                        Data = $"{(int)HttpStatusCode.InternalServerError}",
                    },
                };

                foreach (var requestItem in requestItems)
                {
                    try
                    {
                        logger.LogInformation($"{nameof(Action)}: Getting child response for: {requestItem.Path}/{requestItem.Data}");

                        var application = await applicationService.GetApplicationAsync(requestItem);

                        if (application?.AppRegistrationModel == null)
                        {
                            var errorString = $"The path '{requestItem.Path}' is not registered";

                            logger.LogWarning($"{nameof(Action)}: {errorString}");

                            Response.StatusCode = (int)HttpStatusCode.NoContent;
                        }
                        else if (application.AppRegistrationModel.ExternalURL != null)
                        {
                            logger.LogInformation($"{nameof(Action)}: Redirecting to external for: {application.AppRegistrationModel.Path}/{application.Article}");

                            return Redirect(application.AppRegistrationModel.ExternalURL.ToString());
                        }
                        else
                        {
                            await mapper.Map(application, viewModel);

                            applicationService.RequestBaseUrl = baseUrlService.GetBaseUrl(Request, Url);

                            await applicationService.GetMarkupAsync(application, viewModel, Request.Path, Request.QueryString.Value, Request.Headers);

                            logger.LogInformation($"{nameof(Action)}: Received child response for: {application.AppRegistrationModel.Path}/{application.Article}");

                            if (string.Compare(application.AppRegistrationModel.Path, AlertPathName, true, CultureInfo.InvariantCulture) == 0 && int.TryParse(application.Article, out var statusCode))
                            {
                                Response.StatusCode = statusCode;
                            }

                            break;
                        }
                    }
                    catch (EnhancedHttpException ex)
                    {
                        var errorString = $"The content {ex.Url} responded with {ex.StatusCode}";

                        logger.LogWarning($"{nameof(Action)}: {errorString}");

                        Response.StatusCode = (int)ex.StatusCode;
                        errorRequestViewModel.Data = $"{Response.StatusCode}";
                    }
                    catch (RedirectException ex)
                    {
                        var redirectTo = ex.Location?.OriginalString;

                        logger.LogInformation(ex, $"{nameof(Action)}: Redirecting from: {ex.OldLocation?.ToString()} to: {redirectTo}");

                        Response.Redirect(redirectTo, ex.IsPermenant);
                        break;
                    }
                }
            }

            return View(MainRenderViewName, Map(viewModel));
        }

        [HttpPost]
        public async Task<IActionResult> Action(ActionPostRequestModel requestViewModel)
        {
            var viewModel = versionedFiles.BuildDefaultPageViewModel(configuration);

            if (requestViewModel != null)
            {
                requestViewModel.Path = requestViewModel.Path?.ToLowerInvariant();
                requestViewModel.Data = requestViewModel.Data?.ToLowerInvariant();

                bool postFirstRequest = true;
                var errorRequestViewModel = new ActionPostRequestModel
                {
                    Path = AlertPathName,
                    Data = $"{(int)HttpStatusCode.NotFound}",
                };
                var requestItems = new[]
                {
                    requestViewModel,
                    errorRequestViewModel,
                    new ActionPostRequestModel
                    {
                        Path = AlertPathName,
                        Data = $"{(int)HttpStatusCode.InternalServerError}",
                    },
                };

                foreach (var requestItem in requestItems)
                {
                    try
                    {
                        logger.LogInformation($"{nameof(Action)}: Getting child response for: {requestItem.Path}/{requestItem.Data}");

                        var application = await applicationService.GetApplicationAsync(requestItem);

                        if (application?.AppRegistrationModel == null)
                        {
                            var errorString = $"The path '{requestItem.Path}' is not registered";

                            logger.LogWarning($"{nameof(Action)}: {errorString}");

                            Response.StatusCode = (int)HttpStatusCode.NotFound;
                        }
                        else
                        {
                            await mapper.Map(application, viewModel);

                            applicationService.RequestBaseUrl = baseUrlService.GetBaseUrl(Request, Url);

                            KeyValuePair<string, string>[] formParameters = null;

                            if (requestItem.FormCollection != null && requestItem.FormCollection.Any())
                            {
                                formParameters = (from a in requestItem.FormCollection
                                                  select new KeyValuePair<string, string>(a.Key, a.Value)).ToArray();
                            }

                            if (postFirstRequest)
                            {
                                postFirstRequest = false;
                                await applicationService.PostMarkupAsync(application, formParameters, viewModel, string.Empty, Request.Headers);
                                if (viewModel.IsFileDownload)
                                {
                                    var fileDetails = viewModel.FileDownloadModel;
                                    return File(fileDetails.FileBytes, fileDetails.FileContentType, fileDetails.FileName);
                                }
                            }
                            else
                            {
                                await applicationService.GetMarkupAsync(application, viewModel, string.Empty, string.Empty, Request.Headers);
                            }

                            logger.LogInformation($"{nameof(Action)}: Received child response for: {application.AppRegistrationModel.Path}/{application.Article}");

                            if (string.Compare(application.AppRegistrationModel.Path, AlertPathName, true, CultureInfo.InvariantCulture) == 0)
                            {
                                if (int.TryParse(application.Article, out var statusCode))
                                {
                                    Response.StatusCode = statusCode;
                                }
                            }

                            break;
                        }
                    }
                    catch (EnhancedHttpException ex)
                    {
                        var errorString = $"The content {ex.Url} responded with {ex.StatusCode}";

                        logger.LogWarning($"{nameof(Action)}: {errorString}");

                        Response.StatusCode = (int)ex.StatusCode;
                        errorRequestViewModel.Data = $"{Response.StatusCode}";
                    }
                    catch (RedirectException ex)
                    {
                        string redirectTo = ex.Location?.OriginalString;

                        logger.LogInformation(ex, $"{nameof(Action)}: Redirecting from: {ex.OldLocation?.ToString()} to: {redirectTo}");

                        Response.Redirect(redirectTo, ex.IsPermenant);

                        break;
                    }
                }
            }

            return View(MainRenderViewName, Map(viewModel));
        }

        private static PageViewModelResponse Map(PageViewModel source)
        {
            var result = new PageViewModelResponse
            {
                BrandingAssetsCdn = source.BrandingAssetsCdn,
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

            return result;
        }

        private static HtmlString GetContent(PageViewModel pageViewModel, PageRegion pageRegionType)
        {
            var result = string.Empty;
            var pageRegionContentModel = pageViewModel.PageRegionContentModels.FirstOrDefault(x => x.PageRegionType == pageRegionType);
            if (pageRegionContentModel != null && pageRegionContentModel.Content != null)
            {
                result = pageRegionContentModel.Content.Value;
            }

            return new HtmlString(result);
        }
    }
}