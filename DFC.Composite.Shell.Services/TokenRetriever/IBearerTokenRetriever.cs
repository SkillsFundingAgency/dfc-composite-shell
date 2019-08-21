using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace DFC.Composite.Shell.Services.TokenRetriever
{
    public interface IBearerTokenRetriever
    {
        Task<string> GetToken(HttpContext httpContext);
    }
}