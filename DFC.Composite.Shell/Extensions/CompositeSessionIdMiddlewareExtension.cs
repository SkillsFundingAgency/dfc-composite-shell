using DFC.Composite.Shell.Middleware;
using Microsoft.AspNetCore.Builder;

namespace DFC.Composite.Shell.Extensions
{
    public static class CompositeSessionIdMiddlewareExtension
    {
        public static IApplicationBuilder UseCompositeSessionId(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<CompositeSessionIdMiddleware>();
        }
    }
}
