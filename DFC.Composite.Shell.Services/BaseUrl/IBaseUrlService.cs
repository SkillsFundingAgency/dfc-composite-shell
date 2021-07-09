using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;

namespace DFC.Composite.Shell.Services.BaseUrl
{
    public interface IBaseUrlService
    {
        Uri GetBaseUrl(HttpRequest request, IUrlHelper urlHelper);
    }
}