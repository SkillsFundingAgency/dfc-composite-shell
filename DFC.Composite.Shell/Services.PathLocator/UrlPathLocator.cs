using Microsoft.AspNetCore.Http;

namespace DFC.Composite.Shell.Services.PathLocator
{
    public class UrlPathLocator : IPathLocator
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public UrlPathLocator(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public string GetPath()
        {
            return _httpContextAccessor.HttpContext.Request.Path.Value.Replace(@"/", string.Empty);
        }
    }
}
