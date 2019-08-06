using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DFC.Composite.Shell.Services.BaseUrlService
{
    public class BaseUrlService : IBaseUrlService
    {
        public string GetBaseUrl(HttpRequest request, IUrlHelper urlHelper)
        {
            if (request != null && urlHelper != null)
            {
                return $"{request.Scheme}://{request.Host}{urlHelper.Content("~")}";
            }

            return string.Empty;
        }
    }
}