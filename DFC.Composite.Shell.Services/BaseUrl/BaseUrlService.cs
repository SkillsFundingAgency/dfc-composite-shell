using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;

namespace DFC.Composite.Shell.Services.BaseUrl
{
    public class BaseUrlService : IBaseUrlService
    {
        public Uri GetBaseUrl(HttpRequest request, IUrlHelper urlHelper)
        {
            var anyElementNull = request == null || urlHelper == null;

            return anyElementNull ?
                null : new Uri($"{request?.Scheme}://{request.Host}{urlHelper?.Content("~")}");
        }
    }
}