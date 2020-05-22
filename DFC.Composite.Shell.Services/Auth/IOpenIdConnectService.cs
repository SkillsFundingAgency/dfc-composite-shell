using DFC.Composite.Shell.Services.Auth.Models;
using System.Threading.Tasks;

namespace DFC.Composite.Shell.Services.Auth
{
    public interface IOpenIdConnectService
    {
        public Task<OpenIdConnectConfig> GetOpenIDConnectConfig();

        public Task<string> GetJwkKey();
    }
}