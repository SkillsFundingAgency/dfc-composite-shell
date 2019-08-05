using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;

namespace DFC.Composite.Shell.Services.PathLocator
{
    public class UrlPathLocator : IPathLocator
    {
        private readonly IHttpContextAccessor httpContextAccessor;
        private readonly ILogger<UrlPathLocator> logger;

        public UrlPathLocator(IHttpContextAccessor httpContextAccessor, ILogger<UrlPathLocator> logger)
        {
            this.httpContextAccessor = httpContextAccessor;
            this.logger = logger;
        }

        public string GetPath()
        {
            var result = httpContextAccessor.HttpContext.Request.Path.Value.Replace(@"/", string.Empty, StringComparison.OrdinalIgnoreCase);
            logger.LogDebug($"PathLocator. Request.Path is {httpContextAccessor.HttpContext.Request.Path.Value} and path is {result}");
            return result;
        }
    }
}