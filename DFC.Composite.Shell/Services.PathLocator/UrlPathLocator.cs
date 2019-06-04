using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace DFC.Composite.Shell.Services.PathLocator
{
    public class UrlPathLocator : IPathLocator
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<UrlPathLocator> _logger;

        public UrlPathLocator(IHttpContextAccessor httpContextAccessor, ILogger<UrlPathLocator> logger)
        {
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
        }

        public string GetPath()
        {
            var result = _httpContextAccessor.HttpContext.Request.Path.Value.Replace(@"/", string.Empty);
            _logger.LogDebug($"PathLocator. Request.Path is {_httpContextAccessor.HttpContext.Request.Path.Value} and path is {result}");
            return result;
        }
    }
}
