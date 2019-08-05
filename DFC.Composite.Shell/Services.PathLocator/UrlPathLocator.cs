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
            var result = httpContextAccessor.HttpContext.Request.Path.Value.Trim();
            if (result.StartsWith("/", StringComparison.OrdinalIgnoreCase))
            {
                result = result.Substring(1);
            }
            var forwardSlashPosition = result.IndexOf("/", StringComparison.OrdinalIgnoreCase);
            if (forwardSlashPosition != -1)
            {
                result = result.Substring(0, forwardSlashPosition);
            }
            if (!string.IsNullOrWhiteSpace(result))
            {
                result = result.ToLower();
            }
            logger.LogDebug($"PathLocator. Request.Path is {httpContextAccessor.HttpContext.Request.Path.Value} and located path is {result}");
            return result;
        }
    }
}