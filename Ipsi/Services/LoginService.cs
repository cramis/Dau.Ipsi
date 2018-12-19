using System.Threading.Tasks;
using Flurl;
using Flurl.Http;
using Ipsi.Config;
using Ipsi.Models;

using Microsoft.AspNetCore.Identity;

namespace Ipsi.Services
{
    public class LoginService
    {
        public async Task<TokenInfo> Login(LoginInfo loginInfo)
        {
            var token = await IpsiApiConfig.AuthAuthentication
            .PostJsonAsync(new
            {
                id = loginInfo.Id,
                password = loginInfo.Password,
                apikey = loginInfo.APIKey,
                isEmployeeLogin = loginInfo.isEmployeeLogin
            }).ReceiveJson<TokenInfo>();

            return token;
        }
    }
}