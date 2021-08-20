using DFC.Composite.Shell.Models.Common;

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;

using System.Threading.Tasks;

namespace DFC.Composite.Shell.Services.TokenRetriever
{
    public class BearerTokenRetriever : IBearerTokenRetriever
    {
        public async Task<string> GetToken(HttpContext httpContext)
        {
            return await httpContext.GetTokenAsync(Constants.BearerTokenName);
        }
    }
}