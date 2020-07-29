using System.IdentityModel.Tokens.Jwt;
using System.Threading.Tasks;

namespace DFC.Composite.Shell.Services.Auth
{
    public interface IOpenIdConnectClient
    {
        Task<string> GetRegisterUrl();

        Task<string> GetSignInUrl();

        Task<string> GetResetPasswordUrl();

        Task<JwtSecurityToken> ValidateToken(string token);

        Task<string> GetSignOutUrl(string redirectUrl);
    }
}
